using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsFinder.SourcePrivoder.Auto
{
    [SourceProviderName("auto", "DarkProjector")]
    public class AutoSourceProvider : SourceProviderBase
    {
        public SourceProviderBase[] search_engines = new SourceProviderBase[0];

        private Dictionary<string, SourceProviderBase> cache_provider = new Dictionary<string, SourceProviderBase>();

        public static AutoSourceProvider FindDefaultImplsToCreate() => new AutoSourceProvider(SourceProviderManager.LyricsSourceProvidersTypes
                .Select(x => x.GetCustomAttribute<SourceProviderNameAttribute>())
                .OfType<SourceProviderNameAttribute>()
                .Where(x => !x.Name.Equals("auto", StringComparison.InvariantCultureIgnoreCase))
                .Select(x => SourceProviderManager.GetOrCreateSourceProvier(x.Name))
                .ToArray());

        public AutoSourceProvider(SourceProviderBase[] other_source_providers)
        {
            search_engines = other_source_providers ?? Array.Empty<SourceProviderBase>();
        }

        public override async Task<Lyrics> ProvideLyricAsync(string artist, string title, int time, bool request_trans_lyrics, CancellationToken cancel_token = default)
        {
            var id = artist + title + time;

            //保证同一谱面获取的翻译歌词和原歌词都是同一个歌词源，这样歌词合并的时候会好很多
            if (cache_provider.TryGetValue(id, out var provider))
                return await provider.ProvideLyricAsync(artist, title, time, request_trans_lyrics, cancel_token);

            var (lyrics, providerb) = await GetLyricFromAnySource(artist, title, time, request_trans_lyrics, cancel_token);

            if (lyrics != null)
                cache_provider[id] = providerb;

            return lyrics;
        }

        public async Task<(Lyrics, SourceProviderBase)> GetLyricFromAnySource(string artist, string title, int time, bool request_trans_lyrics, CancellationToken cancel_token)
        {
            var internalCancelTokenSource = new CancellationTokenSource();
            cancel_token.Register(() => internalCancelTokenSource.Cancel());

            var taskMap = new Dictionary<Task<Lyrics>, SourceProviderBase>();

            foreach (var provider in search_engines)
                taskMap[provider.ProvideLyricAsync(artist, title, time, request_trans_lyrics, internalCancelTokenSource.Token)] = provider;

            var running_tasks = taskMap.Keys.ToList();

            while (running_tasks.Count > 0 && !cancel_token.IsCancellationRequested)
            {
                var finishTask = await Task.WhenAny(running_tasks);
                running_tasks.Remove(finishTask);

                var lyrics = await finishTask;
                var provider = taskMap[finishTask];

                if (lyrics is null || provider is null)
                    continue;

                Utils.Debug($"Quick select lyric from provider : {provider.ProviderName}");
                internalCancelTokenSource.Cancel();

                return (lyrics, provider);
            }

            return default;
        }
    }
}