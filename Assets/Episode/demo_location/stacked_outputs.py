
# # food = df["char1_food"].values
# # drink = df["char1_drink"].values
# # sleep = df["char1_sleep"].values
# # stress = df["char1_stress"].values

# import pandas as pd
# import seaborn as sns
# import matplotlib.pyplot as plt
# from io import StringIO

# df = pd.read_csv("D:\\BLverse-Life_Simulation_Game\\Assets\\Episode\\stats_log.csv")

# # Chọn các cột Food và chuyển đổi sang định dạng dài
# df_food = df[['char1_food', 'char1_drink', 'char1_sleep', 'char1_stress']].copy()
# df_melted = df_food.melt(var_name='Mô hình NPC', value_name='Mức độ Food')

# # Đổi tên cột
# df_melted['Mô hình NPC'] = df_melted['Mô hình NPC'].replace({
#     'char1_food': 'char1_food (char1)',
#     'char1_drink': 'char1_drink (char1)',
#     'char1_sleep': 'char1_sleep (char1)',
#     'char1_stress': 'char1_stress (char1)'
# })

# # Tạo Biểu đồ Mật độ Hạt nhân (KDE Plot)
# plt.figure(figsize=(10, 6))
# sns.kdeplot(
#     data=df_melted,
#     x='Mức độ Food',
#     hue='Mô hình NPC',
#     fill=True,
#     alpha=0.2,
#     palette={'char1_food (char1)': 'red', 'char1_drink (char1)': 'blue', 'char1_sleep (char1)': 'green', 'char1_stress (char1)': 'purple'}
# )

# plt.title('Phân phối Mức độ 4 trạng thái', fontsize=14)
# plt.xlabel('Mức độ ', fontsize=12)
# plt.ylabel('Mật độ', fontsize=12)
# plt.grid(True, linestyle='--', alpha=0.7)
# plt.legend(title='Mô hình', loc='upper left')
# plt.show()


import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt

df = pd.read_csv(r"D:\BLverse-Life_Simulation_Game\Assets\Episode\actions.csv")

# Chọn các cột cần thiết
cols = ["PosX","PosY","Sleep","Food","Drink","Stress","Money","DistKitchen","DistFridge","DistSofa","DistDoor","DistBed","Timeline","MoveAction","InteractAction"]
labels_map = {
    "PosX": "PosX",
    "PosY": "PosY",
    "Sleep": "Sleep",
    "Food": "Food",
    "Drink": "Drink",
    "Stress": "Stress",
    "Money": "Money",
    "DistKitchen": "DistKitchen",
    "DistFridge": "DistFridge",
    "DistSofa": "DistSofa",
    "DistDoor": "DistDoor",
    "DistBed": "DistBed",
    "Timeline": "Timeline",
    "MoveAction": "MoveAction",
    "InteractAction": "InteractAction"
}
palette = {
    "PosX": "red",
    "PosY": "blue",
    "Sleep": "green",
    "Food": "purple",
    "Drink": "orange",
    "Stress": "brown",
    "Money": "pink",
    "DistKitchen": "gray",
    "DistFridge": "cyan",
    "DistSofa": "magenta",
    "DistDoor": "yellow",
    "DistBed": "lime",
    "Timeline": "navy",
    "MoveAction": "teal",
    "InteractAction": "olive"
}

plt.figure(figsize=(10, 6))

# Vẽ từng KDE riêng để đảm bảo legend và kiểm soát màu/label
for i, col in enumerate(cols):
    series = df[col].dropna()
    label = labels_map[col]
    sns.kdeplot(
        data=series,
        fill=True,
        alpha=0.4,
        color=palette[label],
        label=label
    )

# Buộc hiển thị legend (đôi khi seaborn có thể tắt tự động)
plt.legend(title='Mô hình', loc='upper left')

# # Vẽ đường trung bình cho mỗi biến và thêm chú thích nhỏ
# means = {labels_map[c]: df[c].mean() for c in cols}
# ymin, ymax = plt.ylim()
# for idx, (label, m) in enumerate(means.items()):
#     plt.axvline(m, color=palette[label], linestyle='--', linewidth=1)
#     # đặt chú thích dọc (điều chỉnh vị trí theo idx để tránh chồng)
#     y_pos = ymax * (0.9 - idx * 0.06)
#     plt.text(m, y_pos, f'Mean {label.split()[0]}: {m:.2f}',
#              color=palette[label], rotation=90, va='top', ha='right', fontsize=9)

