using LyricsFinder.SourcePrivoder;
using LyricsFinder.SourcePrivoder.Auto;
using System;

namespace LyricsFinder.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            SourceProviderManager.LoadDefaultProviders();

            var provider = AutoSourceProvider.FindDefaultImplsToCreate();
            var lyrics = provider.ProvideLyric("ゆある", "アスノヨゾラ哨戒班", 177000, false);

            Console.WriteLine("actual provider name : " + lyrics.ProviderName);

            foreach (var item in lyrics.LyricsSentences)
            {
                Console.WriteLine(item.Content);
            }
        }
    }
}
