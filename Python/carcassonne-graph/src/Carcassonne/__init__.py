from enum import IntEnum

import networkx as nx


class Geography(IntEnum):
    Cloister = 0
    Village = 1
    Road = 2
    Field = 3
    City = 4
    Stream = 5
    CityStream = City + Stream
    RoadStream = Road + Stream
    CityRoad = City + Road

class Link(IntEnum):
    Internal = 0
    Tile = 1
    Feature = 2

def remove_edge_id(board: nx.MultiGraph):
    edges = [(e[0], e[1], board.edges[e]) for e in board.edges]

    old_edges = list(board.edges)
    board.remove_edges_from(old_edges)

def to_graph(board: nx.MultiGraph):
    b = nx.Graph()

    edges = {}
    for e in board.edges:
        if e[:2] not in edges:
            edges[e[:2]] = dict([[l, False] for l in Link])
        edges[e[:2]][board.edges[e]['type']] = True

    b.add_nodes_from(zip(board.nodes, [board.nodes[n] for n in board.nodes]))
    b.add_edges_from([(k[0],k[1],edges[k]) for k in edges])

    return b

def remove_last(board: nx.Graph):
    turn = board.nodes[list(board.nodes)[-1]]['turn']
    tile_ids = list(filter(lambda n: board.nodes[n]['turn'] == turn, board.nodes))

    if len(tile_ids) == 4:
        tile_ids = [tile_ids[0], tile_ids[1], tile_ids[3], tile_ids[2]]  # Clockwise
    else:
        tile_ids = [tile_ids[0], tile_ids[1], tile_ids[3], tile_ids[2], tile_ids[4]]  # Clockwise

    tile_subgraph = board.subgraph(tile_ids)
    tile_edges = tile_subgraph.edges

    # How the last tile was linked into the board
    last_links = list(filter(lambda e: e[0] in tile_ids or e[1] in tile_ids, board.edges - tile_edges))

    # Create the tile
    tile = nx.Graph()
    tile.add_nodes_from(zip(tile_ids, [board.nodes[i] for i in tile_ids]))
    tile.add_edges_from([(k[0],k[1],tile_edges[k]) for k in tile_edges])

    # Remove from the board
    board.remove_nodes_from(tile.nodes)

    return board, tile, last_links

