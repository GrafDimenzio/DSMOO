# DSMOO - Dimenzio's Super Mario Odyssey Online Server

DSMOO is a server implementation for the [Super Mario Odyssey Online Mod](https://github.com/CraftyBoss/SuperMarioOdysseyOnline) with **server-side plugin support**.

The DSMOO server was built **from scratch** with a focus on **modularity, plugin extensibility, and a cleaner, more maintainable codebase**.  
It includes all features of the original SMOO server while introducing many new improvements and capabilities.

## Features

- Uses the **latest SMOO network protocol**
- **Compatible with all SMOO client versions**
- Optional plugin providing **full support for SMOO+ features**
- Powerful **server-side plugin API**
- Advanced plugin capabilities including:
    - Packet handling
    - Dummy players
    - Custom gamemodes

## Windows Setup

1. Download the latest build from the [Releases](https://github.com/GrafDimenzio/DSMOO/releases) page.
2. Run `DSMOOConsole.exe`.
3. *(Optional)* Download plugins and place them in the `plugins` directory.
4. Configure the server by editing the JSON files in the `configs` directory that will be generated on first launch.

## Notes

- The server will automatically generate the required configuration files when started for the first time.
- Make sure the `plugins` and `configs` folders remain in the same directory as the server executable.