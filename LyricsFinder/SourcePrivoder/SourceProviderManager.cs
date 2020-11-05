using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LyricsFinder.SourcePrivoder
{
    public static class SourceProviderManager
    {
        internal static HashSet<Type> LyricsSourceProvidersTypes { get; } = new HashSet<Type>();
        private static readonly Type SourceProviderBaseType = typeof(SourceProviderBase);
        private static HashSet<(SourceProviderBase obj, string name)> cache_obj = new HashSet<(SourceProviderBase obj, string name)>();

        public static void LoadDefaultProviders()
        {
            foreach (var item in typeof(SourceProviderManager).Assembly
                .GetTypes()
                .Where(x => x.GetCustomAttribute<SourceProviderNameAttribute>() != null)
                .Where(x => x.IsSubclassOf(SourceProviderBaseType)))
            {
                AddSourceProvierType(item);
            } 
        }

        public static void AddSourceProvierType(Type type)
        {
            if (LyricsSourceProvidersTypes.Contains(type))
            {
                Utils.Output($"source provider type {type.Name} had been loaded.");
                return;
            }

            LyricsSourceProvidersTypes.Add(type);
            Utils.Output($"loaded source provider type {type.Name} sucessfully.");
        }

        public static SourceProviderBase GetOrCreateSourceProvier(string provider_name)
        {
            var cache = cache_obj.FirstOrDefault(x => x.name.Equals(provider_name, StringComparison.InvariantCultureIgnoreCase));

            if (cache.obj!=null)
                return cache.obj;

            var provider = LyricsSourceProvidersTypes.FirstOrDefault(x => x.GetCustomAttribute<SourceProviderNameAttribute>()?.Name?.Equals(provider_name, StringComparison.InvariantCultureIgnoreCase)??false);

            if (provider!=null)
            {
                var obj = provider.Assembly.CreateInstance(provider.FullName) as SourceProviderBase;
                cache_obj.Add((obj, provider_name));

                return obj;
            }

            return null;
        }
    }
}