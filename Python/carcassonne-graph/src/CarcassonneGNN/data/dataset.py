from pathlib import Path

import torch
from torch_geometric.data import InMemoryDataset
from torch_geometric.transforms import NormalizeFeatures, RandomNodeSplit, Compose

from Carcassonne.state.board import Board


class CarcassonneDataset(InMemoryDataset):
    # url = 'https://mah365-my.sharepoint.com/:u:/g/personal/david_kadish_mau_se/ESX2pvPsk_lNkeG8bKNW2goBD_fxTIBFyD1Nd04H9fDURA?download=1'
    # url = 'https://drive.google.com/uc?export=download&id=1qvHq3oARgNHnDkvJTSYSDnwT0lK14dNf'
    FILE_ID = '943652199351'
    SHARE_NAME = 'f5g7j58ol23ns9fwrwmlkugmpxc30192'
    url = f'https://app.box.com/index.php?rm=box_download_shared_file&shared_name={SHARE_NAME}&file_id=f_{FILE_ID}'

    def __init__(self, root, transform=None, pre_transform=None, pre_filter=None):
        super().__init__(root, transform, pre_transform, pre_filter)
        self.data, self.slices = torch.load(self.processed_paths[0])

    @property
    def _raw_dir_iterator(self):
        return list((Path(self.raw_dir) / 'self_play_data').iterdir())

    @property
    def raw_files(self):
        return filter(lambda p: p.suffix == '.graphml', self._raw_dir_iterator)

    @property
    def raw_file_names(self):
        return [f.name for f in self._raw_dir_iterator if f.suffix == '.graphml']

    @property
    def processed_file_names(self):
        return ['data.pt']

    def download(self):
        # Download to `self.raw_dir`.
        # download_url(self.url, self.raw_dir)
        pass

    def process(self):
        # Read data into huge `Data` list.
        data_list = []

        # For every file
        for f in self.raw_files:
            print(f'Processing graph from {f}')
            board = Board.from_graphml(f)

            # Recursively process turns
            while board.number_of_nodes() > 4:  # TODO Don't hardcode 4. Check to see turn numbers on nodes.
                data_list.append(board.data)
                board = board.previous

        if self.pre_filter is not None:
            data_list = [data for data in data_list if self.pre_filter(data)]

        if self.pre_transform is not None:
            data_list = [self.pre_transform(data) for data in data_list]

        data, slices = self.collate(data_list)
        torch.save((data, slices), self.processed_paths[0])


if __name__ == '__main__':
    dataset = CarcassonneDataset(root=Path('/Users/am0472/PycharmProjects/carcassonne-graph/data'))

    print(dataset)
