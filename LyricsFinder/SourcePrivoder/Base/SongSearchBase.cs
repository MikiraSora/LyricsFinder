using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsFinder.SourcePrivoder
{
    public abstract class SongSearchBase<T> where T : SearchSongResultBase, new()
    {
        public abstract Task<List<T>> SearchAsync(IEnumerable<string> param_arr, CancellationToken cancel_token);
    }
}