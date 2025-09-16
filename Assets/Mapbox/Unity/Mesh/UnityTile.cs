using Mapbox.Unity.Map.Interfaces;

namespace Mapbox.Unity.MeshGeneration.Data
{
    using UnityEngine;
    using Mapbox.Unity.MeshGeneration.Enums;
    using Mapbox.Unity.Utilities;
    using Utils;
    using Mapbox.Map;
    using System;
    using Mapbox.Unity.Map;
    using System.Collections.Generic;
    using Mapbox.Unity.MeshGeneration.Factories;

    public class UnityTile : MonoBehaviour
    {
        public TileTerrainType ElevationType;

        [SerializeField]
        private Texture2D _rasterData;
        public VectorTile VectorData { get; private set; }
        private Texture2D _heightTexture;

        private Texture2D _loadingTexture;
        //keeping track of tile objects to be able to cancel them safely if tile is destroyed before data fetching finishes
        private List<Tile> _tiles = new List<Tile>();
        [SerializeField] private float _tileScale;

        public bool IsRecycled = false;

        #region CachedUnityComponents
        MeshRenderer _meshRenderer;
        public MeshRenderer MeshRenderer
        {
            get
            {
                if (_meshRenderer == null)
                {
                    _meshRenderer = GetComponent<MeshRenderer>();
                    if (_meshRenderer == null)
                    {
                        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
                    }
                }
                return _meshRenderer;
            }
        }

        private MeshFilter _meshFilter;
        public MeshFilter MeshFilter
        {
            get
            {
                if (_meshFilter == null)
                {
                    _meshFilter = GetComponent<MeshFilter>();
                    if (_meshFilter == null)
                    {
                        _meshFilter = gameObject.AddComponent<MeshFilter>();
                        _meshFilter.sharedMesh = new Mesh()
                        {
                            vertices = new Vector3[4]
                            {
                                Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,
                            },
                            
                        };
                        ElevationType = TileTerrainType.None;
                    }
                }
                return _meshFilter;
            }
        }

        private Collider _collider;
        public Collider Collider
        {
            get
            {
                if (_collider == null)
                {
                    _collider = GetComponent<Collider>();
                }
                return _collider;
            }
        }
        #endregion

        #region Tile Positon/Scale Properties
        public RectD Rect { get; private set; }
        public int InitialZoom { get; private set; }
        public int CurrentZoom { get; private set; }

        public float TileScale
        {
            get { return _tileScale; }
            private set { _tileScale = value; }
        }

        public UnwrappedTileId UnwrappedTileId { get; private set; }
        public CanonicalTileId CanonicalTileId { get; private set; }

        private float _relativeScale;
        #endregion

        [SerializeField]
        private TilePropertyState _rasterDataState;
        public TilePropertyState RasterDataState
        {
            get
            {
                return _rasterDataState;
            }
            internal set
            {
                if (_rasterDataState != value)
                {
                    _rasterDataState = value;
                    OnRasterDataChanged(this);
                }
            }
        }
        [SerializeField]
        private TilePropertyState _heightDataState;
        public TilePropertyState HeightDataState
        {
            get
            {
                return _heightDataState;
            }
            internal set
            {
                if (_heightDataState != value)
                {
                    _heightDataState = value;
                    OnHeightDataChanged(this);
                }
            }
        }
        [SerializeField]
        private TilePropertyState _vectorDataState;
        public TilePropertyState VectorDataState
        {
            get
            {
                return _vectorDataState;
            }
            internal set
            {
                if (_vectorDataState != value)
                {
                    _vectorDataState = value;
                    OnVectorDataChanged(this);
                }
            }
        }
        private TilePropertyState _tileState = TilePropertyState.None;
        public TilePropertyState TileState
        {
            get
            {
                return _tileState;
            }
            set
            {
                if (_tileState != value)
                {
                    _tileState = value;
                }
            }
        }

