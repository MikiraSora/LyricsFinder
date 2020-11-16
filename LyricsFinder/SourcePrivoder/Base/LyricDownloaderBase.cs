using System.Threading;
using System.Threading.Tasks;

namespace LyricsFinder.SourcePrivoder
{
    public abstract class LyricDownloaderBase<T> where T : SearchSongResultBase
    {
        public abstract ValueTask<string> DownloadLyricAsync(T song, bool request_trans_lyrics, CancellationToken cancel_token);
    }
}