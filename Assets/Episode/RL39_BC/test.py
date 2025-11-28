# import argparse
# import numpy as np
# import matplotlib.pyplot as plt
# from tensorboard.backend.event_processing import event_accumulator
# import os

# LOG_DIR = r"D:\BLverse-Life_Simulation_Game\Assets\Episode\RL_02_StateFixAndStat\RL_02_StateFixAndStat\GridBrain"
# REWARD_TAG = "Environment/Cumulative Reward"

# def load_all_rewards(log_dir, tag=REWARD_TAG):
#     steps = []
#     vals = []

#     # Accept either a directory containing tfevents files or a single event file path
#     event_files = []
#     if os.path.isdir(log_dir):
#         event_files = [os.path.join(log_dir, f) for f in os.listdir(log_dir) if "tfevents" in f]
#     elif os.path.isfile(log_dir):
#         if "tfevents" in os.path.basename(log_dir):
#             event_files = [log_dir]
#         else:
#             raise NotADirectoryError(f"Provided file is not a tfevents file: {log_dir}")
#     else:
#         raise FileNotFoundError(f"Log directory or file not found: {log_dir}")

#     for f in event_files:
#         ea = event_accumulator.EventAccumulator(f)
#         try:
#             ea.Reload()
#         except Exception:
#             continue
#         if tag not in ea.Tags().get("scalars", []):
#             continue
#         for e in ea.Scalars(tag):
#             steps.append(e.step)
#             vals.append(e.value)

#     # sort by step
#     arr = sorted(zip(steps, vals), key=lambda x: x[0])
#     if arr:
#         steps_sorted, vals_sorted = zip(*arr)
#         return np.array(steps_sorted), np.array(vals_sorted)
#     return np.array([]), np.array([])

# def moving_average(x, w):
#     if len(x) < w:
#         return np.convolve(x, np.ones(len(x))/len(x), mode='valid')
#     return np.convolve(x, np.ones(w)/w, mode='valid')

# if __name__ == "__main__":
#     parser = argparse.ArgumentParser(description="Plot TensorBoard reward scalars")
#     parser.add_argument("--log-dir", type=str, default=LOG_DIR, help="Path to tfevents file or directory")
#     parser.add_argument("--show", action="store_true", help="Show plots interactively instead of saving")
#     parser.add_argument("--ma-window", type=int, default=1000, help="Moving average window size")
#     args = parser.parse_args()

#     SHOW_PLOTS = args.show
#     log_dir = args.log_dir

#     steps, rewards = load_all_rewards(log_dir)
#     if len(steps) == 0:
#         print("No reward scalar found.")
#         exit(1)

#     ma_w = max(1, args.ma_window)
#     rewards_ma = moving_average(rewards, ma_w)
#     # Align steps with moving-average output: take last N steps where N = len(rewards_ma)
#     if len(rewards_ma) > 0:
#         steps_ma = steps[len(steps) - len(rewards_ma):]
#     else:
#         steps_ma = np.array([])

#     plt.figure(figsize=(10,6))
#     plt.plot(steps, rewards, alpha=0.25, label="raw reward")
#     plt.plot(steps_ma, rewards_ma, linewidth=2, label=f"MA({ma_w})")
#     plt.xlabel("Step")
#     plt.ylabel("Cumulative Reward")
#     plt.title("Reward over Steps (raw + smoothed)")
#     plt.legend()
#     plt.grid(True)
#     plt.tight_layout()
#     if SHOW_PLOTS:
#         plt.show()
#     else:
#         plt.savefig("reward_over_steps_smoothed.png", dpi=300)
#         plt.close()
#         print("Saved reward_over_steps_smoothed.png")


import numpy as np
import matplotlib.pyplot as plt
from tensorboard.backend.event_processing import event_accumulator
import os

LOG_DIR_PPO = r"D:\BLverse-Life_Simulation_Game\Assets\Episode\RL12_phase02\GridBrain\events.out.tfevents.1764283063.WIN-8H6JMQBVGP2.1676.0"  # Thay bằng log PPO
LOG_DIR_BC = r"D:\BLverse-Life_Simulation_Game\Assets\Episode\RL_02_StateFixAndStat\RL_02_StateFixAndStat\GridBrain\events.out.tfevents.1764164944.LongWings.3656.0"  # Log BC+PPO
REWARD_TAG = "Environment/Cumulative Reward"

def load_rewards(log_dir, tag=REWARD_TAG):
    steps, vals = [], []

    # Support passing either a directory containing tfevents files or a single tfevents file
    event_files = []
    if os.path.isdir(log_dir):
        event_files = [os.path.join(log_dir, f) for f in os.listdir(log_dir) if "tfevents" in f]
    elif os.path.isfile(log_dir):
        if "tfevents" in os.path.basename(log_dir):
            event_files = [log_dir]
        else:
            raise NotADirectoryError(f"Provided file is not a tfevents file: {log_dir}")
    else:
        raise FileNotFoundError(f"Log directory or file not found: {log_dir}")

    for f in event_files:
        try:
            ea = event_accumulator.EventAccumulator(f)
            ea.Reload()
        except Exception:
            # skip files that cannot be read
            continue

        tags = ea.Tags().get("scalars", [])
        if tag in tags:
            for e in ea.Scalars(tag):
                steps.append(e.step)
                vals.append(e.value)

    arr = sorted(zip(steps, vals), key=lambda x: x[0])
    if arr:
        steps_arr = np.array([s for s, v in arr])
        vals_arr = np.array([v for s, v in arr])
        return steps_arr, vals_arr
    return np.array([]), np.array([])

def moving_average(x, w):
    x = np.asarray(x)
    n = len(x)
    if n == 0:
        return np.array([])
    window = min(w, n)
    return np.convolve(x, np.ones(window)/window, mode='valid')

steps_ppo, rewards_ppo = load_rewards(LOG_DIR_PPO)
steps_bc, rewards_bc = load_rewards(LOG_DIR_BC)
ma_w = 1000
rewards_ppo_ma = moving_average(rewards_ppo, ma_w)
rewards_bc_ma = moving_average(rewards_bc, ma_w)

# Align steps separately for PPO and BC moving averages
if len(rewards_ppo_ma) > 0:
    steps_ppo_ma = steps_ppo[len(steps_ppo) - len(rewards_ppo_ma):]
else:
    steps_ppo_ma = np.array([])

if len(rewards_bc_ma) > 0:
    steps_bc_ma = steps_bc[len(steps_bc) - len(rewards_bc_ma):]
else:
    steps_bc_ma = np.array([])

plt.figure(figsize=(10,6))
if len(steps_ppo) > 0:
    plt.plot(steps_ppo, rewards_ppo, alpha=0.25, label="PPO raw", color='blue')
if len(steps_ppo_ma) > 0:
    plt.plot(steps_ppo_ma, rewards_ppo_ma, linewidth=2, label=f"PPO MA({ma_w})", color='blue')
if len(steps_bc) > 0:
    plt.plot(steps_bc, rewards_bc, alpha=0.25, label="BC+PPO raw", color='red')
if len(steps_bc_ma) > 0:
    plt.plot(steps_bc_ma, rewards_bc_ma, linewidth=2, label=f"BC+PPO MA({ma_w})", color='red')
plt.xlabel("Steps"); plt.ylabel("Reward"); plt.title("Reward Curves: PPO vs BC+PPO")
plt.legend(); plt.grid(True); plt.tight_layout()
plt.savefig("reward_curves_comparison.png", dpi=300)
plt.show()  # Hoặc plt.close() nếu không show