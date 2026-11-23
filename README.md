<div align="center">

# ⚡ ZynDrives
### Ultra-Fast Cloud SSD Virtual Disk Manager for Windows

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D6.svg?logo=windows)](https://microsoft.com/windows)
[![Architecture](https://img.shields.io/badge/Architecture-x64%20%7C%20Any%20CPU-success.svg)]()
[![Zero Cache](https://img.shields.io/badge/C%3A%20Cache-0%20Bytes-brightgreen.svg)]()
[![Executable Size](https://img.shields.io/badge/App%20Size-28%20KB-orange.svg)]()
[![Version](https://img.shields.io/badge/Version-v1.0.0-blueviolet.svg)]()

**ZynDrives** is an open-source, ultra-lightweight Windows desktop utility engineered to turn Google Drive, OneDrive, and cloud storage into native Windows virtual SSD drives (`J:`, `K:`, `L:`) with zero `C:` drive cache wear and silent background auto-mounting.

Developed by **ZynOps Solutions**.

[🌐 Live Landing Page](https://hafizabdulahad0.github.io/ZynDrives/) • [⬇️ Download Installer](bin/ZynDrives-Setup.exe) • [🚀 Download Portable](bin/ZynDrives.exe)

</div>

---

## 🚀 Why ZynDrives?

Official cloud desktop sync applications (such as Google Drive Desktop) force gigabytes of metadata and cache files directly onto your system `C:` drive, fill local SSD storage, and trigger filesystem locks that freeze AI coding agents (Antigravity, Claude Code, Codex, Cursor).

**ZynDrives solves this permanently:**
- Mounts unlimited cloud remotes into standard Windows drive letters (`J:`, `K:`, `L:`).
- Virtualized as native local SSD storage directly accessible to Windows File Explorer, command terminals, and compilers.
- **Zero system drive wear:** Writes stream through optimized transient memory chunk buffers without burdening `C:`.
- **100% Completely Silent:** Runs as a background service with a sleek system tray controller. No command prompt windows ever appear.
- **Windows Startup Auto-Mount:** Drives mount automatically upon user logon.

---

## 📊 Comparison

| Feature | Official Google Drive Desktop | Manual Batch Scripts | ZynDrives |
| :--- | :---: | :---: | :---: |
| **C: Drive Cache Staging** | ⚠️ Fills `C:\` drive | ⚠️ Uncontrolled | **✅ 0 Bytes (Pure RAM Streaming)** |
| **Silent Background Service** | ❌ Heavy background daemon | ❌ Pops up terminal windows | **✅ 100% Hidden Background Tray** |
| **AI Agent & IDE Compatibility** | ⚠️ Filesystem lock conflicts | ⚠️ Path breakages | **✅ Full Symlink & Atomic VFS Writes** |
| **Windows Startup Integration** | ❌ Heavy (~300 MB RAM) | ⚠️ Fragile batch files | **✅ 1-Click Native Registry Startup** |
| **Storage & Health HUD** | Basic | ❌ None | **✅ Live Capacity, Used & Free Space** |
| **Executable Footprint** | ~350 MB | N/A | **✅ Under 28 KB (0% GPU, <15 MB RAM)** |

---

## ✨ Features

- **⚡ 1-Click Instant Mount & Unmount:** Mount all configured cloud drives simultaneously with a single click.
- **🚀 Windows Startup Auto-Mount:** Automatically initializes all cloud drives silently on Windows boot.
- **🤖 AI Coding Agent Optimized:** Fully compatible with Antigravity, Claude Code, Codex, Cursor, and VS Code using atomic write modes and symlink flags.
- **🛡️ 100% Zero-Cache Engine:** Streams directly using in-memory chunk buffers (`--vfs-cache-mode writes --drive-chunk-size 128M`), preventing SSD wear.
- **📁 Multi-Account Support:** Map separate Google Drive accounts to separate drive letters (e.g., `ahad_dev:` ➔ `J:`, `hafizahad:` ➔ `K:`, `ahadawan:` ➔ `L:`).
- **🎨 Modern Dark UI:** Clean dashboard showing total combined cloud storage, active mount statuses, and free capacity.
- **🪶 Ultra-Lightweight Binary:** Self-contained executable under **28 KB** with zero external frameworks or heavy runtimes required.

---

## 📦 Installation

### Option 1: One-Click Installer (Recommended)
1. Download **`ZynDrives-Setup.exe`** from [bin/ZynDrives-Setup.exe](bin/ZynDrives-Setup.exe) or the GitHub Releases page.
2. Run `ZynDrives-Setup.exe`.
3. Choose Desktop Shortcut & Windows Startup auto-mount, then click **Install**.

### Option 2: Portable Executable (No Install Required)
1. Download **`ZynDrives.exe`** from [bin/ZynDrives.exe](bin/ZynDrives.exe).
2. Run it directly from any folder or USB drive.

---

## 💻 Command Line Interface (CLI)

ZynDrives can be driven directly via command line or automated tasks:

```cmd
# Mount all configured drives silently in the background
ZynDrives.exe --mount-all

# Unmount all active drives
ZynDrives.exe --unmount-all

# Output drive mount statuses to terminal
ZynDrives.exe --status
```

---

## 🛠️ Building from Source

ZynDrives compiles using the built-in Windows .NET Framework C# compiler (`csc.exe`) with zero external package dependencies:

```cmd
git clone https://github.com/hafizabdulahad0/ZynDrives.git
cd ZynDrives
build.bat
```

Build outputs will be generated in `bin/`:
- `bin\ZynDrives.exe` (~28 KB)
- `bin\ZynDrives-Setup.exe` (~15 KB)

---

## 📄 License

Distributed under the **MIT License**. See `LICENSE` for details.

Developed with ⚡ by **ZynOps Solutions**.
