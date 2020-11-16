using LyricsFinder.SourcePrivoder;
using LyricsFinder.SourcePrivoder.Auto;
using LyricsFinder.SourcePrivoder.Kugou;
using LyricsFinder.SourcePrivoder.Xiami;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsFinder.Example
{
    class Program
    {
        async static Task Main(string[] args)
        {
            GlobalSetting.DebugMode = true;

            SourceProviderManager.LoadDefaultProviders();
            //var provider = AutoSourceProvider.FindDefaultImplsToCreate();
            var provider = new XiamiSourceProvider();
            var result = await provider.ProvideLyricAsync("Leaf", "時の魔法", 321036, false);

        }
    }
}
