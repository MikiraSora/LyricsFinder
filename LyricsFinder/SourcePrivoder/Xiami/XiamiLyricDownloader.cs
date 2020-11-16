using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsFinder.SourcePrivoder.Xiami
{
    public class XiamiLyricDownloader : LyricDownloaderBase<XiamiSearchResultSong>
    {
        private static Regex xlrcRegex = new Regex(@"<\d+?>");

        public override async ValueTask<string> DownloadLyricAsync(XiamiSearchResultSong song, bool request_trans_lyrics, CancellationToken cancel_token)
        {
            if (string.IsNullOrWhiteSpace(song.lyricInfo.lyricFile))
                return default;

            HttpWebRequest request = HttpWebRequest.CreateHttp(song.lyricInfo.lyricFile);

            if (GlobalSetting.SearchAndDownloadTimeout > 0)
                request.Timeout = GlobalSetting.SearchAndDownloadTimeout;

            var response = await request.GetResponseAsync();
            if (cancel_token.IsCancellationRequested)
                return default;

            string content = string.Empty;

            using var reader = new StreamReader(response.GetResponseStream());
            content = await reader.CancelableReadToEndAsync(cancel_token);
            if (cancel_token.IsCancellationRequested)
                return default;

            if (song.lyricInfo.lyricFile.EndsWith(".xtrc") ||
                song.lyricInfo.lyricType == XiamiLyricsInfo.LyricType.XTRC ||
                song.lyricInfo.lyricFile.EndsWith(".trc") ||
                song.lyricInfo.lyricType == XiamiLyricsInfo.LyricType.TRC)
            {
                //移除xlrc歌词文本中单词时间点
                content = xlrcRegex.Replace(content,"");
            }

            return content;
        }
    }
}