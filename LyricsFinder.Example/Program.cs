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

            foreach (var item in lyrics.LyricSentencs)
            {
                Console.WriteLine(item.Content);
            }
        }
    }
}
