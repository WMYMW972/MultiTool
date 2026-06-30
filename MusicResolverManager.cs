using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tool
{
    public class MusicResolverManager
    {
        private readonly List<IMusicResolver> _resolvers;
        private readonly Dictionary<string, int> _failCount = new Dictionary<string, int>();
        private const int MAX_FAIL_COUNT = 3;

        public MusicResolverManager()
        {
            _resolvers = new List<IMusicResolver>
            {
                new Resolvers.GdStudioResolver()
            };
        }

        public async Task<string> GetSongUrl(string songId, int quality = 320)
        {
            var sorted = _resolvers
                .Where(r => r.IsAvailable)
                .OrderBy(r => _failCount.ContainsKey(r.Name) ? _failCount[r.Name] : 0)
                .ToList();

            if (sorted.Count == 0)
                return "";

            foreach (var resolver in sorted)
            {
                try
                {
                    string url = await resolver.GetSongUrl(songId, quality);
                    if (!string.IsNullOrEmpty(url))
                    {
                        _failCount[resolver.Name] = 0;
                        return url;
                    }
                    else
                    {
                        if (!_failCount.ContainsKey(resolver.Name))
                            _failCount[resolver.Name] = 0;
                        _failCount[resolver.Name]++;
                    }
                }
                catch
                {
                    if (!_failCount.ContainsKey(resolver.Name))
                        _failCount[resolver.Name] = 0;
                    _failCount[resolver.Name]++;
                }
            }

            return "";
        }

        public async Task<List<SongInfo>> Search(string keyword)
        {
            var resolver = _resolvers.FirstOrDefault(r => r.IsAvailable);
            if (resolver == null) return new List<SongInfo>();
            return await resolver.Search(keyword);
        }

        public string GetStatus()
        {
            return string.Join("\n", _resolvers.Select(r =>
                $"{r.Name}: {(r.IsAvailable ? "可用" : "不可用")} " +
                $"({(_failCount.ContainsKey(r.Name) ? _failCount[r.Name] : 0)}次失败)"
            ));
        }
    }
}