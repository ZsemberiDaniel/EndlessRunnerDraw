using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierCurve))]
class BezierCurveEditor : Editor {

    private int lineSteps = 10;
    private float directionScale = 0.5f;

    private BezierCurve curve;
    private Transform handleTransform;
    private Quaternion handleRotation;

    private void OnEnable() {
        curve = target as BezierCurve;
        handleTransform = curve.transform;
        // take unity's pivot rotation mode into consideration
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;
    }

    private void OnSceneGUI() {
        // show points with handles and transform them to world space
        Vector3 p0 = ShowPoint(0);
        Vector3 p1 = ShowPoint(1);
        Vector3 p2 = ShowPoint(2);
        Vector2 p3 = ShowPoint(3);

        ShowDirections();
        // draw out curve
        Handles.DrawBezier(p0, p3, p1, p2, Color.red, EditorGUIUtility.whiteTexture, 1.3f);
    }

    private void ShowDirections() {
        Handles.color = Color.green;

        Vector3 point;
        for (int i = 0; i <= lineSteps; i++) {
            point = curve.GetPoint((float) i / lineSteps);
            
            Handles.DrawLine(point, point + curve.GetDirection((float) i / lineSteps) * directionScale);
        }
    }

    private Vector3 ShowPoint(int index) {
        // transform point to world space
        Vector3 point = handleTransform.TransformPoint(curve.points[index]);

        EditorGUI.BeginChangeCheck();
        {
            point = Handles.DoPositionHandle(point, handleRotation);
        }
        if (EditorGUI.EndChangeCheck()) { // if handle changed
            // so we can undo the drag operations
            Undo.RecordObject(curve, "Move Point");
            // after record undo we have to call this
            EditorUtility.SetDirty(curve);

            // assign the new position
            curve.points[index] = handleTransform.InverseTransformPoint(point);
        }
        return point;
    }

}