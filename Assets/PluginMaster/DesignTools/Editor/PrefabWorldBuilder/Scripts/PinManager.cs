/*
Copyright (c) 2021 Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Writen by Omar Duarte, 2021.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System.Linq;
using UnityEngine;

namespace PluginMaster
{
    #region DATA & SETTINGS
    [System.Serializable]
    public class TerrainFlatteningSettings
    {
        [SerializeField] private float _hardness = 0f;
        [SerializeField] private float _padding = 0f;
        [SerializeField] private bool _clearTrees = true;
        [SerializeField] private bool _clearDetails = true;
        private Vector2 _coreSize = Vector2.one;
        private Vector2 _density = Vector2.zero;
        private float _angle = 0;
        private bool _updateHeightmap = true;
        private float[,] _heightmap = null;

        public TerrainFlatteningSettings() { }

        public float hardness
        {
            get => _hardness;
            set
            {
                if (_hardness == value) return;
                _hardness = value;
                _updateHeightmap = true;
                PWBCore.SetSavePending();
            }
        }
        public float padding
        {
            get => _padding;
            set
            {
                value = Mathf.Max(value, 0);
                if (_padding == value) return;
                _padding = value;
                _updateHeightmap = true;
                PWBCore.SetSavePending();
            }
        }
        public bool clearTrees
        {
            get => _clearTrees;
            set
            {
                if (_clearTrees == value) return;
                _clearTrees = value;
                PWBCore.SetSavePending();
            }
        }
        public bool clearDetails
        {
            get => _clearDetails;
            set
            {
                if (_clearDetails == value) return;
                _clearDetails = value;
                PWBCore.SetSavePending();
            }
        }
        public Vector2 size
        {
            get => _coreSize;
            set
            {
                if (_coreSize == value) return;
                _coreSize = value;
                _updateHeightmap = true;
            }
        }
        public Vector2 density
        {
            set
            {
                if (_density == value) return;
                _density = value;
                _updateHeightmap = true;
            }
        }
        public float angle
        {
            get => _angle;
            set
            {
                if (_angle == value) return;
                _angle = value;
                _updateHeightmap = true;
            }
        }
        public float[,] heightmap
        {
            get
            {
                if (_updateHeightmap || _heightmap == null) UpdateHeightmap();
                return _heightmap;
            }
        }

        private void UpdateHeightmap()
        {
            _updateHeightmap = false;
            var cornerSize = (_coreSize.x + _coreSize.y) / 2 * (1 - _hardness);
            if (_hardness < 0.8) cornerSize = Mathf.Max(cornerSize, 10f / Mathf.Min(_density.x, _density.y));
            var coreWithPaddingSize = _coreSize + Vector2.one * _padding * 2;
            var size = coreWithPaddingSize + Vector2.one * (cornerSize * 2);
            var unrotatedSize = new Vector2Int(Mathf.RoundToInt(size.x * _density.x),
                Mathf.RoundToInt(size.y * _density.y));

            var coreMapSize = new Vector2Int(Mathf.RoundToInt(coreWithPaddingSize.x * _density.x),
                Mathf.RoundToInt(coreWithPaddingSize.y * _density.y));
            var cornerMapSize = new Vector2Int(Mathf.RoundToInt(_density.x * cornerSize),
                Mathf.RoundToInt(_density.y * cornerSize));
            var unrotatedMap = new float[unrotatedSize.x, unrotatedSize.y];

            var minCore = new Vector2Int(Mathf.Max(cornerMapSize.x - 1, 0), Mathf.Max(cornerMapSize.y - 1, 0));
            var maxCoreI = Mathf.Min(coreMapSize.y + cornerMapSize.y + 1, unrotatedSize.y);
            var maxCoreJ = Mathf.Min(coreMapSize.x + cornerMapSize.x + 1, unrotatedSize.x);
            for (int i = minCore.y; i < maxCoreI; ++i)
                for (int j = minCore.x; j < maxCoreJ; ++j)
                    unrotatedMap[j, i] = 1f;

            float ParametricBlend(float t)
            {
                if (t > 1) return 1;
                if (t < 0) return 0;
                float tSquared = t * t;
                return tSquared / (2.0f * (tSquared - t) + 1.0f);
            }


            for (int i = 0; i < cornerMapSize.x; ++i)
            {
                var i1 = unrotatedSize.x - 1 - i;
                var iDistance = (cornerMapSize.x - i) / _density.x;
                for (int j = 0; j < cornerMapSize.y; ++j)
                {
                    var j1 = unrotatedSize.y - 1 - j;
                    var jDistance = (cornerMapSize.y - j) / _density.y;
                    var distance = 1 - Mathf.Sqrt(iDistance * iDistance + jDistance * jDistance) / cornerSize;
                    var h = ParametricBlend(distance);
                    unrotatedMap[i, j] = unrotatedMap[i1, j] = unrotatedMap[i, j1] = unrotatedMap[i1, j1] = h;

                }
                var distanceNorm = 1 - (float)iDistance / cornerSize;
                var iH = ParametricBlend(distanceNorm);
                for (int j = minCore.y; j < maxCoreI; ++j) unrotatedMap[i, j] = unrotatedMap[i1, j] = iH;
            }
            for (int j = 0; j < cornerMapSize.y; ++j)
            {
                var j1 = unrotatedSize.y - 1 - j;
                var jDistance = (cornerMapSize.y - j) / _density.y;
                var distanceNorm = 1 - (float)jDistance / cornerSize;
                var jH = ParametricBlend(distanceNorm);
                for (int i = minCore.x; i < maxCoreJ; ++i) unrotatedMap[i, j] = unrotatedMap[i, j1] = jH;
            }

            if (_angle == 0)
            {
                _heightmap = unrotatedMap;
                return;
            }

            var angleRad = _angle * Mathf.Deg2Rad;
            var cos = Mathf.Cos(angleRad);
            var sin = Mathf.Sin(angleRad);
            var aspect = _density.x / _density.y;
            Vector2Int RotatePoint(Vector2 centerToPoint)
            {
                if (_angle == 0) return new Vector2Int(Mathf.RoundToInt(centerToPoint.x), Mathf.RoundToInt(centerToPoint.y));
                var result = Vector2Int.zero;
                centerToPoint.y = centerToPoint.y * aspect;
                result.x = Mathf.RoundToInt((centerToPoint.x * cos - centerToPoint.y * sin));
                result.y = Mathf.RoundToInt((centerToPoint.x * sin + centerToPoint.y * cos) / aspect);
                return result;
            }
            var centerToCorner1 = new Vector2Int(Mathf.CeilToInt(unrotatedSize.x / 2f), Mathf.CeilToInt(unrotatedSize.y / 2f));
            var rotatedCorner1 = RotatePoint(centerToCorner1);
            rotatedCorner1 = new Vector2Int(Mathf.Abs(rotatedCorner1.x), Mathf.Abs(rotatedCorner1.y));
            var centerToCorner2 = new Vector2Int(-Mathf.CeilToInt(unrotatedSize.x / 2f),
                Mathf.CeilToInt(unrotatedSize.y / 2f));
            var rotatedCorner2 = RotatePoint(centerToCorner2);
            rotatedCorner2 = new Vector2Int(Mathf.Abs(rotatedCorner2.x), Mathf.Abs(rotatedCorner2.y));
            var rotatedCorner = Vector2Int.Max(rotatedCorner1, rotatedCorner2);

            var rotationPadding = Vector2Int.Max(rotatedCorner - centerToCorner1, Vector2Int.zero);

            var rotatedHeightmapSize = unrotatedSize + rotationPadding * 2;
            _heightmap = new float[rotatedHeightmapSize.x, rotatedHeightmapSize.y];


            Vector2Int ClampPoint(Vector2Int point) => new Vector2Int(Mathf.Clamp(point.x, 0, rotatedHeightmapSize.x - 1),
                    Mathf.Clamp(point.y, 0, rotatedHeightmapSize.y - 1));

            void SetHeight(Vector2Int point, float value)
            {
                var clampPoint = ClampPoint(point);
                _heightmap[clampPoint.x, clampPoint.y] = value;
                var points = new Vector2Int[] { ClampPoint(point + Vector2Int.up), ClampPoint(point + Vector2Int.down),
                    ClampPoint(point + Vector2Int.left), ClampPoint(point + Vector2Int.right)};
                foreach (var p in points)
                    _heightmap[p.x, p.y] = _heightmap[p.x, p.y] < 0.0001 ? value : (_heightmap[p.x, p.y] * 6 + value) / 7;
            }

            var unrotatedCenter = new Vector2Int(Mathf.FloorToInt(unrotatedSize.x / 2f),
                Mathf.FloorToInt(unrotatedSize.y / 2f));
            var center = new Vector2Int(Mathf.FloorToInt(rotatedHeightmapSize.x / 2f),
                Mathf.FloorToInt(rotatedHeightmapSize.y / 2f));
            for (int i = 0; i < unrotatedSize.y; ++i)
            {
                for (int j = 0; j < unrotatedSize.x; ++j)
                {
                    var h = unrotatedMap[j, i];
                    var point = new Vector2(j, i);
                    var centerToPoint = point - unrotatedCenter;
                    var rotatedPoint = RotatePoint(centerToPoint) + center;
                    SetHeight(rotatedPoint, h);
                }
            }

            var smoothMap = new float[rotatedHeightmapSize.x, rotatedHeightmapSize.y];
            for (int i = 0; i < rotatedHeightmapSize.x; ++i)
            {
                for (int j = 0; j < rotatedHeightmapSize.y; ++j)
                {
                    var count = 0;
                    var sum = 0f;
                    var corners = new float[] { i == 0 || j == 0 ? 0 : _heightmap[i-1, j-1],
                        i == rotatedHeightmapSize.x-1 || j == 0? 0 :_heightmap[i+1, j -1],
                        i == 0 || j == rotatedHeightmapSize.y-1 ? 0 :_heightmap[i-1, j+1],
                        i == rotatedHeightmapSize.x-1 || j == rotatedHeightmapSize.y-1 ? 0 : _heightmap[i+1, j+1] };
                    for (int n = 0; n < 4; ++n)
                    {
                        if (corners[n] < 0.0001) continue;
                        ++count;
                        sum += corners[n];
                    }
                    var neighbors = new float[] { i == 0 ? 0 : _heightmap[i - 1, j],
                        i == rotatedHeightmapSize.x -1 ? 0 :_heightmap[i + 1, j],
                        j == 0 ? 0 :_heightmap[i, j - 1], j == rotatedHeightmapSize.y -1 ? 0 : _heightmap[i, j + 1] };
                    for (int n = 0; n < 4; ++n)
                    {
                        if (neighbors[n] < 0.0001) continue;
                        count += 2;
                        sum += neighbors[n] * 2;
                    }
                    if (count == 0)
                    {
                        smoothMap[i, j] = _heightmap[i, j];
                        continue;
                    }
                    if (!(_heightmap[i, j] < 0.0001 && ((neighbors[0] > 0.0001 && neighbors[1] > 0.0001)
                        || (neighbors[2] > 0.0001 && neighbors[3] > 0.0001))))
                    {
                        sum += _heightmap[i, j] * 3;
                        count += 3;
                    }
                    var avg = sum / count;
                    smoothMap[i, j] = avg;
                }
            }
            _heightmap = smoothMap;
        }
    }

    [System.Serializable]
    public class PinSettings : PaintOnSurfaceToolSettings, IPaintToolSettings
    {
        [SerializeField] private bool _repeat = false;
        public bool repeat
        {
            get => _repeat;
            set
            {
                if (_repeat == value) return;
                _repeat = value;
                OnDataChanged();
            }
        }
        [SerializeField] private TerrainFlatteningSettings _flatteningSettings = new TerrainFlatteningSettings();
        public TerrainFlatteningSettings flatteningSettings => _flatteningSettings;
        [SerializeField] private bool _flattenTerrain = false;
        public bool flattenTerrain
        {
            get => _flattenTerrain;
            set
            {
                if (_flattenTerrain == value) return;
                _flattenTerrain = value;
                PWBCore.SetSavePending();
            }
        }

        [SerializeField] private PaintToolSettings _paintTool = new PaintToolSettings();
        public Transform parent { get => _paintTool.parent; set => _paintTool.parent = value; }
        public bool overwritePrefabLayer
        {
            get => _paintTool.overwritePrefabLayer;
            set => _paintTool.overwritePrefabLayer = value;
        }
        public int layer { get => _paintTool.layer; set => _paintTool.layer = value; }
        public bool autoCreateParent { get => _paintTool.autoCreateParent; set => _paintTool.autoCreateParent = value; }
        public bool createSubparentPerPalette
        {
            get => _paintTool.createSubparentPerPalette;
            set => _paintTool.createSubparentPerPalette = value;
        }
        public bool createSubparentPerTool
        {
            get => _paintTool.createSubparentPerTool;
            set => _paintTool.createSubparentPerTool = value;
        }
        public bool createSubparentPerBrush
        {
            get => _paintTool.createSubparentPerBrush;
            set => _paintTool.createSubparentPerBrush = value;
        }
        public bool createSubparentPerPrefab
        {
            get => _paintTool.createSubparentPerPrefab;
            set => _paintTool.createSubparentPerPrefab = value;
        }
        public bool overwriteBrushProperties
        {
            get => _paintTool.overwriteBrushProperties;
            set => _paintTool.overwriteBrushProperties = value;
        }
        public BrushSettings brushSettings => _paintTool.brushSettings;

        public PinSettings() : base() => _paintTool.OnDataChanged += DataChanged;

        public override void Copy(IToolSettings other)
        {
            var otherPinSettings = other as PinSettings;
            if (otherPinSettings == null) return;
            base.Copy(other);
            _repeat = otherPinSettings._repeat;
            _paintTool.Copy(otherPinSettings._paintTool);
            _flattenTerrain = otherPinSettings._flattenTerrain;
        }
        public override void DataChanged()
        {
            base.DataChanged();
            BrushstrokeManager.UpdateBrushstroke();
        }
    }

    [System.Serializable]
    public class PinManager : ToolManagerBase<PinSettings>
    {
        private static float _rotationSnapValueStatic = 5f;
        [SerializeField] private float _rotationSnapValue = _rotationSnapValueStatic;

        public static float rotationSnapValue
        {
            get => _rotationSnapValueStatic;
            set
            {
                if (_rotationSnapValueStatic == value) return;
                _rotationSnapValueStatic = value;
                PWBCore.staticData.Save();
            }
        }

        public override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
            _rotationSnapValue = _rotationSnapValueStatic;
        }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            _rotationSnapValueStatic = _rotationSnapValue;
        }
    }
    #endregion

    #region PWBIO
    public static partial class PWBIO
    {
        private static bool _pinned = false;
        private static Vector3 _pinMouse = Vector3.zero;
        private static RaycastHit _pinHit = new RaycastHit();
        private static Vector3 _pinAngle = Vector3.zero;
        private static Vector3 _previousPinAngle = Vector3.zero;
        private static float _pinScale = 1f;
        private static Vector3 _pinOffset = Vector3.zero;
        private static System.Collections.Generic.List<System.Collections.Generic.List<Vector3>> _initialPinBoundPoints
            = new System.Collections.Generic.List<System.Collections.Generic.List<Vector3>>();
        private static System.Collections.Generic.List<System.Collections.Generic.List<Vector3>> _pinBoundPoints
            = new System.Collections.Generic.List<System.Collections.Generic.List<Vector3>>();
        private static int _pinBoundPointIdx = 0;
        private static int _pinBoundLayerIdx = 0;
        private static bool _snapToVertex = false;
        private static float _pinDistanceFromSurface = 0f;

        private static Vector2 _flatteningSize = Vector2.zero;
        private static bool _globalFlattening = false;
        private static Vector3 _flatteningCenter = Vector3.zero;
        private static void UpdatePinScale(float value)
        {
            if (_pinScale == value) return;
            _pinScale = value;
            for (int l = 0; l < _pinBoundPoints.Count; ++l)
                for (int p = 0; p < _pinBoundPoints[l].Count; ++p)
                    _pinBoundPoints[l][p] = _initialPinBoundPoints[l][p] * _pinScale;
            _pinOffset = _pinBoundPoints[_pinBoundLayerIdx][_pinBoundPointIdx];
        }

        private static Vector3 pivotBoundPoint
        {
            get
            {
                _pinBoundPointIdx = 0;
                _pinBoundLayerIdx = 0;
                return _pinBoundPoints[_pinBoundLayerIdx][_pinBoundPointIdx];
            }
        }
        private static Vector3 nextBoundPoint
        {
            get
            {
                ++_pinBoundPointIdx;
                if (_pinBoundPointIdx >= _pinBoundPoints[_pinBoundLayerIdx].Count) _pinBoundPointIdx = 0;
                return _pinBoundPoints[_pinBoundLayerIdx][_pinBoundPointIdx];
            }
        }

        private static Vector3 nextBoundLayer
        {
            get
            {
                ++_pinBoundLayerIdx;
                if (_pinBoundLayerIdx >= _pinBoundPoints.Count) _pinBoundLayerIdx = 0;
                if (_pinBoundLayerIdx == 1) _pinBoundPointIdx = Mathf.Max(_pinBoundPointIdx - 1, 0);
                return _pinBoundPoints[_pinBoundLayerIdx][_pinBoundPointIdx];
            }
        }

        private static Vector3 prevBoundLayer
        {
            get
            {
                --_pinBoundLayerIdx;
                if (_pinBoundLayerIdx < 0) _pinBoundLayerIdx = _pinBoundPoints.Count - 1;
                if (_pinBoundLayerIdx == 1) _pinBoundPointIdx = Mathf.Max(_pinBoundPointIdx - 1, 0);
                return _pinBoundPoints[_pinBoundLayerIdx][_pinBoundPointIdx];
            }
        }

        private static void SetPinPivotLayer()
        {
            _pinBoundLayerIdx = 0;
            _pinOffset = _pinBoundPoints[_pinBoundLayerIdx][_pinBoundPointIdx];
        }
        private static void SetPinBottomLayer()
        {
            _pinBoundLayerIdx = 1;
            _pinBoundPointIdx = Mathf.Max(_pinBoundPointIdx - 1, 0);
            _pinOffset = _pinBoundPoints[_pinBoundLayerIdx][_pinBoundPointIdx];
        }
        public static void ResetPinValues()
        {
            _pinned = false;
            _pinMouse = Vector3.zero;
            _pinHit = new RaycastHit();
            _pinAngle = Vector3.zero;
            _pinScale = 1f;
            _pinBoundLayerIdx = 0;
            _pinDistanceFromSurface = 0f;
            if (BrushstrokeManager.brushstroke.Length == 0) return;
            var strokeItem = BrushstrokeManager.brushstroke[0];
            var prefab = strokeItem.settings.prefab;
            if (prefab == null) return;
            var isSprite = prefab.GetComponentsInChildren<SpriteRenderer>()
                .Where(r => r.enabled && r.sprite != null && r.gameObject.activeSelf).ToArray().Length > 0;

            var bounds = BoundsUtils.GetBoundsRecursive(prefab.transform, Quaternion.identity);
            var halfSize = bounds.size * 0.5f;
            _pinBoundPoints.Clear();
            _initialPinBoundPoints.Clear();
            _pinBoundPoints.Add(new System.Collections.Generic.List<Vector3>() { });
            _initialPinBoundPoints.Add(new System.Collections.Generic.List<Vector3>() { });
            var centerToPivot = prefab.transform.position - bounds.center;
            var centerToPivotOnPlane = new Vector3(centerToPivot.x, 0f, centerToPivot.z);

            _pinBoundPoints[0].Add(centerToPivotOnPlane);
            _initialPinBoundPoints[0].Add(centerToPivotOnPlane);
            _pinBoundPointIdx = 0;

            bool addPivotPoint = true;
            int l = 0;
            int zSign = 1;
            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    var boxPoint = isSprite ? new Vector3(x, z * zSign, 0f) : new Vector3(x, 0f, z * zSign);
                    var point = Vector3.Scale(boxPoint, halfSize) + centerToPivotOnPlane;
                    if (point == Vector3.zero)
                    {
                        addPivotPoint = false;
                        _pinBoundPointIdx = _pinBoundPoints[l].Count;
                    }
                    _pinBoundPoints[l].Add(point);
                    _initialPinBoundPoints[l].Add(point);
                }
                zSign = -zSign;
            }
            if (addPivotPoint)
            {
                var pivotTocenter = isSprite ? new Vector3(0, -centerToPivot.y, 0) : Vector3.zero;
                _pinBoundPoints[l].Insert(0, pivotTocenter);
                _initialPinBoundPoints[l].Insert(0, pivotTocenter);
            }
            l = 1;
            for (int y = -1; y <= 1; y += 2)
            {
                var newY = -y * halfSize.y + centerToPivot.y;
                if (Mathf.Abs(newY) < 0.01) continue;
                _pinBoundPoints.Add(new System.Collections.Generic.List<Vector3>());
                _initialPinBoundPoints.Add(new System.Collections.Generic.List<Vector3>());
                var center = new Vector3(0f, -y * halfSize.y, 0f) + centerToPivot;

                _pinBoundPoints[l].Add(center);
                _initialPinBoundPoints[l].Add(center);
                zSign = 1;
                for (int x = -1; x <= 1; x += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        var point = Vector3.Scale(!isSprite
                            ? new Vector3(x, -y, z * zSign)
                            : new Vector3(x, z * zSign, -y), halfSize) + centerToPivot;
                        _pinBoundPoints[l].Add(point);
                        _initialPinBoundPoints[l].Add(point);
                    }
                    zSign = -zSign;
                }
                ++l;
            }
            BrushSettings brushSettings = strokeItem.settings;
            if (PinManager.settings.overwriteBrushProperties) brushSettings = PinManager.settings.brushSettings;
            else SetPinPivotLayer();
            repaint = true;
            UnityEditor.SceneView.RepaintAll();
        }

        private static void PinDuringSceneGUI(UnityEditor.SceneView sceneView)
        {
            PinInput(sceneView);
            if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout) return;
            var mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            bool snappedToVertex = false;
            var closestVertexInfo = new RaycastHit();
            var settings = PinManager.settings;
            if (_snapToVertex) snappedToVertex = SnapToVertex(mouseRay, out closestVertexInfo, sceneView.in2DMode);
            if (snappedToVertex) DrawPin(sceneView, closestVertexInfo, false);
            else
            {
                if (settings.mode == PinSettings.PaintMode.ON_SHAPE)
                {
                    if (GridRaycast(mouseRay, out RaycastHit planeHit))
                        DrawPin(sceneView, planeHit, SnapManager.settings.snappingEnabled);
                    else _paintStroke.Clear();
                }
                else
                {
                    if (MouseRaycast(mouseRay, out RaycastHit mouseHit, out GameObject collider, float.MaxValue,
                        -1, settings.paintOnPalettePrefabs, settings.paintOnMeshesWithoutCollider))
                        DrawPin(sceneView, mouseHit, SnapManager.settings.snappingEnabled);
                    else if (_pinned) DrawPin(sceneView, _pinHit, SnapManager.settings.snappingEnabled);
                    else if (settings.mode == PinSettings.PaintMode.AUTO)
                    {
                        if (GridRaycast(mouseRay, out RaycastHit planeHit))
                            DrawPin(sceneView, planeHit, SnapManager.settings.snappingEnabled);
                    }
                    else _paintStroke.Clear();
                }
            }
        }

        private static void DrawPin(UnityEditor.SceneView sceneView, RaycastHit hit, bool snapToGrid)
        {
            if (PaletteManager.selectedBrush == null) return;
            if (!_pinned)
            {
                hit.point = SnapAndUpdateGridOrigin(hit.point, snapToGrid,
                   PinManager.settings.paintOnPalettePrefabs, PinManager.settings.paintOnMeshesWithoutCollider,
                   PinManager.settings.mode == PaintOnSurfaceToolSettings.PaintMode.ON_SHAPE);
                _pinHit = hit;
            }
            PinPreview(sceneView.camera);
        }

        private static void PinPreview(Camera camera)
        {
            _paintStroke.Clear();
            if (BrushstrokeManager.brushstroke.Length == 0) return;
            var strokeItem = BrushstrokeManager.brushstroke[0];
            var prefab = strokeItem.settings.prefab;
            if (prefab == null) return;
            BrushSettings brushSettings = strokeItem.settings;
            if (PinManager.settings.overwriteBrushProperties) brushSettings = PinManager.settings.brushSettings;

            var itemRotation = Quaternion.identity;
            var itemPosition = _pinHit.point;
            if (brushSettings.rotateToTheSurface && !PinManager.settings.flattenTerrain)
            {
                if (_pinHit.normal == Vector3.zero) _pinHit.normal = Vector3.up;
                var itemTangent = Vector3.Cross(_pinHit.normal, Vector3.left);
                if (itemTangent.sqrMagnitude < 0.000001) itemTangent = Vector3.Cross(_pinHit.normal, Vector3.back);
                itemTangent = itemTangent.normalized;
                if(_pinHit.collider == null && strokeItem.settings.is2DAsset)
                    itemRotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                else 
                    itemRotation = Quaternion.LookRotation(itemTangent, _pinHit.normal);
            }

            if (_pinHit.collider != null)
            {
                var obj = _pinHit.collider.gameObject;
                var hitParent = _pinHit.collider.transform.parent;
                if (hitParent != null && hitParent.gameObject.GetInstanceID() == PWBCore.parentColliderId)
                    obj = PWBCore.GetGameObjectFromTempColliderId(obj.GetInstanceID());
            }
            GameObject objUnderMouse = null;
            if (_pinHit.collider != null)
            {
                var parentUnderMouse = _pinHit.collider.transform.parent;
                if (parentUnderMouse != null
                    && parentUnderMouse.gameObject.GetInstanceID() == PWBCore.parentColliderId)
                    objUnderMouse = PWBCore.GetGameObjectFromTempColliderId(
                        _pinHit.collider.gameObject.GetInstanceID());
                else objUnderMouse = _pinHit.collider.gameObject;
            }

            if (PinManager.settings.paintOnSelectedOnly && objUnderMouse != null
                && !SelectionManager.selection.Contains(objUnderMouse)) return;
            itemRotation *= Quaternion.Euler(strokeItem.additionalAngle);
            itemRotation *= Quaternion.Euler(_pinAngle);
            itemPosition += itemRotation * _pinOffset;
            itemPosition += itemRotation * brushSettings.localPositionOffset;

            var scaleMult = strokeItem.scaleMultiplier * _pinScale;
            var itemScale = Vector3.Scale(prefab.transform.localScale, scaleMult);

            itemPosition += _pinHit.normal * (brushSettings.surfaceDistance + _pinDistanceFromSurface);

            if (brushSettings.embedInSurface && !brushSettings.embedAtPivotHeight
                    && PinManager.settings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
            {
                var TRS = Matrix4x4.TRS(itemPosition, itemRotation,
                    Vector3.Scale(prefab.transform.localScale, strokeItem.scaleMultiplier));
                var bottomDistanceToSurfce = GetBottomDistanceToSurface(strokeItem.settings.bottomVertices,
                    TRS, Mathf.Abs(strokeItem.settings.bottomMagnitude), PinManager.settings.paintOnPalettePrefabs,
                    PinManager.settings.paintOnMeshesWithoutCollider);

                itemPosition += itemRotation * new Vector3(0f, -bottomDistanceToSurfce, 0f);
            }

            var layer = PinManager.settings.overwritePrefabLayer ? PinManager.settings.layer : prefab.layer;
            Transform parentTransform = GetParent(PinManager.settings, prefab.name, false);

            var translateMatrix = Matrix4x4.Translate(-prefab.transform.position);
            
            if (strokeItem.settings.is2DAsset)
            {
                var boundsCenter = BoundsUtils.GetBoundsRecursive(prefab.transform, prefab.transform.rotation).center
                    - prefab.transform.position;
                boundsCenter = Vector3.Project(boundsCenter, prefab.transform.up);

                var centerOffset = itemRotation * boundsCenter;
                translateMatrix = Matrix4x4.Translate(-prefab.transform.position - centerOffset);
            }
            
            _paintStroke.Add(new PaintStrokeItem(prefab, itemPosition,
                itemRotation * Quaternion.Euler(prefab.transform.eulerAngles), itemScale, layer, parentTransform));

            var rootToWorld = Matrix4x4.TRS(itemPosition, itemRotation, scaleMult) * translateMatrix;
            PreviewBrushItem(prefab, rootToWorld, layer, camera);

            DrawPinHandles(Matrix4x4.TRS(itemPosition, itemRotation, scaleMult) * translateMatrix);
        }

        private static void FlatenTerrain()
        {
            var terrain = _pinHit.collider.GetComponent<Terrain>();
            if (terrain == null) return;
            var terrainData = terrain.terrainData;

            terrainData.SetTerrainLayersRegisterUndo(terrainData.terrainLayers, "Paint");
            var resolution = terrainData.heightmapResolution;

            var heighMap = terrainData.GetHeights(0, 0, resolution, resolution);
            var transformScale = terrain.transform.localScale;
            terrain.transform.localScale = Vector3.one;
            var localCenter = terrain.transform.InverseTransformPoint(_flatteningCenter);
            var localHit = terrain.transform.InverseTransformPoint(_pinHit.point);
            terrain.transform.localScale = transformScale;

            var density = new Vector2(1 / terrainData.heightmapScale.x, 1 / terrainData.heightmapScale.z);
            var mapCenterX = Mathf.RoundToInt(localCenter.x * density.x);
            var mapCenterZ = Mathf.RoundToInt(localCenter.z * density.y);
            var mapHitX = Mathf.RoundToInt(localHit.x * density.x);
            var mapHitZ = Mathf.RoundToInt(localHit.z * density.y);

            var hitHmapVal = heighMap[mapHitZ, mapHitX];
            var flattenSettings = PinManager.settings.flatteningSettings;
            flattenSettings.density = density;
            flattenSettings.angle = _globalFlattening ? 0 : -_pinAngle.y;
            if (_globalFlattening) flattenSettings.size = _flatteningSize;
            else
            {
                var paintItem = _paintStroke[0];
                var itemSize = BoundsUtils.GetBoundsRecursive(paintItem.prefab.transform).size * _pinScale;
                flattenSettings.size = new Vector2(itemSize.x, itemSize.z);
            }
            var itemHeighmap = flattenSettings.heightmap;
            var itemHeighmapH = itemHeighmap.GetLength(0);
            var itemHeighmapW = itemHeighmap.GetLength(1);
            int itemMinX = Mathf.Max(itemHeighmapH / 2 - mapCenterX, 0);
            int itemMinZ = Mathf.Max(itemHeighmapW / 2 - mapCenterZ, 0);
            int itemMaxX = itemHeighmapH;
            if (Mathf.CeilToInt(itemHeighmapH / 2) + mapCenterX > resolution)
                itemMaxX -= (Mathf.CeilToInt(itemHeighmapH / 2) + mapCenterX) - resolution + 1;
            int itemMaxZ = itemHeighmapW;
            if (Mathf.CeilToInt(itemHeighmapW / 2) + mapCenterZ > resolution)
                itemMaxZ -= (Mathf.CeilToInt(itemHeighmapW / 2) + mapCenterZ) - resolution + 1;
            int w = itemMaxZ - itemMinZ;
            int h = itemMaxX - itemMinX;
            var heights = new float[w, h];

            int terrHmapMinX = Mathf.Max(mapCenterX - itemHeighmapH / 2, 0);
            int terrHmapMinZ = Mathf.Max(mapCenterZ - itemHeighmapW / 2, 0);

            for (int x = itemMinX; x < itemMaxX; ++x)
            {
                for (int z = itemMinZ; z < itemMaxZ; ++z)
                {
                    var terrHmapI = Mathf.Clamp(mapCenterZ - Mathf.CeilToInt(itemHeighmapW / 2) + z, 0, resolution - 1);
                    var terrHmapJ = Mathf.Clamp(mapCenterX - Mathf.CeilToInt(itemHeighmapH / 2) + x, 0, resolution - 1);
                    var terrHmapVal = heighMap[terrHmapI, terrHmapJ];

                    var itemI = z - itemMinZ;
                    var itemJ = x - itemMinX;
                    var itemHmapVal = itemHeighmap[x, z];
                    heights[itemI, itemJ] = terrHmapVal * (1 - itemHmapVal) + hitHmapVal * itemHmapVal;
                }
            }

            terrainData.SetHeights(terrHmapMinX, terrHmapMinZ, heights);

            ////////////////////
            if (flattenSettings.clearDetails)
            {
                var heightToDetail = (float)terrainData.detailResolution / terrainData.heightmapResolution;
                var heightToDetailInt = Mathf.CeilToInt(heightToDetail) + 1;
                var terrainDetailLayers = new System.Collections.Generic.List<int[,]>();
                var detailLayers = new System.Collections.Generic.List<int[,]>();
                var densityInt = new Vector2Int(Mathf.CeilToInt(density.x), Mathf.CeilToInt(density.y));
                var detailsW = Mathf.CeilToInt(w * heightToDetail) + 4 * densityInt.y;
                var detailsH = Mathf.CeilToInt(h * heightToDetail) + 4 * densityInt.x;
                var terrDetailMinX = Mathf.RoundToInt((localCenter.x * density.x - itemHeighmapH / 2f) * heightToDetail)
                    - 2 * densityInt.x;
                var terrDetailMinY = Mathf.RoundToInt((localCenter.z * density.y - itemHeighmapW / 2f) * heightToDetail)
                    - 2 * densityInt.y;

                void SetDetailToZero(int layer, int i, int j)
                {
                    detailLayers[layer][i, j] = 0;
                    for (int k = 1; k <= heightToDetailInt; ++k)
                    {
                        if (i - k >= 0)
                        {
                            detailLayers[layer][i - k, j] = 0;
                            if (j - k >= 0) detailLayers[layer][i - k, j - k] = 0;
                            else if (j + k < detailsH - 1) detailLayers[layer][i - k, j + k] = 0;
                        }
                        else if (i + k < detailsW - 1)
                        {
                            detailLayers[layer][i + k, j] = 0;
                            if (j - k >= 0) detailLayers[layer][i + k, j - k] = 0;
                            else if (j + k < detailsH - 1) detailLayers[layer][i + k, j + k] = 0;
                        }
                        else
                        {
                            if (j - k >= 0) detailLayers[layer][i, j - k] = 0;
                            else if (j + k < detailsH - 1) detailLayers[layer][i, j + k] = 0;
                        }
                    }
                }

                for (int k = 0; k < terrainData.detailPrototypes.Length; ++k)
                {

                    terrainDetailLayers.Add(terrainData.GetDetailLayer(0, 0,
                        terrainData.detailWidth, terrainData.detailHeight, k));
                    detailLayers.Add(new int[detailsW, detailsH]);
                    for (int itemDetailI = 0; itemDetailI < detailsW; ++itemDetailI)
                    {
                        for (int itemDetailJ = 0; itemDetailJ < detailsH; ++itemDetailJ)
                        {
                            var terrDetailI = terrDetailMinY + itemDetailI;
                            var terrDetailJ = terrDetailMinX + itemDetailJ;
                            var layerValue = terrainDetailLayers[k][terrDetailI, terrDetailJ];
                            detailLayers[k][itemDetailI, itemDetailJ] = layerValue;

                            var itemHmapX = Mathf.Clamp(Mathf.RoundToInt((itemDetailJ - 2 * densityInt.y)
                                / heightToDetail), 0, itemHeighmapH - 1);
                            var itemHmapZ = Mathf.Clamp(Mathf.RoundToInt((itemDetailI - 2 * densityInt.x)
                                / heightToDetail), 0, itemHeighmapW - 1);
                            var itemHmapVal = itemHeighmap[itemHmapX, itemHmapZ];
                            if (itemHmapVal > 0.9) SetDetailToZero(k, itemDetailI, itemDetailJ);
                        }
                    }
                    terrainData.SetDetailLayer(terrDetailMinX, terrDetailMinY, k, detailLayers[k]);
                }
            }
            if (flattenSettings.clearTrees)
            {
                for (int k = 0; k < terrainData.detailPrototypes.Length; ++k)
                {
                    var treeInstances = new System.Collections.Generic.List<TreeInstance>();
                    foreach (var treeInstance in terrainData.treeInstances)
                    {
                        var hmapX = Mathf.RoundToInt(treeInstance.position.x * resolution);
                        var hmapZ = Mathf.RoundToInt(treeInstance.position.z * resolution);
                        var itemHmapX = hmapX - terrHmapMinX;
                        var itemHmapZ = hmapZ - terrHmapMinZ;
                        if (itemHmapX < 0 || itemHmapX >= itemHeighmapH || itemHmapZ < 0 || itemHmapZ >= itemHeighmapW)
                        {
                            treeInstances.Add(treeInstance);
                            continue;
                        }
                        var itemHmapVal = itemHeighmap[itemHmapX, itemHmapZ];
                        if (itemHmapVal < 0.9) treeInstances.Add(treeInstance);
                    }
                    terrainData.treeInstances = treeInstances.ToArray();
                }
            }
            //////////////////

        }

        private static void PinInput(UnityEditor.SceneView sceneView)
        {
            if (PaletteManager.selectedBrush == null) return;
            var keyCode = Event.current.keyCode;
            if (Event.current.button == 0)
            {
                if (Event.current.type == EventType.MouseUp && !Event.current.alt)
                {
                    if (PinManager.settings.flattenTerrain) FlatenTerrain();
                    Paint(PinManager.settings);
                    _pinned = false;
                    Event.current.Use();
                }
                if (Event.current.type == EventType.KeyDown)
                {
                    if (keyCode == KeyCode.PageUp
                        || (keyCode == KeyCode.U && Event.current.control && !Event.current.alt && Event.current.shift))
                        _pinOffset = nextBoundLayer;
                    else if (keyCode == KeyCode.PageDown
                        || (keyCode == KeyCode.J && Event.current.control && !Event.current.alt && Event.current.shift))
                        _pinOffset = prevBoundLayer;
                    else if (keyCode == KeyCode.End
                        || (keyCode == KeyCode.Y && Event.current.control && !Event.current.alt && Event.current.shift))
                        _pinOffset = nextBoundPoint;
                    else if ((keyCode == KeyCode.Home && !Event.current.alt && !Event.current.control && !Event.current.shift)
                        || (keyCode == KeyCode.T && Event.current.control && !Event.current.alt && Event.current.shift))
                        _pinOffset = pivotBoundPoint;
                    //add rotation around Y
                    else if ((keyCode == KeyCode.Q || keyCode == KeyCode.LeftArrow)
                        && Event.current.control && !Event.current.alt && !Event.current.shift)
                        _pinAngle.y = (_pinAngle.y + 90) % 360;
                    else if ((keyCode == KeyCode.W || keyCode == KeyCode.RightArrow)
                        && Event.current.control && !Event.current.alt && !Event.current.shift)
                        _pinAngle.y = (_pinAngle.y - 90) % 360;
                    else if ((keyCode == KeyCode.Q || keyCode == KeyCode.LeftArrow)
                        && Event.current.control && !Event.current.alt && Event.current.shift)
                        _pinAngle.y -= PinManager.rotationSnapValue;
                    else if ((keyCode == KeyCode.W || keyCode == KeyCode.RightArrow)
                        && Event.current.control && !Event.current.alt && Event.current.shift)
                        _pinAngle.y += PinManager.rotationSnapValue;
                    //add rotation around X
                    else if ((keyCode == KeyCode.K || keyCode == KeyCode.LeftArrow)
                        && Event.current.control && Event.current.alt && !Event.current.shift)
                        _pinAngle.x = (_pinAngle.x + 90) % 360;
                    else if ((keyCode == KeyCode.L || keyCode == KeyCode.RightArrow)
                        && Event.current.control && Event.current.alt && !Event.current.shift)
                        _pinAngle.x = (_pinAngle.x - 90) % 360;
                    else if ((keyCode == KeyCode.K || keyCode == KeyCode.LeftArrow)
                        && Event.current.control && Event.current.alt && Event.current.shift)
                        _pinAngle.x -= PinManager.rotationSnapValue;
                    else if ((keyCode == KeyCode.L || keyCode == KeyCode.RightArrow)
                        && Event.current.control && Event.current.alt && Event.current.shift)
                        _pinAngle.x += PinManager.rotationSnapValue;
                    //add rotation around Z
                    else if (keyCode == KeyCode.Comma && Event.current.control && Event.current.alt && !Event.current.shift)
                        _pinAngle.z = (_pinAngle.z + 90) % 360;
                    else if (keyCode == KeyCode.Period && Event.current.control && Event.current.alt && !Event.current.shift)
                        _pinAngle.z = (_pinAngle.z - 90) % 360;
                    else if (keyCode == KeyCode.Comma && Event.current.control && Event.current.alt && Event.current.shift)
                        _pinAngle.z -= PinManager.rotationSnapValue;
                    else if (keyCode == KeyCode.Period && Event.current.control && Event.current.alt && Event.current.shift)
                        _pinAngle.z += PinManager.rotationSnapValue;
                    //reset rotation
                    else if ((keyCode == KeyCode.Home && Event.current.control && !Event.current.alt && !Event.current.shift)
                        || (keyCode == KeyCode.M && Event.current.control && !Event.current.alt && Event.current.shift))
                        _pinAngle = Vector3.zero;
                    else if ((keyCode == KeyCode.J || keyCode == KeyCode.DownArrow)
                        && Event.current.control && Event.current.alt && !Event.current.shift)
                        _pinDistanceFromSurface -= 1f;
                    else if ((keyCode == KeyCode.U || keyCode == KeyCode.UpArrow)
                        && Event.current.control && Event.current.alt && !Event.current.shift)
                        _pinDistanceFromSurface += 1f;
                    else if ((keyCode == KeyCode.J || keyCode == KeyCode.DownArrow)
                        && Event.current.control && Event.current.alt && Event.current.shift)
                        _pinDistanceFromSurface -= 0.1f;
                    else if ((keyCode == KeyCode.U || keyCode == KeyCode.UpArrow)
                        && Event.current.control && Event.current.alt && Event.current.shift)
                        _pinDistanceFromSurface += 0.1f;
                    else if (keyCode == KeyCode.G && Event.current.control && !Event.current.alt && Event.current.shift)
                        _pinDistanceFromSurface = 0;
                    else if ((keyCode == KeyCode.Home && Event.current.control && Event.current.shift)
                        || (keyCode == KeyCode.Period && Event.current.control && Event.current.shift))
                        UpdatePinScale(1f);
                    else if (keyCode == KeyCode.T && Event.current.control && !Event.current.alt && !Event.current.shift)
                    {
                        PinManager.settings.repeat = !PinManager.settings.repeat;
                        ToolProperties.RepainWindow();
                    }
                }
                if (Event.current.isScrollWheel)
                {
                    var scrollSign = Mathf.Sign(Event.current.delta.y);
                    if (Event.current.control && !Event.current.alt && !Event.current.shift)
                    {
                        Event.current.Use();
                        UpdatePinScale(Mathf.Max(_pinScale * (1f + scrollSign * 0.05f), 0.01f));
                        sceneView.Repaint();
                        repaint = true;
                    }
                    else if (Event.current.control && Event.current.alt && !Event.current.shift)
                    {
                        Event.current.Use();
                        BrushstrokeManager.SetNextPinBrushstroke((int)scrollSign);
                        sceneView.Repaint();
                        repaint = true;
                    }
                }
            }
            else
            {
                if (Event.current.type == EventType.MouseDown && Event.current.control)
                {
                    _pinned = true;
                    _pinMouse = Event.current.mousePosition;
                    _previousPinAngle = _pinAngle;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseUp && !Event.current.control) _pinned = false;
                const float DEG_PER_PIXEL = 1.8f; //180deg/100px
                if (Event.current.button == 1)
                {
                    if (Event.current.type == EventType.MouseDrag && Event.current.control && !Event.current.shift)
                    {
                        var delta = _pinMouse.x - Event.current.mousePosition.x;
                        _pinAngle.y = _previousPinAngle.y + delta * DEG_PER_PIXEL;
                        if (Event.current.alt && PinManager.rotationSnapValue > 0)
                            _pinAngle.y = Mathf.Round(_pinAngle.y / PinManager.rotationSnapValue)
                                * PinManager.rotationSnapValue;
                        Event.current.Use();
                    }
                    else if (Event.current.type == EventType.MouseDrag && Event.current.control && Event.current.shift)
                    {
                        var delta = _pinMouse.y - Event.current.mousePosition.y;
                        _pinDistanceFromSurface = delta * 0.04f;
                        Event.current.Use();
                    }
                }
                else if (Event.current.button == 2)
                {
                    if (Event.current.type == EventType.MouseDrag && Event.current.control && !Event.current.shift)
                    {
                        var delta = _pinMouse.y - Event.current.mousePosition.y;
                        _pinAngle.x = _previousPinAngle.x + delta * DEG_PER_PIXEL;
                        if (Event.current.alt && PinManager.rotationSnapValue > 0)
                            _pinAngle.x = Mathf.Round(_pinAngle.x / PinManager.rotationSnapValue)
                                * PinManager.rotationSnapValue;
                        Event.current.Use();
                    }
                    if (Event.current.type == EventType.MouseDrag && Event.current.control && Event.current.shift)
                    {
                        var delta = _pinMouse.y - Event.current.mousePosition.y;
                        _pinAngle.z = _previousPinAngle.z + delta * DEG_PER_PIXEL;
                        if (Event.current.alt && PinManager.rotationSnapValue > 0)
                            _pinAngle.z = Mathf.Round(_pinAngle.z / PinManager.rotationSnapValue)
                                * PinManager.rotationSnapValue;
                        Event.current.Use();
                    }
                }
            }
            if ((keyCode == KeyCode.LeftControl || keyCode == KeyCode.RightControl)
                && Event.current.type == EventType.KeyUp) _pinned = false;
        }

        private static void DrawPinHandles(Matrix4x4 rootToWorld)
        {
            if (BrushstrokeManager.brushstroke.Length == 0) return;
            if (_paintStroke.Count == 0) return;

            var strokeItem = BrushstrokeManager.brushstroke[0];
            var prefab = strokeItem.settings.prefab;
            if (prefab == null) return;
            var pos = Vector3.zero;
            var prevPos = Vector3.zero;
            var pos0 = Vector3.zero;
            var handlePoints = new System.Collections.Generic.List<Vector3>();
            UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            if (_pinBoundPoints.Count == 0) ResetPinValues();
            var flatteningPoints = new System.Collections.Generic.List<Vector3>();

            for (int i = 0; i < _pinBoundPoints[_pinBoundLayerIdx].Count; ++i)
            {
                prevPos = pos;
                pos = Vector3.Scale((Vector3)(rootToWorld * (_pinOffset - _pinBoundPoints[_pinBoundLayerIdx][i])),
                    Vector3.one / _pinScale) + _pinHit.point;
                if (i > _pinBoundPoints[_pinBoundLayerIdx].Count - 5)
                {
                    if (i == _pinBoundPoints[_pinBoundLayerIdx].Count - 4) pos0 = pos;
                    else if (i < _pinBoundPoints[_pinBoundLayerIdx].Count)
                    {
                        UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.7f);
                        UnityEditor.Handles.DrawAAPolyLine(6, new Vector3[] { prevPos, pos });
                        UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.7f);
                        UnityEditor.Handles.DrawAAPolyLine(2, new Vector3[] { prevPos, pos });
                    }
                }
                flatteningPoints.Add(pos);
                if (_pinBoundPointIdx == i) continue;
                handlePoints.Add(pos);
            }
            UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.7f);
            UnityEditor.Handles.DrawAAPolyLine(6, new Vector3[] { pos, pos0 });
            UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.7f);
            UnityEditor.Handles.DrawAAPolyLine(2, new Vector3[] { pos, pos0 });

            if (PinManager.settings.flattenTerrain && _pinHit.collider != null
                && _pinHit.collider.GetComponent<Terrain>() != null)
            {
                var scale = rootToWorld.lossyScale;
                var rotation = rootToWorld.rotation;
                var angle = rotation.eulerAngles;
                angle.x = Mathf.Abs(Mathf.Round(angle.x)) % 360;
                angle.z = Mathf.Abs(Mathf.Round(angle.z)) % 360;
                Vector3 p0, p1, p2, p3;
                _globalFlattening = (angle.x > 0 || angle.z > 0);
                var n = flatteningPoints.Count;
                _flatteningCenter = flatteningPoints[n - 5];
                if (_globalFlattening)
                {
                    var bounds = BoundsUtils.GetBoundsRecursive(prefab.transform, rotation, scale);
                    _flatteningSize.x = bounds.size.x;
                    _flatteningSize.y = bounds.size.z;
                    var halfSize = _flatteningSize / 2;
                    p0 = _flatteningCenter + new Vector3(-halfSize.x, 0, -halfSize.y);
                    p1 = _flatteningCenter + new Vector3(-halfSize.x, 0, halfSize.y);
                    p2 = _flatteningCenter + new Vector3(halfSize.x, 0, halfSize.y);
                    p3 = _flatteningCenter + new Vector3(halfSize.x, 0, -halfSize.y);
                }
                else
                {
                    var side1_2 = flatteningPoints[n - 3] - flatteningPoints[n - 4];
                    var side2_3 = flatteningPoints[n - 2] - flatteningPoints[n - 3];
                    var dir1_2 = side1_2.normalized;
                    var dir2_3 = side2_3.normalized;
                    p0 = flatteningPoints[n - 4] + (-dir1_2 - dir2_3) * PinManager.settings.flatteningSettings.padding;
                    p1 = flatteningPoints[n - 3] + (dir1_2 - dir2_3) * PinManager.settings.flatteningSettings.padding;
                    p2 = flatteningPoints[n - 2] + (dir1_2 + dir2_3) * PinManager.settings.flatteningSettings.padding;
                    p3 = flatteningPoints[n - 1] + (-dir1_2 + dir2_3) * PinManager.settings.flatteningSettings.padding;
                }
                UnityEditor.Handles.color = new Color(0.5f, 0f, 1f, 0.7f);
                UnityEditor.Handles.DrawAAPolyLine(6, new Vector3[] { p0, p1, p2, p3, p0 });
                UnityEditor.Handles.color = new Color(0f, 0.5f, 1f, 0.7f);
                UnityEditor.Handles.DrawAAPolyLine(2, new Vector3[] { p0, p1, p2, p3, p0 });
            }

            foreach (var handlePoint in handlePoints)
            {
                UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.7f);
                UnityEditor.Handles.DotHandleCap(795, handlePoint, Quaternion.identity,
                    UnityEditor.HandleUtility.GetHandleSize(pos) * 0.0325f * PWBCore.staticData.controPointSize,
                    EventType.Repaint);
                UnityEditor.Handles.color = UnityEditor.Handles.preselectionColor;
                UnityEditor.Handles.DotHandleCap(795, handlePoint, Quaternion.identity,
                    UnityEditor.HandleUtility.GetHandleSize(pos) * 0.02f * PWBCore.staticData.controPointSize,
                    EventType.Repaint);
            }

            var pinHitPoint = _pinHit.point;
            UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.7f);
            UnityEditor.Handles.DotHandleCap(418, pinHitPoint, Quaternion.identity,
                UnityEditor.HandleUtility.GetHandleSize(pinHitPoint) * 0.0425f * PWBCore.staticData.controPointSize,
                EventType.Repaint);
            UnityEditor.Handles.color = UnityEditor.Handles.selectedColor;
            UnityEditor.Handles.DotHandleCap(418, pinHitPoint, Quaternion.identity,
                UnityEditor.HandleUtility.GetHandleSize(pinHitPoint) * 0.03f * PWBCore.staticData.controPointSize,
                EventType.Repaint);
        }
    }
    #endregion
}
