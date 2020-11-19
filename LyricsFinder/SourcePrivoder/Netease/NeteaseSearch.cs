using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static LyricsFinder.SourcePrivoder.Netease.NeteaseSearch;

namespace LyricsFinder.SourcePrivoder.Netease
{
#pragma warning disable IDE1006 // 命名样式
    public class NeteaseSearch : SongSearchBase<Song>
    {
        #region Search Result

        public class Artist
        {
            public List<string> alias { get; set; }
            public string picUrl { get; set; }
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Album
        {
            public int status { get; set; }
            public int copyrightId { get; set; }
            public string name { get; set; }
            public Artist artist { get; set; }
            public ulong publishTime { get; set; }
            public int id { get; set; }
            public int size { get; set; }
        }

        public class Song : SearchSongResultBase
        {
            public Album album { get; set; }
            public int status { get; set; }
            public int copyrightId { get; set; }
            public string name { get; set; }
            public int mvid { get; set; }
            public List<string> Alias { get; set; }
            public List<Artist> artists { get; set; }
            public int duration { get; set; }
            public int id { get; set; }

            public override int Duration => duration;

            public override string ID => id.ToString();

            public override string Title => name;

            public override string Artist => artists?.First().name;
        }

#pragma warning restore IDE1006 // 命名样式

        #endregion Search Result

        private static readonly string API_URL = "http://music.163.com/api/search/get/";
        private static readonly int SEARCH_LIMIT = 5;

        public override async Task<List<Song>> SearchAsync(string title, string artist, CancellationToken cancel_token)
        {
            Uri url = new Uri($"{API_URL}?s={artist} {title}&limit={SEARCH_LIMIT}&type=1&offset=0");

            HttpWebRequest request = HttpWebRequest.CreateHttp(url);
            request.Method = "POST";
            if (GlobalSetting.SearchAndDownloadTimeout > 0)
                request.Timeout = GlobalSetting.SearchAndDownloadTimeout;
            request.Referer = "http://music.163.com";
            request.Headers["appver"] = $"2.0.2";

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

            JObject json = JObject.Parse(content);

            var count = json["result"]["songCount"]?.ToObject<int>();

            if (count == 0)
            {
                return new List<Song>();
            }

            var result = json["result"]["songs"].ToObject<List<Song>>();

            return result;
        }
    }
}