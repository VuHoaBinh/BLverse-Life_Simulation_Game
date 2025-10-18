import os

from mlagents.trainers.demo_loader import demo_to_buffer, load_demonstration
from mlagents_envs.base_env import BehaviorSpec

demo_path = r"C:/Users/M S I/Desktop/GamePartI/FinalProject/Assets/Arc/SCRIPT/Trajectories/FirstTry2.demo"

behavior_spec, info_action_pairs, meta_data = load_demonstration(demo_path)

print("=== Meta data ===")
print("Số lượng episodes:", meta_data.number_episodes)
print("Số lượng steps:", meta_data.number_steps)


# Xem các trường trong buffer
print("\nCác field trong buffer:", list(buffer.keys()))

# Lấy dữ liệu action
actions = buffer["actions"]
print("\nTổng số step trong buffer:", len(actions))
print("Một vài action đầu:")
for i in range(min(5, len(actions))):
    print(actions[i])
