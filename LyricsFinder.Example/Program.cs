using LyricsFinder.SourcePrivoder;
using LyricsFinder.SourcePrivoder.Auto;
using LyricsFinder.SourcePrivoder.Kugou;
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
            var provider = AutoSourceProvider.FindDefaultImplsToCreate();
            var cancelTokenSource = new CancellationTokenSource();

            new Thread(async () =>
            {
                Console.WriteLine("begin.");

                var lyrics = await provider.ProvideLyricAsync("ゆある", "アスノヨゾラ哨戒班", 177000, false, cancelTokenSource.Token);
                if (lyrics is null)
                {
                    Console.WriteLine("null.");
                    return;
                }

                Console.WriteLine("actual provider name : " + lyrics.ProviderName);

                foreach (var item in lyrics.LyricsSentences)
                {
                    Console.WriteLine(item.Content);
                }
                Console.WriteLine("finish.");
            }).Start();

            Thread.Sleep(200);
            cancelTokenSource.Cancel();
            Console.WriteLine("canceled.");
            Console.ReadLine();
        }
    }
}
