namespace Mapbox.Unity.MeshGeneration.Factories
{
    using Mapbox.Map;
    using UnityEngine;
    using Mapbox.Unity.MeshGeneration.Enums;
    using Mapbox.Unity.MeshGeneration.Data;
    using Mapbox.Unity.Map;


	/// <summary>
	/// Uses raster image services to create materials & textures for terrain
	/// </summary>
	[CreateAssetMenu(menuName = "Mapbox/Factories/Image Factory")]
	public class MapImageFactory : AbstractTileFactory
	{
		[SerializeField]
        public ImageryRasterOptions rasterOptions = new ImageryRasterOptions();
        public LayerSourceOptions sourceOptions = new LayerSourceOptions()
        {
            isActive = true,
            layerSource = MapboxDefaultImagery.GetParameters(ImagerySourceType.MapboxStreets)
        };

        protected ImageDataFetcher DataFetcher;

        public string TilesetId
		{
			get
			{
				return sourceOptions.Id;
			}

			set
			{
				sourceOptions.Id = value;
			}
		}

		#region UnityMethods
		protected virtual void OnDestroy()
		{
			//unregister events
			if (DataFetcher != null)
			{
				DataFetcher.DataRecieved -= OnImageRecieved;
				DataFetcher.FetchingError -= OnDataError;
			}
		}
		#endregion

		#region DataFetcherEvents
		private void OnImageRecieved(UnityTile tile, RasterTile rasterTile)
		{
			if (tile != null)
			{
				_tilesWaitingResponse.Remove(tile);

				if (tile.RasterDataState != TilePropertyState.Unregistered)
				{
					tile.SetRasterData(rasterTile.Data, rasterOptions.useMipMap, rasterOptions.useCompression);
				}
			}
		}

		//merge this with OnErrorOccurred?
		protected virtual void OnDataError(UnityTile tile, RasterTile rasterTile, TileErrorEventArgs e)
		{
			if (tile != null)
			{
				if (tile.RasterDataState != TilePropertyState.Unregistered)
				{
					tile.RasterDataState = TilePropertyState.Error;
					_tilesWaitingResponse.Remove(tile);
					OnErrorOccurred(e);
				}

			}
		}
		#endregion

		#region AbstractFactoryOverrides
		protected override void OnInitialized()
		{
			DataFetcher = ScriptableObject.CreateInstance<ImageDataFetcher>();
			DataFetcher.DataRecieved += OnImageRecieved;
			DataFetcher.FetchingError += OnDataError;
		}


		protected override void OnRegistered(UnityTile tile)
		{
				tile.RasterDataState = TilePropertyState.Loading;
                DataFetcherParameters parameters = new DataFetcherParameters()
				{
					canonicalTileId = tile.CanonicalTileId,
					tile = tile,
					tilesetId = TilesetId,
				};
				DataFetcher.FetchData(parameters);
		}

		/// <summary>
		/// Method to be called when a tile error has occurred.
		/// </summary>
		/// <param name="e"><see cref="T:Mapbox.Map.TileErrorEventArgs"/> instance/</param>
		protected override void OnErrorOccurred(UnityTile tile, TileErrorEventArgs e)
		{
			base.OnErrorOccurred(tile, e);
			if (tile != null)
			{
				tile.RasterDataState = TilePropertyState.Error;
			}
		}

		protected override void OnUnregistered(UnityTile tile)
		{
			if (_tilesWaitingResponse != null && _tilesWaitingResponse.Contains(tile))
			{
				_tilesWaitingResponse.Remove(tile);
			}
		}

		public override void Clear()
		{
			DestroyImmediate(DataFetcher);
		}

		protected override void OnPostProcess(UnityTile tile)
		{

		}

		public override void UnbindEvents()
		{
			base.UnbindEvents();
		}

		protected override void OnUnbindEvents()
		{
		}
		#endregion
	}
}
