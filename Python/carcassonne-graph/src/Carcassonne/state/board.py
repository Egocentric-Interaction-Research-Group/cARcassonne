from itertools import chain

import networkx as nx
import numpy as np
from scipy.spatial.transform import Rotation as R
from torch_geometric.data import Data

from Carcassonne import Link, Geography, to_graph, remove_last
from CarcassonneGNN import board_to_data
from CarcassonneGNN import graphml_to_board


def candidate_board(board: nx.Graph, tile: nx.Graph):
    '''Possible link spots between tile and board.'''
    MIDDLE_NODE_COUNTER = 0

    # Set candidate attribute
    nx.set_node_attributes(board, True, 'placed')
    nx.set_node_attributes(tile, False, 'placed')
    nx.set_node_attributes(board, 0, 'candidate')
    nx.set_node_attributes(tile, 1, 'candidate')

    nodes_by_position = dict([[(board.nodes[n]['x'], board.nodes[n]['y']), n] for n in board.nodes])

    # Get matches by geography type
    geo_matches = {}
    geographies = set([tile.nodes[n]['geography'] for n in tile.nodes])
    for g in geographies:
        matches = nodes_with_matching_open_ports(board, g)
        geo_matches[g] = matches

    # Set Node matches
    matches = {}
    for n in tile.nodes:
        if tile.nodes[n]['geography'] != Geography.Cloister:
            matches[n] = geo_matches[tile.nodes[n]['geography']]

    new_tiles = [board]
    new_edges = []

    link_set = set([])

    tile_nodes = np.array([[0, 1], [1, 0], [0, -1], [-1, 0]])
    link_nodes = 2 * tile_nodes
    for n in matches:
        for m in matches[n]:
            match_position = position(board.nodes[m])  # x,y coordinates of the matching node
            d = tile_link_direction(board, m)  # direction of the link from matching node
            p = d + match_position
            tile_node_positions = tile_nodes + 2 * d + match_position  # x, y coordinates of all tile nodes
            link_node_positions = link_nodes + 2 * d + match_position  # x, y coordinates of all linking nodes to tile placed by matching node

            i = np.where((link_node_positions == match_position).all(axis=1))[0][0]  # index of the matched node
            clockwise_lnp = link_node_positions[list(chain(range(i + 1, 4), range(i))),
                            :]  # link nodes in clockwise arrangement from matched
            clockwise_tnp = tile_node_positions[list(chain(range(i + 1, 4), range(i))),
                            :]  # tile nodes in clockwise arrangement from matched
            clockwise_nodes = reorder_list(list(tile.nodes)[:4],n)[1:]  # reordered without the first element (n)

            match = True
            links = [[f'{n}_{p[0]}_{p[1]}', m]]
            # Test whether link nodes match clockwise nodes
            for pos, node, node_pos in zip(clockwise_lnp, clockwise_nodes, clockwise_tnp):
                t_pos = tuple(pos)
                if t_pos in nodes_by_position:
                    if node_has_matching_open_port(board, nodes_by_position[t_pos], tile.nodes[node]['geography']):
                        links.append([f'{node}_{node_pos[0]}_{node_pos[1]}', nodes_by_position[t_pos]])
                    else:
                        match = False

            # So that we don't add the same position multiple times
            if match and links[0][0] in link_set:
                match = False

            # Add as candidate
            if match:
                # Rename tile nodes to include position
                name_mapping = dict([[node, f'{node}_{p[0]}_{p[1]}'] for node, p in
                                     zip(chain([n], clockwise_nodes), chain([p], list(clockwise_tnp)))])

                # Handle middle nodes
                missing_maps = set(tile.nodes)-set(name_mapping.keys())
                if len(missing_maps) > 0:
                    assert(len(missing_maps) == 1)
                    # Set it as the mid-point of all of the other nodes in the tile
                    missing_node = missing_maps.pop()
                    missing_pos = np.average(np.concatenate([clockwise_tnp,p.reshape(-1,2)]), axis=0).astype(int)
                    name_mapping[missing_node] = f'{missing_node}_{MIDDLE_NODE_COUNTER}'
                    MIDDLE_NODE_COUNTER += 1
                    tile.nodes[missing_node]['x'] = missing_pos[0]
                    tile.nodes[missing_node]['y'] = missing_pos[1]

                new_tile = nx.relabel_nodes(tile, name_mapping)

                # Set x, y
                for node, p in zip(reorder_list(new_tile.nodes, f'{n}_{p[0]}_{p[1]}'), chain([p], list(clockwise_tnp))):
                    new_tile.nodes[node]['x'] = p[0]
                    new_tile.nodes[node]['y'] = p[1]

                nx.set_node_attributes(new_tile, len(new_tiles), 'candidate')

                # Add to board
                new_tiles.append(new_tile)

                # Link to link nodes
                candidate_edges = _link_candidates_to_board(link_set, links, new_tile)

                # board.add_edges_from(candidate_edges)
                new_edges.extend(candidate_edges)

    board = nx.union_all(new_tiles)
    board.add_edges_from(new_edges)

    return board


