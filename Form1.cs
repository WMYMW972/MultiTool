using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using tool;

namespace tool
{
    public partial class Form1 : Form
    {
        private void ClearResolveData()
        {
            res.Clear();
            label4.Text = "";
            label4.Visible = false;
            label5.Text = "";
            label5.Visible = false;
            pictureBox1.Image = null;
            pictureBox1.Visible = false;
            checkedListBox1.Items.Clear();
            checkedListBox1.Visible = false;
            comboBox5.Items.Clear();
            comboBox5.Visible = false;
            res_dwn.Visible = false;
            _currentBvid = "";
            _currentPages.Clear();
        }
        //——————————————输入框水印——————————————
        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);

        private void SetCueBanner(TextBox textBox, string bannerText)
        {
            SendMessage(textBox.Handle, EM_SETCUEBANNER, 0, bannerText);
        }
        // ======================== 工具方法 ========================

        /// <summary>
        /// 获取 ffmpeg.exe 的完整路径
        /// </summary>
        private string GetFfmpegPath()
        {
            return Path.Combine(Application.StartupPath, "Tool", "ffmpeg.exe");
        }

        // ======================== 构造函数和加载事件 ========================

        public Form1()
        {
            InitializeComponent();
        }
        private string _currentBvid = "";
        private List<(long cid, string name)> _currentPages = new List<(long cid, string name)>();
        private BilibiliParser _parser = new BilibiliParser();
        private MusicParser _musicParser = new MusicParser();
        private List<SongInfo> _currentSongs = new List<SongInfo>();

        private void Form1_Load(object sender, EventArgs e)
        {
            SetCueBanner(res, "粘贴B站视频链接...");

            // 默认隐藏
            label5.Visible = false;
            label4.Visible = false;
            pictureBox1.Visible = false;
            checkedListBox1.Visible = false;
            comboBox5.Visible = false;
            res_dwn.Visible = false;
            music_dw.Visible = false;

            // 拖放
            listBox1.AllowDrop = true;
            listBox2.AllowDrop = true;
            listBox3.AllowDrop = true;

            listBox1.DragEnter += ListBox_DragEnter;
            listBox1.DragDrop += listBox1_DragDrop;
            listBox2.DragEnter += ListBox_DragEnter;
            listBox2.DragDrop += listBox2_DragDrop;
            listBox3.DragEnter += ListBox_DragEnter;
            listBox3.DragDrop += listBox3_DragDrop;

            // 固定 label7 标题
            label7.Text = "音乐下载";
        }

        // ======================== 页面切换事件 ========================

        private void label1_Click(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }

        private void image_Click(object sender, EventArgs e)
        {
            index.Visible = false;
            imge.Visible = true;
            radio.Visible = false;
            video_P.Visible = false;
            resolve.Visible = false;
            music_dw.Visible = false;
        }

        private void redio_Click(object sender, EventArgs e)
        {
            imge.Visible = false;
            radio.Visible = true;
            video_P.Visible = false;
            index.Visible = false;
            resolve.Visible = false;
            music_dw.Visible = false;
        }

        private void video_Click(object sender, EventArgs e)
        {
            index.Visible = false;
            imge.Visible = false;
            radio.Visible = false;
            video_P.Visible = true;
            resolve.Visible = false;
            music_dw.Visible = false;
        }

        private void resolve_B_Click(object sender, EventArgs e)
        {
            index.Visible = false;
            imge.Visible = false;
            radio.Visible = false;
            video_P.Visible = false;
            resolve.Visible = true;
            music_dw.Visible = false;
        }

        private void music_dw_index_Click(object sender, EventArgs e)
        {
            index.Visible = false;
            imge.Visible = false;
            radio.Visible = false;
            video_P.Visible = false;
            resolve.Visible = false;
            music_dw.Visible = true;
        }

