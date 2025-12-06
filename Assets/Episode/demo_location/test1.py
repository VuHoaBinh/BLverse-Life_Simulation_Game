# import pandas as pd
# import matplotlib.pyplot as plt
# import seaborn as sns
# from io import StringIO

# # Dữ liệu bạn cung cấp (đã được định dạng)
# data = """PosX,PosY,Sleep,Food,Drink,Stress,Money,DistKitchen,DistFridge,DistSofa,DistDoor,DistBed,Timeline,MoveAction,InteractAction
# 4.5,-11.5,108,121,52,48,20,15,12.04159,3.162278,7.28011,10.29563,1200,0,0
# 4.5,-10.5,107.9444,120.9444,51.94444,48,20,14.4222,11.31371,3,7.615773,9.433981,1201,0,0
# """
# df = pd.read_csv("D:\\BLverse-Life_Simulation_Game\\Assets\\Episode\\actions.csv")

# # Chọn các cột cần thống kê
# metrics = ['Sleep', 'Food', 'Drink', 'Stress', 'Money']

# # Tính toán giá trị trung bình
# mean_values = df[metrics].mean()

# # Tạo DataFrame cho biểu đồ cột
# df_bar = pd.DataFrame({
#     'Thuộc tính': mean_values.index,
#     'Giá trị Trung bình': mean_values.values
# })

# # --- TRỰC QUAN HÓA BẰNG BIỂU ĐỒ CỘT ---
# plt.style.use('seaborn-v0_8-pastel') 
# plt.figure(figsize=(9, 6))
# # Sử dụng seaborn để tạo biểu đồ cột
# ax = sns.barplot(x='Thuộc tính', y='Giá trị Trung bình', data=df_bar, palette='deep')

# # Thêm giá trị lên trên mỗi cột (để hiển thị chi tiết)
# for p in ax.patches:
#     ax.annotate(f'{p.get_height():.2f}', 
#                 (p.get_x() + p.get_width() / 2., p.get_height()), 
#                 ha = 'center', va = 'center', 
#                 xytext = (0, 9), 
#                 textcoords = 'offset points')

# plt.title('Giá trị Trung bình của các Thuộc tính Trạng thái', fontsize=16)
# plt.xlabel('Thuộc tính (Sleep, Food, Drink, Stress, Money)', fontsize=12)
# plt.ylabel('Giá trị Trung bình', fontsize=12)
# plt.ylim(0, 230) # Đặt giới hạn Y để biểu đồ cân đối
# plt.grid(axis='y', linestyle='--', alpha=0.7)
# plt.show()




import pandas as pd
import matplotlib.pyplot as plt

df = pd.read_csv("D:\\BLverse-Life_Simulation_Game\\Assets\\Episode\\actions.csv")

# Đếm tần suất theo từng giá trị
count_move = df['MoveAction'].value_counts().sort_index()
count_interact = df['InteractAction'].value_counts().sort_index()

# Ghép thành 1 DataFrame (fill NaN = 0)
df_count = pd.DataFrame({
    "MoveAction": count_move,
    "InteractAction": count_interact
}).fillna(0)

# Giá trị trục X: vị trí bắt đầu từ 1, còn nhãn giữ nguyên (giá trị action)
labels = df_count.index.tolist()
 x_positions = range(1, len(labels) + 1)

# Vẽ stacked bar
plt.figure(figsize=(8, 5))

# Nếu bạn muốn hiển thị MoveAction dưới cùng, có thể bật lại dòng dưới
# plt.bar(x_positions, df_count["MoveAction"], label="MoveAction (bottom)")
plt.bar(x_positions, df_count["InteractAction"], label="InteractAction (top)")

# Đặt nhãn trục x thành các giá trị action (nhưng đánh số vị trí từ 1 trở đi)
plt.xticks(x_positions, labels)

plt.title("Tần suất của InteractAction", fontsize=14)
plt.xlabel("Giá trị Action", fontsize=12)
plt.ylabel("Số lần xuất hiện", fontsize=12)
plt.legend()
plt.grid(axis='y', linestyle='--', alpha=0.5)

plt.show()
