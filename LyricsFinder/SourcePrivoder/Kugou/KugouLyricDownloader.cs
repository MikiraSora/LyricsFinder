using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsFinder.SourcePrivoder.Kugou
{
    public class KugouLyricDownloader : LyricDownloaderBase<KugouSearchResultSong>
    {
        public static readonly string API_URL = @"http://www.kugou.com/yy/index.php?r=play/getdata&hash={0}";

        public override async ValueTask<string> DownloadLyricAsync(KugouSearchResultSong song, bool request_trans_lyrics, CancellationToken cancel_token)
        {
            //没支持翻译歌词的
            if (request_trans_lyrics)
                return string.Empty;

            Uri url = new Uri(string.Format(API_URL, song.ID));

            HttpWebRequest request = HttpWebRequest.CreateHttp(url);

            if (GlobalSetting.SearchAndDownloadTimeout > 0)
                request.Timeout = GlobalSetting.SearchAndDownloadTimeout;

            var response = await request.GetResponseAsync();
            if (cancel_token.IsCancellationRequested)
                return default;

            string content = string.Empty;

            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                content = await reader.CancelableReadToEndAsync(cancel_token);
                if (cancel_token.IsCancellationRequested)
                    return default;
            }

            var obj = JsonDocument.Parse(content);
            if ((obj.GetValue<int>("err_code"))!=0)
                return null;
            var raw_lyric = obj.GetValue<string>("data","lyrics");
            var lyrics = raw_lyric.Replace("\r\n", "\n");

            return lyrics;
        }
    }
}