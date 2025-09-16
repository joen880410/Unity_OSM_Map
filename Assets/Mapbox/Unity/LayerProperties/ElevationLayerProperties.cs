using System;
namespace Mapbox.Unity.Map
{


    [Serializable]
	public class ElevationLayerProperties : MapboxDataProperty
	{
		public ElevationSourceType sourceType = ElevationSourceType.MapboxTerrain;
		public ElevationLayerType elevationLayerType = ElevationLayerType.FlatTerrain;
		public ElevationRequiredOptions requiredOptions = new ElevationRequiredOptions();
		public ElevationModificationOptions modificationOptions = new ElevationModificationOptions();
		public UnityLayerOptions unityLayerOptions = new UnityLayerOptions();

		public override bool NeedsForceUpdate()
		{
			return true;
		}
	}
}
