﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsFinder.SourcePrivoder.Kugou
{
    public class KugouSearchResultSong : SearchSongResultBase
    {
        public int duration { get; set; }
        public string singername { get; set; }
        public string songname { get; set; }
        //public int audio_id { get; set; }

        /// <summary>
        /// 获取歌词需要这个玩意,所以拿着个当ID吧
        /// </summary>
        public string hash { get; set; }

        public override string Title => songname;
        public override string Artist => singername;
        public override int Duration => duration*1000;
        public override string ID => hash;
    }

    public class KugouSearcher : SongSearchBase<KugouSearchResultSong>
    {
        private static readonly string API_URL = @"http://mobilecdn.kugou.com/api/v3/search/song?format=json&keyword={1} {0}&page=1&pagesize=20&showtype=1";

        public override async Task<List<KugouSearchResultSong>> SearchAsync(IEnumerable<string> param_arr,CancellationToken cancel_token)
        {
            string title = param_arr.FirstOrDefault(), artist = param_arr.LastOrDefault();
            Uri url = new Uri(string.Format(API_URL, artist, title));

            //这纸张酷狗有时候response不回来,但用浏览器就可以.先留校观察
            HttpWebRequest request = HttpWebRequest.CreateHttp(url);
            request.Method = "GET";
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36";
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

            var json = JObject.Parse(content);

            if (!string.IsNullOrWhiteSpace(json["error"].ToString()))
                return new List<KugouSearchResultSong>();

            return json["data"]["info"].ToObject<List<KugouSearchResultSong>>();
        }
    }
}