def _link_candidates_to_board(link_set, links, new_tile):
    '''Generate links between candidate nodes and the board nodes.

    :param link_set:
    :param links:
    :param new_tile:
    :return:
    '''
    attr = {Link.Internal: False, Link.Tile: True, Link.Feature: False}
    attr_f = {Link.Internal: False, Link.Tile: True, Link.Feature: True}
    candidate_edges = []  # [[l[0], l[1], attr] for l in links]
    for l in links:
        link_set.add(l[0])  # Track the newly linked_nodes
        if new_tile.nodes[l[0]]['geography'] in [Geography.City, Geography.Road]:
            candidate_edges.append([l[0], l[1], attr_f])
        else:
            candidate_edges.append([l[0], l[1], attr])
    return candidate_edges


def reorder_list(l: list, e):
    '''Reorders a list so that element e is first and the list loops back so all elements are covered.

    >>> reorder_list([0,1,2,3],0)
    [0, 1, 2, 3]
    >>> reorder_list([0,1,2,3],2)
    [2, 3, 0, 1]
    '''
    chained = list(chain(l, l))
    i = chained.index(e)

    return chained[i:i + len(l)]


def nodes_with_matching_open_ports(board: nx.Graph, geography: Geography) -> list[int]:
    open_matches = filter(lambda n: node_has_matching_open_port(board, n, geography), board.nodes)
    return list(open_matches)


def node_has_matching_open_port(board: nx.Graph, node: int, geography: int):
    '''Check to see if a particular node in the board has an open port that matches geography'''
    if board.nodes[node]['geography'] != geography:
        return False

    node_is_open = True
    for n in board[node]:
        for e in board[node][n]:
            if board[node][n][Link.Tile]:
                node_is_open = False

    return node_is_open


def tile_link_direction(board: nx.Graph, node: int):
    '''Which direction does a tile link go.'''
    p = position(board.nodes[node]) * len(board[node])
    for n in board[node]:
        p -= position(board.nodes[n])

    return (p / np.linalg.norm(p)).astype(int)


def position(node: dict):
    return np.array([node['x'], node['y']])


def rotate2d(vector: np.ndarray, angle: float):
    vec3d = np.append(vector, [0])
    r = R.from_euler('z', angle, degrees=True)
    return np.round(r.apply(vec3d)).astype(int)[:2]


class Board(nx.Graph):

    def __init__(self, incoming_graph_data=None, tile=None, **attr):
        super().__init__(incoming_graph_data, **attr)
        self.tile = tile

    @property
    def previous(self):
        b, t, links = remove_last(self)
        return Board(incoming_graph_data=b, tile=t)

    @property
    def candidates(self):
        return candidate_board(self, self.tile)

    @property
    def data(self) -> Data:
        d = board_to_data(self.candidates, self.tile)
        d['x'] = d['x'].to(float)
        d['y'] = d['y'].to(float)
        return d

    @classmethod
    def from_graphml(cls, f):
        board_multigraph = nx.read_graphml(f, node_type=int)
        board = to_graph(board_multigraph)
        b, t, links = remove_last(board)
        return Board(incoming_graph_data=b, tile=t)


if __name__ == "__main__":
    import doctest

    doctest.testmod()
