namespace Mapbox.Unity.Map
{
    public interface IImageryLayer 
    {

        /// <summary>
        /// Enables or disables Unity Texture2D compression for `IMAGE` outputs.
        /// Enable this if you need performance rather than a high resolution image.
        /// </summary>
        /// <param name="useCompression">Boolean to toggle `Use Compression`.</param>
        void UseCompression(bool useCompression);

        /// <summary>
        /// Enables or disables Unity Texture2D Mipmap for `IMAGE` outputs.
        /// Mipmaps are lists of progressively smaller versions of an image, used
        /// to optimize performance. Enabling mipmaps consumes more memory, but
        /// provides improved performance.
        /// </summary>
        /// <param name="useMipMap">Boolean to toggle `Use Mip Map`.</param>
        void UseMipMap(bool useMipMap);

        /// <summary>
        /// Changes the settings for the `IMAGE` component.
        /// </summary>
        /// <param name="imageSource">`Data Source` for the IMAGE component.</param>
        /// <param name="useRetina">Enables or disables high quality imagery.</param>
        /// <param name="useCompression">Enables or disables Unity Texture2D compression.</param>
        /// <param name="useMipMap">Enables or disables Unity Texture2D image mipmapping.</param>
        void SetProperties(bool useCompression, bool useMipMap);
    }

}
