# 📘 Project Specifications (SPECS)

This file defines the technical and functional specifications of the Unity project.  

---

## 🎮 Game Overview
- **Title:** [Your Game Title]  
- **Genre:** [e.g., Tower Defense, RPG, Puzzle]  
- **Platform:** PC / WebGL / Mobile  
- **Target Audience:** [e.g., casual players, students, competitive gamers]  
- **Game Loop:**  
  1. Player starts game session  
  2. Player interacts with core mechanics  
  3. Score/Progress tracked and stored in leaderboard  
  4. Game session ends → results displayed  

---

## 🛠 Technical Stack
- **Engine:** Unity 6000.1.12f1  
- **Language:** C#  
- **Version Control:** GitHub (with `develop` → `main` workflow)  
- **Project Management:** GitHub Projects (Kanban)  

---

## 🚀 Core Features
- 🎮 **Gameplay Mechanics**  
  - Player movement and interaction system  
  - Tower/feature system [replace with your gameplay core]  
  - Randomized tower gacha mechanic (if TD project)  
  - Tower merging system (3x same tower → upgrade)  

- 🏆 **Leaderboard System**  
  - Backend integration with Hono API  
  - Player score stored & retrieved from PostgreSQL  
  - User authentication with Supabase Auth  

- 🎨 **UI/UX**  
  - Responsive interface  
  - Dynamic HUD (score, timer, health, etc.)  
  - Menu & settings screens  

- 🔊 **Audio**  
  - Background music  
  - SFX for player actions & environment  

---

## 📂 Project Structure
```
Assets/
│── Scripts/      # Core C# scripts
│── Prefabs/      # Reusable prefabs
│── Scenes/       # Unity scenes
│── Materials/    # Materials & shaders
│── UI/           # UI canvases & elements
│── Audio/        # SFX & music
```

---

## 📏 Constraints & Requirements
- Must support cross-platform builds (PC + WebGL).
- Keep project structure clean and follow Unity naming conventions.
- Avoid unnecessary assets to reduce build size.

---

## 📅 Roadmap
- v0.1.0 → Initial prototype (core loop + basic UI)
- v0.5.0 → Add leaderboard & authentication
- v0.8.0 → Add full gameplay mechanics + polish
- v1.0.0 → Stable release with documentation & changelog

---
