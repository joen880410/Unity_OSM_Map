namespace Mapbox.Editor
{
	using System;
	using UnityEngine;
	using UnityEditor;
	using Mapbox.Unity.Map;

	public class VectorLayerPropertiesDrawer
	{
		private string objectId = "";
		/// <summary>
		/// Gets or sets a value to show or hide Vector section <see cref="T:Mapbox.Editor.MapManagerEditor"/>.
		/// </summary>
		/// <value><c>true</c> if show vector; otherwise, <c>false</c>.</value>
		bool ShowLocationPrefabs
		{
			get
			{
				return EditorPrefs.GetBool(objectId + "VectorLayerProperties_showLocationPrefabs");
			}
			set
			{
				EditorPrefs.SetBool(objectId + "VectorLayerProperties_showLocationPrefabs", value);
			}
		}

		

		private GUIContent _requiredTilesetIdGui = new GUIContent
		{
			text = "Required Tileset Id",
			tooltip = "For location prefabs to spawn the \"streets-v7\" tileset needs to be a part of the Vector data source"
		};

		FeaturesSubLayerPropertiesDrawer _vectorSublayerDrawer = new FeaturesSubLayerPropertiesDrawer();
		PointsOfInterestSubLayerPropertiesDrawer _poiSublayerDrawer = new PointsOfInterestSubLayerPropertiesDrawer();

		public void DrawUI(SerializedProperty property)
		{
			objectId = property.serializedObject.targetObject.GetInstanceID().ToString();
			var layerSourceProperty = property.FindPropertyRelative("sourceOptions");
			var sourceTypeProperty = property.FindPropertyRelative("_sourceType");

			var names = sourceTypeProperty.enumNames;
			VectorSourceType sourceTypeValue = ((VectorSourceType) Enum.Parse(typeof(VectorSourceType), names[sourceTypeProperty.enumValueIndex]));
			//VectorSourceType sourceTypeValue = (VectorSourceType)sourceTypeProperty.enumValueIndex;
			string streets_v7 = MapboxDefaultVector.GetParameters(VectorSourceType.MapboxStreets).Id;
			var layerSourceId = layerSourceProperty.FindPropertyRelative("layerSource.Id");
			string layerString = layerSourceId.stringValue;

			//Draw POI Section
			if (sourceTypeValue == VectorSourceType.None)
			{
				return;
			}

			ShowLocationPrefabs = EditorGUILayout.Foldout(ShowLocationPrefabs, "POINTS OF INTEREST");
			if (ShowLocationPrefabs)
			{
				if (sourceTypeValue != VectorSourceType.None && layerString.Contains(streets_v7))
				{
					GUI.enabled = false;
					EditorGUILayout.TextField(_requiredTilesetIdGui, streets_v7);
					GUI.enabled = true;
					_poiSublayerDrawer.DrawUI(property);
				}
				else
				{
					EditorGUILayout.HelpBox("In order to place points of interest please add \"mapbox.mapbox-streets-v7\" to the data source.", MessageType.Error);
				}
			}
		}

		public void PostProcessLayerProperties(SerializedProperty property)
		{

			var layerSourceProperty = property.FindPropertyRelative("sourceOptions");
			var sourceTypeProperty = property.FindPropertyRelative("_sourceType");
			VectorSourceType sourceTypeValue = (VectorSourceType)sourceTypeProperty.enumValueIndex;
			string streets_v7 = MapboxDefaultVector.GetParameters(VectorSourceType.MapboxStreets).Id;
			var layerSourceId = layerSourceProperty.FindPropertyRelative("layerSource.Id");
			string layerString = layerSourceId.stringValue;

			if (ShowLocationPrefabs)
			{
				if (_poiSublayerDrawer.isLayerAdded == true && sourceTypeValue != VectorSourceType.None && layerString.Contains(streets_v7))
				{
					var prefabItemArray = property.FindPropertyRelative("locationPrefabList");
					var prefabItem = prefabItemArray.GetArrayElementAtIndex(prefabItemArray.arraySize - 1);
					PrefabItemOptions prefabItemOptionToAdd = (PrefabItemOptions)EditorHelper.GetTargetObjectOfProperty(prefabItem) as PrefabItemOptions;
					((VectorLayerProperties)EditorHelper.GetTargetObjectOfProperty(property)).OnSubLayerPropertyAdded(new VectorLayerUpdateArgs { property = prefabItemOptionToAdd });
					_poiSublayerDrawer.isLayerAdded = false;
				}
			}
			

		}

	}
}
