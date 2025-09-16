using Mapbox.Map;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mapbox.Platform.Cache
{


    public class MemoryCache : ICache
    {


        // TODO: add support for disposal strategy (timestamp, distance, etc.)
        public MemoryCache(uint maxCacheSize)
        {
#if MAPBOX_DEBUG_CACHE
			_className = this.GetType().Name;
#endif
            _maxCacheSize = maxCacheSize;
            _cachedResponses = new Dictionary<string, CacheItem>();
        }


#if MAPBOX_DEBUG_CACHE
		private string _className;
#endif
        private uint _maxCacheSize;
        private object _lock = new object();
        private Dictionary<string, CacheItem> _cachedResponses;


        public uint MaxCacheSize
        {
            get { return _maxCacheSize; }
        }


        public void ReInit()
        {
            _cachedResponses = new Dictionary<string, CacheItem>();
        }


        public void Add(CanonicalTileId tilesetId, CacheItem item, bool forceInsert)
        {
            string key = tilesetId.ToString();

            lock (_lock)
            {
                if (_cachedResponses.Count >= _maxCacheSize)
                {
                    _cachedResponses.Remove(_cachedResponses.OrderBy(c => c.Value.AddedToCacheTicksUtc).First().Key);
                }

                // TODO: forceInsert
                if (!_cachedResponses.ContainsKey(key))
                {
                    item.AddedToCacheTicksUtc = DateTime.UtcNow.Ticks;
                    _cachedResponses.Add(key, item);
                }
            }
        }


        public CacheItem Get(CanonicalTileId tileId)
        {
            string key = tileId.ToString();

#if MAPBOX_DEBUG_CACHE
			string methodName = _className + "." + new System.Diagnostics.StackFrame().GetMethod().Name;
			UnityEngine.Debug.LogFormat("{0} {1}", methodName, key);
#endif

            lock (_lock)
            {
                if (!_cachedResponses.ContainsKey(key))
                {
                    return null;
                }

                return _cachedResponses[key];
            }
        }


        public void Clear()
        {
            lock (_lock)
            {
                _cachedResponses.Clear();
            }
        }
    }
}
