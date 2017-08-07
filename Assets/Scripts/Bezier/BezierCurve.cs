using UnityEngine;

public class BezierCurve : MonoBehaviour {

    [HideInInspector]
    public Vector3[] points;

    /// <summary>
    /// Return point on this bezier curve
    /// </summary>
    /// <param name="t">[0..1]</param>
    public Vector3 GetPoint(float t) {
        return transform.TransformPoint(Bezier.CubicGetPoint(points[0], points[1], points[2], points[3], t));
    }

    public Vector3 GetVelocity(float t) {
        return transform.TransformPoint(Bezier.CubicGetFirstDerivative(points[0], points[1], points[2], points[3], t)) -
            transform.position; // Because it produces a velocity vector and not a point,
        // it should not be affected by the position of the curve, so we subtract that after transforming.
    }

    public Vector3 GetDirection(float t) {
        return GetVelocity(t).normalized;
    }

    public void Reset() {
        points = new Vector3[] {
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(3f, 0f, 0f),
            new Vector3(4f, 0f, 0f)
        };
    }
}
