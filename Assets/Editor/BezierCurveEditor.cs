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
        Vector3 _p0 = ShowPoint(0);
        Vector3 _p1 = ShowPoint(1);
        Vector3 _p2 = ShowPoint(2);
        Vector3 _p3 = ShowPoint(3);

        ShowDirections();
        // draw out curve
        Handles.DrawBezier(_p0, _p3, _p1, _p2, Color.red, EditorGUIUtility.whiteTexture, 1.3f);
    }

    private void ShowDirections() {
        Handles.color = Color.green;

        Vector3 _point;
        for (int i = 0; i <= lineSteps; i++) {
            _point = curve.GetPoint((float) i / lineSteps);
            
            Handles.DrawLine(_point, _point + curve.GetDirection((float) i / lineSteps) * directionScale);
        }
    }

    private Vector3 ShowPoint(int index) {
        // transform point to world space
        Vector3 _point = handleTransform.TransformPoint(curve.points[index]);

        EditorGUI.BeginChangeCheck();
        {
            _point = Handles.DoPositionHandle(_point, handleRotation);
        }
        if (EditorGUI.EndChangeCheck()) { // if handle changed
            // so we can undo the drag operations
            Undo.RecordObject(curve, "Move Point");
            // after record undo we have to call this
            EditorUtility.SetDirty(curve);

            // assign the new position
            curve.points[index] = handleTransform.InverseTransformPoint(_point);
            Undo.FlushUndoRecordObjects();
        }
        return _point;
    }

}