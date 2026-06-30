using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;

namespace tool
{
    public class BilibiliParser
    {
        private readonly HttpClient _httpClient;
        private string _mixKey = "";

        private static readonly int[] MixKeyIdx = new int[]
        {
            46, 47, 18, 2, 53, 8, 23, 32, 15, 50, 10, 31, 58, 3, 45, 35,
            27, 43, 5, 49, 33, 9, 42, 19, 29, 28, 14, 39, 12, 38, 41, 13,
            37, 48, 7, 16, 24, 55, 40, 61, 26, 17, 0, 1, 60, 51, 30, 4,
            22, 25, 54, 21, 56, 59, 6, 63, 57, 62, 11, 36, 20, 34, 44, 52
        };

        public BilibiliParser()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            _httpClient.DefaultRequestHeaders.Add("Referer", "https://www.bilibili.com");
        }
        /// <summary>
        /// 获取播放地址及可用分辨率列表
        /// </summary>
        private async Task<(string videoUrl, string audioUrl, List<int> acceptQuality, List<string> acceptDescription)> GetPlayUrlWithQuality(long aid, long cid, string bvid)
        {
            var parameters = new Dictionary<string, string>
    {
        { "aid", aid.ToString() },
        { "cid", cid.ToString() },
        { "platform", "web" },
        { "fnval", "4048" },
        { "qn", "80" },
        { "otype", "json" },
        { "fourk", "1" }
    };

            if (!string.IsNullOrEmpty(bvid))
                parameters["bvid"] = bvid;

            var sign = GenerateWbiSign(parameters);
            parameters["w_rid"] = sign.w_rid;
            parameters["wts"] = sign.wts.ToString();

            string playUrl = "https://api.bilibili.com/x/player/playurl";
            string queryString = string.Join("&", parameters.Select(p => $"{p.Key}={HttpUtility.UrlEncode(p.Value)}"));
            string fullUrl = $"{playUrl}?{queryString}";

            var response = await _httpClient.GetAsync(fullUrl);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            var obj = JObject.Parse(json);
            if ((int)obj["code"] != 0)
                throw new Exception($"B站接口返回错误: {obj["message"]?.ToString()}");

            var data = obj["data"];
            string videoUrl = "";
            string audioUrl = "";

            // 提取可用分辨率
            var acceptQuality = new List<int>();
            var acceptDescription = new List<string>();

            if (data["accept_quality"] != null)
            {
                acceptQuality = data["accept_quality"].Select(x => (int)x).ToList();
            }
            if (data["accept_description"] != null)
            {
                acceptDescription = data["accept_description"].Select(x => x.ToString()).ToList();
            }

            // 如果上面没拿到，用默认列表
            if (acceptQuality.Count == 0)
            {
                acceptQuality = new List<int> { 120, 80, 64, 32, 16 };
                acceptDescription = new List<string> { "4K", "1080P+", "1080P", "720P", "480P" };
            }

            var durl = data["durl"];
            if (durl != null && durl.HasValues)
            {
                videoUrl = durl[0]["url"]?.ToString();
            }
            else
            {
                var dash = data["dash"];
                if (dash != null)
                {
                    var video = dash["video"];
                    if (video != null && video.HasValues)
                        videoUrl = video[0]["base_url"]?.ToString();

                    var audio = dash["audio"];
                    if (audio != null && audio.HasValues)
                        audioUrl = audio[0]["base_url"]?.ToString();
                }
            }

            if (string.IsNullOrEmpty(videoUrl))
                throw new Exception("没有获取到视频地址");

            return (videoUrl, audioUrl, acceptQuality, acceptDescription);
        }
        // ==================== 公开方法 ====================

        /// <summary>
        /// 解析视频，返回完整信息（含分P列表和封面）
        /// </summary>
        public async Task<(string videoUrl, string audioUrl, string title, string coverUrl, string bvid, List<(long cid, string name)> pages, List<int> acceptQuality, List<string> acceptDescription)> ParseVideoUrlWithPages(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new Exception("请输入B站视频链接或视频ID");

            string videoId = ExtractVideoId(input);
            if (string.IsNullOrEmpty(videoId))
                throw new Exception("无法识别视频ID");

            bool isBv = videoId.StartsWith("BV", StringComparison.OrdinalIgnoreCase);

            string bvid = "";
            long aid = 0;
            string title = "";
            string coverUrl = "";
            var pages = new List<(long cid, string name)>();
            List<int> acceptQuality = new List<int>();
            List<string> acceptDescription = new List<string>();

            if (isBv)
            {
                bvid = videoId;
                (aid, title, coverUrl, pages) = await GetFullVideoInfoByBvid(bvid);
            }
            else
            {
                string numStr = videoId.Substring(2);
                if (!long.TryParse(numStr, out aid))
                    throw new Exception($"无效的AV号：{videoId}");
                (bvid, title, coverUrl, pages) = await GetFullVideoInfoByAid(aid);
            }

            if (pages.Count == 0)
                throw new Exception("没有获取到视频分P");

            var firstPage = pages[0];
            await RefreshWbiKeys();
            var (videoUrl, audioUrl, qualityList, descList) = await GetPlayUrlWithQuality(aid, firstPage.cid, bvid);

            return (videoUrl, audioUrl, title, coverUrl, bvid, pages, qualityList, descList);
        }

