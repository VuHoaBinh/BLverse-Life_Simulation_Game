import argparse
import numpy as np
import matplotlib.pyplot as plt
from tensorboard.backend.event_processing import event_accumulator
import os

LOG_DIR = r"D:\BLverse-Life_Simulation_Game\Assets\Episode\RL_02_StateFixAndStat\RL_02_StateFixAndStat\GridBrain"
REWARD_TAG = "Environment/Cumulative Reward"

def load_all_rewards(log_dir, tag=REWARD_TAG):
    steps = []
    vals = []

    # Accept either a directory containing tfevents files or a single event file path
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
        ea = event_accumulator.EventAccumulator(f)
        try:
            ea.Reload()
        except Exception:
            continue
        if tag not in ea.Tags().get("scalars", []):
            continue
        for e in ea.Scalars(tag):
            steps.append(e.step)
            vals.append(e.value)

    # sort by step
    arr = sorted(zip(steps, vals), key=lambda x: x[0])
    if arr:
        steps_sorted, vals_sorted = zip(*arr)
        return np.array(steps_sorted), np.array(vals_sorted)
    return np.array([]), np.array([])

def moving_average(x, w):
    if len(x) < w:
        return np.convolve(x, np.ones(len(x))/len(x), mode='valid')
    return np.convolve(x, np.ones(w)/w, mode='valid')

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Plot TensorBoard reward scalars")
    parser.add_argument("--log-dir", type=str, default=LOG_DIR, help="Path to tfevents file or directory")
    parser.add_argument("--show", action="store_true", help="Show plots interactively instead of saving")
    parser.add_argument("--ma-window", type=int, default=1000, help="Moving average window size")
    args = parser.parse_args()

    SHOW_PLOTS = args.show
    log_dir = args.log_dir

    steps, rewards = load_all_rewards(log_dir)
    if len(steps) == 0:
        print("No reward scalar found.")
        exit(1)

    ma_w = max(1, args.ma_window)
    rewards_ma = moving_average(rewards, ma_w)
    # Align steps with moving-average output: take last N steps where N = len(rewards_ma)
    if len(rewards_ma) > 0:
        steps_ma = steps[len(steps) - len(rewards_ma):]
    else:
        steps_ma = np.array([])

    plt.figure(figsize=(10,6))
    plt.plot(steps, rewards, alpha=0.25, label="raw reward")
    plt.plot(steps_ma, rewards_ma, linewidth=2, label=f"MA({ma_w})")
    plt.xlabel("Step")
    plt.ylabel("Cumulative Reward")
    plt.title("Reward over Steps (raw + smoothed)")
    plt.legend()
    plt.grid(True)
    plt.tight_layout()
    if SHOW_PLOTS:
        plt.show()
    else:
        plt.savefig("reward_over_steps_smoothed.png", dpi=300)
        plt.close()
        print("Saved reward_over_steps_smoothed.png")
