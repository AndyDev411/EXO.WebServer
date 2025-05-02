# Exodus Web Server (Backend)

## 💡 Overview

Exodus Web Server is a cross-protocol, easy-to-use multiplayer networking framework. It aims to provide both high-level, plug-and-play components and low-level control when needed. This repository contains only the backend server implementation; client-side engines will be provided in separate repositories. The first target environment is **Unity WebGL**, allowing rapid deployment for game jams and other web-based projects.

## ❤️ Inspiration

This project draws heavily from the following frameworks:

- **Riptide** by Tom Weiland ([https://github.com/RiptideNetworking/Riptide](https://github.com/RiptideNetworking/Riptide))

  - I loved how Riptide uses attributes and static functions in C# to define packet handlers. It feels clean and flexible—letting you work directly with raw packet bytes when necessary.

- **FishNet – Unity Networking Evolved** by FirstGearGames ([https://github.com/FirstGearGames/FishNet](https://github.com/FirstGearGames/FishNet))

  - The simplicity of NetworkTransform and NetworkAnimator components in FishNet is fantastic. Drag-and-drop syncing with SyncVars inspired similar functionality here.

- **Photon PUN** by Photon Engine (Closed source)

  - Photon’s relay-server architecture inspired this project. I wanted a backend that never needs changing—only client code evolves. I also appreciated Photon’s Host/Client model and relay concepts.

## 🚂 Supported Game Engines

- **Unity** – In progress
- **Godot** – Not yet supported, but contributions are welcome!

## ☎️ Supported Transports

- **WebSockets** – Fully supported
- **UDP / TCP** – Planned for future release

## 🛠️ Setup Guide

1. Clone the repository:
   ```bash
   git clone https://github.com/AndyDev411/EXO.WebServer.git
   ```
2. Open the `EXO.WebServer.sln` solution in Visual Studio.
3. Build in `Release` configuration.
4. Create an `appsettings.json` file in the project root with the following content:
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*",
     "Kestrel": {
       "Endpoints": {
         "Http": {
           "Url": "http://*:8080"
         },
         "Https": {
           "Url": "https://*:8080",
           "Certificate": {
             "Path": "PATH/TO/YOUR/.PFX",
             "Password": "YOUR_PFX_PASSWORD"
           }
         }
       }
     }
   }
   ```
5. If using HTTPS/WSS, keep only the `Https` endpoint. If using HTTP/WS, keep only the `Http` endpoint.
6. The client should connect to:
   - Secure WebSockets: `wss://localhost:8080/ws`
   - Insecure WebSockets: `ws://localhost:8080/ws`