        /// <summary>
        /// 按指定分P和分辨率获取视频地址
        /// </summary>
        public async Task<(string videoUrl, string audioUrl, string title)> ParseVideoUrlWithQuality(string bvid, long cid, int qn = 80)
        {
            if (string.IsNullOrEmpty(bvid))
                throw new Exception("BV号不能为空");

            await RefreshWbiKeys();

            // 获取 aid 和 title
            string viewUrl = $"https://api.bilibili.com/x/web-interface/view?bvid={bvid}";
            string viewJson = await _httpClient.GetStringAsync(viewUrl);
            var viewObj = JObject.Parse(viewJson);
            if ((int)viewObj["code"] != 0)
                throw new Exception($"获取视频信息失败: {viewObj["message"]?.ToString()}");

            var data = viewObj["data"];
            long aid = (long)data["aid"];
            string title = data["title"]?.ToString() ?? "bilibili";

            var (videoUrl, audioUrl) = await GetPlayUrl(aid, cid, bvid, qn);
            return (videoUrl, audioUrl, title);
        }

        /// <summary>
        /// 解析视频，返回视频地址（旧版兼容）
        /// </summary>
        public async Task<string> ParseVideoUrl(string input)
        {
            var info = await ParseVideoUrlWithPages(input);
            return info.videoUrl;
        }

        // ==================== 私有方法 ====================

        private string ExtractVideoId(string input)
        {
            input = input.Trim();

            if (Regex.IsMatch(input, @"^\d+$"))
                return $"av{input}";

            Match bvMatch = Regex.Match(input, @"BV[a-zA-Z0-9]{10,12}");
            if (bvMatch.Success)
                return bvMatch.Value;

            Match avMatch = Regex.Match(input, @"[Aa][Vv](\d+)");
            if (avMatch.Success)
                return $"av{avMatch.Groups[1].Value}";

            Match aidMatch = Regex.Match(input, @"[?&]aid=(\d+)");
            if (aidMatch.Success)
                return $"av{aidMatch.Groups[1].Value}";

            Match bvidParamMatch = Regex.Match(input, @"[?&]bvid=(BV[a-zA-Z0-9]{10,12})");
            if (bvidParamMatch.Success)
                return bvidParamMatch.Groups[1].Value;

            if (input.Contains("b23.tv") || input.Contains("bili2233.cn"))
                throw new Exception("检测到B站短链接，请使用完整链接或直接输入BV号");

            throw new Exception("无法识别链接格式");
        }

        /// <summary>
        /// 获取播放地址
        /// </summary>
        private async Task<(string videoUrl, string audioUrl)> GetPlayUrl(long aid, long cid, string bvid, int qn)
        {
            var parameters = new Dictionary<string, string>
            {
                { "aid", aid.ToString() },
                { "cid", cid.ToString() },
                { "platform", "web" },
                { "fnval", "4048" },
                { "qn", qn.ToString() },
                { "otype", "json" },
                { "fourk", "1" }
            };

            if (!string.IsNullOrEmpty(bvid))
                parameters["bvid"] = bvid;

            var sign = GenerateWbiSign(parameters);
            parameters["w_rid"] = sign.w_rid;
            parameters["wts"] = sign.wts.ToString();

            string playUrl = "https://api.bilibili.com/x/player/playurl";
            string queryString = string.Join("&", parameters.Select(p => $"{p.Key}={HttpUtility.UrlEncode(p.Value)}"));
            string fullUrl = $"{playUrl}?{queryString}";

            var response = await _httpClient.GetAsync(fullUrl);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            var obj = JObject.Parse(json);
            if ((int)obj["code"] != 0)
                throw new Exception($"B站接口返回错误: {obj["message"]?.ToString()}");

            var data = obj["data"];
            string videoUrl = "";
            string audioUrl = "";

            var durl = data["durl"];
            if (durl != null && durl.HasValues)
            {
                videoUrl = durl[0]["url"]?.ToString();
            }
            else
            {
                var dash = data["dash"];
                if (dash != null)
                {
                    var video = dash["video"];
                    if (video != null && video.HasValues)
                        videoUrl = video[0]["base_url"]?.ToString();

                    var audio = dash["audio"];
                    if (audio != null && audio.HasValues)
                        audioUrl = audio[0]["base_url"]?.ToString();
                }
            }

            if (string.IsNullOrEmpty(videoUrl))
                throw new Exception("没有获取到视频地址");

            return (videoUrl, audioUrl);
        }

