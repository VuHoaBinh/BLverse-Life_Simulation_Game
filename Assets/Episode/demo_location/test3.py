# import pandas as pd
# import matplotlib.pyplot as plt
# import numpy as np  # Để tạo dữ liệu mẫu nếu cần
# from pathlib import Path

# # Use robust paths: CSV will be placed/read from the parent 'Episode' folder
# script_dir = Path(__file__).resolve().parent
# episode_dir = script_dir.parent  # Assets/Episode
# csv_in_cwd = Path('data.csv')
# episode_csv = episode_dir / 'D:\BLverse-Life_Simulation_Game\Assets\Episode\stats_log.csv'

# # If no CSV exists, create a sample CSV in Assets/Episode
# if not csv_in_cwd.exists() and not episode_csv.exists():
#     np.random.seed(42)
#     n_samples = 100
#     time = np.arange(n_samples)
#     char1_food = np.random.normal(5, 2, n_samples)/240
#     char1_drink = np.random.normal(2, 1, n_samples)/80
#     char1_sleep = np.random.normal(7, 1.5, n_samples)/160
#     char1_stress = 1 - np.random.normal(3, 1.5, n_samples) /72

    
#     df_sample = pd.DataFrame({
#         'time': time,
#         'char1_food': char1_food,
#         'char1_drink': char1_drink,
#         'char1_sleep': char1_sleep,
#         'char1_stress': char1_stress
#     })
#     # Ensure directory exists
#     episode_dir.mkdir(parents=True, exist_ok=True)
#     df_sample.to_csv(episode_csv, index=False)
#     print(f"Created sample CSV at: {episode_csv}")

# # Prefer `data.csv` in cwd if present, else use the episode CSV
# if csv_in_cwd.exists():
#     df = pd.read_csv(csv_in_cwd)
#     print(f"Loaded CSV from current working directory: {csv_in_cwd}")
# elif episode_csv.exists():
#     df = pd.read_csv(episode_csv)
#     print(f"Loaded CSV from: {episode_csv}")
# else:
#     raise FileNotFoundError("No CSV found. Expected 'data.csv' in cwd or 'stats_log.csv' in Assets/Episode.")

# print(df.head())

# # Compute basic statistics for the chosen column
# col = 'char1_food'
# series = df[col].dropna()
# stats = {
#     'count': int(series.count()),
#     'mean': float(series.mean()),
#     'median': float(series.median()),
#     'std': float(series.std()),
#     'min': float(series.min()),
#     'max': float(series.max())
# }

# # Create a figure with two subplots: scatter (left) + histogram (right)
# fig, (ax_scatter, ax_hist) = plt.subplots(ncols=2, figsize=(14, 6), gridspec_kw={'width_ratios':[3,1]})

# # Scatter: Time vs char1_food
# ax_scatter.scatter(df['time'], df[col], alpha=0.7, color='tab:blue', s=50)
# ax_scatter.set_xlabel('Time')
# ax_scatter.set_ylabel('Char1 Food')
# ax_scatter.set_title('Time vs Char1 Food')
# ax_scatter.grid(True)

# # Histogram (distribution of char1_food)
# ax_hist.hist(series, bins=20, orientation='vertical', color='tab:orange', edgecolor='k', alpha=0.8)
# ax_hist.set_xlabel('Count')
# ax_hist.set_ylabel(col)
# ax_hist.set_title('Distribution')

# # Text box with summary stats on the scatter plot
# stat_text = (
#     f"n = {stats['count']}\n"
#     f"mean = {stats['mean']:.3f}\n"
#     f"median = {stats['median']:.3f}\n"
#     f"std = {stats['std']:.3f}\n"
#     f"min = {stats['min']:.3f}\n"
#     f"max = {stats['max']:.3f}"
# )

# props = dict(boxstyle='round', facecolor='white', alpha=0.8)
# ax_scatter.text(0.02, 0.95, stat_text, transform=ax_scatter.transAxes, fontsize=10,
#                 verticalalignment='top', bbox=props)

# # Tight layout and save to file in script folder
# fig.tight_layout()
# out_file = script_dir / 'scatter_with_stats.png'
# try:
#     # Try interactive show first
#     plt.show()
#     fig.savefig(out_file)
#     print(f"Figure also saved to: {out_file}")
# except Exception:
#     fig.savefig(out_file)
#     print(f"Plot saved to {out_file}")

# print('\nSummary statistics:')
# for k, v in stats.items():
#     if isinstance(v, float):
#         print(f"- {k}: {v:.4f}")
#     else:
#         print(f"- {k}: {v}")



# import pandas as pd
# import matplotlib.pyplot as plt


# df = pd.read_csv("D:\BLverse-Life_Simulation_Game\Assets\Episode\stats_log_fixed.csv")

# # normalize char1
# df["char1_food_norm"] = df["char2_food"] / 240
# df["char1_drink_norm"] = df["char2_drink"] / 80
# df["char1_sleep_norm"] = df["char2_sleep"] / 160
# df["char1_stress_norm"] = 1 - df["char2_stress"] / 72

