from pathlib import Path

import networkx as nx

from Carcassonne import to_graph, remove_last
from Carcassonne.graph.draw import draw_board
from Carcassonne.state.board import candidate_board

f = Path('/Users/am0472/Documents/carcassone/cARcassonne/Learning/4.graphml')

board = nx.read_graphml(f, node_type=int)
b = to_graph(board)
_b, _t, _links = remove_last(b)
cboard = candidate_board(_b, _t)

draw_board(cboard)