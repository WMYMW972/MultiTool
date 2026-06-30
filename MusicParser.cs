using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tool
{
    public class MusicParser
    {
        private readonly HttpClient _httpClient;
        private readonly MusicResolverManager _resolverManager;

        public MusicParser()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _resolverManager = new MusicResolverManager();
        }

        public async Task<List<SongInfo>> Search(string keyword)
        {
            return await _resolverManager.Search(keyword);
        }

        public async Task<string> GetDownloadUrl(SongInfo song, int quality = 320)
        {
            return await _resolverManager.GetSongUrl(song.Id, quality);
        }

        public async Task<string> DownloadSong(string saveFolder, SongInfo song, string downloadUrl)
        {
            if (string.IsNullOrEmpty(downloadUrl))
                throw new Exception("没有获取到下载地址");

            string fileName = $"{song.Name} - {song.Artist}";
            string safeName = SanitizeFileName(fileName);
            string filePath = Path.Combine(saveFolder, $"{safeName}.mp3");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                client.DefaultRequestHeaders.Add("Referer", "https://music.163.com");

                var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await response.Content.CopyToAsync(fs);
                }
            }

            return filePath;
        }

        public string GetResolverStatus()
        {
            return _resolverManager.GetStatus();
        }

        private string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "unknown";

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name.Trim();
        }
    }

    public class SongInfo
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Artist { get; set; } = "";
        public string Album { get; set; } = "";
        public string Duration { get; set; } = "";

        public override string ToString()
        {
            string name = Name.Length > 25 ? Name.Substring(0, 25) : Name.PadRight(25);
            string artist = Artist.Length > 15 ? Artist.Substring(0, 15) : Artist.PadRight(15);
            return $"{name} {artist}";
        }
    }
}