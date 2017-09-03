using Bezier;
using EndlessRunner;
using System;
using UnityEditor;
using UnityEngine;
using static BezierEditor.BezierSplineSettingsWindow;

namespace BezierEditor {
    [CustomEditor(typeof(BezierSpline))]
    class BezierSplineEditor : Editor {

        private static readonly GUIContent arcQuality = new GUIContent("Quality of curve", "To how many segments this curve should be divided to in calcualtions.");

        /// <summary>
        /// How the handles in different modes should look like
        /// </summary>
        private static Tuple<Handles.CapFunction, Color>[] handleSettings = {
            Tuple.Create<Handles.CapFunction, Color>(Handles.CubeHandleCap, Color.white),
            Tuple.Create<Handles.CapFunction, Color>(Handles.CubeHandleCap, Color.yellow),
            Tuple.Create<Handles.CapFunction, Color>(Handles.CubeHandleCap, Color.cyan)
        };

        /// <summary>
        /// Which point is selected
        /// </summary>
        private int selectedIndex = -1;

        private BezierSpline spline;
        private Transform handleTransform;
        private Quaternion handleRotation;

        /// <summary>
        /// Whether the parent of this spline is a LevelComponent
        /// </summary>
        private bool isParentLevelComponent;

        /// <summary>
        /// Adds a drawing method for every present spline in the screen to SceneView.onSceneGUIDelegate
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void AddPresentSplinesToDraw() {
            var _splines = FindObjectsOfType<BezierSpline>();

            for (int i = 0; i < _splines.Length; i++) {
                BezierSpline _spline = _splines[i];
                SafeAddSplineDrawToSceneView(_spline);
            }
        }
    
        /// <summary>
        /// Used for drawing when the spline is not selected. It is added to SceneView.onSceneGUIDelegate when
        /// the scripta re reloaded
        /// </summary>
        private static void DrawSplineWithoutHandles(BezierSpline spline) {
            if (spline == null) return;

            // show points with handles and transform them to world space
            Vector3 _p3 = spline.transform.TransformPoint(spline[spline.PointCount - 1]);

            // Go through backwards so it doesn't freak out if we delete something
            for (int i = spline.PointCount - 4; i >= 0; i -= 3) {
                Vector3 _p0 = spline.transform.TransformPoint(spline[i]);
                Vector3 _p1 = spline.transform.TransformPoint(spline[i + 1]);
                Vector3 _p2 = spline.transform.TransformPoint(spline[i + 2]);

                // draw out curve
                Handles.DrawBezier(_p0, _p3, _p1, _p2, spline.color, null, 1.3f);
                _p3 = _p0;
            }
        }

        /// <summary>
        /// Safely adds the draw method for the given spline to SceneView.onSceneGUIDelegate
        /// </summary>
        private static void SafeAddSplineDrawToSceneView(BezierSpline spline) {
            SceneView.onSceneGUIDelegate -= (sceneView) => { DrawSplineWithoutHandles(spline); };
            SceneView.onSceneGUIDelegate += (sceneView) => { DrawSplineWithoutHandles(spline); };
        }

