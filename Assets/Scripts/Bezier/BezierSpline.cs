using System;
using UnityEngine;

public class BezierSpline : MonoBehaviour {

    // Points and their accessors
    private Vector3[] points;
    public int Length {
        get { return points.Length; }
    }
    public Vector3 this[int index] {
        get { return points[index]; }
        set {
            // if moving middle point move the others with it
            if (index % 3 == 0) {
                Vector3 delta = value - points[index];
                if (index > 0) points[index - 1] += delta;
                if (index < points.Length - 1) points[index + 1] += delta;
            } else { // if we moved middle then we moved the others too so no need for enforcement
                EnforceMode(index);
            }

            points[index] = value;
        }
    }

    // Modes and their accessors
    private BezierControlPointMode[] modes;
    public BezierControlPointMode GetControlPointMode(int index) {
        return modes[(index + 1) / 3];
    }
    public void SetControlPointMode(int index, BezierControlPointMode mode) {
        modes[(index + 1) / 3] = mode;
        EnforceMode(index);
    }

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
        // resize array, add points
        Vector3 _point = points[points.Length - 1];
        Array.Resize(ref points, points.Length + 3);

        _point.x += 1f;
        points[points.Length - 3] = _point;

        _point.x += 1f;
        points[points.Length - 2] = _point;

        _point.x += 1f;
        points[points.Length - 1] = _point;

        // mode array
        Array.Resize(ref modes, modes.Length + 1);
        modes[modes.Length - 1] = modes[modes.Length - 2];

        // make sure new points are enforced
        EnforceMode(points.Length - 4);
    }

    private void EnforceMode(int index) {
        int _modeIndex = (index + 1) / 3;

        // in these cases nothing should be enforced
        if (_modeIndex == 0 || _modeIndex == modes.Length - 1 || modes[_modeIndex] == BezierControlPointMode.Free)
            return;

        // middle point of the given handle
        int _middleIndex = _modeIndex * 3;

        int _fixedIndex;
        int _enforcedIndex;
        // When we change a point's mode, it is either a point in between curves or one of its neighbors
        // When we have the middle point selected, we can just keep the previous point fixed and enforce the constraints
        // on the point on the opposite side
        if (index <= _middleIndex) {
            _fixedIndex = _middleIndex - 1;
            _enforcedIndex = _middleIndex + 1;

        // If we have one of the other points selected, we should keep that one
        // fixed and adjust its opposite. That way our selected point always stays where it is
        } else {
            _fixedIndex = _middleIndex + 1;
            _enforcedIndex = _middleIndex - 1;
        }

        switch (modes[_modeIndex]) {
            // To mirror around the middle point, we have to take the vector from the middle to the fixed point
            // and invert it. This is the enforced tangent, and adding it to the middle gives us our enforced point.
            case BezierControlPointMode.Mirrored:
                points[_enforcedIndex] = points[_middleIndex] - (points[_fixedIndex] - points[_middleIndex]);
                break;
            // For the aligned mode, we also have to make sure that the new tangent has the same length as the old one
            // So we normalize it and then multiply by the distance between the middle and the old enforced point.
            case BezierControlPointMode.Aligned:
                points[_enforcedIndex] = points[_middleIndex] -
                    ( points[_fixedIndex] - points[_middleIndex]).normalized *
                      Vector3.Distance(points[_enforcedIndex], points[_middleIndex] );
                break;
        }
    }

    public void Reset() {
        points = new Vector3[] {
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(3f, 0f, 0f),
            new Vector3(4f, 0f, 0f)
        };
        modes = new BezierControlPointMode[] {
            BezierControlPointMode.Free,
            BezierControlPointMode.Free
        };
    }
}