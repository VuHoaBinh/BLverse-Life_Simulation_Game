"""
Simple CSV plotting tool.

Reads a CSV/TSV and plots histogram (of `reward`) and scatter (`step` vs `reward`).
Defaults to columns named `step` and `reward`.
"""

import argparse
import csv
import os
import sys
from typing import Tuple, List

import numpy as np
import matplotlib.pyplot as plt
import onnx
from onnx import numpy_helper


def detect_delimiter(sample: str) -> str:
    # try common delimiters
    for d in [',', '\t', ';', '|']:
        if d in sample:
            return d
    return ','


def read_two_columns(path: str, xcol: str = 'step', ycol: str = 'reward', delimiter: str = None) -> Tuple[np.ndarray, np.ndarray]:
    if not os.path.exists(path):
        raise FileNotFoundError(path)

    with open(path, 'r', newline='', encoding='utf-8') as f:
        # peek to detect delimiter if not provided
        sample = f.read(4096)
        f.seek(0)
        if not delimiter:
            delimiter = detect_delimiter(sample)
        reader = csv.DictReader(f, delimiter=delimiter)
        if reader.fieldnames is None:
            raise ValueError('No header found in CSV file')

        if xcol not in reader.fieldnames or ycol not in reader.fieldnames:
            raise ValueError(f"Required columns not found. Available: {reader.fieldnames}")

        xs: List[float] = []
        ys: List[float] = []
        for i, row in enumerate(reader):
            try:
                xv = row.get(xcol, None)
                yv = row.get(ycol, None)
                if xv is None or yv is None:
                    continue
                xv_f = float(xv)
                yv_f = float(yv)
                xs.append(xv_f)
                ys.append(yv_f)
            except Exception:
                # skip rows with conversion issues
                continue

    if not xs or not ys:
        raise ValueError('No numeric data found for the requested columns')

    return np.array(xs), np.array(ys)


def read_from_onnx(path: str, init_name: str = None) -> Tuple[np.ndarray, np.ndarray]:
    if not os.path.exists(path):
        raise FileNotFoundError(path)
    try:
        model = onnx.load(path)
    except Exception as e:
        raise RuntimeError(f'Failed to load ONNX: {e}')

    inits = {init.name: numpy_helper.to_array(init) for init in model.graph.initializer}
    if not inits:
        raise ValueError('No initializers found in ONNX model')

    if init_name:
        if init_name not in inits:
            raise ValueError(f"Initializer '{init_name}' not found. Available: {list(inits.keys())[:10]}")
        arr = inits[init_name].ravel()
    else:
        # concatenate all initializers into one long 1D array
        arr = np.concatenate([v.ravel() for v in inits.values()])

    if arr.size == 0:
        raise ValueError('No numeric data in selected initializers')

    x = np.arange(arr.size)
    y = arr.astype(float)
    return x, y


def plot_hist_reward(y: np.ndarray, bins: int = 50, save: str = None):
    plt.figure(figsize=(8, 4))
    plt.hist(y, bins=bins, color='C0', alpha=0.8)
    mean_val = float(np.mean(y))
    median_val = float(np.median(y))
    plt.axvline(mean_val, color='k', linestyle='--', linewidth=1, label=f'mean={mean_val:.4g}')
    plt.axvline(median_val, color='orange', linestyle=':', linewidth=1, label=f'median={median_val:.4g}')
    plt.title('Reward Distribution')
    plt.xlabel('Reward')
    plt.ylabel('Count')
    plt.legend()
    plt.grid(True)
    if save:
        plt.savefig(save, dpi=150, bbox_inches='tight')
        print(f'Saved histogram to {save}')
    plt.show()


def plot_scatter_step_reward(x: np.ndarray, y: np.ndarray, save: str = None):
    plt.figure(figsize=(8, 4))
    plt.scatter(x, y, s=10, alpha=0.6, color='C2')
    plt.xlabel('Step')
    plt.ylabel('Reward')
    plt.title('Step vs Reward')
    plt.grid(True)
    if save:
        plt.savefig(save, dpi=150, bbox_inches='tight')
        print(f'Saved scatter to {save}')
    plt.show()


