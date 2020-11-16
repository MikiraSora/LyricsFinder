using LyricsFinder.SourcePrivoder.Kugou;

namespace LyricsFinder.SourcePrivoder.Xiami
{
    [SourceProviderName("xiami", "DarkProjector")]
    public class XiamiSourceProvider : SourceProviderBase<XiamiSearchResultSong, XiamiSearcher, XiamiLyricDownloader, DefaultLyricsParser>
    {

    }
}