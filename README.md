# DSMOO – Dimenzio's Super Mario Odyssey Online Server

**DSMOO** is a standalone server implementation for
the [Super Mario Odyssey Online Mod](https://github.com/CraftyBoss/SuperMarioOdysseyOnline) with **server-side plugin
support**.

The DSMOO server was **built from scratch** with a focus on **modularity, plugin extensibility, and a cleaner,
maintainable codebase**.  
It includes all features of the original SMOO server while introducing many new improvements and capabilities.

---

## Features

- Uses the **latest SMOO network protocol**
- **Compatible with all SMOO client versions**
- Optional plugin for **full SMOO+ feature support**
- Powerful **server-side plugin API**
- Advanced plugin capabilities, including:
    - Packet handling
    - Dummy players
    - Custom game modes

---

## Windows Setup

1. Download the latest build from the [Releases](https://github.com/GrafDimenzio/DSMOO/releases) page.
2. Run `DSMOOConsole.exe`.
3. *(Optional)* Download plugins and place them in the `plugins` directory.
4. Configure the server by editing the JSON files in the `configs` directory, which are generated on first launch.

> **Note:** The `plugins` and `configs` folders must stay in the same directory as the server executable.

---

## Docker

DSMOO can be run via Docker. A docker-compose.yml is included in the repo.

```bash
docker-compose up -d
```

Configs, plugins, and mods are loaded from `/dsmoo`

---

## Usage

### Commands

- Use the `help` command to list all available commands.
- Wrap arguments containing spaces in quotes, e.g., `sendall "Awesome Stage"` will teleport players to the full stage
  name instead of splitting it.
- Use the arrow keys to navigate through previous commands.

### Configs

- All configuration files are located in the `configs` folder next to your server executable.
- After editing a JSON file, use `config load` to reload the changes.

### Plugins

- Install plugins by placing `.dll` files in the `plugins` folder.
- All `.dll` files, including those in subdirectories, will be loaded automatically.

### Mods

- Place additional files in the `mods` folder to add support for custom stages or kingdoms.
- To force a stage that is unknown to the server, append `!` to the stage name, e.g., `sendall "Awesome Stage!"`.

---

## Plugins

- **DSMOO Flip** – Players are displayed upside-down for other players.
- **DSMOODiscordBot** – Ported from the original SMOO server. Configure bot token, prefix, and channel ID in
  `discord_bot.json`.
- **DSMOOPlus** – Adds support for the [SMOO+ Client](https://github.com/DaDev123/SMOO-Plus) and includes commands like
  `sethealth`. Required by some plugins.
- **DSMOOWebInterface & DSMOOProximityVoiceChat** – Work-in-progress; must be compiled manually to test.

---

## Creating a Plugin

Learn how to create a plugin in our wiki: [First Plugin](https://github.com/GrafDimenzio/DSMOO/wiki/First-Plugin)