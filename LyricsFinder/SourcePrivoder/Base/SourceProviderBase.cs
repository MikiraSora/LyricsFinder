using LyricsFinder.SourcePrivoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsFinder
{
    public abstract class SourceProviderBase
    {
        private string _providerName;
        public string ProviderName => _providerName ?? (_providerName = this.GetType().GetCustomAttribute<SourceProviderNameAttribute>()?.Name ?? "unknown");

        public abstract ValueTask<Lyrics> ProvideLyricAsync(string artist, string title, int time, bool request_trans_lyrics, CancellationToken cancel_token);
    }

    public abstract class SourceProviderBase<SEARCHRESULT, SEARCHER, DOWNLOADER, PARSER> : SourceProviderBase where DOWNLOADER : LyricDownloaderBase<SEARCHRESULT>, new() where PARSER : LyricParserBase, new() where SEARCHER : SongSearchBase<SEARCHRESULT>, new() where SEARCHRESULT : SearchSongResultBase, new()
    {
        public DOWNLOADER Downloader { get; } = new DOWNLOADER();
        public SEARCHER Seadrcher { get; } = new SEARCHER();
        public PARSER Parser { get; } = new PARSER();

        public override async ValueTask<Lyrics> ProvideLyricAsync(string artist, string title, int time, bool request_trans_lyrics, CancellationToken cancel_token = default)
        {
            try
            {
                var search_result = await Seadrcher.SearchAsync(title, artist, cancel_token);
                if (cancel_token.IsCancellationRequested)
                    return default;

                var (lyrics, picked_result) = await PickLyricAsync(artist, title, time, search_result, request_trans_lyrics, cancel_token);
                if (cancel_token.IsCancellationRequested)
                    return default;

                if (lyrics != null)
                    lyrics.ProviderName = ProviderName;

                return lyrics;
            }
            catch (Exception e)
            {
                Utils.Output($"{GetType().Name}获取歌词失败:{e.Message}");
                return null;
            }
        }

        public virtual async ValueTask<(Lyrics , SEARCHRESULT)> PickLyricAsync(string artist, string title, int time, List<SEARCHRESULT> search_result, bool request_trans_lyrics, CancellationToken cancel_token)
        {
            DumpSearchList("-", time, search_result);

            FuckSearchFilte(artist, title, time, ref search_result);

            DumpSearchList("+", time, search_result);

            if (search_result.Count==0)
                return default;

            Lyrics lyric_cont = null;
            SEARCHRESULT cur_result = null;

            foreach (var result in search_result)
            {
                var content = await Downloader.DownloadLyricAsync(result, request_trans_lyrics, cancel_token);
                if (cancel_token.IsCancellationRequested)
                    return default;

                cur_result =result;

                if (string.IsNullOrWhiteSpace(content))
                    continue;

                lyric_cont = Parser.Parse(content);

                //过滤没有实质歌词内容的玩意,比如没有时间轴的歌词文本
                if (lyric_cont?.LyricsSentences?.Count==0)
                    continue;

                Utils.Debug($"* Picked [{ProviderName}] music_id:{result.ID} artist:{result.Artist} title:{result.Title}");

                break;
            }

            if (lyric_cont==null)
                return default;

            WrapInfo(lyric_cont);

            return (lyric_cont,cur_result);

            #region Wrap Methods

            //封装信息
            void WrapInfo(Lyrics l)
            {
                if (l==null)
                    return;

                Info raw_info = new Info()
                {
                    Artist=artist,
                    Title=title
                }, query_info = new Info()
                {
                    Artist=cur_result.Artist,
                    Title=cur_result.Title,
                    ID=cur_result.ID
                };

                l.RawInfo=raw_info;
                l.QueryInfo=query_info;
                l.IsTranslatedLyrics=request_trans_lyrics;
            }

            #endregion Wrap Methods
        }

        private void DumpSearchList(string prefix, int time, List<SEARCHRESULT> search_list)
        {
            if (GlobalSetting.DebugMode)
                foreach (var r in search_list)
                    Utils.Debug($"{prefix} [{ProviderName}] music_id:{r.ID} artist:{r.Artist} title:{r.Title} time{r.Duration}({Math.Abs(r.Duration-time):F2})");
        }

        public virtual void FuckSearchFilte(string artist, string title, int time, ref List<SEARCHRESULT> search_result)
        {
            //删除长度不对的
            search_result.RemoveAll((r) => Math.Abs(r.Duration-time) > GlobalSetting.DurationThresholdValue);

            string check_Str = $"{title.Trim()}".ToLower();

            //删除标题看起来不匹配的(超过1/3内容不对就出局),严格模式就要全匹配(防止 https://puu.sh/D0FCB/53ac51f034.png )
            float threhold_length = GlobalSetting.StrictMatch ? 0 : check_Str.Length*(1.0f/3);

            search_result.RemoveAll((r) =>
            {
                //XXXXX和XXXXX(Full version)这种情况可以跳过
                if (r.Title.Trim().ToLower().StartsWith(check_Str))
                    return false;//不用删除，通过

                var distance = _GetEditDistance(r);
                return distance>threhold_length;
            }
            );

            //search_result.Sort((a, b) => Math.Abs(a.Duration - time) - Math.Abs(b.Duration - time));
            search_result.Sort((a, b) => _GetEditDistance(a)-_GetEditDistance(b));

            int _GetEditDistance(SearchSongResultBase s) => Utils.EditDistance($"{s.Title}".ToLowerInvariant(), check_Str.ToLowerInvariant());
        }
    }
}