        private void OnEnable() {
            spline = target as BezierSpline;
            handleTransform = spline.transform;
            // take unity's pivot rotation mode into consideration
            handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;
        }
        private void OnSceneGUI() {
            // show points with handles and transform them to world space
            Vector3 _p3 = ShowPoint(spline.PointCount - 1);

            ShowDirections();
            // Go through backwards so it doesn't freak out if we delete something
            for (int i = spline.PointCount - 4; i >= 0; i -= 3) {
                Vector3 _p0 = ShowPoint(i);
                Vector3 _p1 = ShowPoint(i + 1);
                Vector3 _p2 = ShowPoint(i + 2);

                // Draw the handle lines
                Handles.color = Color.gray;
                Handles.DrawLine(_p0, _p1);
                Handles.DrawLine(_p2, _p3);

                // draw out curve
                Handles.DrawBezier(_p0, _p3, _p1, _p2, spline.color, null, 1.3f);
                _p3 = _p0;
            }


            // Handle shortcuts
            Event e = Event.current;
            if (e.type == EventType.KeyDown) {
                if (e.keyCode == Settings.HotkeyNewCurve) {
                    AddNewCurve();
                }
            }
        }
        public override void OnInspectorGUI() {
            // we need to recalculate the isParentLEvelComponent because parent may have changed
            if (spline.transform.hasChanged) {
                isParentLevelComponent = spline.GetComponentInParent<LevelComponent>() != null;
            }

            if (selectedIndex >= 0 && selectedIndex < spline.PointCount) {
                DrawSelectedPointEditor();
            }

            spline.color = EditorGUILayout.ColorField("Spline color", spline.color);

            // if parent is levelcomponent then the spline attributes are controlled by that
            if (!isParentLevelComponent) {
                spline.ArcLengthQuality = EditorGUILayout.IntField(arcQuality, spline.ArcLengthQuality);
            } else {
                EditorGUILayout.LabelField("Attributes are controlled by the parent LevelComponent", EditorStyles.helpBox);
            }

            // We'll be able to add new curves to the spline
            if (GUILayout.Button("Add Curve")) {
                AddNewCurve();
            }
        }

        private void AddNewCurve() {
            Undo.RecordObject(spline, "Add Curve");
            spline.AddCurve();
            EditorUtility.SetDirty(spline);
            Undo.FlushUndoRecordObjects();
        }

        /// <summary>
        /// Draws the spline without the selectable handles
        /// </summary>
        private void DrawSplineWithoutHandles(SceneView sceneView = null) {
            if (spline == null) {
                SceneView.onSceneGUIDelegate -= DrawSplineWithoutHandles;

                return;
            }

            // show points with handles and transform them to world space
            Vector3 _p3 = handleTransform.TransformPoint(spline[spline.PointCount - 1]);

            ShowDirections();
            // Go through backwards so it doesn't freak out if we delete something
            for (int i = spline.PointCount - 4; i >= 0; i -= 3) {
                Vector3 _p0 = handleTransform.TransformPoint(spline[i]);
                Vector3 _p1 = handleTransform.TransformPoint(spline[i + 1]);
                Vector3 _p2 = handleTransform.TransformPoint(spline[i + 2]);

                // draw out curve
                Handles.DrawBezier(_p0, _p3, _p1, _p2, spline.color, null, 1.3f);
                _p3 = _p0;
            }
        }

        /// <summary>
        /// Draws the inspector for the selected point
        /// </summary>
        private void DrawSelectedPointEditor() {
            GUILayout.Label("Selected Point (" + selectedIndex + ")", EditorStyles.boldLabel);

            // Position
            EditorGUI.BeginChangeCheck();
            Vector3 _point = EditorGUILayout.Vector3Field("Position", spline[selectedIndex]);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);

                spline[selectedIndex] = _point;
                Undo.FlushUndoRecordObjects();
            }

            // ControlPointMode
            EditorGUI.BeginChangeCheck();
            BezierControlPointMode _mode = (BezierControlPointMode) EditorGUILayout.EnumPopup(
                "Mode", spline.GetControlPointMode(selectedIndex)
            );
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(spline, "Control Point Mode change");
                EditorUtility.SetDirty(spline);

