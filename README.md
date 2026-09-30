# Bouncing Valheim

A funny, lightweight **BepInEx mod for Valheim** that adds a squash-and-stretch bounce effect to characters and NPCs, complete with custom audio!

---

## Features

- **Dynamic Bounce Physics:** Adds a bouncing motion with realistic squash-and-stretch scaling to characters.
- **Custom Sound Effects:** Plays a custom sound (`bounce.wav`) during movement.
- **Highly Configurable:** Toggle effects separately for players and NPCs, adjust bounce speeds, and control audio volume via BepInEx configs.
- **Pause & Menu Friendly:** Automatically mutes audio when in menus, paused, or dead.

---

## ⚙️ Requirements

- [Valheim](https://store.steampowered.com/app/892970/Valheim/)
- [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/)

---

## 📥 Installation

### Via Thunderstore Mod Manager (Recommended)
1. Install via the **Thunderstore Mod Manager** or **R2Modman**.
2. Click **Start Modded**.

### Manual Installation
1. Download the latest release `.zip`.
2. Extract the contents into your `BepInEx/plugins/` folder.
3. Ensure the folder structure looks like this:
   ```text
   BepInEx/
   └── plugins/
       └── BouncingValheim/
           ├── BouncingValheim.dll
           └── Audio/
               └── bounce.wav
