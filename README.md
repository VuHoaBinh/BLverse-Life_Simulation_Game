# 🎮 BLverse – Adaptive NPC Behavior using Reinforcement Learning

## 📌 Introduction
**BLverse** is a graduation thesis project that focuses on applying **Reinforcement Learning (RL)**, specifically **Proximal Policy Optimization (PPO)**, to develop **adaptive Non-Player Character (NPC) behaviors** in a **2D role-playing game (2D RPG)** built with Unity.

Instead of relying on predefined scripts, NPCs in BLverse are designed to **learn, adapt, and respond dynamically** to player actions and environmental changes, aiming to simulate more realistic human-like behaviors.

---

## 🧠 Objectives
- Apply Reinforcement Learning in a game simulation environment  
- Train NPCs to make adaptive decisions based on state and reward  
- Improve realism, autonomy, and flexibility of NPC behaviors  
- Evaluate the effectiveness of **PPO** and **PPO combined with Behavioral Cloning (BC)** in a 2D RPG context  

---

## 🛠️ Technologies Used
- **Programming Languages**: C#, Python  
- **Game Engine**: Unity (2D)  
- **Reinforcement Learning**: Proximal Policy Optimization (PPO)  
- **AI Techniques**: Reinforcement Learning, Machine Learning, Deep Learning  
- **Tools**: Git, GitHub  

---

## 🏗️ System Architecture
The system consists of the following components:
- **Game Environment**: Unity-based 2D RPG simulation  
- **RL Agent (NPC)**: Interacts with the environment through observations and actions  
- **Reward Function**: Guides NPC learning toward desired behaviors  
- **Training Module**: PPO algorithm for policy optimization and updates  

---

## 🎯 Key Features
- Adaptive NPC decision-making using Reinforcement Learning  
- Real-time interaction between NPCs and players  
- Dynamic behaviors learned from experience rather than scripts  
- Modular and extensible AI architecture for future expansion  

---

## 📊 Training Process
1. Initialize NPC agent and game environment  
2. Observe the current state from the environment  
3. Select an action based on the current policy  
4. Receive a reward and next state from the environment  
5. Update the policy using PPO  
6. Repeat the process until the policy converges  

---

## 🔬 PPO vs PPO + Behavioral Cloning (BC)

### 🔹 PPO (Proximal Policy Optimization)
PPO is a pure **Reinforcement Learning** approach where NPCs learn behaviors solely through interaction with the environment and reward feedback.

**Advantages:**
- Does not require expert data  
- Strong exploration capability  
- Suitable for dynamic and complex environments  

**Limitations:**
- Slower convergence  
- Unstable or unrealistic behavior in early training stages  
- Highly dependent on reward design  

---

### 🔹 PPO + Behavioral Cloning (BC)
PPO + BC combines **imitation learning** and **reinforcement learning**. NPCs first learn basic behaviors from expert demonstrations and then refine them using PPO.

**Advantages:**
- Faster convergence  
- More natural and stable initial behaviors  
- Reduced random exploration in early stages  

**Limitations:**
- Requires expert demonstration data  
- Potential bias from low-quality demonstrations  
- May limit exploration if BC influence is too strong  


---

## 🎯 Application in BLverse
- **PPO** is suitable for NPCs that must explore and learn new behaviors in open-ended environments.  
- **PPO + BC** is more effective for simulating human-like daily activities such as working, socializing, and interacting with objects.

In BLverse, PPO + BC helps NPCs exhibit **realistic behaviors in early stages**, while PPO enables continuous **adaptation to player actions** over time.

---

## 🚀 Installation & Run
```bash
# Clone repository
git clone https://github.com/VuHoaBinh/BLverse-Life_Simulation_Game.git