        private void m_B_index_Click(object sender, EventArgs e)
        {
            index.Visible = true;
            imge.Visible = false;
            radio.Visible = false;
            video_P.Visible = false;
            resolve.Visible = false;
            music_dw.Visible = false;
        }

        // ======================== 数据存储列表 ========================

        private List<string> filePaths = new List<string>();
        private List<string> audioFilePaths = new List<string>();
        private List<string> videoFilePaths = new List<string>();

        // ======================== 拖放事件 ========================

        private void ListBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void listBox2_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                string ext = Path.GetExtension(file).ToLower();
                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" ||
                    ext == ".gif" || ext == ".tiff" || ext == ".tif" || ext == ".webp" || ext == ".heic")
                {
                    listBox2.Items.Add(Path.GetFileName(file));
                    filePaths.Add(file);
                }
            }
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                string ext = Path.GetExtension(file).ToLower();

                if (ext == ".mp3" || ext == ".wav" || ext == ".flac" || ext == ".aac" ||
                    ext == ".m4a" || ext == ".ogg" || ext == ".wma" || ext == ".opus" || ext == ".ncm")
                {
                    listBox1.Items.Add(Path.GetFileName(file));
                    audioFilePaths.Add(file);
                }
                else if (ext == ".mp4" || ext == ".avi" || ext == ".mkv" || ext == ".mov" ||
                         ext == ".webm" || ext == ".flv" || ext == ".wmv" || ext == ".m4v" ||
                         ext == ".ts" || ext == ".mts" || ext == ".m2ts" || ext == ".3gp")
                {
                    listBox1.Items.Add(Path.GetFileName(file) + " (视频)");
                    audioFilePaths.Add(file);
                }
            }
        }

        private void listBox3_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                string ext = Path.GetExtension(file).ToLower();
                if (ext == ".mp4" || ext == ".avi" || ext == ".mkv" || ext == ".mov" ||
                    ext == ".webm" || ext == ".flv" || ext == ".wmv" || ext == ".m4v" ||
                    ext == ".ts" || ext == ".mts" || ext == ".m2ts" || ext == ".3gp")
                {
                    listBox3.Items.Add(Path.GetFileName(file));
                    videoFilePaths.Add(file);
                }
            }
        }

        // ======================== 图片转换相关 ========================

        private void OpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Title = "请选择文件";
            dialog.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff;*.tif;*.webp;*.heic";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                listBox2.Items.Clear();
                filePaths.Clear();

                foreach (string filePath in dialog.FileNames)
                {
                    string fileName = Path.GetFileName(filePath);
                    listBox2.Items.Add(fileName);
                    filePaths.Add(filePath);
                }
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e) { }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) { }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            if (filePaths.Count == 0)
            {
                MessageBox.Show("请先选择文件");
                this.Cursor = Cursors.Default;
                return;
            }

            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("请先选择目标格式");
                this.Cursor = Cursors.Default;
                return;
            }
            string targetFormat = comboBox1.SelectedItem.ToString().ToUpper();

            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
            {
                this.Cursor = Cursors.Default;
                return;
            }

            string saveFolder = folderBrowserDialog1.SelectedPath;
            int successCount = 0;

            for (int i = 0; i < filePaths.Count; i++)
            {
                try
                {
                    using (var image = SixLabors.ImageSharp.Image.Load(filePaths[i]))
                    {
                        string nameWithoutExt = Path.GetFileNameWithoutExtension(filePaths[i]);
                        string savePath = Path.Combine(saveFolder, nameWithoutExt + "." + targetFormat.ToLower());

                        switch (targetFormat)
                        {
                            case "JPG":
                            case "JPEG":
                                using (var newImage = new Image<Rgba32>(image.Width, image.Height, SixLabors.ImageSharp.Color.White))
                                {
                                    newImage.Mutate(ctx => ctx.DrawImage(image, new SixLabors.ImageSharp.Point(0, 0), 1f));
                                    newImage.SaveAsJpeg(savePath);
                                }
                                break;

                            case "PNG":
                                image.SaveAsPng(savePath);
                                break;

                            case "BMP":
                                image.SaveAsBmp(savePath);
                                break;

                            case "GIF":
                                image.SaveAsGif(savePath);
                                break;

                            case "TIFF":
                                image.SaveAsTiff(savePath);
                                break;

                            case "WEBP":
                                image.SaveAsWebp(savePath);
                                break;

                            default:
                                MessageBox.Show($"不支持的格式：{targetFormat}");
                                this.Cursor = Cursors.Default;
                                return;
                        }

                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"处理 {Path.GetFileName(filePaths[i])} 失败：{ex.Message}");
                }
            }

            MessageBox.Show($"转换完成！成功 {successCount} 个文件。");
            System.Diagnostics.Process.Start(saveFolder);

            this.Cursor = Cursors.Default;
        }

        // ======================== 返回首页按钮 ========================

        private void button2_Click(object sender, EventArgs e)
        {
            imge.Visible = false;
            index.Visible = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            index.Visible = true;
            imge.Visible = false;
        }

        // ======================== 音频转换相关 ========================

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }

        private void ra_1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            if (audioFilePaths.Count == 0)
            {
                MessageBox.Show("请先选择文件");
                this.Cursor = Cursors.Default;
                return;
            }

            if (comboBox2.SelectedItem == null)
            {
                MessageBox.Show("请先选择目标格式");
                this.Cursor = Cursors.Default;
                return;
            }
            string targetFormat = comboBox2.SelectedItem.ToString().ToLower();

            string ffmpeg = GetFfmpegPath();
            if (!File.Exists(ffmpeg))
            {
                MessageBox.Show("找不到 ffmpeg.exe，请放到 Tool 文件夹里");
                this.Cursor = Cursors.Default;
                return;
            }

            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
            {
                this.Cursor = Cursors.Default;
                return;
            }

            string saveFolder = folderBrowserDialog1.SelectedPath;
            int successCount = 0;

            for (int i = 0; i < audioFilePaths.Count; i++)
            {
                try
                {
                    string currentPath = audioFilePaths[i];
                    string originalPath = audioFilePaths[i];
                    string ext = Path.GetExtension(currentPath).ToLower();

                    bool isVideoFile = (ext == ".mp4" || ext == ".avi" || ext == ".mkv" || ext == ".mov" ||
                                        ext == ".webm" || ext == ".flv" || ext == ".wmv" || ext == ".m4v" ||
                                        ext == ".ts" || ext == ".mts" || ext == ".m2ts" || ext == ".3gp");

                    if (ext == ".ncm")
                    {
                        string ncmdumpPath = Path.Combine(Application.StartupPath, "Tool", "ncmdump.exe");

                        if (!File.Exists(ncmdumpPath))
                        {
                            MessageBox.Show("找不到 ncmdump.exe，请放到 Tool 文件夹里");
                            this.Cursor = Cursors.Default;
                            return;
                        }

                        System.Diagnostics.ProcessStartInfo ncmPsi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = ncmdumpPath,
                            Arguments = $"\"{currentPath}\"",
                            CreateNoWindow = true,
                            UseShellExecute = false
                        };

                        using (System.Diagnostics.Process p = System.Diagnostics.Process.Start(ncmPsi))
                        {
                            p.WaitForExit();
                            if (p.ExitCode != 0)
                            {
                                MessageBox.Show($"解密失败：{Path.GetFileName(currentPath)}，错误代码：{p.ExitCode}");
                                continue;
                            }
                        }

                        string dir = Path.GetDirectoryName(currentPath);
                        string baseName = Path.GetFileNameWithoutExtension(currentPath);
                        string[] matches = Directory.GetFiles(dir, baseName + ".*")
                                                      .Where(f => !f.EndsWith(".ncm"))
                                                      .ToArray();

                        if (matches.Length > 0)
                            currentPath = matches[0];
                        else
                        {
                            MessageBox.Show($"解密后找不到文件：{Path.GetFileName(currentPath)}");
                            continue;
                        }
                    }

                    string nameWithoutExt = Path.GetFileNameWithoutExtension(originalPath);
                    string savePath = Path.Combine(saveFolder, nameWithoutExt + "." + targetFormat);

                    string arguments;

                    if (isVideoFile)
                    {
                        switch (targetFormat)
                        {
                            case "mp3":
                                arguments = $"-i \"{currentPath}\" -vn -acodec libmp3lame -q:a 2 -y \"{savePath}\"";
                                break;
                            case "wav":
                                arguments = $"-i \"{currentPath}\" -vn -acodec pcm_s16le -ar 44100 -ac 2 -y \"{savePath}\"";
                                break;
                            case "flac":
                                arguments = $"-i \"{currentPath}\" -vn -acodec flac -y \"{savePath}\"";
                                break;
                            case "aac":
                            case "m4a":
                                arguments = $"-i \"{currentPath}\" -vn -acodec aac -b:a 192k -y \"{savePath}\"";
                                break;
                            case "ogg":
                                arguments = $"-i \"{currentPath}\" -vn -acodec libvorbis -q:a 5 -y \"{savePath}\"";
                                break;
                            case "opus":
                                arguments = $"-i \"{currentPath}\" -vn -acodec libopus -b:a 128k -y \"{savePath}\"";
                                break;
                            default:
                                arguments = $"-i \"{currentPath}\" -vn -y \"{savePath}\"";
                                break;
                        }
                    }
                    else
                    {
                        switch (targetFormat)
                        {
                            case "mp3":
                                arguments = $"-i \"{currentPath}\" -acodec libmp3lame -q:a 2 -y \"{savePath}\"";
                                break;
                            case "wav":
                                arguments = $"-i \"{currentPath}\" -acodec pcm_s16le -ar 44100 -ac 2 -y \"{savePath}\"";
                                break;
                            case "flac":
                                arguments = $"-i \"{currentPath}\" -acodec flac -y \"{savePath}\"";
                                break;
                            case "aac":
                            case "m4a":
                                arguments = $"-i \"{currentPath}\" -acodec aac -b:a 192k -y \"{savePath}\"";
                                break;
                            case "ogg":
                                arguments = $"-i \"{currentPath}\" -acodec libvorbis -q:a 5 -y \"{savePath}\"";
                                break;
                            case "opus":
                                arguments = $"-i \"{currentPath}\" -acodec libopus -b:a 128k -y \"{savePath}\"";
                                break;
                            default:
                                arguments = $"-i \"{currentPath}\" -y \"{savePath}\"";
                                break;
                        }
                    }

                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = ffmpeg,
                        Arguments = arguments,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        WorkingDirectory = saveFolder
                    };

                    string errorMsg = "";

                    using (System.Diagnostics.Process p = new System.Diagnostics.Process())
                    {
                        p.StartInfo = psi;
                        p.ErrorDataReceived += (o, args) => errorMsg += args.Data + Environment.NewLine;
                        p.OutputDataReceived += (o, args) => { };

                        p.Start();
                        p.BeginErrorReadLine();
                        p.BeginOutputReadLine();
                        p.WaitForExit();

                        if (p.ExitCode != 0)
                        {
                            MessageBox.Show($"ffmpeg 执行失败！\n退出码：{p.ExitCode}\n错误信息：{errorMsg}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    if (currentPath != originalPath && !isVideoFile)
                    {
                        try { File.Delete(currentPath); } catch { }
                    }

                    successCount++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"处理 {Path.GetFileName(audioFilePaths[i])} 失败：{ex.Message}");
                }
            }

            MessageBox.Show($"转换完成！成功 {successCount} 个文件。");
            System.Diagnostics.Process.Start(saveFolder);

            this.Cursor = Cursors.Default;
        }

        private void ra_2_Click(object sender, EventArgs e)
        {
            imge.Visible = false;
            radio.Visible = false;
            index.Visible = true;
        }

        private void comboBox2_SelectedIndexChanged_1(object sender, EventArgs e) { }
        private void listBox1_SelectedIndexChanged_2(object sender, EventArgs e) { }

        private void button1_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Title = "请选择音频或视频文件";
            dialog.Filter = "支持的文件|*.mp3;*.wav;*.flac;*.aac;*.m4a;*.ogg;*.wma;*.opus;*.ncm;*.mp4;*.avi;*.mkv;*.mov;*.webm;*.flv;*.wmv;*.m4v;*.ts;*.mts;*.m2ts;*.3gp";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                listBox1.Items.Clear();
                audioFilePaths.Clear();

                foreach (string filePath in dialog.FileNames)
                {
                    string ext = Path.GetExtension(filePath).ToLower();
                    string fileName = Path.GetFileName(filePath);

                    if (ext == ".mp4" || ext == ".avi" || ext == ".mkv" || ext == ".mov" ||
                        ext == ".webm" || ext == ".flv" || ext == ".wmv" || ext == ".m4v" ||
                        ext == ".ts" || ext == ".mts" || ext == ".m2ts" || ext == ".3gp")
                    {
                        listBox1.Items.Add(fileName + " (视频)");
                    }
                    else
                    {
                        listBox1.Items.Add(fileName);
                    }
                    audioFilePaths.Add(filePath);
                }
            }
        }

        // ======================== 视频转换相关 ========================

        private void vid_bu_Click(object sender, EventArgs e)
        {
            imge.Visible = false;
            radio.Visible = false;
            video_P.Visible = false;
            index.Visible = true;
        }

        private void vid_bu1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Title = "请选择视频文件";
            dialog.Filter = "视频文件|*.mp4;*.avi;*.mkv;*.mov;*.webm;*.flv;*.wmv;*.m4v;*.ts;*.mts;*.m2ts;*.3gp";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                listBox3.Items.Clear();
                videoFilePaths.Clear();

                foreach (string filePath in dialog.FileNames)
                {
                    string fileName = Path.GetFileName(filePath);
                    listBox3.Items.Add(fileName);
                    videoFilePaths.Add(filePath);
                }
            }
        }

        private void vid_bu2_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            if (videoFilePaths.Count == 0)
            {
                MessageBox.Show("请先选择视频文件");
                this.Cursor = Cursors.Default;
                return;
            }

            if (comboBox3.SelectedItem == null)
            {
                MessageBox.Show("请先选择目标格式");
                this.Cursor = Cursors.Default;
                return;
            }
            string targetFormat = comboBox3.SelectedItem.ToString().ToLower();

            string ffmpeg = GetFfmpegPath();
            if (!File.Exists(ffmpeg))
            {
                MessageBox.Show("找不到 ffmpeg.exe，请放到 Tool 文件夹里");
                this.Cursor = Cursors.Default;
                return;
            }

            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
            {
                this.Cursor = Cursors.Default;
                return;
            }

            string saveFolder = folderBrowserDialog1.SelectedPath;
            int successCount = 0;

            for (int i = 0; i < videoFilePaths.Count; i++)
            {
                try
                {
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(videoFilePaths[i]);
                    string savePath = Path.Combine(saveFolder, nameWithoutExt + "." + targetFormat);

                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = ffmpeg,
                        Arguments = $"-i \"{videoFilePaths[i]}\" -y \"{savePath}\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };

                    using (System.Diagnostics.Process p = System.Diagnostics.Process.Start(psi))
                    {
                        p.WaitForExit();
                    }

                    successCount++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"处理 {Path.GetFileName(videoFilePaths[i])} 失败：{ex.Message}");
                }
            }

            MessageBox.Show($"转换完成！成功 {successCount} 个文件。");
            System.Diagnostics.Process.Start(saveFolder);

            this.Cursor = Cursors.Default;
        }

        private void panel3_Paint(object sender, PaintEventArgs e) { }

        // ======================== 删除文件按钮 ========================

        private void del_vid_Click(object sender, EventArgs e)
        {
            if (listBox3.SelectedIndex < 0)
            {
                MessageBox.Show("请先选中要移除的文件");
                return;
            }

            int index = listBox3.SelectedIndex;
            listBox3.Items.RemoveAt(index);
            videoFilePaths.RemoveAt(index);
        }

        private void del_ima_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex < 0)
            {
                MessageBox.Show("请先选中要移除的文件");
                return;
            }

            int index = listBox2.SelectedIndex;
            listBox2.Items.RemoveAt(index);
            filePaths.RemoveAt(index);
        }

        private void del_ra_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0)
            {
                MessageBox.Show("请先选中要移除的文件");
                return;
            }

            int index = listBox1.SelectedIndex;
            listBox1.Items.RemoveAt(index);
            audioFilePaths.RemoveAt(index);
        }

        private void panel2_Paint(object sender, PaintEventArgs e) { }

        private void textBox1_TextChanged(object sender, EventArgs e) { }

        private string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }

        // ======================== B站解析相关 ========================

        private async void res_button_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(res.Text) || res.Text == "粘贴B站视频链接...")
            {
                MessageBox.Show("请先输入B站视频链接或BV号", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            res_button.Enabled = false;

            try
            {
                var info = await _parser.ParseVideoUrlWithPages(res.Text.Trim());

                _currentBvid = info.bvid;
                _currentPages = info.pages;

                label4.Text = info.title;
                label4.Visible = true;
                label5.Visible = true;

                if (!string.IsNullOrEmpty(info.coverUrl))
                {
                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync(info.coverUrl);
                        var stream = await response.Content.ReadAsStreamAsync();
                        pictureBox1.Image = System.Drawing.Image.FromStream(stream);
                        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                        pictureBox1.Visible = true;
                    }
                }

                checkedListBox1.Items.Clear();
                int idx = 1;
                foreach (var page in info.pages)
                {
                    string displayName = $"{idx}. {page.name}";
                    checkedListBox1.Items.Add(displayName, false);
                    idx++;
                }
                if (checkedListBox1.Items.Count > 0)
                    checkedListBox1.SetItemChecked(0, true);
                checkedListBox1.Visible = true;

                comboBox5.Items.Clear();
                for (int i = 0; i < info.acceptQuality.Count && i < info.acceptDescription.Count; i++)
                {
                    comboBox5.Items.Add($"{info.acceptDescription[i]} ({info.acceptQuality[i]})");
                }
                if (comboBox5.Items.Count > 0)
                    comboBox5.SelectedIndex = 0;
                comboBox5.Visible = true;

                res_dwn.Visible = true;

                MessageBox.Show("解析成功！", "成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"解析失败：{ex.Message}", "错误");
            }
            finally
            {
                this.Cursor = Cursors.Default;
                res_button.Enabled = true;
            }
        }

        private async Task<string> DownloadAndMerge(string saveFolder, (string videoUrl, string audioUrl, string title) info)
        {
            string baseName = $"{SanitizeFileName(info.title)}_{DateTime.Now:yyyyMMddHHmmss}";
            string videoPath = Path.Combine(saveFolder, $"{baseName}_video.mp4");
            string audioPath = Path.Combine(saveFolder, $"{baseName}_audio.mp3");
            string outputPath = Path.Combine(saveFolder, $"{baseName}.mp4");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Referer", "https://www.bilibili.com");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                var videoResponse = await client.GetAsync(info.videoUrl, HttpCompletionOption.ResponseHeadersRead);
                videoResponse.EnsureSuccessStatusCode();
                using (var fs = new FileStream(videoPath, FileMode.Create))
                    await videoResponse.Content.CopyToAsync(fs);

                if (!string.IsNullOrEmpty(info.audioUrl))
                {
                    var audioResponse = await client.GetAsync(info.audioUrl, HttpCompletionOption.ResponseHeadersRead);
                    audioResponse.EnsureSuccessStatusCode();
                    using (var fs = new FileStream(audioPath, FileMode.Create))
                        await audioResponse.Content.CopyToAsync(fs);
                }

                string ffmpeg = GetFfmpegPath();
                string arguments;
                if (File.Exists(audioPath))
                {
                    arguments = $"-i \"{videoPath}\" -i \"{audioPath}\" -c:v copy -c:a aac -map 0:v:0 -map 1:a:0 \"{outputPath}\" -y";
                }
                else
                {
                    File.Move(videoPath, outputPath);
                    return outputPath;
                }

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = ffmpeg,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = saveFolder
                };

                using (var p = System.Diagnostics.Process.Start(psi))
                {
                    await Task.Run(() => p.WaitForExit());
                }

                if (File.Exists(videoPath)) File.Delete(videoPath);
                if (File.Exists(audioPath)) File.Delete(audioPath);

                return outputPath;
            }
        }

        private async Task<string> DownloadSeparate(string saveFolder, (string videoUrl, string audioUrl, string title) info)
        {
            string baseName = $"{SanitizeFileName(info.title)}_{DateTime.Now:yyyyMMddHHmmss}";
            string videoPath = Path.Combine(saveFolder, $"{baseName}_video.mp4");
            string audioPath = Path.Combine(saveFolder, $"{baseName}_audio.mp3");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Referer", "https://www.bilibili.com");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                var videoResponse = await client.GetAsync(info.videoUrl, HttpCompletionOption.ResponseHeadersRead);
                videoResponse.EnsureSuccessStatusCode();
                using (var fs = new FileStream(videoPath, FileMode.Create))
                    await videoResponse.Content.CopyToAsync(fs);

                if (!string.IsNullOrEmpty(info.audioUrl))
                {
                    string tempAudio = Path.Combine(saveFolder, $"{baseName}_temp.m4s");
                    var audioResponse = await client.GetAsync(info.audioUrl, HttpCompletionOption.ResponseHeadersRead);
                    audioResponse.EnsureSuccessStatusCode();
                    using (var fs = new FileStream(tempAudio, FileMode.Create))
                        await audioResponse.Content.CopyToAsync(fs);

                    string ffmpeg = GetFfmpegPath();
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = ffmpeg,
                        Arguments = $"-i \"{tempAudio}\" -acodec libmp3lame -q:a 2 \"{audioPath}\" -y",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WorkingDirectory = saveFolder
                    };

                    using (var p = System.Diagnostics.Process.Start(psi))
                    {
                        await Task.Run(() => p.WaitForExit());
                    }

                    if (File.Exists(tempAudio)) File.Delete(tempAudio);
                }

                return videoPath;
            }
        }

        private void res_del_button_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentBvid) || checkedListBox1.Items.Count > 0)
            {
                DialogResult result = MessageBox.Show("是否清空当前解析数据？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    ClearResolveData();
                }
            }

            index.Visible = true;
            imge.Visible = true;
            redio.Visible = true;
            video.Visible = true;
            resolve.Visible = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) { }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e) { }

        private async void res_dwn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentBvid) || _currentPages.Count == 0)
            {
                MessageBox.Show("请先解析视频", "提示");
                return;
            }

            List<int> selectedIndices = new List<int>();
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i))
                    selectedIndices.Add(i);
            }

            if (selectedIndices.Count == 0)
            {
                MessageBox.Show("请至少勾选一个分P", "提示");
                return;
            }

            string selectedText = comboBox5.SelectedItem?.ToString() ?? "1080P+ (80)";
            int qn = 80;
            if (selectedText.Contains("(") && selectedText.Contains(")"))
            {
                int start = selectedText.IndexOf("(") + 1;
                int end = selectedText.IndexOf(")");
                if (start > 0 && end > start)
                {
                    string numStr = selectedText.Substring(start, end - start);
                    int.TryParse(numStr, out qn);
                }
            }

            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "选择保存文件夹";
                if (fbd.ShowDialog() != DialogResult.OK)
                    return;

                string saveFolder = fbd.SelectedPath;

                this.Cursor = Cursors.WaitCursor;
                res_dwn.Enabled = false;

                try
                {
                    int successCount = 0;
                    int failCount = 0;
                    List<string> downloadedFiles = new List<string>();

                    foreach (int idx in selectedIndices)
                    {
                        try
                        {
                            var page = _currentPages[idx];
                            var videoInfo = await _parser.ParseVideoUrlWithQuality(_currentBvid, page.cid, qn);

                            string title = $"{idx + 1}_{SanitizeFileName(page.name)}";
                            var infoWithTitle = (videoInfo.videoUrl, videoInfo.audioUrl, title);

                            string filePath;
                            if (checkBox1.Checked)
                                filePath = await DownloadSeparate(saveFolder, infoWithTitle);
                            else
                                filePath = await DownloadAndMerge(saveFolder, infoWithTitle);

                            downloadedFiles.Add(filePath);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            failCount++;
                            MessageBox.Show($"第 {idx + 1} 个分P下载失败：{ex.Message}", "警告");
                        }
                    }

                    string summary = $"下载完成！成功 {successCount} 个，失败 {failCount} 个";
                    if (successCount > 0)
                    {
                        MessageBox.Show($"{summary}\n共 {successCount} 个文件", "完成");
                        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{downloadedFiles[0]}\"");
                    }
                    else
                    {
                        MessageBox.Show(summary, "完成");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"下载失败：{ex.Message}", "错误");
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    res_dwn.Enabled = true;
                }
            }
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e) { }

        private void label5_Click(object sender, EventArgs e) { }

        private void music_dw_Paint(object sender, PaintEventArgs e) { }

        private void textBox1_TextChanged_1(object sender, EventArgs e) { }

        // ======================== 音乐下载相关 ========================

        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            // label7 固定显示"音乐下载"，不做任何修改
        }

        private void label7_Click(object sender, EventArgs e) { }

        // 下载
        private async void m_dw_b_Click(object sender, EventArgs e)
        {
            if (!(listBox4.SelectedItem is SongInfo song))
            {
                MessageBox.Show("请先选择一首歌曲", "提示");
                return;
            }

            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "选择保存文件夹";
                if (fbd.ShowDialog() != DialogResult.OK)
                    return;

                string saveFolder = fbd.SelectedPath;

                this.Cursor = Cursors.WaitCursor;
                m_dw_b.Enabled = false;

                try
                {
                    string downloadUrl = await _musicParser.GetDownloadUrl(song);

                    if (string.IsNullOrEmpty(downloadUrl))
                    {
                        MessageBox.Show($"无法获取下载地址\n\n歌曲: {song.Name}\n歌手: {song.Artist}", "提示");
                        return;
                    }

                    string filePath = await _musicParser.DownloadSong(saveFolder, song, downloadUrl);

                    MessageBox.Show($"下载完成！\n{Path.GetFileName(filePath)}", "成功");
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"下载失败：{ex.Message}", "错误");
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    m_dw_b.Enabled = true;
                }
            }
        }

        // 搜索
        private async void sousuo_Click(object sender, EventArgs e)
        {
            string keyword = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                MessageBox.Show("请输入歌曲名称", "提示");
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            sousuo.Enabled = false;

            try
            {
                var songs = await _musicParser.Search(keyword);
                _currentSongs = songs;

                listBox4.Items.Clear();
                foreach (var song in songs)
                {
                    listBox4.Items.Add(song);
                }

                if (songs.Count == 0)
                {
                    MessageBox.Show("未找到相关歌曲", "提示");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"搜索失败：{ex.Message}", "错误");
            }
            finally
            {
                this.Cursor = Cursors.Default;
                sousuo.Enabled = true;
            }
        }
    }
}