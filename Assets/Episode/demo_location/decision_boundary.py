import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from scipy.interpolate import interp1d
import os


def make_stacked_plot(csv_path=None, num_points=200, save_path=None):
    if csv_path is None:
        csv_path = os.path.join('D:', os.sep, 'BLverse-Life_Simulation_Game', 'Assets', 'Episode', 'stats_log.csv')

    df = pd.read_csv(csv_path)

    # Expect columns: time,char1_food,char1_drink,char1_sleep,char1_stress
    required = ['char1_food', 'char1_drink', 'char1_sleep', 'char1_stress']
    for c in required:
        if c not in df.columns:
            raise ValueError(f'Missing column in CSV: {c}')

    # Use 'time' column if present, else use index
    if 'time' in df.columns:
        x = df['time'].astype(float).values
    else:
        x = np.arange(len(df), dtype=float)

    # Normalize x to range [-1, 1]
    x_min, x_max = x.min(), x.max()
    if x_max == x_min:
        x_norm = np.linspace(-1, 1, num_points)
    else:
        x_norm = (x - x_min) / (x_max - x_min) * 2.0 - 1.0

    x_interp = np.linspace(-1.0, 1.0, num_points)

    # Interpolate each required column onto x_interp
    df_interp = pd.DataFrame(index=x_interp)
    for col in required:
        y = df[col].astype(float).values
        # interp1d expects increasing x; ensure x_norm is sorted
        sort_idx = np.argsort(x_norm)
        f = interp1d(x_norm[sort_idx], y[sort_idx], kind='linear', fill_value='extrapolate')
        df_interp[col] = f(x_interp)

    # Normalize rows so the stacked areas are comparable
    # First ensure no negative values (if present, shift up)
    min_row = df_interp.min(axis=1).min()
    if min_row < 0:
        df_interp = df_interp - min_row

    # Row-wise normalize to sum=1, then scale to ~2 and shift down for visual range
    df_interp = df_interp.div(df_interp.sum(axis=1), axis=0).fillna(0) * 2.0
    df_interp = df_interp - 0.25

    # Colors (pastel-ish close to example)
    colors = ['#5fd3d9', '#2f77b0', '#d74b4b', '#e299d6']

    fig, ax = plt.subplots(figsize=(10, 6))
    ax.stackplot(x_interp,
                 df_interp['char1_food'],
                 df_interp['char1_drink'],
                 df_interp['char1_sleep'],
                 df_interp['char1_stress'],
                 labels=['Food', 'Drink', 'Sleep', 'Stress'],
                 colors=colors,
                 alpha=0.95)

    ax.set_xlabel('position x')
    ax.set_ylabel('position y')
    ax.set_title('Trained Network Outputs')
    ax.set_xlim(-1, 1)
    ax.set_ylim(-0.5, 1.5)
    ax.legend(loc='upper left')
    ax.grid(True, alpha=0.3)

    plt.tight_layout()

    if save_path:
        plt.savefig(save_path, dpi=200)
        print(f'Saved figure to {save_path}')
    else:
        plt.show()


if __name__ == '__main__':
    import argparse
    parser = argparse.ArgumentParser(description='Draw stacked outputs from stats_log.csv')
    parser.add_argument('--file', '-f', help='Path to CSV file', default=None)
    parser.add_argument('--points', type=int, default=200, help='Interpolation points')
    parser.add_argument('--save', help='Path to save PNG (optional)')
    args = parser.parse_args()
    make_stacked_plot(csv_path=args.file, num_points=args.points, save_path=args.save)
