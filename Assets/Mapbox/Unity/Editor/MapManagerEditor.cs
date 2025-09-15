namespace Mapbox.Editor
{
    using UnityEngine;
    using UnityEditor;
    using Mapbox.Unity.Map;

    [CustomEditor(typeof(AbstractMap))]
    [CanEditMultipleObjects]
    public class MapManagerEditor : Editor
    {
        private string objectId = "";
        private Color previewButtonColor = new Color(0.7f, 1.0f, 0.7f);
        /// <summary>
        /// Gets or sets a value indicating whether to show general section <see cref="T:Mapbox.Editor.MapManagerEditor"/>.
        /// </summary>
        /// <value><c>true</c> then show general section; otherwise hide, <c>false</c>.</value>
        bool ShowGeneral
        {
            get
            {
                return EditorPrefs.GetBool(objectId + "MapManagerEditor_showGeneral");
            }
            set
            {
                EditorPrefs.SetBool(objectId + "MapManagerEditor_showGeneral", value);
            }
        }

        static float _lineHeight = EditorGUIUtility.singleLineHeight;


        public override void OnInspectorGUI()
        {
            objectId = serializedObject.targetObject.GetInstanceID().ToString();
            serializedObject.Update();
            var previewOptions = serializedObject.FindProperty("_previewOptions");
            var prevProp = previewOptions.FindPropertyRelative("isPreviewEnabled");
            var prev = prevProp.boolValue;

            Color guiColor = GUI.color;
            GUI.color = (prev) ? previewButtonColor : guiColor;
            ShowGeneral = EditorGUILayout.Foldout(ShowGeneral, new GUIContent { text = "GENERAL", tooltip = "Options related to map data" });

            if (ShowGeneral)
            {
                DrawMapOptions(serializedObject);
            }


        }
        bool ShowPosition
        {
            get
            {
                return EditorPrefs.GetBool(objectId + "MapManagerEditor_showPosition");
            }
            set
            {
                EditorPrefs.SetBool(objectId + "MapManagerEditor_showPosition", value);
            }
        }
        void DrawMapOptions(SerializedObject mapObject)
        {
            var property = mapObject.FindProperty("_options");
            EditorGUILayout.LabelField("Location ", GUILayout.Height(_lineHeight));

            EditorGUILayout.PropertyField(property.FindPropertyRelative("locationOptions"));
            var extentOptions = property.FindPropertyRelative("extentOptions");
            GUILayout.Space(-_lineHeight);
            EditorGUILayout.PropertyField(extentOptions);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_initializeOnStart"));
            ShowPosition = EditorGUILayout.Foldout(ShowPosition, "Others");
            if (ShowPosition)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.Space(-_lineHeight);
                EditorGUI.BeginChangeCheck();
                var scalingOptions = property.FindPropertyRelative("scalingOptions");
                EditorGUILayout.PropertyField(scalingOptions);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorHelper.CheckForModifiedProperty(scalingOptions);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(property.FindPropertyRelative("loadingTexture"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("tileMaterial"));
                if (EditorGUI.EndChangeCheck())
                {
                    EditorHelper.CheckForModifiedProperty(property);
                }
            }
        }
    }
}
