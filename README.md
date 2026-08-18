# MiAir for Windows 🎵

> **小爱音箱 AirPlay 与 DLNA 投播桥接工具（Windows 11 原生 WinUI 3 客户端）**  
> 完美支持 **Windows on ARM (WOA / Parallels Desktop 虚拟机)** 与 **传统 x64 Windows 11/10**。

---

## ✨ 核心特性

- 🪟 **精美 Windows 11 Fluent 视觉**：全原生 WinUI 3 架构，具备 Mica（云母）半透明材质、标准圆角卡片与细腻阴影微动效。
- 🔔 **系统托盘驻留**：支持关闭窗口时自动最小化到系统任务栏托盘（NotifyIcon），双击快速呼出，右键菜单一键切换音箱与退出。
- 🚀 **双架构原生支持**：
  - **ARM64 原生**：在 Apple Silicon Mac 的 **Parallels Desktop Windows 11 虚拟机** 或骁龙 X Elite 本上 100% 原生运行，零仿真开销。
  - **x64 原生**：支持传统 Intel / AMD Windows 10/11 设备。
- 📱 **多协议音频投播接收**：
  - **Apple AirPlay 1 (RAOP)**：iPhone、iPad、Mac 任意音乐应用无缝推流，ALAC 无损解码。
  - **DLNA / UPnP (MediaRenderer)**：网易云音乐、QQ 音乐、AirMusic 等应用一键投送。
- ⚡ **超低延迟起播**：内置首包瞬时突发灌水机制，局域网预缓冲可低至 300ms 甚至 0ms。
- 🔄 **智能 Token 保活与预热**：内置后台保活协程，启动即预热，每 6 小时自动与小米云端换票刷新，永不过期。
- 🔀 **智能音源抢占调度**：支持“最新设备优先”、“当前设备锁定”、“空闲后接管”、“按协议优先级”四种并发策略。

---

## 🛠️ 快速上手与编译调试

### 开发环境要求
- **操作系统**：Windows 11 / Windows 10 (1809+) 或 macOS 下的 **Parallels Desktop Windows 11 虚拟机**
- **开发工具**：[Visual Studio 2022](https://visualstudio.microsoft.com/)（勾选 `.NET 桌面开发` 与 `Windows 应用程序 SDK C# 模板`）
- **.NET SDK**：[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Go 语言环境（可选）**：仓库内已内置预编译好的 `win-arm64` 与 `win-x64` 的 `miair-core.exe`。

---

### 方式一：使用 Visual Studio 2022 打开运行（推荐）

1. 在 Windows 或 Parallels 虚拟机中打开 `MiAir.sln`。
2. 在顶部工具栏选择您的目标架构：
   - 如果是 **Parallels ARM64 虚拟机**：选择 **`ARM64`** 配置；
   - 如果是 **普通 x64 电脑**：选择 **`x64`** 配置。
3. 按 **`F5`** 或点击 **`启动`**，即可直接编译并运行调试！

---

### 方式二：使用 PowerShell 命令行一键编译

在项目根目录下打开 PowerShell 执行：

```powershell
# 自动探测当前机器架构并发布
.\build.ps1 -Configuration Release

# 或显式指定架构 (ARM64 或 x64)
.\build.ps1 -Configuration Release -Platform ARM64
```

编译产物将生成在：
`src/MiAir.WinUI/bin/{Platform}/Release/net8.0-windows10.0.19041.0/win-{Platform}/publish/`

---

## 💡 在 Parallels Desktop 虚拟机中的测试提示

1. **网络模式**：
   - 建议在 Parallels 设置中将网络适配器设置为 **【桥接网络 (Bridged Network)】** 或 **【共享网络 (Shared Network)】**。
   - 在桥接模式下，Windows 虚拟机将获得与宿主机相同的独立局域网 IP（如 `192.168.10.x`），手机可以在局域网内直接搜索到 `小爱音箱投放 (AirPlay/DLNA)`。
2. **防火墙提示**：
   - 首次启动时，Windows Defender 防火墙若弹出网络访问请求，请点击 **【允许访问专有网络】** 以开放端口（AirPlay 5000 / DLNA 8301 / HTTP 8300）。

---

## 📦 目录结构说明

```
MiAir for Windows/
├── MiAir.sln                      # Visual Studio 解决方案
├── Directory.Build.props          # 全局构建配置
├── build.ps1                      # PowerShell 自动化打包脚本
├── core/                          # 高性能 Go 守护核心
│   ├── main.go                    # 入口与 REST API
│   ├── airplay/                   # AirPlay 引擎
│   ├── dlna/                      # DLNA 引擎
│   ├── miservice/                 # 小米云端 API
│   └── bin/                       # 预编译多架构二进制
│       ├── win-arm64/miair-core.exe
│       └── win-x64/miair-core.exe
└── src/
    └── MiAir.WinUI/               # WinUI 3 桌面端项目
        ├── App.xaml / MainWindow  # Mica 主窗口与托盘生命周期
        ├── Models/                # 数据模型
        ├── ViewModels/            # MVVM 视图模型
        ├── Views/                 # Fluent 设计页面
        └── Services/              # 进程、API、设置与开机自启服务
```
