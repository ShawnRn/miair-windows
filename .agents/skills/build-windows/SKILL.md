---
name: build-windows
description: >-
  Builds the MiAir for Windows client for ARM64 and x64 targets.
---

# Build MiAir for Windows

## Steps

1. **Build Dual-Native Go Daemon**:
   ```bash
   ./scripts/build-core.sh
   ```

2. **Publish WinUI 3 Desktop App (in Windows / Parallels VM)**:
   ```powershell
   .\build.ps1 -Configuration Release -Platform Auto
   ```

3. **Output Location**:
   `src/MiAir.WinUI/bin/{Platform}/Release/net8.0-windows10.0.19041.0/win-{Platform}/publish/`
