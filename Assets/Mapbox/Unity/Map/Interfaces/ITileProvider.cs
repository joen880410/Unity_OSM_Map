using System;
using Mapbox.Map;
using Mapbox.Unity.Map.TileProviders;

namespace Mapbox.Unity.Map.Interfaces
{
	public interface ITileProvider
	{
		event EventHandler<ExtentArgs> ExtentChanged;
		ITileProviderOptions Options { get; }
		void Initialize(IMap map);
		// TODO: Maybe combine both these methods.
		void SetOptions(ITileProviderOptions options);

	}
}
