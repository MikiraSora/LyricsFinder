using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsFinder.SourcePrivoder.Xiami
{
    public class XiamiSearchResultSong : SearchSongResultBase
    {
        public string songStringId { get; set; }
        public string songName { get; set; }
        public string artistName { get; set; }
        public int length { get; set; }

        public override string Title => songName;
        public override string Artist => artistName;
        public override int Duration => length;
        public override string ID => songStringId;

        public string LyricFile { get; set; }

        internal XiamiSearchResultSong ProcessJsonSelf(JToken x)
        {
            LyricFile = x.SelectToken("lyricInfo.lyricFile")?.ToString();
            return this;
        }
    }

    public class XiamiSearcher : SongSearchBase<XiamiSearchResultSong>
    {
        public override async Task<List<XiamiSearchResultSong>> SearchAsync(IEnumerable<string> param_arr, CancellationToken cancel_token = default)
        {
            string title = param_arr.FirstOrDefault(), artist = param_arr.LastOrDefault();

            var paramQ = $"{{\"key\":\"{title} {artist}\",\"pagingVO\":{{\"page\":1,\"pageSize\":30}}}}";

            var json = await GlobalXiamiHttpProcessor.ProcessRequestAsync("/api/search/searchSongs", paramQ, cancellationToken: cancel_token);

            if (json is null)
                return default;

            var result = json["result"]["data"]["songs"].Select(x => x.ToObject<XiamiSearchResultSong>().ProcessJsonSelf(x)).Where(x => !string.IsNullOrWhiteSpace(x.LyricFile)).ToList();
            return result;
        }
    }
}
