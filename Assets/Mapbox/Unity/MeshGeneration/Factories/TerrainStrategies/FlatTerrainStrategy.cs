using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;

namespace Mapbox.Unity.MeshGeneration.Factories.TerrainStrategies
{
    public class MeshDataArray
    {
        public Vector3[] Vertices;
        public Vector3[] Normals;
        public int[] Triangles;
        public Vector2[] Uvs;
    }
    public class FlatTerrainStrategy : TerrainStrategy
    {
        MeshDataArray _cachedQuad;

        public override int RequiredVertexCount
        {
            get { return 4; }
        }

        public override void Initialize(ElevationLayerProperties elOptions)
        {
            _elevationOptions = elOptions;
        }

        public override void RegisterTile(UnityTile tile)
        {
            if (_elevationOptions.unityLayerOptions.addToLayer && tile.gameObject.layer != _elevationOptions.unityLayerOptions.layerId)
            {
                tile.gameObject.layer = _elevationOptions.unityLayerOptions.layerId;
            }

            if ((int)tile.ElevationType != (int)ElevationLayerType.FlatTerrain)
            {
                tile.MeshFilter.sharedMesh.Clear();
                // HACK: This is here in to make the system trigger a finished state.
                GetQuad(tile);
                tile.ElevationType = TileTerrainType.Flat;
            }
        }

        private void GetQuad(UnityTile tile)
        {
            if (_cachedQuad != null)
            {
                var mesh = tile.MeshFilter.sharedMesh;
                mesh.vertices = _cachedQuad.Vertices;
                mesh.normals = _cachedQuad.Normals;
                mesh.triangles = _cachedQuad.Triangles;
                mesh.uv = _cachedQuad.Uvs;
            }
            else
            {
                BuildQuad(tile);
            }
        }

        private void BuildQuad(UnityTile tile)
        {
            var unityMesh = tile.MeshFilter.sharedMesh;
            var verts = new Vector3[4];
            var norms = new Vector3[4];
            verts[0] = tile.TileScale * ((tile.Rect.Min - tile.Rect.Center).ToVector3xz());
            verts[1] = tile.TileScale * (new Vector3((float)(tile.Rect.Max.x - tile.Rect.Center.x), 0, (float)(tile.Rect.Min.y - tile.Rect.Center.y)));
            verts[2] = tile.TileScale * ((tile.Rect.Max - tile.Rect.Center).ToVector3xz());
            verts[3] = tile.TileScale * (new Vector3((float)(tile.Rect.Min.x - tile.Rect.Center.x), 0, (float)(tile.Rect.Max.y - tile.Rect.Center.y)));
            norms[0] = Constants.Math.Vector3Up;
            norms[1] = Constants.Math.Vector3Up;
            norms[2] = Constants.Math.Vector3Up;
            norms[3] = Constants.Math.Vector3Up;

            unityMesh.vertices = verts;
            unityMesh.normals = norms;

            var trilist = new int[6] { 0, 1, 2, 0, 2, 3 };
            unityMesh.triangles = trilist;

            var uvlist = new Vector2[4]
            {
                    new Vector2(0,1),
                    new Vector2(1,1),
                    new Vector2(1,0),
                    new Vector2(0,0)
            };
            unityMesh.uv = uvlist;
            _cachedQuad = new MeshDataArray()
            {
                Vertices = verts,
                Normals = norms,
                Triangles = trilist,
                Uvs = uvlist
            };
        }
    }
}
