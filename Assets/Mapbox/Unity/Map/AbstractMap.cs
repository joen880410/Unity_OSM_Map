using Mapbox.Unity.Map.Interfaces;
using Mapbox.Unity.Map.TileProviders;

namespace Mapbox.Unity.Map
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Mapbox.Unity.Utilities;
    using Utils;
    using UnityEngine;
    using Mapbox.Map;
    using Mapbox.Unity.MeshGeneration.Factories;
    using Mapbox.Unity.MeshGeneration.Data;
    using System.Globalization;

    /// <summary>
    /// Abstract map.
    /// This is the main monobehavior which controls the map. It controls the visualization of map data.
    /// Abstract map encapsulates the image, terrain and vector sources and provides a centralized interface to control the visualization of the map.
    /// </summary>
    public class AbstractMap : MonoBehaviour, IMap
    {
        #region Private Fields

        [SerializeField] private bool _initializeOnStart = true;
        [SerializeField] protected ImageryLayer _imagery = new ImageryLayer();
        [SerializeField] protected AbstractTileProvider _tileProvider;
        [SerializeField] protected HashSet<UnwrappedTileId> _currentExtent;
        private List<UnwrappedTileId> tilesToProcess;

        protected AbstractMapVisualizer _mapVisualizer;
        protected float _unityTileSize = 1;
        protected bool _worldHeightFixed = false;
        protected int _initialZoom;
        protected Vector2d _centerLatitudeLongitude;
        protected Vector2d _centerMercator;
        protected float _worldRelativeScale;
        protected Vector3 _mapScaleFactor;

        protected Vector3 _cachedPosition;
        protected Quaternion _cachedRotation;
        protected Vector3 _cachedScale = Vector3.one;

        private TerrainFactoryBase _elevationFactory;
        #region Options

        #region MapScalingOptions

        [Tooltip("Size of each tile in Unity units.")]
        public float unityTileSize = 100f;
        #endregion

        #region LocationOptions

        [Geocode]
        [Tooltip("The coordinates to build a map around")]
        public string latitudeLongitude = "0,0";
        [Range(0, 22)]
        [Tooltip("The zoom level of the map")]
        public float zoom = 4.0f;
        #endregion

        public MapExtentOptions extentOptions = new MapExtentOptions(MapExtentType.RangeAroundCenter);
        [Tooltip("Texture used while tiles are loading.")]
        public Texture2D loadingTexture = null;
        public Material tileMaterial = null;

        #endregion

        #endregion

        #region Properties

        public AbstractMapVisualizer MapVisualizer
        {
            get
            {
                if (_mapVisualizer == null)
                {
                    _mapVisualizer = ScriptableObject.CreateInstance<MapVisualizer>();
                }
                return _mapVisualizer;
            }
            set
            {
                _mapVisualizer = value;
            }
        }

        public AbstractTileProvider TileProvider
        {
            get
            {
                return _tileProvider;
            }
            set
            {
                if (_tileProvider != null)
                {
                    _tileProvider.ExtentChanged -= OnMapExtentChanged;
                }
                _tileProvider = value;
                if (_tileProvider != null)
                {
                    _tileProvider.ExtentChanged += OnMapExtentChanged;
                }
            }
        }


        public Vector2d CenterLatitudeLongitude
        {
            get
            {
                return _centerLatitudeLongitude;
            }
        }

        public Vector2d CenterMercator
        {
            get
            {
                return _centerMercator;
            }
        }

        public float WorldRelativeScale
        {
            get
            {
                return _worldRelativeScale;
            }
        }

        public float UnityTileSize
        {
            get
            {
                return _unityTileSize;
            }
        }

        /// <summary>
        /// Gets the absolute zoom of the tiles being currently rendered.
        /// <seealso cref="Zoom"/>
        /// </summary>
        /// <value>The absolute zoom.</value>
        public int AbsoluteZoom
        {
            get
            {
                return (int)Math.Floor(zoom);
            }
        }

        /// <summary>
        /// Gets the current zoom value of the map.
        /// Use <c>AbsoluteZoom</c> to get the zoom level of the tileset.
        /// <seealso cref="AbsoluteZoom"/>
        /// </summary>
        /// <value>The zoom.</value>
        public float Zoom
        {
            get
            {
                return zoom;
            }
        }

        public void SetZoom(float zoom)
        {
            this.zoom = zoom;
        }

        /// <summary>
        /// Gets the initial zoom at which the map was initialized.
        /// This parameter is useful in calculating the scale of the tiles and the map.
        /// </summary>
        /// <value>The initial zoom.</value>
        public int InitialZoom
        {
            get
            {
                return _initialZoom;
            }
        }


        public Transform Root
        {
            get
            {
                return transform;
            }
        }

        /// <summary>
        /// Setting to trigger map initialization in Unity's Start method.
        /// if set to false, Initialize method should be called explicitly to initialize the map.
        /// </summary>
        public bool InitializeOnStart
        {
            get
            {
                return _initializeOnStart;
            }
            set
            {
                _initializeOnStart = value;
            }
        }

        public HashSet<UnwrappedTileId> CurrentExtent
        {
            get
            {
                return _currentExtent;
            }
        }

        /// <summary>
        /// Gets the loading texture used as a placeholder while the image tile is loading.
        /// </summary>
        /// <value>The loading texture.</value>
        public Texture2D LoadingTexture
        {
            get
            {
                return loadingTexture;
            }
        }

        /// <summary>
        /// Gets the tile material used for map tiles.
        /// </summary>
        /// <value>The tile material.</value>
        public Material TileMaterial
        {
            get
            {
                return tileMaterial;
            }
        }

        public Type ExtentCalculatorType
        {
            get
            {
                return TileProvider.GetType();
            }
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Initialize the map using the specified latLon and zoom.
        /// Map will automatically get initialized in the <c>Start</c> method.
        /// Use this method to explicitly initialize the map and disable intialize on <c>Start</c>
        /// </summary>
        /// <returns>The initialize.</returns>
        /// <param name="latLon">Lat lon.</param>
        /// <param name="zoom">Zoom.</param>
        public virtual void Initialize(Vector2d latLon, int zoom)
        {
            _initializeOnStart = false;
            this.latitudeLongitude = string.Format(CultureInfo.InvariantCulture, "{0},{1}", latLon.x, latLon.y);
            this.zoom = zoom;

            SetUpMap();
        }

        protected virtual void Update()
        {
            if (TileProvider != null)
            {
                TileProvider.UpdateTileProvider();
            }
        }

        public void UpdateMap()
        {
            UpdateMap(Conversions.StringToLatLon(latitudeLongitude), Zoom);
        }

        public void UpdateMap(Vector2d latLon)
        {
            UpdateMap(latLon, Zoom);
        }

        public void UpdateMap(float zoom)
        {
            UpdateMap(Conversions.StringToLatLon(latitudeLongitude), zoom);
        }

        /// <summary>
        /// Updates the map.
        /// Use this method to update the location of the map.
        /// Update method should be used when panning, zooming or changing location of the map.
        /// This method avoid startup delays that might occur on re-initializing the map.
        /// </summary>
        /// <param name="latLon">LatitudeLongitude.</param>
        /// <param name="zoom">Zoom level.</param>
        public void UpdateMap(Vector2d latLon, float zoom)
        {
            //so map will be snapped to zero using next new tile loaded
            _worldHeightFixed = false;
            // Update map zoom, if it has changed.
            if (Math.Abs(Zoom - zoom) > Constants.EpsilonFloatingPoint)
            {
                SetZoom(zoom);
            }

            // Compute difference in zoom. Will be used to calculate correct scale of the map.
            float differenceInZoom = Zoom - InitialZoom;
            bool isAtInitialZoom = (differenceInZoom - 0.0 < Constants.EpsilonFloatingPoint);

            //Update center latitude longitude
            var centerLatitudeLongitude = latLon;
            double xDelta = centerLatitudeLongitude.x;
            double zDelta = centerLatitudeLongitude.y;

            xDelta = xDelta > 0 ? Mathd.Min(xDelta, Constants.LatitudeMax) : Mathd.Max(xDelta, -Constants.LatitudeMax);
            zDelta = zDelta > 0 ? Mathd.Min(zDelta, Constants.LongitudeMax) : Mathd.Max(zDelta, -Constants.LongitudeMax);

            //Set Center in Latitude Longitude and Mercator.
            SetCenterLatitudeLongitude(new Vector2d(xDelta, zDelta));
            SetUpScaling();
            SetCenterMercator(Conversions.LatLonToMeters(CenterLatitudeLongitude));
            //Scale the map accordingly.
            if (Math.Abs(differenceInZoom) > Constants.EpsilonFloatingPoint || isAtInitialZoom)
            {
                _mapScaleFactor = Vector3.one * Mathf.Pow(2, differenceInZoom);
                Root.localScale = _mapScaleFactor;
            }

            //Update Tile extent.
            if (TileProvider != null)
            {
                TileProvider.UpdateTileExtent();
            }

            if (OnUpdated != null)
            {
                OnUpdated();
            }
        }

        private void Reset()
        {
            DisableEditorPreview();
        }

        #endregion

        #region Private/Protected Methods

        private void OnEnable()
        {
            tilesToProcess = new List<UnwrappedTileId>();
            if (tileMaterial == null)
            {
                tileMaterial = new Material(Shader.Find("Standard"));
            }

            if (loadingTexture == null)
            {
                loadingTexture = new Texture2D(1, 1);
            }
        }

        public void OnDestroy()
        {
            if (TileProvider != null)
            {
                TileProvider.ExtentChanged -= OnMapExtentChanged;
            }
            _mapVisualizer.ClearMap();
            _mapVisualizer.Destroy();
        }

        public void Awake()
        {
            MapOnAwakeRoutine();

        }
        public void Start()
        {
            MapOnStartRoutine();
        }

        private void MapOnAwakeRoutine()
        {
            // Destroy any ghost game objects.
            DestroyChildObjects();
            // Setup a visualizer to get a "Starter" map.

            if (_mapVisualizer == null)
            {
                _mapVisualizer = ScriptableObject.CreateInstance<MapVisualizer>();
            }
            _mapVisualizer.OnTileFinished += (s) =>
            {
                OnTileFinished(s);
            };
        }

        public void DestroyChildObjects()
        {
            int destroyChildStartIndex = transform.childCount - 1;
            for (int i = destroyChildStartIndex; i >= 0; i--)
            {
                transform.GetChild(i).gameObject.Destroy();
            }
        }

        private void MapOnStartRoutine()
        {
            if (_initializeOnStart)
            {
                SetUpMap();
            }
        }


        public void DisableEditorPreview()
        {
            if (_mapVisualizer != null)
            {
                _mapVisualizer.ClearMap();
            }
            DestroyTileProvider();

            OnEditorPreviewDisabled?.Invoke();

            transform.SetPositionAndRotation(_cachedPosition, _cachedRotation);
            transform.localScale = _cachedScale;
        }

        public void DestroyTileProvider()
        {
            var tileProvider = TileProvider ?? gameObject.GetComponent<AbstractTileProvider>();
            if (extentOptions.extentType != MapExtentType.Custom && tileProvider != null)
            {
                tileProvider.gameObject.Destroy();
                _tileProvider = null;
            }
        }

        /// <summary>
        /// Sets up map.
        /// This method uses the mapOptions and layer properties to setup the map to be rendered.
        /// Override <c>SetUpMap</c> to write custom behavior to map setup.
        /// </summary>
        protected virtual void SetUpMap()
        {
            _elevationFactory = ScriptableObject.CreateInstance<TerrainFactoryBase>();
            SetTileProvider();
            _imagery.Initialize();
            _mapVisualizer.Factories = new List<AbstractTileFactory>
            {
                _elevationFactory,
                _imagery.Factory,
            };

            InitializeMap();
        }

        protected virtual void TileProvider_OnTileAdded(UnwrappedTileId tileId)
        {
            _mapVisualizer.LoadTile(tileId);
        }

        protected virtual void TileProvider_OnTileRemoved(UnwrappedTileId tileId)
        {
            _mapVisualizer.DisposeTile(tileId);
        }

        protected virtual void TileProvider_OnTileRepositioned(UnwrappedTileId tileId)
        {
            _mapVisualizer.RepositionTile(tileId);
        }

        protected void SendInitialized()
        {
            OnInitialized();
        }


        /// <summary>
        /// Initializes the map using the mapOptions.
        /// </summary>
        /// <param name="options">Options.</param>
        protected virtual void InitializeMap()
        {
            _worldHeightFixed = false;
            _centerLatitudeLongitude = Conversions.StringToLatLon(latitudeLongitude);
            _initialZoom = (int)zoom;
            SetUpScaling();
            //Set up events for changes.
            _mapVisualizer.Initialize(this);
            TileProvider.Initialize(this);
            SendInitialized();
            TileProvider.UpdateTileExtent();
            PropertyHasChanged();
        }

        private void SetTileProvider()
        {
            ITileProviderOptions tileProviderOptions = extentOptions.GetTileProviderOptions();
            string tileProviderName = "TileProvider";
            // Setup tileprovider based on type.
            if (TileProvider != null)
            {
                if (!(TileProvider is QuadTreeTileProvider))
                {
                    TileProvider.gameObject.Destroy();
                    var go = new GameObject(tileProviderName);
                    go.transform.parent = transform;
                    TileProvider = go.AddComponent<QuadTreeTileProvider>();
                }
            }
            else
            {
                var go = new GameObject(tileProviderName);
                go.transform.parent = transform;
                TileProvider = go.AddComponent<QuadTreeTileProvider>();
            }

            TileProvider.SetOptions(tileProviderOptions);

        }

        private void TriggerTileRedrawForExtent(ExtentArgs currentExtent)
        {
            var _activeTiles = _mapVisualizer.ActiveTiles;
            _currentExtent = new HashSet<UnwrappedTileId>(currentExtent.activeTiles);

            if (tilesToProcess == null)
            {
                tilesToProcess = new List<UnwrappedTileId>();
            }
            else
            {
                tilesToProcess.Clear();
            }
            foreach (var item in _activeTiles)
            {
                if (TileProvider.Cleanup(item.Key))
                {
                    tilesToProcess.Add(item.Key);
                }
            }

            if (tilesToProcess.Count > 0)
            {
                OnTilesDisposing(tilesToProcess);

                foreach (var t2r in tilesToProcess)
                {
                    TileProvider_OnTileRemoved(t2r);
                }
            }

            foreach (var tile in _activeTiles)
            {
                // Reposition tiles in case we panned.
                TileProvider_OnTileRepositioned(tile.Key);
            }

            tilesToProcess.Clear();
            foreach (var tile in _currentExtent)
            {
                if (!_activeTiles.ContainsKey(tile))
                {
                    tilesToProcess.Add(tile);
                }
            }

            if (tilesToProcess.Count > 0)
            {
                OnTilesStarting(tilesToProcess);
                foreach (var tileId in tilesToProcess)
                {
                    _mapVisualizer.State = ModuleState.Working;
                    TileProvider_OnTileAdded(tileId);
                }
            }
        }

        private void OnMapExtentChanged(object sender, ExtentArgs currentExtent)
        {
            TriggerTileRedrawForExtent(currentExtent);
        }

        private void OnTileProviderChanged()
        {

            SetTileProvider();
            TileProvider.Initialize(this);
            TileProvider.UpdateTileExtent();
        }

        #endregion

        #region Conversion and Height Query Methods
        private Vector3 GeoToWorldPositionXZ(Vector2d latitudeLongitude)
        {
            // For quadtree implementation of the map, the map scale needs to be compensated for.
            var scaleFactor = Mathf.Pow(2, (InitialZoom - AbsoluteZoom));
            var worldPos = Conversions.GeoToWorldPosition(latitudeLongitude, CenterMercator, WorldRelativeScale * scaleFactor).ToVector3xz();
            return Root.TransformPoint(worldPos);
        }

        protected virtual float QueryElevationAtInternal(Vector2d latlong, out float tileScale)
        {
            var _meters = Conversions.LatLonToMeters(latlong.x, latlong.y);
            UnityTile tile;
            bool foundTile = MapVisualizer.ActiveTiles.TryGetValue(Conversions.LatitudeLongitudeToTileId(latlong.x, latlong.y, (int)Zoom), out tile);
            if (foundTile)
            {
                tileScale = tile.TileScale;
                var _rect = tile.Rect;
                return tile.QueryHeightData((float)((_meters - _rect.Min).x / _rect.Size.x), (float)((_meters.y - _rect.Max.y) / _rect.Size.y));
            }
            else
            {
                tileScale = 1f;
                return 0f;
            }

        }

        /// <summary>
        /// Converts a latitude longitude into map space position.
        /// </summary>
        /// <returns>Position in map space.</returns>
        /// <param name="latitudeLongitude">Latitude longitude.</param>
        /// <param name="queryHeight">If set to <c>true</c> will return the terrain height(in Unity units) at that point.</param>
        public virtual Vector3 GeoToWorldPosition(Vector2d latitudeLongitude, bool queryHeight = true)
        {
            Vector3 worldPos = GeoToWorldPositionXZ(latitudeLongitude);

            if (queryHeight)
            {
                //Query Height.
                float tileScale = 1f;
                float height = QueryElevationAtInternal(latitudeLongitude, out tileScale);

                // Apply height inside the unity tile space
                UnityTile tile;
                if (MapVisualizer.ActiveTiles.TryGetValue(Conversions.LatitudeLongitudeToTileId(latitudeLongitude.x, latitudeLongitude.y, (int)Zoom), out tile))
                {
                    if (tile != null)
                    {
                        // Calculate height in the local space of the tile gameObject.
                        // Height is aligned with the y axis in local space.
                        // This also helps us avoid scale values when setting the height.
                        var localPos = tile.gameObject.transform.InverseTransformPoint(worldPos);
                        localPos.y = height;
                        worldPos = tile.gameObject.transform.TransformPoint(localPos);
                    }
                }
            }

            return worldPos;
        }

        /// <summary>
        /// Converts a position in map space into a laitude longitude.
        /// </summary>
        /// <returns>Position in Latitude longitude.</returns>
        /// <param name="realworldPoint">Realworld point.</param>
        public virtual Vector2d WorldToGeoPosition(Vector3 realworldPoint)
        {
            // For quadtree implementation of the map, the map scale needs to be compensated for.
            var scaleFactor = Mathf.Pow(2, (InitialZoom - AbsoluteZoom));

            return (Root.InverseTransformPoint(realworldPoint)).GetGeoPosition(CenterMercator, WorldRelativeScale * scaleFactor);
        }

        /// <summary>
        /// Queries the real world elevation data in Unity units at a given latitude longitude.
        /// </summary>
        /// <returns>The height data.</returns>
        /// <param name="latlong">Latlong.</param>
        public virtual float QueryElevationInUnityUnitsAt(Vector2d latlong)
        {
            float tileScale = 1f;
            return QueryElevationAtInternal(latlong, out tileScale);
        }

        /// <summary>
        /// Queries the real world elevation data in Meters at a given latitude longitude.
        /// </summary>
        /// <returns>The height data.</returns>
        /// <param name="latlong">Latlong.</param>
        public virtual float QueryElevationInMetersAt(Vector2d latlong)
        {
            float tileScale = 1f;
            float height = QueryElevationAtInternal(latlong, out tileScale);
            return (height / tileScale);
        }
        #endregion

        #region Map Property Related Changes Methods
        public virtual void SetCenterMercator(Vector2d centerMercator)
        {
            _centerMercator = centerMercator;
        }

        public virtual void SetCenterLatitudeLongitude(Vector2d centerLatitudeLongitude)
        {
            latitudeLongitude = string.Format("{0}, {1}", centerLatitudeLongitude.x, centerLatitudeLongitude.y);
            _centerLatitudeLongitude = centerLatitudeLongitude;
        }

        public virtual void SetWorldRelativeScale(float scale)
        {
            _worldRelativeScale = scale;
        }

        public virtual void SetLoadingTexture(Texture2D loadingTexture)
        {
            this.loadingTexture = loadingTexture;
        }

        public virtual void SetTileMaterial(Material tileMaterial)
        {
            this.tileMaterial = tileMaterial;
        }

        #endregion

        #region Events
        /// <summary>
        /// Event delegate, gets called after map is initialized
        /// <seealso cref="OnUpdated"/>
        /// </summary>
        public event Action OnInitialized = delegate { };
        /// <summary>
        /// Event delegate, gets called after map is updated.
        /// <c>UpdateMap</c> will trigger this event.
        /// <seealso cref="OnInitialized"/>
        /// </summary>
        public event Action OnUpdated = delegate { };
        public event Action OnMapRedrawn = delegate { };

        /// <summary>
        /// Event delegate, gets called when map preview is enabled
        /// </summary>
        public event Action OnEditorPreviewEnabled = delegate { };
        /// <summary>
        /// Event delegate, gets called when map preview is disabled
        /// </summary>
        public event Action OnEditorPreviewDisabled = delegate { };
        /// <summary>
        /// Event delegate, gets called when a tile is completed.
        /// </summary>
        public event Action<UnityTile> OnTileFinished = delegate { };
        /// <summary>
        /// Event delegate, gets called when new tiles coordinates are registered.
        /// </summary>
        public event Action<List<UnwrappedTileId>> OnTilesStarting = delegate { };
        /// <summary>
        /// Event delegate, gets called before a tile is getting recycled.
        /// </summary>
        public event Action<List<UnwrappedTileId>> OnTilesDisposing = delegate { };
        #endregion

        #region options
        public void SetUpScaling()
        {
            var referenceTileRect = Conversions.TileBounds(TileCover.CoordinateToTileId(CenterLatitudeLongitude, AbsoluteZoom));
            SetWorldRelativeScale((float)(unityTileSize / referenceTileRect.Size.x));
        }
        public void PropertyHasChanged()
        {
            UpdateMap();
            OnTileProviderChanged();
            if (TileProvider != null)
            {
                TileProvider.UpdateTileExtent();
            }
        }
        #endregion
    }
}
