using UnityEngine;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.Map;
using Mapbox.Map;
using Mapbox.Unity.MeshGeneration.Enums;
using Mapbox.Unity.MeshGeneration.Factories.TerrainStrategies;

namespace Mapbox.Unity.MeshGeneration.Factories
{
    public class TerrainFactoryBase : AbstractTileFactory
    {
        public TerrainStrategy Strategy = new FlatTerrainStrategy();
        [SerializeField]
        protected ElevationLayerProperties _elevationOptions = new ElevationLayerProperties();
        protected TerrainDataFetcher DataFetcher;


        #region UnityMethods
        private void OnDestroy()
        {
            if (DataFetcher != null)
            {
                DataFetcher.DataRecieved -= OnTerrainRecieved;
                DataFetcher.FetchingError -= OnDataError;
            }
        }
        #endregion

        #region AbstractFactoryOverrides
        protected override void OnInitialized()
        {
            Strategy.Initialize(_elevationOptions);
            DataFetcher = ScriptableObject.CreateInstance<TerrainDataFetcher>();
            DataFetcher.DataRecieved += OnTerrainRecieved;
            DataFetcher.FetchingError += OnDataError;
        }

        public override void SetOptions(MapboxDataProperty options)
        {
            _elevationOptions = (ElevationLayerProperties)options;
            Strategy.Initialize(_elevationOptions);
        }

        protected override void OnRegistered(UnityTile tile)
        {
            //reseting height data
            Strategy.RegisterTile(tile);
            tile.HeightDataState = TilePropertyState.Loaded;
        }

        protected override void OnUnregistered(UnityTile tile)
        {
            if (_tilesWaitingResponse != null && _tilesWaitingResponse.Contains(tile))
            {
                _tilesWaitingResponse.Remove(tile);
            }
            Strategy.UnregisterTile(tile);
        }

        public override void Clear()
        {
            DestroyImmediate(DataFetcher);
        }

        protected override void OnPostProcess(UnityTile tile)
        {
            Strategy.PostProcessTile(tile);
        }

        public override void UnbindEvents()
        {
            base.UnbindEvents();
        }

        protected override void OnUnbindEvents()
        {
        }
        #endregion

        #region DataFetcherEvents
        private void OnTerrainRecieved(UnityTile tile, RasterTile pngRasterTile)
        {
            if (tile != null)
            {
                _tilesWaitingResponse.Remove(tile);

                if (tile.HeightDataState != TilePropertyState.Unregistered)
                {
                    Strategy.RegisterTile(tile);
                }
            }
        }

        private void OnDataError(UnityTile tile, RasterTile rawTile, TileErrorEventArgs e)
        {
            base.OnErrorOccurred(tile, e);
            if (tile != null)
            {
                _tilesWaitingResponse.Remove(tile);
                if (tile.HeightDataState != TilePropertyState.Unregistered)
                {
                    Strategy.DataErrorOccurred(tile, e);
                    tile.HeightDataState = TilePropertyState.Error;
                }
            }
        }
        #endregion

    }
}
