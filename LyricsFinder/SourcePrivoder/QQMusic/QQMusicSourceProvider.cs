using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsFinder.SourcePrivoder.QQMusic
{
    [SourceProviderName("qqmusic", "DarkProjector")]
    public class QQMusicSourceProvider : SourceProviderBase<Song, QQMusicSearch, QQMusicLyricDownloader, DefaultLyricsParser>
    {
        public override async ValueTask<(Lyrics, Song)> PickLyricAsync(string artist, string title, int time, List<Song> search_result, bool request_trans_lyrics, CancellationToken cancel_token)
        {
            var (result, temp_picked_result) = await base.PickLyricAsync(artist, title, time, search_result, request_trans_lyrics, cancel_token);
            if (cancel_token.IsCancellationRequested)
                return default;

            if (result!=null)
            {
                switch (result.LyricsSentences.Count)
                {
                    case 0:
                        Utils.Debug($"{temp_picked_result?.ID}:无任何歌词在里面,rej");
                        return default;

                    case 1:
                        var first_sentence = result.LyricsSentences.First();
                        if (first_sentence.StartTime<=0&&first_sentence.Content.Contains("纯音乐")&&first_sentence.Content.Contains("没有填词"))
                        {
                            Utils.Debug($"{temp_picked_result?.ID}:纯音乐? : "+first_sentence);
                            return default;
                        }
                        break;

                    default:
                        break;
                }
            }

            return (result, temp_picked_result);
        }
    }
}