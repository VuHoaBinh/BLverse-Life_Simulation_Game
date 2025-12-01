#!/usr/bin/env python3
"""Read an events file and produce a histogram image.

Supports CSV, TSV, JSON/JSONL, or simple numeric-per-line text files.
If `pandas` is available it will be used for CSV parsing.
"""
from __future__ import annotations
import argparse
import os
import sys
import json
import csv
from typing import Iterable, List, Union

try:
    import pandas as pd
except Exception:
    pd = None

import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt


def is_number(s: str) -> bool:
    try:
        float(s)
        return True
    except Exception:
        return False


def read_data(path: str, column: Union[str, int, None] = None) -> List[float]:
    path = os.path.abspath(path)
    if not os.path.exists(path):
        raise FileNotFoundError(path)

    ext = os.path.splitext(path)[1].lower()
    # CSV/TSV
    if ext in ('.csv', '.tsv'):
        sep = '\t' if ext == '.tsv' else ','
        if pd is not None:
            df = pd.read_csv(path, sep=sep)
            if column is None:
                num_cols = df.select_dtypes(include='number').columns
                if len(num_cols) == 0:
                    raise ValueError('No numeric column found in CSV')
                series = df[num_cols[0]].dropna().astype(float)
                return series.tolist()
            else:
                # column can be name or index
                if isinstance(column, int) or (isinstance(column, str) and column.isdigit()):
                    idx = int(column)
                    series = df.iloc[:, idx].dropna().astype(float)
                    return series.tolist()
                else:
                    series = df[column].dropna().astype(float)
                    return series.tolist()
        else:
            # lightweight csv fallback
            vals: List[float] = []
            with open(path, newline='', encoding='utf-8') as f:
                rdr = csv.reader(f, delimiter=sep)
                headers = next(rdr, None)
                idx = None
                if column is not None:
                    try:
                        idx = int(column)
                    except Exception:
                        if headers and column in headers:
                            idx = headers.index(column)
                for row in rdr:
                    if idx is None:
                        # pick first numeric in row
                        for x in row:
                            if is_number(x):
                                vals.append(float(x))
                                break
                    else:
                        if idx < len(row) and is_number(row[idx]):
                            vals.append(float(row[idx]))
            return vals

    # JSON / JSONL
    if ext in ('.json', '.jsonl', '.ndjson'):
        vals: List[float] = []
        with open(path, 'r', encoding='utf-8') as f:
            for line in f:
                line = line.strip()
                if not line:
                    continue
                obj = json.loads(line)
                if column is None:
                    # find first numeric value
                    found = False
                    if isinstance(obj, dict):
                        for v in obj.values():
                            if isinstance(v, (int, float)):
                                vals.append(float(v))
                                found = True
                                break
                    if not found and isinstance(obj, (int, float)):
                        vals.append(float(obj))
                else:
                    v = obj.get(column) if isinstance(obj, dict) else None
                    if isinstance(v, (int, float)):
                        vals.append(float(v))
        return vals

    # fallback: treat as plain text, parse numbers
    vals: List[float] = []
    with open(path, 'r', encoding='utf-8') as f:
        for line in f:
            parts = line.strip().split()
            for p in parts:
                if is_number(p):
                    vals.append(float(p))
    return vals


def make_histogram(values: Iterable[float], bins: int, xlabel: str | None, title: str | None, outpath: str) -> None:
    values = list(values)
    if len(values) == 0:
        raise ValueError('No numeric data found to plot')
    plt.figure(figsize=(8, 5))
    plt.hist(values, bins=bins, edgecolor='black')
    plt.xlabel(xlabel or 'Value')
    plt.ylabel('Count')
    if title:
        plt.title(title)
    plt.tight_layout()
    plt.savefig(outpath, dpi=150)


def parse_args():
    p = argparse.ArgumentParser(description='Create histogram from events file (CSV/JSON/text).')
    p.add_argument('input', help='Path to events file (csv, jsonl, txt, etc)')
    p.add_argument('--column', '-c', default=None, help='Column name or index (for CSV/JSON) to plot')
    p.add_argument('--bins', '-b', type=int, default=30, help='Number of histogram bins')
    p.add_argument('--output', '-o', default='histogram.png', help='Output image path')
    p.add_argument('--xlabel', default=None, help='X axis label')
    p.add_argument('--title', default=None, help='Plot title')
    return p.parse_args()


def main():
    args = parse_args()
    try:
        data = read_data(args.input, args.column)
    except Exception as e:
        print('Error reading data:', e, file=sys.stderr)
        sys.exit(2)
    try:
        make_histogram(data, args.bins, args.xlabel, args.title or f'Histogram of {os.path.basename(args.input)}', args.output)
    except Exception as e:
        print('Error plotting histogram:', e, file=sys.stderr)
        sys.exit(3)
    print('Saved histogram to', args.output)


if __name__ == '__main__':
    main()
