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
using System;

namespace PluginMaster
{
    [UnityEditor.InitializeOnLoad]
    public static class ToolManager
    {
        public enum PaintTool
        {
            NONE,
            PIN,
            BRUSH,
            GRAVITY,
            LINE,
            SHAPE,
            TILING,
            REPLACER,
            ERASER,
            SELECTION,
            EXTRUDE,
            MIRROR
        }
        private static PaintTool _tool = ToolManager.PaintTool.NONE;
        public enum ToolState { NONE, PREVIEW, EDIT, PERSISTENT }

        private static bool _editMode = false;
        public static Action<PaintTool> OnToolChange;
        public static Action OnToolModeChanged;
        public static bool _triggerToolChangeEvent = true;
        static ToolManager()
        {
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode += OnSceneChange;
            PaletteManager.OnBrushChanged += TilingManager.settings.UpdateCellSize;
        }

        public static bool editMode
        {
            get => _editMode;
            set
            {
                if (_editMode == value) return;
                _editMode = value;
                if (OnToolModeChanged != null) OnToolModeChanged();
            }
        }
        public static ToolManager.PaintTool tool
        {
            get => _tool;
            set
            {
                if (_tool == value) return;
                var prevTool = _tool;
                _tool = value;
                if (_tool != prevTool)
                {
                    BoundsUtils.ClearBoundsDictionaries();
                    if (_triggerToolChangeEvent && OnToolChange != null) OnToolChange(prevTool);
                    _editMode = false;
                    _triggerToolChangeEvent = true;
                }
               
                switch (_tool)
                {
                    case PaintTool.PIN:
                        if (PinManager.settings.paintOnMeshesWithoutCollider) PWBCore.UpdateTempColliders();
                        PWBIO.ResetPinValues();
                        break;
                    case PaintTool.BRUSH:
                        if (BrushManager.settings.paintOnMeshesWithoutCollider) PWBCore.UpdateTempColliders();
                        break;
                    case PaintTool.GRAVITY:
                        PWBCore.DestroyTempColliders();
                        break;
                    case PaintTool.REPLACER:
                        PWBIO.UpdateOctree();
                        PWBIO.ResetReplacer();
                        break;
                    case PaintTool.ERASER:
                        PWBIO.UpdateOctree();
                        break;
                    case PaintTool.EXTRUDE:
                        SelectionManager.UpdateSelection();
                        PWBIO.ResetUnityCurrentTool();
                        PWBIO.ResetExtrudeState(false);
                        break;
                    case PaintTool.LINE:
                        if (LineManager.settings.paintOnMeshesWithoutCollider) PWBCore.UpdateTempColliders();
                        PWBIO.ResetLineState(false);
                        PWBCore.staticData.VersionUpdate();
                        break;
                    case PaintTool.SHAPE:
                        if (ShapeManager.settings.paintOnMeshesWithoutCollider) PWBCore.UpdateTempColliders();
                        PWBIO.ResetShapeState(false);
                        break;
                    case PaintTool.TILING:
                        if (TilingManager.settings.paintOnMeshesWithoutCollider) PWBCore.UpdateTempColliders();
                        PWBIO.ResetTilingState(false);
                        break;
                    case PaintTool.SELECTION:
                        SelectionManager.UpdateSelection();
                        PWBIO.ResetUnityCurrentTool();
                        break;
                    case PaintTool.MIRROR:
                        SelectionManager.UpdateSelection();
                        PWBIO.InitializeMirrorPose();
                        break;
                    case PaintTool.NONE:
                        PWBIO.ResetUnityCurrentTool();
                        PWBIO.ResetReplacer();
                        PWBCore.DestroyTempColliders();
                        break;
                    default: break;
                }

                if (_tool != PaintTool.NONE)
                {
                    PWBIO.SaveUnityCurrentTool();
                    ToolProperties.ShowWindow();
                    PaletteManager.pickingBrushes = false;
                }

                if (_tool == PaintTool.BRUSH || _tool == PaintTool.PIN || _tool == PaintTool.GRAVITY
                    || _tool == PaintTool.REPLACER || _tool == PaintTool.ERASER || _tool == PaintTool.LINE
                    || _tool == PaintTool.SHAPE || _tool == PaintTool.TILING)
                {
                    PrefabPalette.ShowWindow();
                    BrushProperties.ShowWindow();
                    SelectionManager.UpdateSelection();
                    if (_tool == PaintTool.BRUSH || _tool == PaintTool.PIN
                        || _tool == PaintTool.GRAVITY || _tool == PaintTool.REPLACER)
                        BrushstrokeManager.UpdateBrushstroke();
                    PWBIO.ResetAutoParent();
                }
                ToolProperties.RepainWindow();
                if (BrushProperties.instance != null) BrushProperties.instance.Repaint();
                if (UnityEditor.SceneView.sceneViews.Count > 0) ((UnityEditor.SceneView)
                        UnityEditor.SceneView.sceneViews[0]).Focus();
            }
        }

        public static void DeselectTool(bool triggerToolChangeEvent = true)
        {
            _triggerToolChangeEvent = triggerToolChangeEvent;
            if (tool == ToolManager.PaintTool.REPLACER) PWBIO.ResetReplacer();
            tool = ToolManager.PaintTool.NONE;
            PWBIO.ResetUnityCurrentTool();
            PWBToolbar.RepaintWindow();
        }

        private static void OnSceneChange(UnityEngine.SceneManagement.Scene previous,
            UnityEngine.SceneManagement.Scene current) => DeselectTool();

        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            DeselectTool();
            PWBCore.DestroyTempColliders();
        }

        public static void OnPaletteClosed()
        {
            if (tool != ToolManager.PaintTool.ERASER && tool != ToolManager.PaintTool.EXTRUDE)
                tool = ToolManager.PaintTool.NONE;
        }
    }
}