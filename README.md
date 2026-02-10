# LatticeApp Frontend

<div align="center">

![LatticeApp](https://img.shields.io/badge/LatticeApp-MC%20Server%20Panel-FF6B9D?style=for-the-badge)

**A modern Minecraft server management panel built with Avalonia UI**

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square)](https://dotnet.microsoft.com/)
[![Avalonia](https://img.shields.io/badge/Avalonia-11.0-01B4E1?style=flat-square)](https://avaloniaui.net/)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)

</div>

---

## Features

- **Cross-Platform** - Windows, Linux, macOS (x64 & ARM64)
- **Modern UI** - WinUI 3 inspired design with pink & blue accent colors
- **Dark/Light Themes** - Built-in theme switching
- **Real-time Console** - WebSocket powered live log streaming
- **Plugin Market** - Browse and download from Modrinth & Hangar
- **AI Assistant** - Integrated AI chat for server management help

## Screenshots

> Coming soon

## Requirements

- [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (if not using self-contained build)
- [LatticeApp Backend](https://github.com/[owner]/latticeapp-backend) running

## Installation

### Download

Download the latest release for your platform:

| Platform | x64 | ARM64 |
|----------|-----|-------|
| Windows | `LatticeApp-win-x64.zip` | `LatticeApp-win-arm64.zip` |
| Linux | `LatticeApp-linux-x64.tar.gz` | `LatticeApp-linux-arm64.tar.gz` |
| macOS | `LatticeApp-osx-x64.zip` | `LatticeApp-osx-arm64.zip` |

### Run

**Windows:**
```powershell
# Extract and run
Expand-Archive LatticeApp-win-x64.zip
.\Lattice.exe
```

**Linux:**
```bash
# Extract and run
tar -xzf LatticeApp-linux-x64.tar.gz
chmod +x Lattice
./Lattice
```

**macOS:**
```bash
# Extract and run
unzip LatticeApp-osx-arm64.zip
./Lattice
```

## Build from Source

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Build

```bash
# Clone repository
git clone https://github.com/[owner]/latticeapp-frontend.git
cd latticeapp-frontend

# Restore dependencies
dotnet restore

# Build
dotnet build --configuration Release

# Publish for your platform
dotnet publish -c Release -r win-x64 --self-contained true
```

## Configuration

The frontend connects to the backend at `http://localhost:3000` by default.

To change the backend URL, edit `Services/ApiService.cs`:
```csharp
private const string BaseUrl = "http://your-backend-url:port/api";
```

## Tech Stack

| Component | Technology |
|-----------|------------|
| UI Framework | Avalonia UI 11 |
| Language | C# / .NET 8 |
| MVVM | CommunityToolkit.Mvvm |
| WebSocket | Websocket.Client |
| HTTP | HttpClient |

## Project Structure

```
├── Views/
│   ├── Auth/           # Login & Register
│   ├── Dashboard/      # Server list & details
│   ├── Console/        # Live console
│   ├── Plugins/        # Plugin management & market
│   └── AI/             # AI assistant
├── ViewModels/         # MVVM view models
├── Services/           # API, WebSocket, Auth
├── Models/             # Data models
├── Converters/         # Value converters
└── Styles/             # Themes & control styles
```

## Related

- [LatticeApp Backend](https://github.com/[owner]/latticeapp-backend) - Node.js backend server

## License

[MIT](LICENSE)

---

<div align="center">

Made with ❤️ by LatticeApp Team

</div>
