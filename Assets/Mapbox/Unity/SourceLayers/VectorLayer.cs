using System.Linq;
using Mapbox.Utils;
using System;
using UnityEngine;
using System.Collections.Generic;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.MeshGeneration.Factories;
using Mapbox.Unity.Utilities;

namespace Mapbox.Unity.Map
{
	[Serializable]
	public class VectorLayer : AbstractLayer
	{
		private VectorTileFactory _vectorTileFactory;

		//Events
		public EventHandler SubLayerAdded;
		public EventHandler SubLayerRemoved;

		
		public VectorTileFactory Factory
		{
			get
			{
				return _vectorTileFactory;
			}
		}

		//Public Methods
		public void Initialize(LayerProperties properties)
		{
			Initialize();
		}

		public void Initialize()
		{
			//_vectorTileFactory = ScriptableObject.CreateInstance<VectorTileFactory>();
			//_vectorTileFactory.TileFactoryHasChanged += OnVectorTileFactoryOnTileFactoryHasChanged;
		}


		public void Update(LayerProperties properties)
		{
			Initialize(properties);
		}

		public void UnbindAllEvents()
		{
			if (_vectorTileFactory != null)
			{
				_vectorTileFactory.UnbindEvents();
			}
		}

		private void OnVectorTileFactoryOnTileFactoryHasChanged(object sender, EventArgs args)
		{
			NotifyUpdateLayer(args as LayerUpdateArgs);
		}
	}
}