plt.title('Phân phối mật độ 4 thuộc tính Sinh tồn của PPO + BC', fontsize=14)
plt.xlabel('Giá trị thuộc tính (food, drink, sleep, stress)', fontsize=12)
plt.ylabel('Mật độ (Tần suất Tương đối)', fontsize=12)
plt.grid(True, linestyle='--', alpha=0.7)
plt.show()

# import pandas as pd
# import matplotlib.pyplot as plt
# from io import StringIO

# # Dữ liệu mẫu (Giả định)
# data = """time,char1_food,char1_drink,char1_sleep,char1_stress,char2_food,char2_drink,char2_sleep,char2_stress
# 1,180,60,120,5,220,70,140,0
# 2,170,55,115,10,215,68,138,0
# 3,160,50,110,15,210,66,136,0
# 4,150,45,105,20,205,64,134,5
# 5,240,80,160,0,200,62,132,5
# 6,230,75,155,0,245,85,165,0
# 7,140,40,100,25,240,80,160,0
# 8,130,35,95,30,235,78,158,0
# 9,120,30,90,35,230,76,156,5
# 10,110,25,85,40,225,74,154,5
# """
# df = pd.read_csv("D:\\BLverse-Life_Simulation_Game\\Assets\\Episode\\stats_log.csv")

# # TÙY CHỈNH UI: Sử dụng style
# plt.style.use('seaborn-v0_8-whitegrid') 

# # --- CẤU HÌNH ---
# prefix = 'char1_'
# model_name = 'PPO thuần (Char1)'
# # Sử dụng bảng màu cải tiến
# colors = {'food': '#E74C3C', 'drink': '#3498DB', 'sleep': '#9B59B6', 'stress': '#F39C12'}
# main_attrs = ['food', 'drink', 'sleep']
# stress_attr = 'stress'

# # --- TẠO BIỂU ĐỒ (DUAL Y-AXIS) ---
# fig, ax1 = plt.subplots(figsize=(14, 7)) # Tăng kích thước hình
# fig.suptitle(f'Quỹ đạo hành vi của {model_name} theo thời gian', fontsize=18, fontweight='bold')

# # 1. TRỤC X
# ax1.set_xlabel('Thời gian (Time)', fontsize=12)

# # 2. TRỤC Y CHÍNH (Trái): FOOD, DRINK, SLEEP
# ax1.set_ylabel('Mức độ Nhu cầu Sinh tồn', fontsize=12, color='black')

# for attr in main_attrs:
#     ax1.plot(df['time'], df[f'{prefix}{attr}'], 
#              label=attr.capitalize(), 
#              color=colors[attr], 
#              linestyle='-', 
#              linewidth=2.5) # Tăng độ dày đường
    
# # Thêm đường ngưỡng Food nguy hiểm
# ax1.axhline(y=100, color='#839192', linestyle=':', linewidth=1)
# ax1.text(df['time'].iloc[-1], 100, 'Ngưỡng Rủi ro', color='#839192', ha='left', fontsize=9)


# # 3. TRỤC Y PHỤ (Phải): STRESS
# ax2 = ax1.twinx()  
# ax2.set_ylabel('Mức độ Stress (Thang 0-100)', fontsize=12, color=colors[stress_attr])

# ax2.plot(df['time'], df[f'{prefix}{stress_attr}'], 
#          label=stress_attr.capitalize(), 
#          color=colors[stress_attr], 
#          linestyle='--', 
#          linewidth=2.5) # Tăng độ dày đường
# ax2.tick_params(axis='y', labelcolor=colors[stress_attr])
# ax2.set_ylim(0, 100) 

# # Thêm đường ngưỡng Stress cao
# ax2.axhline(y=80, color=colors['stress'], linestyle=':', linewidth=1)


# # 4. GỘP CHÚ THÍCH (LEGEND) và Cải thiện vị trí
# lines, labels = ax1.get_legend_handles_labels()
# lines2, labels2 = ax2.get_legend_handles_labels()
# ax1.legend(lines + lines2, labels + labels2, 
#            loc='upper center', 
#            bbox_to_anchor=(0.5, 1.15), # Đặt phía trên biểu đồ
#            ncol=4, 
#            title='Thuộc tính')

# plt.tight_layout(rect=[0, 0, 1, 0.95])
# plt.show()