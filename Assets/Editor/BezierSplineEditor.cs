using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierSpline))]
class BezierSplineEditor : Editor {

    // Settings
    private int stepsPerCurve = 10;
    private float directionScale = 0.5f;
    private float handleSize = 0.04f;
    private float pickSize = 0.06f;

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
        Vector3 p0 = ShowPoint(0);
        
        for (int i = 1; i < spline.points.Length; i += 3) {  
            Vector3 p1 = ShowPoint(i);
            Vector3 p2 = ShowPoint(i + 1);
            Vector2 p3 = ShowPoint(i + 2);

            // Draw the handle lines
            Handles.color = Color.gray;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);

            // draw out curve
            Handles.DrawBezier(p0, p3, p1, p2, Color.red, EditorGUIUtility.whiteTexture, 1.3f);
            p0 = p3;
        }
        ShowDirections();
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        // We'll ba able to add new curves to the spline
        if (GUILayout.Button("Add Curve")) {
            Undo.RecordObject(spline, "Add Curve");
            spline.AddCurve();
            EditorUtility.SetDirty(spline);
        }
    }

    private void ShowDirections() {
        Handles.color = Color.green;

        int steps = spline.CurveCount * stepsPerCurve;
        Vector3 point;
        for (int i = 0; i <= steps; i++) {
            point = spline.GetPoint((float) i / steps);

            Handles.DrawLine(point, point + spline.GetDirection((float) i / steps) * directionScale);
        }
    }

    private Vector3 ShowPoint(int index) {
        // transform point to world space
        Vector3 point = handleTransform.TransformPoint(spline.points[index]);

        // This method gives us a fixed screen size for any point in world space
        float size = HandleUtility.GetHandleSize(point);
        // This button will look like a white dot   when clicked will turn into the active point
        Handles.color = Color.white;
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap)) {
            selectedIndex = index;
        }
        //  we only show the position handle if the point's index matches the selected index
        if (selectedIndex == index ) { 
            EditorGUI.BeginChangeCheck();
            {
                point = Handles.DoPositionHandle(point, handleRotation);
            }
            if (EditorGUI.EndChangeCheck()) { // if handle changed
                // so we can undo the drag operations
                Undo.RecordObject(spline, "Move Point");
                // after record undo we have to call this
                EditorUtility.SetDirty(spline);

                // assign the new position
                spline.points[index] = handleTransform.InverseTransformPoint(point);
            }
        }
        return point;
    }

}