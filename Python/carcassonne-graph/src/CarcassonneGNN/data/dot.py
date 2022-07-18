import json
from pathlib import Path

import pydot
from torch import Tensor
from torch_geometric.data import Data

# graphviz.Source.from_file('graph4.dot')

def dotToPyG(dotfile: Path) -> Data:
    graphs = pydot.graph_from_dot_file(dotfile)
    graph = graphs[0]

    # Compute Node feature matrix
    nodes = graph.get_node_list()
    node_tensor = []

    for node in nodes:
        if 'comment' in node.get_attributes():
            comment = node.get_attributes()['comment']

            node_json = json.loads(json.loads(comment)) # twice to undo the escaping of the \\

            node_list = node_json['location'] + [node_json[key] for key in ['geography', 'shield', 'meeple', 'player', 'turn']]

            node_tensor.append(node_list)

    x = Tensor(node_tensor)

    # Compute edge index


    # Compute edge feature matrix
    edges = graph.get_edge_list()
    edge_tensor = []
    
    for edge in edges:
        if 'comment' in edge.get_attributes():
            comment = edge.get_attributes()['comment']

            edge_json = json.loads(json.loads(comment)) # twice to undo the escaping of the \\

            edge_list = [edge_json['type']]

            edge_tensor.append(edge_list)

def nodeJsonToMatrix():
    pass


if __name__ == '__main__':
    import argparse

    parser = argparse.ArgumentParser(description='Load DOT file.')
    parser.add_argument('file', metavar='F', type=Path,
                        help='file to load')

    args = parser.parse_args()
    print(dotToPyG(args.file))