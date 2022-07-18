from pathlib import Path
from tarfile import TarFile


def make_tar(directory, tarfile):
    print(directory.absolute(), tarfile)
    with TarFile.open(tarfile, 'w') as t:

        for d in directory.absolute().iterdir():
            if d.is_dir():
                last_file = [f for f in d.iterdir()][-1]
                t.add(last_file, arcname=f'{last_file.parent.name}_{last_file.name}')


if __name__ == '__main__':
    import argparse

    parser = argparse.ArgumentParser(description='Tar data files.')
    parser.add_argument('directory', metavar='DIR', type=Path,
                        help='directory root for data files')
    parser.add_argument('tarfile', metavar='TAR', type=Path,
                        help='tar file path')

    args = parser.parse_args()
    make_tar(args.directory, args.tarfile)