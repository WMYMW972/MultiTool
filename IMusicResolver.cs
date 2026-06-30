using System.Collections.Generic;
using System.Threading.Tasks;

namespace tool
{
    public interface IMusicResolver
    {
        string Name { get; }
        bool IsAvailable { get; }
        Task<string> GetSongUrl(string songId, int quality = 320);
        Task<List<SongInfo>> Search(string keyword);
    }
}