                spline.SetControlPointMode(selectedIndex, _mode);
                Undo.FlushUndoRecordObjects();
            }

            // Remove button
            if (GUILayout.Button("Remove point")) {
                Undo.RecordObject(spline, "Removed curve");
                EditorUtility.SetDirty(spline);

                spline.RemoveCurve(selectedIndex);
                Undo.FlushUndoRecordObjects();
            }
        }

        /// <summary>
        /// Draws the directions of the bezier curve
        /// </summary>
        private void ShowDirections() { 
            for (int i = 0; i < spline.CurveCount; i++) {
                if (Settings.ShowSteps) { 
                    for (int s = 0; s < Settings.StepsPerCurve; s++) { 
                        Vector3 _point = handleTransform.TransformPoint(Bezier.BezierMath.CubicGetPoint(
                                spline[i * 3],
                                spline[i * 3 + 1],
                                spline[i * 3 + 2],
                                spline[i * 3 + 3], (float) s / Settings.StepsPerCurve));
                        Vector3 _pointDerivative = handleTransform.TransformDirection(Bezier.BezierMath.CubicGetFirstDerivative(
                                spline[i * 3],
                                spline[i * 3 + 1],
                                spline[i * 3 + 2],
                                spline[i * 3 + 3], (float) s / Settings.StepsPerCurve));

                        // which way is the middle point's right
                        Vector3 _pointRight = (Quaternion.LookRotation(Vector3.right) * _pointDerivative).normalized;
                        // which way is the middle point's left
                        Vector3 _pointLeft = (Quaternion.LookRotation(Vector3.left) * _pointDerivative).normalized;
                        // which way is up
                        Vector3 _pointUp = Vector3.Cross(_pointLeft, _pointDerivative).normalized;

                        Handles.color = Color.blue;
                        Handles.DrawLine(_point, _point + _pointUp * Settings.DirectionScale);
                        Handles.color = Color.green;
                        Handles.DrawLine(_point, _point + _pointLeft * Settings.DirectionScale);
                        Handles.color = Color.red;
                        Handles.DrawLine(_point, _point + _pointRight * Settings.DirectionScale);
                    }
                }

                if (Settings.ShowPivots) {
                    Vector3 _pivot;
                    // get pivot -> the intersection of the point normals at the start and end points
                    Math3D.LineLineIntersection(
                        out _pivot,
                        spline[i * 3],
                        (Quaternion.LookRotation(Vector3.right) * Bezier.BezierMath.CubicGetFirstDerivative(spline[i * 3], spline[i * 3 + 1], spline[i * 3 + 2], spline[i * 3 + 3], 0.01f)).normalized,
                        spline[i * 3 + 3],
                        (Quaternion.LookRotation(Vector3.right) * Bezier.BezierMath.CubicGetFirstDerivative(spline[i * 3], spline[i * 3 + 1], spline[i * 3 + 2], spline[i * 3 + 3], 0.99f)).normalized
                    );
                    _pivot = handleTransform.TransformPoint(_pivot);

                    Handles.color = spline.color;

                    Handles.DrawLine(spline[i * 3], _pivot);
                    Handles.DrawLine(spline[i * 3 + 3], _pivot);
                    Handles.DrawWireCube(_pivot, new Vector3(1, 1, 1));
                }
            }
        }

        /// <summary>
        /// Draws a point from the bezier curve
        /// </summary>
        private Vector3 ShowPoint(int index) {
            // transform point to world space
            Vector3 _point = handleTransform.TransformPoint(spline[index]);

            // This method gives us a fixed screen size for a handle for any point in world space
            float _size = HandleUtility.GetHandleSize(_point);
            // This button will look like a white dot   when clicked will turn into the active point
            var _handleSettings = handleSettings[(int) spline.GetControlPointMode(index)];
            Handles.color = _handleSettings.Item2;
            if (Handles.Button(_point, Quaternion.identity, _size * Settings.HandleSize, _size * Settings.PickSize, _handleSettings.Item1)) {
                selectedIndex = index;
                Repaint();
            }
            //  we only show the position handle if the point's index matches the selected index
            if (selectedIndex == index ) { 
                EditorGUI.BeginChangeCheck();
                {
                    _point = Handles.DoPositionHandle(_point, handleRotation);
                }
                if (EditorGUI.EndChangeCheck()) { // if handle changed
                    // so we can undo the drag operations
                    Undo.RecordObject(spline, "Move Point");
                    // after record undo we have to call this
                    EditorUtility.SetDirty(spline);

                    // assign the new position
                    spline[index] = handleTransform.InverseTransformPoint(_point);
                    Undo.FlushUndoRecordObjects();
                }
            }
            return _point;
        }

    }
}