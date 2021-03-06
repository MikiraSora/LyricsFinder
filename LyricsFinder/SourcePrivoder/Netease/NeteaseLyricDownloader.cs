﻿using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static LyricsFinder.SourcePrivoder.Netease.NeteaseSearch;
using System.Text.Json;

namespace LyricsFinder.SourcePrivoder.Netease
{
    public class NeteaseLyricDownloader : LyricDownloaderBase<Song>
    {
        //tv=-1 是翻译版本的歌词
        //lv=1 是源版本歌词
        private static readonly string LYRIC_API_URL = "https://music.163.com/api/song/lyric?id={0}&{1}";

        public override async ValueTask<string> DownloadLyricAsync(Song song, bool request_trans_lyrics, CancellationToken cancel_token)
        {
            HttpWebRequest request = HttpWebRequest.CreateHttp(string.Format(LYRIC_API_URL, song.ID, request_trans_lyrics ? "tv=-1" : "lv=1"));

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

            var json = JsonDocument.Parse(content);

            return json.GetValue<string>((request_trans_lyrics ? "tlyric" : "lrc"), "lyric") ?? string.Empty;
        }
    }
}