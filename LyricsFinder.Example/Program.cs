using LyricsFinder.SourcePrivoder;
using LyricsFinder.SourcePrivoder.Auto;
using LyricsFinder.SourcePrivoder.Kugou;
using System;
using System.Threading.Tasks;

namespace LyricsFinder.Example
{
    class Program
    {
        async static Task Main(string[] args)
        {
            GlobalSetting.DebugMode = true;

            SourceProviderManager.LoadDefaultProviders();

            var provider = AutoSourceProvider.FindDefaultImplsToCreate();
            var lyrics = await provider.ProvideLyricAsync("ゆある", "アスノヨゾラ哨戒班", 177000, false);

            Console.WriteLine("actual provider name : " + lyrics.ProviderName);

            foreach (var item in lyrics.LyricsSentences)
            {
                Console.WriteLine(item.Content);
            }
        }
    }
}
