//-----------------------------------------------------------------------
// <copyright file="TileResource.cs" company="Mapbox">
//     Copyright (c) 2016 Mapbox. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Mapbox.Map
{
    using Platform;
    using System;
    using Mapbox.Unity.Telemetry;
    using UnityEngine;

    public sealed class TileResource : IResource
    {
        readonly string _query;

        internal TileResource(string query)
        {
            _query = query;
        }
        internal static TileResource MakeRetinaRaster(CanonicalTileId id, string styleUrl)
        {
            return new TileResource($"https://tile.openstreetmap.org/{id}.png");
        }

        public string GetUrl()
        {
            var uriBuilder = new UriBuilder(_query);
            if (uriBuilder.Query != null && uriBuilder.Query.Length > 1)
            {
                uriBuilder.Query = uriBuilder.Query.Substring(1);
            }
            //return uriBuilder.ToString();
            return uriBuilder.Uri.ToString();
        }
    }
}
