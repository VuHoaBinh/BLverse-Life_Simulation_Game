import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from scipy.interpolate import interp1d
import os


def make_stacked_plot(csv_path=None, num_points=200, save_path=None):
    # Read CSV
    if csv_path is None:
        csv_path = os.path.join('D:', os.sep, 'BLverse-Life_Simulation_Game', 'Assets', 'Episode', 'stats_log.csv')
    df = pd.read_csv(csv_path)

    # Extract char1 and char2 columns (if present)
    df_char1 = df.filter(like='char1_')
    df_char1.columns = df_char1.columns.str.replace('char1_', '')  # rename to food/drink/sleep/stress

    df_char2 = df.filter(like='char2_')
    if not df_char2.empty:
        df_char2.columns = df_char2.columns.str.replace('char2_', '')

    # Determine time vector
    if 'time' in df.columns:
        x = df['time'].astype(float).values
    else:
        x = np.arange(len(df), dtype=float)

    # Normalize x to [-1,1]
    x_min, x_max = x.min(), x.max()
    x_norm = (x - x_min) / (x_max - x_min) * 2.0 - 1.0 if x_max != x_min else np.linspace(-1, 1, num_points)
    x_interp = np.linspace(-1.0, 1.0, num_points)

    def interp_and_normalize(df_source):
        # df_source expected columns: food, drink, sleep, stress (after renaming)
        required_short = ['food', 'drink', 'sleep', 'stress']
        # If some metrics missing, raise
        for c in required_short:
            if c not in df_source.columns:
                raise ValueError(f"Missing column in extracted data: {c}")
        df_i = pd.DataFrame(index=x_interp)
        sort_idx = np.argsort(x_norm)
        for col in required_short:
            y = df_source[col].astype(float).values
            f = interp1d(x_norm[sort_idx], y[sort_idx], kind='linear', fill_value='extrapolate')
            df_i[col] = f(x_interp)

        # shift up if negatives
        min_row = df_i.min(axis=1).min()
        if min_row < 0:
            df_i = df_i - min_row

        # normalize rows to sum=1, scale and shift for visual
        df_i = df_i.div(df_i.sum(axis=1), axis=0).fillna(0) * 2.0
        df_i = df_i - 0.25
        return df_i

    df1_i = interp_and_normalize(df_char1)
    df2_i = interp_and_normalize(df_char2) if not df_char2.empty else None

    colors = ['#5fd3d9', '#2f77b0', '#d74b4b', '#e299d6']

    # Plot side-by-side for comparison
    ncols = 2 if df2_i is not None else 1
    fig, axes = plt.subplots(1, ncols, figsize=(12, 5), sharey=True)
    if ncols == 1:
        axes = [axes]

    labels = ['Food', 'Drink', 'Sleep', 'Stress']

    # left: char1
    ax = axes[0]
    ax.stackplot(x_interp,
                 df1_i['food'], df1_i['drink'], df1_i['sleep'], df1_i['stress'],
                 labels=labels, colors=colors, alpha=0.95)
    ax.set_title('char1')
    ax.set_xlim(-1, 1)
    ax.set_ylim(-0.5, 1.5)
    ax.set_xlabel('position x')

    # right: char2 if exists
    if df2_i is not None:
        ax2 = axes[1]
        ax2.stackplot(x_interp,
                      df2_i['food'], df2_i['drink'], df2_i['sleep'], df2_i['stress'],
                      labels=labels, colors=colors, alpha=0.95)
        ax2.set_title('char2')
        ax2.set_xlim(-1, 1)
        ax2.set_xlabel('position x')

    # shared y label and legend
    axes[0].set_ylabel('position y')
    axes[0].grid(True, alpha=0.3)
    if df2_i is not None:
        axes[1].grid(True, alpha=0.3)

    # Legend on left
    axes[0].legend(loc='upper left')
    fig.suptitle('Trained Network Outputs (char1 vs char2)')
    plt.tight_layout(rect=[0, 0, 1, 0.96])

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