        /// <summary>
        /// 通过 BV 号获取完整信息（含分P列表）
        /// </summary>
        private async Task<(long aid, string title, string coverUrl, List<(long cid, string name)> pages)> GetFullVideoInfoByBvid(string bvid)
        {
            string url = $"https://api.bilibili.com/x/web-interface/view?bvid={bvid}";
            string json = await _httpClient.GetStringAsync(url);

            var obj = JObject.Parse(json);
            if ((int)obj["code"] != 0)
                throw new Exception($"获取视频信息失败: {obj["message"]?.ToString()}");

            var data = obj["data"];
            long aid = (long)data["aid"];
            string title = data["title"]?.ToString() ?? "bilibili";
            string coverUrl = data["pic"]?.ToString() ?? "";

            var pages = new List<(long cid, string name)>();
            foreach (var page in data["pages"])
            {
                long cid = (long)page["cid"];
                string name = page["part"]?.ToString() ?? $"P{page["page"]}";
                pages.Add((cid, name));
            }

            return (aid, title, coverUrl, pages);
        }

        /// <summary>
        /// 通过 AV 号获取完整信息（含分P列表）
        /// </summary>
        private async Task<(string bvid, string title, string coverUrl, List<(long cid, string name)> pages)> GetFullVideoInfoByAid(long aid)
        {
            string url = $"https://api.bilibili.com/x/web-interface/view?aid={aid}";
            string json = await _httpClient.GetStringAsync(url);

            var obj = JObject.Parse(json);
            if ((int)obj["code"] != 0)
                throw new Exception($"获取视频信息失败: {obj["message"]?.ToString()}");

            var data = obj["data"];
            string bvid = data["bvid"]?.ToString() ?? "";
            string title = data["title"]?.ToString() ?? "bilibili";
            string coverUrl = data["pic"]?.ToString() ?? "";

            var pages = new List<(long cid, string name)>();
            foreach (var page in data["pages"])
            {
                long cid = (long)page["cid"];
                string name = page["part"]?.ToString() ?? $"P{page["page"]}";
                pages.Add((cid, name));
            }

            return (bvid, title, coverUrl, pages);
        }

        /// <summary>
        /// 通过 BV 号获取视频信息（旧版兼容）
        /// </summary>
        private async Task<(long aid, long cid)> GetVideoInfoByBvid(string bvid)
        {
            string url = $"https://api.bilibili.com/x/web-interface/view?bvid={bvid}";
            string json = await _httpClient.GetStringAsync(url);

            var obj = JObject.Parse(json);
            if ((int)obj["code"] != 0)
                throw new Exception($"获取视频信息失败: {obj["message"]?.ToString()}");

            var data = obj["data"];
            long aid = (long)data["aid"];
            long cid = (long)data["pages"][0]["cid"];
            return (aid, cid);
        }

        /// <summary>
        /// 通过 AV 号获取视频信息（旧版兼容）
        /// </summary>
        private async Task<(long aid, long cid)> GetVideoInfoByAid(long aid)
        {
            string url = $"https://api.bilibili.com/x/web-interface/view?aid={aid}";
            string json = await _httpClient.GetStringAsync(url);

            var obj = JObject.Parse(json);
            if ((int)obj["code"] != 0)
                throw new Exception($"获取视频信息失败: {obj["message"]?.ToString()}");

            var data = obj["data"];
            long cid = (long)data["pages"][0]["cid"];
            return (aid, cid);
        }

        // ==================== WBI 签名 ====================

        private async Task RefreshWbiKeys()
        {
            string url = "https://api.bilibili.com/x/web-interface/nav";
            string json = await _httpClient.GetStringAsync(url);

            var obj = JObject.Parse(json);
            var data = obj["data"];
            var wbiImg = data["wbi_img"];

            string imgUrl = wbiImg["img_url"]?.ToString();
            string subUrl = wbiImg["sub_url"]?.ToString();

            if (string.IsNullOrEmpty(imgUrl) || string.IsNullOrEmpty(subUrl))
                throw new Exception("无法获取 WBI 密钥");

            string imgKey = Path.GetFileNameWithoutExtension(imgUrl);
            string subKey = Path.GetFileNameWithoutExtension(subUrl);

            string rawKey = imgKey + subKey;
            var mixed = new StringBuilder();
            for (int i = 0; i < MixKeyIdx.Length && i < rawKey.Length; i++)
            {
                if (MixKeyIdx[i] < rawKey.Length)
                    mixed.Append(rawKey[MixKeyIdx[i]]);
            }
            _mixKey = mixed.ToString().Substring(0, Math.Min(32, mixed.Length));
        }

        private (string w_rid, int wts) GenerateWbiSign(Dictionary<string, string> parameters)
        {
            int wts = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var sorted = new SortedDictionary<string, string>();
            foreach (var p in parameters)
            {
                if (p.Key != "w_rid" && p.Key != "wts")
                    sorted[p.Key] = p.Value;
            }

            var parts = new List<string>();
            foreach (var p in sorted)
                parts.Add($"{p.Key}={HttpUtility.UrlEncode(p.Value)}");
            parts.Add($"wts={wts}");

            string signStr = string.Join("&", parts);
            string toHash = signStr + _mixKey;

            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(toHash));
                string w_rid = BitConverter.ToString(hash).Replace("-", "").ToLower();
                return (w_rid, wts);
            }
        }
    }
}