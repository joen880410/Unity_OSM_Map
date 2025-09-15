using Mapbox.Unity.MeshGeneration.Data;

namespace Mapbox.Unity.Map
{
    using System;
    using UnityEngine;
    using Mapbox.Unity.MeshGeneration.Factories;
    using Mapbox.Unity.Utilities;

    [Serializable]
    public class ImageryLayer : AbstractLayer, IImageryLayer
    {
        [Tooltip("Use Unity compression for the tile texture.")]
        public bool useCompression = false;
        [Tooltip("Use texture with Unity generated mipmaps.")]
        public bool useMipMap = false;
        public void Initialize()
        {
            _imageFactory = ScriptableObject.CreateInstance<MapImageFactory>();
            //_imageFactory.SetOptions(_layerProperty);
        }

        public void RedrawLayer(object sender, System.EventArgs e)
        {
            NotifyUpdateLayer(_imageFactory, sender as MapboxDataProperty, false);
        }

        private MapImageFactory _imageFactory;
        public MapImageFactory Factory
        {
            get
            {
                return _imageFactory;
            }
        }

        #region API Methods
        /// <summary>
        /// Enable Texture2D compression for image factory outputs.
        /// </summary>
        /// <param name="useCompression"></param>
        public virtual void UseCompression(bool useCompression)
        {
            if (this.useCompression != useCompression)
            {
                this.useCompression = useCompression;
            }
        }

        /// <summary>
        /// Enable Texture2D MipMap option for image factory outputs.
        /// </summary>
        /// <param name="useMipMap"></param>
        public virtual void UseMipMap(bool useMipMap)
        {
            if (this.useMipMap != useMipMap)
            {
                this.useMipMap = useMipMap;
            }
        }

        /// <summary>
        /// Change image layer settings.
        /// </summary>
        /// <param name="imageSource">Data source for the image provider.</param>
        /// <param name="useRetina">Enable/Disable high quality imagery.</param>
        /// <param name="useCompression">Enable/Disable Unity3d Texture2d image compression.</param>
        /// <param name="useMipMap">Enable/Disable Unity3d Texture2d image mipmapping.</param>
        public virtual void SetProperties(bool useCompression, bool useMipMap)
        {

            if (this.useCompression != useCompression || this.useMipMap != useMipMap)
            {
                this.useCompression = useCompression;
                this.useMipMap = useMipMap;
            }
        }
        #endregion
    }
}
