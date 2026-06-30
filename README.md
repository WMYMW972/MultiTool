# 🛠️ MultiTool

一个基于 WinForms 开发的本地多功能转换工具小玩意，支持图片/音频/视频格式互转，以及 B站 视频解析下载、网易云音乐下载。（只是一个测试，孩子做着玩的）

---

## ✨ 功能列表

### 🖼️ 图片转换
- 支持格式：**JPG、PNG、BMP、GIF、TIFF、WEBP**
- 支持拖放文件，批量转换
- 自动选择输出目录

### 🎵 音频转换
- 支持格式：**MP3、WAV、FLAC、AAC、M4A、OGG、OPUS**
- 支持视频文件转音频（自动提取音轨）
- 支持 **NCM（网易云音乐加密格式）解密**

### 🎬 视频转换
- 支持格式：**MP4、AVI、MKV、MOV、WEBM、FLV、WMV、M4V、TS、MTS、M2TS、3GP**

### 🎵 网易云音乐下载
- 输入歌曲名自动搜索
- 支持付费/VIP歌曲下载（通过第三方解析接口）
- 一键下载 MP3 格式
- 自动保存到指定目录

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
从 Releases 页面下载最新版本，解压到任意目录。

### 2. 准备 ffmpeg
本工具的音视频转换功能依赖 **ffmpeg**，请自行下载并放入 `Tool` 文件夹：

- 下载地址：[ffmpeg.org](https://ffmpeg.org/) 或 [gyan.dev](https://www.gyan.dev/ffmpeg/builds/)
- 将 `ffmpeg.exe` 放入 `Tool` 文件夹

### 3. 运行
双击 `工具.exe` 即可运行。

---

## 📁 目录结构

```text
工具/
├── Tool/
│   └── ffmpeg.exe     
|   └── ncmdump.exe
├── Properties/
│   ├── AssemblyInfo.cs
│   ├── Resources.Designer.cs
│   ├── Resources.resx
│   ├── Settings.Designer.cs
│   └── Settings.settings
├── Resolvers/
│   ├── GdStudioResolver.cs
│   └── NeteaseApiResolver.cs
├── .gitignore
├── App.config
├── BilibiliParser.cs
├── Form1.Designer.cs
├── Form1.cs
├── Form1.resx
├── IMusicResolver.cs
├── MusicParser.cs
├── MusicResolverManager.cs
├── Program.cs
├── README.md
├── packages.config
├── 工具.csproj
└── 工具.sln
```
    
---

## 🧩 技术栈

- **开发语言**：C#（WinForms）
- **框架**：.NET Framework 4.7.2
- **第三方库**：
  - SixLabors.ImageSharp（图片处理）
  - Newtonsoft.Json（JSON 解析）
- **外部工具**：FFmpeg（音视频处理）

---

## ⚠️ 注意事项

- `ffmpeg.exe` 需自行下载并放入 `Tool` 文件夹，否则音视频转换功能不可用
- NCM 解密需要 `ncmdump.exe`，请自行放入 `Tool` 文件夹
- B站 解析依赖 B站 官方接口，如遇解析失败请检查网络
- 网易云音乐下载依赖第三方解析接口，可能随时失效
