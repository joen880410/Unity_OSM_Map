namespace Mapbox.Unity.Map
{
    using Mapbox.Unity.MeshGeneration.Factories;

    public class LayerUpdateArgs : System.EventArgs
	{
		public AbstractTileFactory factory;
		public MapboxDataProperty property;
	}
}
