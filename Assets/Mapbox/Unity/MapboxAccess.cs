namespace Mapbox.Unity
{
    using UnityEngine;
    using System;
    using System.IO;
    using Mapbox.Platform;
    using Mapbox.Platform.Cache;
    using Mapbox.Map;

    /// <summary>
    /// Object for retrieving an API token and making http requests.
    /// Contains a lazy <see cref="T:Mapbox.Geocoding.Geocoder">Geocoder</see> and a lazy <see cref="T:Mapbox.Directions.Directions">Directions</see> for convenience.
    /// </summary>
    public class MapboxAccess : IFileSource
    {
        CachingWebFileSource _fileSource;
        private static MapboxAccess _instance;

        /// <summary>
        /// The singleton instance.
        /// </summary>
        public static MapboxAccess Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MapboxAccess();
                }
                return _instance;
            }
        }
        private MapboxAccess()
        {
            SetConfiguration();
        }

        private MapboxConfiguration _configuration = new MapboxConfiguration();

        /// <summary>
        /// The Mapbox API access token.
        /// </summary>
        public MapboxConfiguration Configuration
        {
            get
            {
                return _configuration;
            }
        }

        public void SetConfiguration()
        {
            ConfigureFileSource();
        }


        public void ClearAllCacheFiles()
        {
            // explicity call Clear() to close any connections that might be referenced by the current scene
            _fileSource?.Clear();

            // remove all left over files (eg orphaned .journal) from the cache directory
            string cacheDirectory = Path.Combine(Application.persistentDataPath, "cache");
            if (!Directory.Exists(cacheDirectory)) { return; }

            foreach (var file in Directory.GetFiles(cacheDirectory))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception deleteEx)
                {
                    Debug.LogErrorFormat("Could not delete [{0}]: {1}", file, deleteEx);
                }
            }

            //reinit caches after clear
            _fileSource?.ReInit();

            Debug.Log("done clearing caches");
        }


        void ConfigureFileSource()
        {
            _fileSource = new CachingWebFileSource( _configuration.AutoRefreshCache)
                .AddCache(new MemoryCache(_configuration.MemoryCacheSize))
                .AddCache(new SQLiteCache(_configuration.FileCacheSize))
                ;
        }

        /// <summary>
        /// Makes an asynchronous url query.
        /// </summary>
        /// <returns>The request.</returns>
        /// <param name="url">URL.</param>
        /// <param name="callback">Callback.</param>
        public IAsyncRequest Request(
            string url
            , Action<Response> callback
            , int timeout = 10
            , CanonicalTileId tileId = new CanonicalTileId()
            , string tilesetId = null
        )
        {
            return _fileSource.Request(url, callback, _configuration.DefaultTimeout, tileId, tilesetId);
        }

        class InvalidTokenException : Exception
        {
            public InvalidTokenException(string message) : base(message)
            {
            }
        }
    }

    public class MapboxConfiguration
    {
        public uint MemoryCacheSize = 500;
        public uint FileCacheSize = 2500;
        public int DefaultTimeout = 30;
        public bool AutoRefreshCache = false;
    }
}
