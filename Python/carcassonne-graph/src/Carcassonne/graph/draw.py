import networkx as nx

from Carcassonne import Link, Geography


def draw_board(board: nx.Graph):
    pos = position(board)
    draw_nodes(board, pos)
    draw_edges(board, pos)

    # Change candidate positions to "C"
    labels = dict([(k, type(k) == int and k or '*') for k in pos])

    nx.draw_networkx_labels(board, pos, labels)

def draw_nodes(board: nx.Graph, pos: dict):
    n_col = node_colours(board)

    nx.draw_networkx_nodes(board, pos, node_color=n_col, vmin=min(Geography), vmax=4)

def draw_edges(board: nx.Graph, pos: dict):
    e_col = edge_colours(board)
    types = set(e_col)
    nx.draw_networkx_edges(board, pos, list(board.edges), edge_color=e_col, edge_vmin=min(types), edge_vmax=max(types))
    # edges_by_colour = dict(zip(board.edges, e_col))
    # types = set(e_col)
    #
    # for t in types:
    #     print(f'arc3,rad={0.2*t/max(types)}')
    #     edges = list(filter(lambda e: edges_by_colour[e] == t, edges_by_colour))
    #     nx.draw_networkx_edges(board, pos, edges, edge_color=[t for _ in range(len(edges))], edge_vmin=min(types),
    #                            edge_vmax=max(types))#, connectionstyle='arc3,rad=0.4')#ConnectionStyle.Arc3(rad=0.2*t/max(types)))



def position(board: nx.Graph):
    return dict([(i, [board.nodes[i]['x'], board.nodes[i]['y']]) for i in board.nodes])

def node_colours(board: nx.Graph):
    return [board.nodes[i]['geography'] for i in board.nodes]

# def edge_bitmask(edge_values):
#     return reduce(lambda a, b: (a << 1) + int(b), list(edge_values)[::-1])

def edge_colours(board: nx.Graph):
    ec = []
    for e in board.edges:
        if board.edges[e][Link.Internal]:
            ec.append(Link.Internal)
        elif board.edges[e][Link.Tile]:
            ec.append(Link.Tile)
        elif board.edges[e][Link.Feature]:
            ec.append(Link.Feature)

    return ec