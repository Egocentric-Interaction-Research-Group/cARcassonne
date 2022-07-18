import networkx as nx

import torch
from torch_geometric.data import Data
from torch_geometric.utils import from_networkx

group_node_attrs = ['geography', 'x', 'y', 'placed', 'meeple', 'player', 'shield', 'turn']


def board_to_data(board, tile) -> Data:
    # Convert from networkx.Graph
    data = from_networkx(board, group_node_attrs=group_node_attrs)

    # Assign the edge attributes
    data.edge_attributes = torch.cat(
        [torch.unsqueeze(d, 1) for d in [data['Link.Internal'], data['Link.Tile'], data['Link.Feature']]], axis=1)

    # Assign the truth data
    data.y = truth_table(data, tile)

    # Assign the position data
    data.pos = data.x[:, 1:3]

    # Normalize the X data
    x = data.x
    data.x = x / torch.tensor([4.0, x[:, 1].abs().max(), x[:, 2].abs().max(), 1.0, 1.0, 1.0, 1.0, 71.0])

    return data


def truth_table(data: Data, tile: nx.Graph) -> torch.Tensor:
    candidate_mask = data.x[:, 3].bool().logical_not()
    n = len(tile.nodes)

    tt = []
    for i in range(len(tile.nodes)):
        tt.append(
            torch.unsqueeze(data.x[:, :3].eq(
                torch.tensor(
                    [[tile.nodes[n]['geography'], tile.nodes[n]['x'], tile.nodes[n]['y']] for n in tile.nodes])[i,
                :]).all(axis=1), 1)
        )

    tt = torch.cat(tt, 1).any(axis=1)

    # Deal with partial rotational symmetries
    tt_tile_row = tt[candidate_mask].reshape([-1, n])
    tt_tile_row[tt[candidate_mask].reshape([-1, n]).all(axis=1).logical_not()] = 0.0
    tt[candidate_mask] = tt_tile_row.reshape([-1])

    return tt
