from pathlib import Path
import networkx as nx
import numpy as np
import torch
from torch_geometric.data import Data

from Carcassonne import to_graph


def graphml_to_board(f: Path) -> nx.Graph:
    board = nx.read_graphml(f, node_type=int)
    b = to_graph(board)

    return b


if __name__ == '__main__':
    import argparse

    parser = argparse.ArgumentParser(description='Load GraphML file.')
    parser.add_argument('file', metavar='F', type=Path,
                        help='file to load')

    args = parser.parse_args()
    # print(graphMlToPyG(args.file))