using Mapbox.Map;
using Mapbox.Unity;
using Mapbox.Unity.MeshGeneration.Data;
using System;
using UnityEngine;

public class DataFetcherParameters
{
	public CanonicalTileId canonicalTileId;
	public string tilesetId;
	public UnityTile tile;
}

public abstract class DataFetcher : ScriptableObject
{
	protected MapboxAccess _fileSource;

	public void OnEnable()
	{
		_fileSource = MapboxAccess.Instance;
	}

	public abstract void FetchData(DataFetcherParameters parameters);
}

public class TerrainDataFetcher : DataFetcher
{
	public Action<UnityTile, RasterTile> DataRecieved = (t, s) => { };
	public Action<UnityTile, RasterTile, TileErrorEventArgs> FetchingError = (t, r, s) => { };

	//tile here should be totally optional and used only not to have keep a dictionary in terrain factory base
	public override void FetchData(DataFetcherParameters parameters)
	{
		if(parameters == null)
		{
			return;
		}
		var pngRasterTile = new RasterTile();
		pngRasterTile.Initialize(_fileSource, parameters.canonicalTileId, parameters.tilesetId, () =>
		{
			if (parameters.tile.CanonicalTileId != pngRasterTile.Id)
			{
				//this means tile object is recycled and reused. Returned data doesn't belong to this tile but probably the previous one. So we're trashing it.
				return;
			}

			if (pngRasterTile.HasError)
			{
                FetchingError(parameters.tile, pngRasterTile, new TileErrorEventArgs(parameters.canonicalTileId, pngRasterTile.GetType(), null, pngRasterTile.Exceptions));
			}
			else
			{
                DataRecieved(parameters.tile, pngRasterTile);
			}
		});
	}
}
