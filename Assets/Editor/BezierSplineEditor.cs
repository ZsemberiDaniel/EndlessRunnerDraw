using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierSpline))]
class BezierSplineEditor : Editor {

    // Settings
    private int stepsPerCurve = 10;
    private float directionScale = 0.5f;
    private float handleSize = 0.1f;
    private float pickSize = 0.01f;
    // How the handles in different modes should look like
    private static Tuple<Handles.CapFunction, Color>[] handleSettings = {
        Tuple.Create<Handles.CapFunction, Color>(Handles.CubeHandleCap, Color.white),
        Tuple.Create<Handles.CapFunction, Color>(Handles.ConeHandleCap, Color.yellow),
        Tuple.Create<Handles.CapFunction, Color>(Handles.SphereHandleCap, Color.cyan)
    };

    // Which point is selected
    private int selectedIndex = -1;

    private BezierSpline spline;
    private Transform handleTransform;
    private Quaternion handleRotation;

    private void OnEnable() {
        spline = target as BezierSpline;
        handleTransform = spline.transform;
        // take unity's pivot rotation mode into consideration
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;
    }

    private void OnSceneGUI() {
        // show points with handles and transform them to world space
        Vector3 _p0 = ShowPoint(0);

        ShowDirections();
        for (int i = 1; i < spline.Length; i += 3) {  
            Vector3 _p1 = ShowPoint(i);
            Vector3 _p2 = ShowPoint(i + 1);
            Vector3 _p3 = ShowPoint(i + 2);

            // Draw the handle lines
            Handles.color = Color.gray;
            Handles.DrawLine(_p0, _p1);
            Handles.DrawLine(_p2, _p3);

            // draw out curve
            Handles.DrawBezier(_p0, _p3, _p1, _p2, Color.red, null, 1.3f);
            _p0 = _p3;
        }
    }

    public override void OnInspectorGUI() {
        if (selectedIndex >= 0 && selectedIndex < spline.Length) {
            DrawSelectedPointEditor();
        }

        // We'll be able to add new curves to the spline
        if (GUILayout.Button("Add Curve")) {
            Undo.RecordObject(spline, "Add Curve");
            spline.AddCurve();
            EditorUtility.SetDirty(spline);
        }
    }

    private void DrawSelectedPointEditor() {
        GUILayout.Label("Selected Point", EditorStyles.boldLabel);

        // Position
        EditorGUI.BeginChangeCheck();
        Vector3 _point = EditorGUILayout.Vector3Field("Position", spline[selectedIndex]);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(spline, "Move Point");
            EditorUtility.SetDirty(spline);

            spline[selectedIndex] = _point;
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
        }
    }

    private void ShowDirections() {
        Handles.color = Color.green;

        int _steps = spline.CurveCount * stepsPerCurve;
        Vector3 _point;
        for (int i = 0; i <= _steps; i++) {
            _point = spline.GetPoint(i / (float) _steps);

            Handles.DrawLine(_point, _point + spline.GetDirection(i / (float) _steps) * directionScale);
        }
    }

    private Vector3 ShowPoint(int index) {
        // transform point to world space
        Vector3 _point = handleTransform.TransformPoint(spline[index]);

        // This method gives us a fixed screen size for a handle for any point in world space
        float _size = HandleUtility.GetHandleSize(_point);
        // This button will look like a white dot   when clicked will turn into the active point
        var _handleSettings = BezierSplineEditor.handleSettings[(int) spline.GetControlPointMode(index)];
        Handles.color = _handleSettings.Item2;
        if (Handles.Button(_point, Quaternion.identity, _size * handleSize, _size * pickSize, _handleSettings.Item1)) {
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
            }
        }
        return _point;
    }

}