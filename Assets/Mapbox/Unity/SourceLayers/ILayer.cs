using System.Linq;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.SourceLayers;

namespace Mapbox.Unity.Map
{
	using System;
	using System.Collections.Generic;
	using Mapbox.Unity.MeshGeneration.Filters;
	using Mapbox.Utils;
	using UnityEngine;


	public interface ISubLayerFiltering
	{
		ILayerFilter AddStringFilterContains(string key, string property);
		ILayerFilter AddNumericFilterEquals(string key, float value);
		ILayerFilter AddNumericFilterLessThan(string key, float value);
		ILayerFilter AddNumericFilterGreaterThan(string key, float value);
		ILayerFilter AddNumericFilterInRange(string key, float min, float max);

		ILayerFilter GetFilter(int index);

		void RemoveFilter(int index);
		void RemoveFilter(LayerFilter filter);
		void RemoveFilter(ILayerFilter filter);
		void RemoveAllFilters();

		IEnumerable<ILayerFilter> GetAllFilters();
		IEnumerable<ILayerFilter> GetFiltersByQuery(System.Func<ILayerFilter, bool> query);

		LayerFilterCombinerOperationType GetFilterCombinerType();

		void SetFilterCombinerType(LayerFilterCombinerOperationType layerFilterCombinerOperationType);
	}

	public interface ILayerFilter
	{
		bool FilterKeyContains(string key);
		bool FilterKeyMatchesExact(string key);
		bool FilterUsesOperationType(LayerFilterOperationType layerFilterOperationType);
		bool FilterPropertyContains(string property);
		bool FilterPropertyMatchesExact(string property);
		bool FilterNumberValueEquals(float value);
		bool FilterNumberValueIsGreaterThan(float value);
		bool FilterNumberValueIsLessThan(float value);
		bool FilterIsInRangeValueContains(float value);

		string GetKey { get; }
		LayerFilterOperationType GetFilterOperationType { get; }

		string GetPropertyValue { get; }
		float GetNumberValue { get; }

		float GetMinValue { get; }
		float GetMaxValue { get; }

		void SetStringContains(string key, string property);
		void SetNumberIsEqual(string key, float value);
		void SetNumberIsLessThan(string key, float value);
		void SetNumberIsGreaterThan(string key, float value);
		void SetNumberIsInRange(string key, float min, float max);

	}

	public interface IVectorSubLayer
	{
		/// <summary>
		/// Gets `Filters` data from the feature.
		/// </summary>
		ISubLayerFiltering Filtering { get; }
		/// <summary>
		/// Gets `Modeling` data from the feature.
		/// </summary>
		ISubLayerModeling Modeling { get; }
		/// <summary>
		/// Gets `Texturing` data from the feature.
		/// </summary>
		ISubLayerTexturing Texturing { get; }
		/// <summary>
		/// Gets `Behavior Modifiers` data from the feature.
		/// </summary>
		ISubLayerBehaviorModifiers BehaviorModifiers { get; }


	}
}
