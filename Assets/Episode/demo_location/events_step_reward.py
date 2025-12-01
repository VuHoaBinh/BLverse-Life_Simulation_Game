#!/usr/bin/env python3
"""Extract step vs reward from TensorBoard event files and plot/save chart.

Requires `tensorboard` (EventAccumulator) to parse `.tfevents` files. If import
fails, the script will print instructions to install a compatible tensorboard/protobuf.

Usage examples:
  python events_step_reward.py path/to/events.dir -t "Environment/Cumulative Reward" -o step_reward.png
  python events_step_reward.py path/to/events.out.tfevents... -t Reward -c --csv out.csv
"""
from __future__ import annotations
import argparse
import os
import sys
from typing import List, Tuple

try:
    from tensorboard.backend.event_processing import event_accumulator
    HAVE_TENSORBOARD = True
except Exception as e:
    event_accumulator = None
    HAVE_TENSORBOARD = False
    _IMPORT_ERROR = e

import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt


def _gather_event_paths(path: str) -> List[str]:
    if os.path.isfile(path):
        return [os.path.abspath(path)]
    if os.path.isdir(path):
        files = [os.path.join(path, f) for f in os.listdir(path) if f.startswith('events.out.tfevents')]
        files.sort(key=lambda p: os.path.getmtime(p))
        return files
    raise FileNotFoundError(path)


def extract_step_reward(paths: List[str], tag: str) -> List[Tuple[int, float]]:
    if not HAVE_TENSORBOARD:
        raise RuntimeError(f"TensorBoard EventAccumulator not available: {_IMPORT_ERROR}")

    pairs: List[Tuple[int, float]] = []
    for p in paths:
        try:
            ea = event_accumulator.EventAccumulator(p)
            ea.Reload()
        except Exception as e:
            print(f"Warning: failed to load '{p}': {e}", file=sys.stderr)
            continue

        keys = []
        try:
            keys = ea.scalars.Keys()
        except Exception:
            pass
        if tag not in keys:
            continue

        items = ea.scalars.Items(tag)
        for it in items:
            try:
                pairs.append((int(it.step), float(it.value)))
            except Exception:
                continue

    pairs.sort(key=lambda x: x[0])
    return pairs


def plot_step_reward(pairs: List[Tuple[int, float]], outpath: str, title: str | None = None, show: bool = False) -> None:
    if not pairs:
        raise ValueError('No step/reward pairs to plot')
    steps, rewards = zip(*pairs)
    plt.figure(figsize=(10, 5))
    plt.plot(steps, rewards, marker='.', linestyle='-', alpha=0.8)
    plt.xlabel('Step')
    plt.ylabel('Reward')
    if title:
        plt.title(title)
    plt.grid(True)
    plt.tight_layout()
    plt.savefig(outpath, dpi=150)
    if show:
        try:
            plt.show()
        except Exception:
            pass


def parse_args():
    p = argparse.ArgumentParser(description='Plot step vs reward from TensorBoard event files')
    p.add_argument('path', help='Path to event file or directory containing event files')
    p.add_argument('-t', '--tag', default='Environment/Cumulative Reward', help='Scalar tag name to extract')
    p.add_argument('-o', '--output', default='step_reward.png', help='Output image path')
    p.add_argument('--csv', help='Optional path to write CSV of step,reward')
    p.add_argument('--show', action='store_true', help='Also attempt to show the plot (may fail on headless)')
    return p.parse_args()


def main():
    args = parse_args()
    if not HAVE_TENSORBOARD:
        print("Error: failed to import TensorBoard EventAccumulator.", file=sys.stderr)
        print("Import error:", _IMPORT_ERROR, file=sys.stderr)
        print("Try: python -m pip install 'tensorboard' 'protobuf'", file=sys.stderr)
        sys.exit(2)

    try:
        paths = _gather_event_paths(args.path)
    except Exception as e:
        print(f"Error locating event files: {e}", file=sys.stderr)
        sys.exit(3)

    pairs = extract_step_reward(paths, args.tag)
    if not pairs:
        print(f"No data found for tag '{args.tag}' in provided event files.", file=sys.stderr)
        sys.exit(4)

    try:
        plot_step_reward(pairs, args.output, title=f"{args.tag}", show=args.show)
    except Exception as e:
        print(f"Error plotting: {e}", file=sys.stderr)
        sys.exit(5)

    if args.csv:
        try:
            with open(args.csv, 'w', encoding='utf-8') as f:
                f.write('step,reward\n')
                for s, r in pairs:
                    f.write(f"{s},{r}\n")
        except Exception as e:
            print(f"Warning: failed to write CSV: {e}", file=sys.stderr)

    print(f"Saved plot to {args.output}")
    if args.csv:
        print(f"Saved CSV to {args.csv}")


if __name__ == '__main__':
    main()