# # point
# df["point"] = df[[
#     "char1_food_norm",
#     "char1_drink_norm",
#     "char1_sleep_norm",
#     "char1_stress_norm"
# ]].mean(axis=1)

# # plot
# plt.plot(df["time"], df["point"], color = 'blue')
# plt.xlabel("Thời gian")
# plt.ylabel("Trung bình (sleep, food, drink, stress)")
# plt.title("Quỹ Đạo Hành Vi NPC với PPO + BC")
# plt.grid(True)
# plt.show()

# import pandas as pd
# import matplotlib.pyplot as plt

# df = pd.read_csv("D:\\BLverse-Life_Simulation_Game\\Assets\\Episode\\stats_log_fixed.csv")

# # --- Normalize char1 ---
# df["char1_food_norm"] = df["char1_food"] / 240
# df["char1_drink_norm"] = df["char1_drink"] / 80
# df["char1_sleep_norm"] = df["char1_sleep"] / 160
# df["char1_stress_norm"] = 1 - df["char1_stress"] / 72

# # df["char1_point"] = df[[
# #     "char1_food_norm",
# #     "char1_drink_norm",
# #     "char1_sleep_norm",
# #     "char1_stress_norm"
# # ]].mean(axis=1)


# # # --- Normalize char2 ---
# # df["char2_food_norm"] = df["char2_food"] / 240
# # df["char2_drink_norm"] = df["char2_drink"] / 80
# # df["char2_sleep_norm"] = df["char2_sleep"] / 160
# # df["char2_stress_norm"] = 1 - df["char2_stress"] / 72

# # df["char2_point"] = df[[
# #     "char2_food_norm",
# #     "char2_drink_norm",
# #     "char2_sleep_norm",
# #     "char2_stress_norm"
# # ]].mean(axis=1)


# # --- Plot cả char1 và char2 ---
# plt.figure(figsize=(12, 5))

# plt.plot(df["time"], df["char1_food_norm"], label="Food", linewidth=2)
# plt.plot(df["time"], df["char1_drink_norm"], label="Drink", linewidth=2)
# plt.plot(df["time"], df["char1_sleep_norm"], label="Sleep", linewidth=2)
# plt.plot(df["time"], df["char1_stress_norm"], label="Stress", linewidth=2)

# plt.xlabel("Thời gian")
# plt.ylabel("Điểm trung bình")
# plt.title("Quỹ đạo của Food")
# plt.grid(True)
# plt.legend()    
# plt.show()

import pandas as pd
import matplotlib.pyplot as plt

df = pd.read_csv("D:\\BLverse-Life_Simulation_Game\\Assets\\Episode\\stats_log_fixed.csv")

# --- Normalize char1 ---
df["char1_food_norm"] = df["char1_food"] / 240
df["char1_drink_norm"] = df["char1_drink"] / 80
df["char1_sleep_norm"] = df["char1_sleep"] / 160
df["char1_stress_norm"] = 1 - df["char1_stress"] / 72

# --- Normalize char2 ---
df["char2_food_norm"] = df["char2_food"] / 240
df["char2_drink_norm"] = df["char2_drink"] / 80
df["char2_sleep_norm"] = df["char2_sleep"] / 160
df["char2_stress_norm"] = 1 - df["char2_stress"] / 72

# --- Subplots 2x2 ---
fig, axes = plt.subplots(2, 2, figsize=(8, 6))
fig.suptitle("Quỹ đạo trạng thái NPC theo thời gian", fontsize=14)

# --- 1. FOOD ---
axes[0,0].plot(df["time"], df["char1_food_norm"], label="Food PPO", linewidth=2, color='red')
axes[0,0].plot(df["time"], df["char2_food_norm"], label="Food PPO + BC", linewidth=2, color='blue')
axes[0,0].set_title("Food")
axes[0,0].grid(True)
axes[0,0].legend()

# --- 2. DRINK ---
axes[0,1].plot(df["time"], df["char1_drink_norm"], label="Drink PPO", linewidth=2, color='red')
axes[0,1].plot(df["time"], df["char2_drink_norm"], label="Drink PPO + BC", linewidth=2, color='blue')
axes[0,1].set_title("Drink")
axes[0,1].grid(True)
axes[0,1].legend()

# --- 3. SLEEP ---
axes[1,0].plot(df["time"], df["char1_sleep_norm"], label="Sleep PPO", linewidth=2 , color='red')
axes[1,0].plot(df["time"], df["char2_sleep_norm"], label="Sleep PPO + BC", linewidth=2, color='blue')
axes[1,0].set_title("Sleep")
axes[1,0].grid(True)
axes[1,0].legend()

# --- 4. STRESS ---
axes[1,1].plot(df["time"], df["char1_stress_norm"], label="Stress PPO", linewidth=2 , color='red')
axes[1,1].plot(df["time"], df["char2_stress_norm"], label="Stress PPO + BC", linewidth=2, color='blue')
axes[1,1].set_title("Stress")
axes[1,1].grid(True)
axes[1,1].legend()

plt.tight_layout(rect=[0, 0, 1, 0.96])

plt.show()


