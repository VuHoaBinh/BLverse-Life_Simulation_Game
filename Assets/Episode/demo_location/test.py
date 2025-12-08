# import os
# import matplotlib.pyplot as plt
# from tensorboard.backend.event_processing import event_accumulator

# # File events
# LOG_FILE = r"D:\BLverse-Life_Simulation_Game\Assets\Episode\RLBC_07\RLBC_07\GridBrain\events.out.tfevents.1764447731.LongWings.20416.0"


# def load_step_reward(filepath):
#     ea = event_accumulator.EventAccumulator(filepath)
#     ea.Reload()

#     tag = "Environment/Cumulative Reward"

#     if tag not in ea.scalars.Keys():
#         raise ValueError(f"Tag '{tag}' not found")

#     scalars = ea.scalars.Items(tag)

#     steps = []
#     rewards = []

#     for s in scalars:
#         steps.append(int(s.step))     # step
#         rewards.append(float(s.value))  # reward

#     return steps, rewards


# # --- Load data ---
# steps, rewards = load_step_reward(LOG_FILE)


# # =====================================================
# # 1️⃣ LINE CHART — Step vs Reward
# # =====================================================
# plt.figure(figsize=(10, 5))
# plt.plot(steps, rewards,color='blue')
# plt.xlabel("Step")
# plt.ylabel("Cumulative Reward")
# plt.title("PPO Training — Line Chart (Step vs Reward)")
# plt.grid(True)
# plt.tight_layout()
# plt.show()



# # =====================================================
# # 2️⃣ SCATTER PLOT — Step vs Reward
# # =====================================================
# plt.figure(figsize=(10, 5))
# plt.scatter(steps, rewards, s=6 ,color='blue')
# plt.xlabel("Step")
# plt.ylabel("Cumulative Reward")
# plt.title("PPO Training — Scatter Plot")
# plt.grid(True)
# plt.tight_layout()
# plt.show()


# # =====================================================
# # 3️⃣ HISTOGRAM — Reward Distribution
# # =====================================================
# plt.figure(figsize=(10, 5))
# plt.hist(rewards, bins=40, color='blue')
# plt.xlabel("Reward Value")
# plt.ylabel("Frequency")
# plt.title("Reward Distribution — Histogram")
# plt.grid(True)
# plt.tight_layout()
# plt.show()


import os
import matplotlib.pyplot as plt
from tensorboard.backend.event_processing import event_accumulator

# Folder chứa nhiều file event
EVENT_DIR = r"D:\BLverse-Life_Simulation_Game\Assets\Episode\RLnotBC_01\RLnotBC_01\GridBrain"

TAG = "Environment/Cumulative Reward"


def load_event_file(filepath):
    ea = event_accumulator.EventAccumulator(filepath)
    ea.Reload()

    if TAG not in ea.scalars.Keys():
        return [], []

    scalars = ea.scalars.Items(TAG)

    steps = [int(s.step) for s in scalars]
    rewards = [float(s.value) for s in scalars]

    return steps, rewards


# =====================================================
# GOM NHIỀU FILE EVENT
# =====================================================
all_steps = []
all_rewards = []

for fname in os.listdir(EVENT_DIR):
    if fname.startswith("events.out.tfevents"):
        file_path = os.path.join(EVENT_DIR, fname)
        print("Đang load:", file_path)

        steps, rewards = load_event_file(file_path)
        all_steps.extend(steps)
        all_rewards.extend(rewards)

# Sort theo step
combined = sorted(zip(all_steps, all_rewards), key=lambda x: x[0])
all_steps = [c[0] for c in combined]
all_rewards = [c[1] for c in combined]


# =====================================================
# LINE CHART
# =====================================================
plt.figure(figsize=(12, 6))
plt.plot(all_steps, all_rewards)
plt.xlabel("Step")
plt.ylabel("Cumulative Reward")
plt.title("PPO Training — Combined Line Chart (Multiple Event Files)")
plt.grid(True)
plt.tight_layout()
plt.show()


# =====================================================
# SCATTER PLOT
# =====================================================
plt.figure(figsize=(12, 6))
plt.scatter(all_steps, all_rewards, s=5, color='red')
plt.xlabel("Step")
plt.ylabel("Cumulative Reward")
plt.title("Scatter Plot PPO")
plt.grid(True)
plt.tight_layout()
plt.show()


# =====================================================
# HISTOGRAM
# =====================================================
plt.figure(figsize=(12, 5))
plt.hist(all_rewards, bins=50)
plt.xlabel("Reward")
plt.ylabel("Frequency")
plt.title("Reward Distribution — Combined Histogram")
plt.grid(True)
plt.tight_layout()
plt.show()