def main():
    p = argparse.ArgumentParser(description='Plot step vs reward from a CSV/TSV')
    p.add_argument('csv', help='Path to CSV/TSV file (must contain header)')
    p.add_argument('--xcol', default='step', help='Column name for x (default: step)')
    p.add_argument('--ycol', default='reward', help='Column name for y (default: reward)')
    p.add_argument('--delimiter', help='Delimiter to use (auto-detected if omitted)')
    p.add_argument('--hist', action='store_true', help='Plot histogram of y (reward)')
    p.add_argument('--scatter', action='store_true', help='Plot scatter x vs y (step vs reward)')
    p.add_argument('--bins', type=int, default=50, help='Number of bins for histogram')
    p.add_argument('--save', help='Save plot to file (PNG/JPEG/etc)')
    p.add_argument('--init', help='(ONNX) initializer name to use (optional)')
    p.add_argument('--bar', action='store_true', help='Bar (column) plot: step vs reward')
    p.add_argument('--max-bars', type=int, default=1000, help='Maximum number of bars to plot (downsamples if larger)')

    args = p.parse_args()

    # choose reader based on file extension
    try:
        if args.csv.lower().endswith('.onnx'):
            x, y = read_from_onnx(args.csv, init_name=getattr(args, 'init', None))
        else:
            x, y = read_two_columns(args.csv, xcol=args.xcol, ycol=args.ycol, delimiter=args.delimiter)
    except Exception as e:
        print('Error reading input:', e)
        sys.exit(1)

    if not args.hist and not args.scatter:
        # default behavior: both
        args.hist = True
        args.scatter = True

    # If saving and both plots requested, create filenames for each unless save ends with an image extension and single requested
    save_hist = None
    save_scatter = None
    if args.save:
        base = args.save
        if args.hist and not args.scatter:
            save_hist = base
        elif args.scatter and not args.hist:
            save_scatter = base
        else:
            # both: append suffixes
            root, ext = os.path.splitext(base)
            if ext == '':
                ext = '.png'
            save_hist = f"{root}_hist{ext}"
            save_scatter = f"{root}_scatter{ext}"

    if args.hist:
        plot_hist_reward(y, bins=args.bins, save=save_hist)

    if args.scatter:
        plot_scatter_step_reward(x, y, save=save_scatter)

    if args.bar:
        # downsample if too many bars
        def _get_sampled(xarr, yarr, max_bars):
            n = xarr.size
            if n <= max_bars:
                return xarr, yarr
            # choose roughly evenly spaced indices
            idx = np.linspace(0, n - 1, max_bars).astype(int)
            return xarr[idx], yarr[idx]

        xs, ys = _get_sampled(x, y, args.max_bars)
        def plot_bar_step_reward(xa: np.ndarray, ya: np.ndarray, save: str = None):
            plt.figure(figsize=(12, 4))
            plt.bar(xa, ya, width=1.0, color='C4', alpha=0.8)
            plt.xlabel('Step')
            plt.ylabel('Reward')
            plt.title('Bar plot: Step vs Reward')
            plt.grid(True, axis='y')
            if save:
                plt.savefig(save, dpi=150, bbox_inches='tight')
                print(f'Saved bar plot to {save}')
            plt.show()

        # build save filename for bar plot if saving requested
        save_bar = None
        if args.save:
            if save_hist is None and save_scatter is None:
                # only bar requested
                save_bar = args.save
            else:
                root, ext = os.path.splitext(args.save)
                if ext == '':
                    ext = '.png'
                save_bar = f"{root}_bar{ext}"

        plot_bar_step_reward(xs, ys, save=save_bar)


if __name__ == '__main__':
    main()