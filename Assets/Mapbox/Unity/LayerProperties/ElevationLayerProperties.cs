namespace Mapbox.Unity.Map
{
	using System;
	using System.ComponentModel;
	using Mapbox.Unity.MeshGeneration.Data;
	using Mapbox.Unity.MeshGeneration.Factories;

	[Serializable]
	public class ElevationLayerProperties : MapboxDataProperty
	{
		public ElevationSourceType sourceType = ElevationSourceType.MapboxTerrain;
		public ElevationLayerType elevationLayerType = ElevationLayerType.FlatTerrain;
		public ElevationRequiredOptions requiredOptions = new ElevationRequiredOptions();
		public TerrainColliderOptions colliderOptions = new TerrainColliderOptions();
		public ElevationModificationOptions modificationOptions = new ElevationModificationOptions();
		public UnityLayerOptions unityLayerOptions = new UnityLayerOptions();
		public TerrainSideWallOptions sideWallOptions = new TerrainSideWallOptions();

		public override bool NeedsForceUpdate()
		{
			return true;
		}
	}
}
