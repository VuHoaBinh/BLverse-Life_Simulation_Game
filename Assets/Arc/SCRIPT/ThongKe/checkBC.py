from mlagents_envs.demonstrations import DemoBuffer

demo_path = "C:/Users/M S I/Desktop/GamePartI/FinalProject/Assets/Arc/SCRIPT/Trajectories/BC01.demo"

# Tạo DemoBuffer
demo_buffer = DemoBuffer()
demo_buffer.load_demo(demo_path)

# Lấy tất cả actions
actions = demo_buffer.actions
print("Actions shape:", actions.shape)
print("Action distribution (mean per action dim):", actions.mean(axis=0))
