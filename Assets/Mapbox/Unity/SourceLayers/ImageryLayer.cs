namespace Mapbox.Unity.Map
{
    using System;
    using UnityEngine;
    using Mapbox.Unity.MeshGeneration.Factories;

    [Serializable]
    public class ImageryLayer : AbstractLayer
    {
        [Tooltip("Use Unity compression for the tile texture.")]
        public bool useCompression = true;
        [Tooltip("Use texture with Unity generated mipmaps.")]
        public bool useMipMap = true;
        public void Initialize()
        {
            _imageFactory = ScriptableObject.CreateInstance<MapImageFactory>();
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

        #endregion
    }
}
