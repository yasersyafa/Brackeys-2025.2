# 🛠 Development Guide

This document describes the **workflow**, **release process**, and **naming conventions** for this Unity project.  
Please follow these guidelines to ensure consistent and collaborative development.  

---

## 🚦 Workflow
- The `develop` branch is the **main working branch**.  
- Always create a **new branch from `develop`** when starting a feature or fix.  
- Branch names **must** start with:

1. feat/{feature-name}
2. fix/{issue-name}
3. chore/{task-name}

Example: `feat/leaderboard-system`  


- ❌ Do **not** merge directly into `main` or `develop`.  
- ✅ Always use **Pull Requests (PRs)** into `develop` and request a review from **@dhityawe**.  
- PR titles must be consistent: `Merge request from {your-branch} to develop`
- Don’t forget to update the **Kanban board** in GitHub Projects when tasks progress.  

---

## 🎮 Releasing Game
1. Create a **Pull Request** from `develop` → `main`.  
2. Update `CHANGELOG.md` with new features, fixes, and changes.  
3. Once merged, the `main` branch represents the latest stable release.  

---

## ✨ Naming Conventions
- Follow Unity’s common practices for naming:  
- **Scripts:** `PascalCase` (e.g., `PlayerController.cs`)  
- **Variables & Methods:** `camelCase` (e.g., `playerSpeed`, `movePlayer()`)  
- **Constants:** `ALL_CAPS_WITH_UNDERSCORES` (e.g., `MAX_HEALTH`)  
- **Folders/Assets:** Use clear, descriptive names (e.g., `Audio/SFX`, `Scenes/MainMenu`)  
- Avoid abbreviations unless they are well-known (e.g., `UI`, `SFX`).
- Avoid public variables unless its properties, use `[SerializeField]` instead 

---

✅ Following this guide ensures smooth collaboration and a clean release pipeline.
