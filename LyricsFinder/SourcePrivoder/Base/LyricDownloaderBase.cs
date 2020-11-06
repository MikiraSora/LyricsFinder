using System.Threading;
using System.Threading.Tasks;

namespace LyricsFinder.SourcePrivoder
{
    public abstract class LyricDownloaderBase
    {
        public abstract Task<string> DownloadLyricAsync(SearchSongResultBase song, bool request_trans_lyrics, CancellationToken cancel_token);
    }
}