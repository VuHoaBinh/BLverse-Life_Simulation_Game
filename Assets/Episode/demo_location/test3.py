import pandas as pd
import matplotlib.pyplot as plt
import numpy as np  # Để tạo dữ liệu mẫu nếu cần
from pathlib import Path

# Use robust paths: CSV will be placed/read from the parent 'Episode' folder
script_dir = Path(__file__).resolve().parent
episode_dir = script_dir.parent  # Assets/Episode
csv_in_cwd = Path('data.csv')
episode_csv = episode_dir / 'D:\BLverse-Life_Simulation_Game\Assets\Episode\stats_log.csv'

# If no CSV exists, create a sample CSV in Assets/Episode
if not csv_in_cwd.exists() and not episode_csv.exists():
    np.random.seed(42)
    n_samples = 100
    time = np.arange(n_samples)
    char1_food = np.random.normal(5, 2, n_samples)
    char1_drink = np.random.normal(2, 1, n_samples)
    char1_sleep = np.random.normal(7, 1.5, n_samples)
    char1_stress = np.random.normal(3, 1.5, n_samples)

    df_sample = pd.DataFrame({
        'time': time,
        'char1_food': char1_food,
        'char1_drink': char1_drink,
        'char1_sleep': char1_sleep,
        'char1_stress': char1_stress
    })
    # Ensure directory exists
    episode_dir.mkdir(parents=True, exist_ok=True)
    df_sample.to_csv(episode_csv, index=False)
    print(f"Created sample CSV at: {episode_csv}")

# Prefer `data.csv` in cwd if present, else use the episode CSV
if csv_in_cwd.exists():
    df = pd.read_csv(csv_in_cwd)
    print(f"Loaded CSV from current working directory: {csv_in_cwd}")
elif episode_csv.exists():
    df = pd.read_csv(episode_csv)
    print(f"Loaded CSV from: {episode_csv}")
else:
    raise FileNotFoundError("No CSV found. Expected 'data.csv' in cwd or 'stats_log.csv' in Assets/Episode.")

print(df.head())

# Compute basic statistics for the chosen column
col = 'char1_food'
series = df[col].dropna()
stats = {
    'count': int(series.count()),
    'mean': float(series.mean()),
    'median': float(series.median()),
    'std': float(series.std()),
    'min': float(series.min()),
    'max': float(series.max())
}

# Create a figure with two subplots: scatter (left) + histogram (right)
fig, (ax_scatter, ax_hist) = plt.subplots(ncols=2, figsize=(14, 6), gridspec_kw={'width_ratios':[3,1]})

# Scatter: Time vs char1_food
ax_scatter.scatter(df['time'], df[col], alpha=0.7, color='tab:blue', s=50)
ax_scatter.set_xlabel('Time')
ax_scatter.set_ylabel('Char1 Food')
ax_scatter.set_title('Time vs Char1 Food')
ax_scatter.grid(True)

# Histogram (distribution of char1_food)
ax_hist.hist(series, bins=20, orientation='vertical', color='tab:orange', edgecolor='k', alpha=0.8)
ax_hist.set_xlabel('Count')
ax_hist.set_ylabel(col)
ax_hist.set_title('Distribution')

# Text box with summary stats on the scatter plot
stat_text = (
    f"n = {stats['count']}\n"
    f"mean = {stats['mean']:.3f}\n"
    f"median = {stats['median']:.3f}\n"
    f"std = {stats['std']:.3f}\n"
    f"min = {stats['min']:.3f}\n"
    f"max = {stats['max']:.3f}"
)

props = dict(boxstyle='round', facecolor='white', alpha=0.8)
ax_scatter.text(0.02, 0.95, stat_text, transform=ax_scatter.transAxes, fontsize=10,
                verticalalignment='top', bbox=props)

# Tight layout and save to file in script folder
fig.tight_layout()
out_file = script_dir / 'scatter_with_stats.png'
try:
    # Try interactive show first
    plt.show()
    fig.savefig(out_file)
    print(f"Figure also saved to: {out_file}")
except Exception:
    fig.savefig(out_file)
    print(f"Plot saved to {out_file}")

print('\nSummary statistics:')
for k, v in stats.items():
    if isinstance(v, float):
        print(f"- {k}: {v:.4f}")
    else:
        print(f"- {k}: {v}")