using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace LyricsFinder.SourcePrivoder.Kugou
{
    public class KugouLyricDownloader : LyricDownloaderBase
    {
        public static readonly string API_URL = @"http://www.kugou.com/yy/index.php?r=play/getdata&hash={0}";

        public override async Task<string> DownloadLyricAsync(SearchSongResultBase song, bool request_trans_lyrics)
        {
            //没支持翻译歌词的
            if (request_trans_lyrics)
                return string.Empty;

            Uri url = new Uri(string.Format(API_URL, song.ID));

            HttpWebRequest request = HttpWebRequest.CreateHttp(url);
            request.Timeout = GlobalSetting.SearchAndDownloadTimeout;

            var response = await request.GetResponseAsync();

            string content = string.Empty;

            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                content = await reader.ReadToEndAsync();
            }

            JObject obj = JObject.Parse(content);
            if ((int)obj["err_code"]!=0)
                return null;
            var raw_lyric = obj["data"]["lyrics"].ToString();
            var lyrics = raw_lyric.Replace("\r\n", "\n");

            return lyrics;
        }
    }
}