import os
import matplotlib.pyplot as plt
from tensorboard.backend.event_processing import event_accumulator

# File events
LOG_FILE = r"D:\BLverse-Life_Simulation_Game\Assets\Episode\RLBC_07\RLBC_07\GridBrain\events.out.tfevents.1764451312.LongWings.15476.0"


def load_step_reward(filepath):
    ea = event_accumulator.EventAccumulator(filepath)
    ea.Reload()

    tag = "Environment/Cumulative Reward"

    if tag not in ea.scalars.Keys():
        raise ValueError(f"Tag '{tag}' not found")

    scalars = ea.scalars.Items(tag)

    steps = []
    rewards = []

    for s in scalars:
        steps.append(int(s.step))     # step
        rewards.append(float(s.value))  # reward

    return steps, rewards


# --- Load data ---
steps, rewards = load_step_reward(LOG_FILE)


# =====================================================
# 1️⃣ LINE CHART — Step vs Reward
# =====================================================
plt.figure(figsize=(10, 5))
plt.plot(steps, rewards,color='blue')
plt.xlabel("Step")
plt.ylabel("Cumulative Reward")
plt.title("PPO Training — Line Chart (Step vs Reward)")
plt.grid(True)
plt.tight_layout()
plt.show()



# =====================================================
# 2️⃣ SCATTER PLOT — Step vs Reward
# =====================================================
plt.figure(figsize=(10, 5))
plt.scatter(steps, rewards, s=6 ,color='blue')
plt.xlabel("Step")
plt.ylabel("Cumulative Reward")
plt.title("PPO Training — Scatter Plot")
plt.grid(True)
plt.tight_layout()
plt.show()


# =====================================================
# 3️⃣ HISTOGRAM — Reward Distribution
# =====================================================
plt.figure(figsize=(10, 5))
plt.hist(rewards, bins=40, color='blue')
plt.xlabel("Reward Value")
plt.ylabel("Frequency")
plt.title("Reward Distribution — Histogram")
plt.grid(True)
plt.tight_layout()
plt.show()


