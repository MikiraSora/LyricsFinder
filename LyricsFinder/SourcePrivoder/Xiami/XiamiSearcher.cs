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
using static LyricsFinder.SourcePrivoder.Xiami.XiamiLyricsInfo;

namespace LyricsFinder.SourcePrivoder.Xiami
{
    public class XiamiLyricsInfo
    {
        public string lyricFile { get; set; }

        public enum LyricType
        {
            NONE = -1,
            Text = 1,
            LRC = 2,
            TRC = 3,
            TranslatedLRC = 4,
            XTRC = 7,
        }

        public LyricType lyricType { get; set; }
    }

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

        public XiamiLyricsInfo lyricInfo { get; set; }

        private static LyricType[] avaliableLyricsTypeList { get; set; } = new[]
        {
            LyricType.TranslatedLRC,LyricType.TRC,LyricType.XTRC,LyricType.LRC
        };

        internal bool ContainLyrics()
        {
            return (!string.IsNullOrWhiteSpace(lyricInfo?.lyricFile))
                && avaliableLyricsTypeList.Contains(lyricInfo?.lyricType?? LyricType.NONE);
        }
    }

    public class XiamiSearcher : SongSearchBase<XiamiSearchResultSong>
    {
        public override async Task<List<XiamiSearchResultSong>> SearchAsync(string title, string artist, CancellationToken cancel_token = default)
        {
            var paramQ = $"{{\"key\":\"{title} {artist}\",\"pagingVO\":{{\"page\":1,\"pageSize\":30}}}}";

            var json = await GlobalXiamiHttpProcessor.ProcessRequestAsync("/api/search/searchSongs", paramQ, cancellationToken: cancel_token);

            if (json is null)
                return default;

            var result = json["result"]["data"]["songs"]
                .Select(x => x.ToObject<XiamiSearchResultSong>())
                .Where(x => x.ContainLyrics()).ToList();
            return result;
        }
    }
}
