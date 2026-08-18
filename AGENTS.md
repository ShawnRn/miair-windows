# AGENTS.md - MiAir for Windows 开发与维护指南

本文档为 AI Agent 及开发者维护 **MiAir for Windows**（Windows 11 原生 WinUI 3 小爱音箱投播桥接客户端）提供完整的技术架构、代码规范、多架构适配与构建指引。

---

## 1. 项目概览与双层架构

**MiAir for Windows** 采用 **WinUI 3 现代化界面 + Go 后台高性能守护进程** 的解耦架构：

```
┌─────────────────────────────────────────────────────────────┐
│                 MiAir for Windows (WinUI 3 前端)             │
│  - Windows App SDK (WinUI 3) + .NET 8                       │
│  - Mica (云母材质) + Windows 11 Fluent 视觉与圆角卡片          │
│  - 系统任务栏托盘 (NotifyIcon) + 最小化到托盘运行               │
│  - MVVM CommunityToolkit 响应式状态流                        │
└──────────────────────────────┬──────────────────────────────┘
                               │ Localhost REST API (127.0.0.1:8302)
                               ▼
┌─────────────────────────────────────────────────────────────┐
│               miair-core.exe (高性能 Go 原生守护进程)          │
│  - ARM64 原生编译 (Parallels Desktop 虚拟机 / WOA 设备)      │
│  - x64 原生编译 (传统 Intel / AMD 电脑)                      │
│  - AirPlay 1 (RAOP) 实时解密与 ALAC 解码                     │
│  - DLNA / UPnP MediaRenderer 实时音频流代理                 │
│  - 小米账号扫码登录、Token 6 小时主动保活与启动预热           │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. 目录结构说明

```
MiAir for Windows/
├── MiAir.sln                      # Visual Studio 2022 解决方案
├── Directory.Build.props          # 全局构建配置 (.NET 8 + WinAppSDK + win-arm64/win-x64)
├── build.ps1                      # Windows PowerShell 一键打包脚本
├── README.md                      # 用户手册与 Parallels 调试指南
├── core/                          # Go 后台守护进程源码
│   ├── main.go                    # 入口、命令行解析与 REST API 启动
│   ├── api/server.go              # 本地 127.0.0.1:8302 REST 控制接口
│   ├── airplay/                   # AirPlay 1 (RAOP) 引擎
│   ├── dlna/                      # DLNA / UPnP 媒体渲染代理
│   ├── miservice/                 # 小米云端 API 与 Token 主动保活
│   ├── playback/                  # 播放调度协调器
│   ├── source/                    # 多音源抢占策略管理器
│   └── bin/                       # 预编译双架构二进制
│       ├── win-arm64/miair-core.exe (ARM64 原生)
│       └── win-x64/miair-core.exe   (x64 原生)
├── src/
│   └── MiAir.WinUI/               # WinUI 3 桌面端项目
│       ├── MiAir.WinUI.csproj
│       ├── app.manifest           # PerMonitorV2 高清缩放配置
│       ├── App.xaml / App.xaml.cs # 应用程序入口与 Mica 窗口生命周期
│       ├── MainWindow.xaml (.cs)  # Mica 主窗口、自定义标题栏与托盘交互
│       ├── Models/                # 状态、设备与配置数据模型
│       ├── ViewModels/            # MVVM 视图模型 (Dashboard, Devices, Settings)
│       ├── Views/                 # Fluent 设计页面与扫码弹窗
│       ├── Services/              # 进程生命周期、REST 客户端、设置与自启服务
│       └── Converters/            # XAML 绑定转换器
└── .github/
    └── workflows/build.yml        # GitHub Actions 云端多架构自动打包流水线
```

---

## 3. 核心设计规范与开发准则

### 3.1 双架构 (ARM64 + x64) 支持规范
- **Parallels Desktop 虚拟机运行**：
  - Apple Silicon Mac 上的 Windows 11 虚拟机为 ARM64 架构。
  - 在 Visual Studio 2022 中必须选择 **`ARM64`** 配置进行构建和调试，直接加载运行 `core/bin/win-arm64/miair-core.exe`，实现 100% 原生性能与极低 CPU 占用。
- **构建输出映射**：
  - `MiAir.WinUI.csproj` 会根据编译时的 `$(PlatformTarget)` 自动打包对应架构的 `miair-core.exe` 到输出根目录。

### 3.2 数据持久化与 Windows 规范
- **存储路径**：
  - 严禁在共享网络目录使用相对路径锁死文件。
  - 数据文件统一存放在 Windows 标准目录 `%LOCALAPPDATA%\MiAir\`：
    - Token 凭据：`%LOCALAPPDATA%\MiAir\token.json`
    - 运行状态：`%LOCALAPPDATA%\MiAir\status.json`
    - 应用设置：`%LOCALAPPDATA%\MiAir\settings.json`

### 3.3 前后端 REST API 契约
- REST 控制服务运行于 `http://127.0.0.1:8302`：
  - `GET /api/status`：获取运行状态、活跃会话与 Token 保活情况。
  - `GET /api/qr` & `GET /api/qr/poll?lp=...`：获取二维码与轮询登录。
  - `GET /api/devices`：获取小爱音箱列表。
  - `POST /api/speaker/bind`：切换绑定指定 DID 音箱。
  - `POST /api/speaker/pause` & `POST /api/speaker/volume`：控制音箱。
  - `POST /api/account/logout`：注销登录。

---

## 4. 常用维护命令

### 4.1 在 macOS 上交叉编译 Go 核心
```bash
# 交叉编译出 win-arm64 与 win-x64 二进制
./scripts/build-core.sh
```

### 4.2 在 Windows / Parallels 虚拟机中构建 WinUI 3
```powershell
# 自动探测当前机器架构并发布
.\build.ps1 -Configuration Release

# 或指定 ARM64 构建
.\build.ps1 -Configuration Release -Platform ARM64
```
