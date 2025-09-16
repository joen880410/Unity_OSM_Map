namespace Mapbox.Unity.Map
{
	using Mapbox.Unity.MeshGeneration.Factories;
	using Mapbox.Unity.MeshGeneration.Interfaces;
	using Mapbox.Unity.MeshGeneration.Modifiers;

	public class LayerUpdateArgs : System.EventArgs
	{
		public AbstractTileFactory factory;
		public MapboxDataProperty property;
	}

	public class VectorLayerUpdateArgs : LayerUpdateArgs
	{
		public LayerVisualizerBase visualizer;
		public ModifierBase modifier;
	}

}