        public event Action<UnityTile> OnHeightDataChanged = delegate { };
        public event Action<UnityTile> OnRasterDataChanged = delegate { };
        public event Action<UnityTile> OnVectorDataChanged = delegate { };

        private bool _isInitialized = false;


        internal void Initialize(IMapReadable map, UnwrappedTileId tileId, float scale, int zoom, Texture2D loadingTexture = null)
        {
            gameObject.hideFlags = HideFlags.DontSave;

            ElevationType = TileTerrainType.None;
            TileScale = scale;
            _relativeScale = 1 / Mathf.Cos(Mathf.Deg2Rad * (float)map.CenterLatitudeLongitude.x);
            Rect = Conversions.TileBounds(tileId);
            UnwrappedTileId = tileId;
            CanonicalTileId = tileId.Canonical;
            _loadingTexture = loadingTexture;

            float scaleFactor = 1.0f;
            if (_isInitialized == false)
            {
                _isInitialized = true;
                InitialZoom = zoom;
            }
            CurrentZoom = zoom;
            scaleFactor = Mathf.Pow(2, (map.InitialZoom - zoom));
            gameObject.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            gameObject.SetActive(true);

            IsRecycled = false;


            // Setup Loading as initial state - Unregistered
            // When tile registers with factories, it will set the appropriate state.
            // None, if Factory source is None, Loading otherwise.
        }

        internal void Recycle()
        {
            if (_loadingTexture && MeshRenderer != null && MeshRenderer.sharedMaterial != null)
            {
                MeshRenderer.sharedMaterial.mainTexture = _loadingTexture;
            }

            gameObject.SetActive(false);
            IsRecycled = true;

            // Reset internal state.
            RasterDataState = TilePropertyState.Unregistered;
            HeightDataState = TilePropertyState.Unregistered;
            VectorDataState = TilePropertyState.Unregistered;
            TileState = TilePropertyState.Unregistered;

            OnHeightDataChanged = delegate { };
            OnRasterDataChanged = delegate { };
            OnVectorDataChanged = delegate { };

            Cancel();
            _tiles.Clear();
        }

        public void SetRasterData(byte[] data, bool useMipMap = true, bool useCompression = false)
        {
            // Don't leak the texture, just reuse it.
            if (RasterDataState != TilePropertyState.Unregistered)
            {
                //reset image on null data
                if (data == null)
                {
                    MeshRenderer.material.mainTexture = null;
                    return;
                }

                if (_rasterData == null)
                {
                    _rasterData = new Texture2D(1, 1, TextureFormat.RGB24, useMipMap);
                    _rasterData.wrapMode = TextureWrapMode.Clamp;
                }

                _rasterData.LoadImage(data);
                if (useCompression)
                {
                    // High quality = true seems to decrease image quality?
                    _rasterData.Compress(false);
                }

                MeshRenderer.sharedMaterial.mainTexture = _rasterData;

                RasterDataState = TilePropertyState.Loaded;
            }
        }

        public void SetLoadingTexture(Texture2D texture)
        {
            MeshRenderer.material.mainTexture = texture;
        }

        public Texture2D GetRasterData()
        {
            return _rasterData;
        }

        internal void AddTile(Tile tile)
        {
            _tiles.Add(tile);
        }

        public void ClearAssets()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                DestroyImmediate(_heightTexture, true);
                DestroyImmediate(_rasterData, true);
                DestroyImmediate(_meshFilter.sharedMesh);
                DestroyImmediate(_meshRenderer.sharedMaterial);
            }
        }

        public void Cancel()
        {
            for (int i = 0, _tilesCount = _tiles.Count; i < _tilesCount; i++)
            {
                _tiles[i].Cancel();
            }
        }

        protected virtual void OnDestroy()
        {
            Cancel();
            if (_heightTexture != null)
            {
                _heightTexture.Destroy();
            }
            if (_rasterData != null)
            {
                _rasterData.Destroy();
            }
        }
    }
}
