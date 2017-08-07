using System;
using UnityEngine;

public class BezierSpline : MonoBehaviour {

    public Vector3[] points;

    /// <summary>
    /// How many curves are in this spline
    /// </summary>
    public int CurveCount {
        get { return (points.Length - 1) / 3; }
    }

    /// <summary>
    /// Return point on this bezier curve
    /// </summary>
    public Vector3 GetPoint(float t) {
        int i;
        if (t >= 1f) {
            t = 1f;
            i = points.Length - 4;
        } else {
            // To get to the actual points, we have to multiply the curve index by three
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int) t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.CubicGetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
    }

    public Vector3 GetVelocity(float t) {
        int i;
        if (t >= 1f) {
            t = 1f;
            i = points.Length - 4;
        } else {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int) t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.CubicGetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) -
            transform.position; // Because it produces a velocity vector and not a point,
        // it should not be affected by the position of the curve, so we subtract that after transforming.
    }

    public Vector3 GetDirection(float t) {
        return GetVelocity(t).normalized;
    }

    /// <summary>
    /// Adds a new curve at the end of this spline
    /// </summary>
    public void AddCurve() {
        Vector3 point = points[points.Length - 1];
        Array.Resize(ref points, points.Length + 3);

        point.x += 1f;
        points[points.Length - 3] = point;

        point.x += 1f;
        points[points.Length - 2] = point;

        point.x += 1f;
        points[points.Length - 1] = point;
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