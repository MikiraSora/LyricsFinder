﻿
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsFinder.SourcePrivoder.QQMusic
{
    public class QQMusicLyricDownloader : LyricDownloaderBase<Song>
    {
        //public static readonly string API_URL = @"http://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric.fcg?nobase64=1&musicid={0}&callback=json&g_tk=5381&loginUin=0&hostUin=0&format=json&inCharset=utf-8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&songtype={1}";

        public static readonly string NEW_API_URL = @"https://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg?g_tk=753738303&songmid={0}&callback=json&songtype={1}";

        public override async ValueTask<string> DownloadLyricAsync(Song song, bool request_trans_lyrics, CancellationToken cancel_token)
        {
            var song_type = (song as Song)?.type ?? 0;

            Uri url = new Uri(string.Format(NEW_API_URL, song.ID, song_type));

            HttpWebRequest request = HttpWebRequest.CreateHttp(url);

            if (GlobalSetting.SearchAndDownloadTimeout > 0)
                request.Timeout = GlobalSetting.SearchAndDownloadTimeout;
            request.Referer = "https://y.qq.com/portal/player.html";
            request.Headers.Add("Cookie", "skey=@LVJPZmJUX; p");

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

            if (content.StartsWith("json("))
            {
                content = content.Remove(0, 5);
            }

            if (content.EndsWith(")"))
            {
                content = content.Remove(content.Length - 1);
            }

            content = System.Web.HttpUtility.HtmlDecode(content);

            var json = await JsonDocument.ParseAsync(new MemoryStream(Encoding.UTF8.GetBytes(content)));

            int result = json.GetValue<int>("retcode");
            if (result < 0)
                return null;

            content = json.GetValue<string>(request_trans_lyrics ? "trans" : "lyric");
            if (string.IsNullOrWhiteSpace(content))
                return null;

            content = Utils.Base64Decode(content);

            return content;
        }
    }
}