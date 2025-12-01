# from tbparse import SummaryReader
# import csv
# import matplotlib.pyplot as plt

# # event_file = r"D:\BLverse-Life_Simulation_Game\Assets\Episode\RL39_BC\GridBrain\events.out.tfevents.1764164944.LongWings.3656.0"

# # reader = SummaryReader(event_file, extra_columns={'wall_time'})

# # df = reader.scalars  # toàn bộ scalar trong file

# # # ML-Agents thường log tại tag:
# # tag = "Environment/Cumulative Reward"

# # reward_df = df[df["tag"] == tag]

# # steps = reward_df["step"].tolist()
# # rewards = reward_df["value"].tolist()
# # wall_times = reward_df["wall_time"].tolist()

# # Xuất CSV
# csv_path = r"D:\BLverse-Life_Simulation_Game\Assets\Episode\RL39_BC\rewards_steps.csv"
# with open(csv_path, "w", newline="", encoding="utf-8") as f:
#     writer = csv.writer(f)
#     writer.writerow(["step", "reward", "wall_time"])
#     for s, r, t in zip(steps, rewards, wall_times):
#         writer.writerow([s, r, t])

# print(f"✔ Wrote {len(steps)} rows to {csv_path}")
# print("First 10:", list(zip(steps, rewards))[:10])
# print("Last 10:", list(zip(steps, rewards))[-10:])

# # Histogram
# plt.hist(rewards, bins=50)
# plt.xlabel("Reward")
# plt.ylabel("Count")
# plt.title("Histogram of Rewards")
# plt.show()


import csv
import matplotlib.pyplot as plt

csv_path = r"D:\BLverse-Life_Simulation_Game\Assets\Episode\RL39_BC\rewards_steps.csv"

# Ghi file CSV
with open(csv_path, "w", newline="", encoding="utf-8") as f:
    writer = csv.writer(f)
    writer.writerow(["step", "reward", "wall_time"])
    for s, r, t in zip(steps, rewards, wall_times):
        writer.writerow([s, r, t])

# In thống kê
print(f"✔ Wrote {len(steps)} rows to {csv_path}")
print("First 10:", list(zip(steps, rewards))[:10])
print("Last 10:", list(zip(steps, rewards))[-10:])

# Vẽ histogram reward
plt.hist(rewards, bins=50)
plt.xlabel("Reward")
plt.ylabel("Count")
plt.title("Histogram of Rewards")
plt.show()
