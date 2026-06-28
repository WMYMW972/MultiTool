# 🛠️ 转换工具

一个基于 WinForms 开发的本地多功能转换工具小玩意，支持图片/音频/视频格式互转，以及 B站 视频解析下载。（只是一个测试，孩子做着玩的）

---

## ✨ 功能列表

### 🖼️ 图片转换
- 支持格式：JPG、PNG、BMP、GIF、TIFF、WEBP
- 支持拖放文件，批量转换
- 自动选择输出目录

### 🎵 音频转换
- 支持格式：MP3、WAV、FLAC、AAC、M4A、OGG、OPUS
- 支持视频文件转音频（自动提取音轨）
- 支持 NCM（网易云音乐加密格式）解密

### 🎬 视频转换
- 支持格式：MP4、AVI、MKV、MOV、WEBM、FLV、WMV、M4V、TS、MTS、M2TS、3GP
- 支持视频转音频

### 📺 B站 视频解析下载
- 输入 BV 号或完整链接自动解析
- 显示视频标题、封面
- 支持多分 P 选择（勾选下载）
- 支持分辨率选择（4K / 1080P+ / 1080P / 720P / 480P）
- 支持完整视频下载（视频+音轨合并）
- 支持视频和音轨分开下载
- 批量下载多个分 P

---

## 📥 使用方法

### 1. 下载并解压
从 [Releases](https://github.com/WMYMW972/Tool/releases) 页面下载最新版本，解压到任意目录。

### 2. 准备 ffmpeg
本工具的音视频转换功能依赖 ffmpeg，请自行下载并放入 `Tool` 文件夹：
- 下载地址：[ffmpeg.org](https://ffmpeg.org/download.html) 或 [gyan.dev](https://www.gyan.dev/ffmpeg/builds/)
- 将 `ffmpeg.exe` 放入 `Tool` 文件夹

### 3. 运行
双击 `工具.exe` 即可运行。

---

## 📁 目录结构

```text
工具/
├── Tool/                          # ffmpeg 存放目录（需自行放入 ffmpeg.exe）
├── Form1.cs                       # 主窗体代码
├── Form1.Designer.cs              # 主窗体设计器代码
├── Form1.resx                     # 主窗体资源文件
├── BilibiliParser.cs              # B站解析核心
├── Program.cs                     # 程序入口
├── App.config                     # 配置文件
├── packages.config                # NuGet 包配置
├── 工具.csproj                    # 项目文件
├── 工具.sln                       # 解决方案文件
└── Properties/                    # 项目属性文件夹
    └── AssemblyInfo.cs
```
    
---

## 🧩 技术栈

- 开发语言：C#（WinForms）
- 框架：.NET Framework 4.7.2
- 第三方库：
  - `SixLabors.ImageSharp`（图片处理）
  - `Newtonsoft.Json`（JSON 解析）
- 外部工具：FFmpeg（音视频处理）

---

## ⚠️ 注意事项

1. `ffmpeg.exe` 需自行下载并放入 `Tool` 文件夹，否则音视频转换功能不可用
2. NCM 解密需要 `ncmdump.exe`，请自行放入 `Tool` 文件夹
3. B站 解析依赖 B站 官方接口，如遇解析失败请检查网络
4. 本工具仅供学习交流使用，请勿用于商业用途

---
