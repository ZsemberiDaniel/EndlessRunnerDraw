using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bezier {
    public class BezierSpline : MonoBehaviour {

        /// <summary>
        /// To how many segments the spline should be split
        /// </summary>
        private int arcLengthQuality = 1000;
        public int ArcLengthQuality {
            get { return arcLengthQuality; }
            set { arcLengthQuality = value; }
        }

        // Points and their accessors
        [SerializeField]
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
        [SerializeField]
        private BezierControlPointMode[] modes;
        public BezierControlPointMode GetControlPointMode(int index) {
            return modes[(index + 1) / 3];
        }
        public void SetControlPointMode(int index, BezierControlPointMode mode) {
            modes[(index + 1) / 3] = mode;
            EnforceMode(index);
        }

        /// <summary>
        /// Color in the editor
        /// </summary>
        public Color color = Color.red;

        /// <summary>
        /// How many curves are in this spline
        /// </summary>
        public int CurveCount {
            get { return (points.Length - 1) / 3; }
        }

        private float splineLength = 0f;
        private float[] arcLengths;

        private void Start() {
            arcLengths = new float[arcLengthQuality + 1];

            Vector3 _lastPoint = GetPoint(0f);
            Vector3 _currPoint;
            // init the arc length array which contains the spline split into acrLengthQuality amount
            // of points which are not equidistant from each other but we can make eqidistant points from them
            for (int i = 1; i <= arcLengthQuality; i++) {
                _currPoint = GetPoint((float) i / arcLengthQuality);

                arcLengths[i] = arcLengths[i - 1] + Vector3.Distance(_lastPoint, _currPoint);
                _lastPoint = _currPoint;
            }
            splineLength = arcLengths[arcLengths.Length - 1];
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
        /// Returns a point on this bezier curve with a constant velocity
        /// </summary>
        public Vector3 EquidistGetPoint(float u) {
            float _targetArcLength = u * splineLength;
            int _low = 0, _high = arcLengths.Length, _index = 0;

            // first we do a simple binary search on our stored lengths to find the largest
            /// length that's smaller then targetArcLength
            while (_low < _high) {
                _index = _low + (((_high - _low) / 2) | 0);

                if (arcLengths[_index] < _targetArcLength) {
                    _low = _index + 1;
                } else {
                    _high = _index;
                }
            }
            // make sure we have one at _index that is smaller than _targetArcLength
            if (arcLengths[_index] > _targetArcLength) _index--;

            float _lengthBefore = arcLengths[_index];

            // we just return or do the interpolation and return
            if (_lengthBefore == _targetArcLength)
                return GetPoint((float) _index / arcLengths.Length);
            else
                return GetPoint(
                    (_index + (_targetArcLength - _lengthBefore) / (arcLengths[_index + 1] - _lengthBefore))
                        / arcLengthQuality
                );
        }

        /// <summary>
        /// Adds a new curve at the end of this spline
        /// </summary>
        public void AddCurve() {
            // resize array, add points
            Vector3 _point = points[points.Length - 1];
            Vector3 _add = (points[points.Length - 1] - points[points.Length - 2]).normalized;
            Array.Resize(ref points, points.Length + 3);

            for (int i = 3; i >= 1; i--) {
                _point += _add;
                points[points.Length - i] = _point;
            }

            // mode array
            Array.Resize(ref modes, modes.Length + 1);
            modes[modes.Length - 1] = modes[modes.Length - 2];

            // make sure new points are enforced
            EnforceMode(points.Length - 4);
        }
        public void RemoveCurve(int index) {
            int _modeIndex = (index + 1) / 3;
            int _middleIndex = _modeIndex * 3;

            // Remove from points
            // if at start we only need to remove 2 points
            int _removeCount = _middleIndex == 0 ? 2 : 3;
            for (int i = _middleIndex + 2; i < points.Length; i++)
                if (i - _removeCount >= 0)
                    points[i - _removeCount] = points[i];

            Array.Resize(ref points, points.Length - _removeCount);

            // Remove control points
            for (int i = _modeIndex + 1; i < modes.Length; i++) {
                modes[i - 1] = modes[i];
            }
            Array.Resize(ref modes, modes.Length - 1);
        }

        /// <summary>
        /// Enforces the modes from the array to this spline
        /// </summary>
        /// <param name="index"></param>
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

        public BezierSpline GetShiftedSpline(float amount, float safeDist = 0.3f, BezierSpline newSpline = null) {
            BezierSpline _shiftedSpline;
            if (newSpline != null) {
                _shiftedSpline = newSpline;
            } else {
                _shiftedSpline = new GameObject().AddComponent<BezierSpline>();
            }
            _shiftedSpline.Clear();
            _shiftedSpline.name = "Lane" + amount;

            // So, you cannot offset a Bézier curve perfectly with another Bézier curve, no matter how high-order you make that
            // other Bézier curve. However, we can chop up a curve into "safe" sub-curves (where safe means that all the control
            // points are always on a single side of the baseline, and the midpoint of the curve at t=0.5 is roughly in the centre
            // of the polygon defined by the curve coordinates) and then point-scale each sub-curve with respect to its scaling
            // origin (which is the intersection of the point normals at the start and end points).

            // A good way to do this reduction is to first find the curve's extreme points, and use these as initial splitting points.
            // After this initial split, we can check each individual segment to see if it's "safe enough" based on where the
            // center of the curve is.If the on-curve point for t = 0.5 is too far off from the center, we simply split the 
            // segment down the middle. Generally this is more than enough to end up with safe segments.

            Vector3 _p0 = points[0];
            Vector3 _p1, _p2, _p3;
            List<Vector3[]> _newPointsSpline = new List<Vector3[]>();
            for (int i = 3; i < Length; i += 3) {
                _p1 = points[i - 2];
                _p2 = points[i - 1];
                _p3 = points[i];

                // roots of our cubic bezier curve to find extremes
                SortedSet<float> _solutions = new SortedSet<float>();
                {
                    float? _sol1, _sol2;

                    Bezier.CubicGetRoots(_p0.x, _p1.x, _p2.x, _p3.x, out _sol1, out _sol2);
                    if (_sol1.HasValue && _sol1.Value < 1 && _sol1.Value > 0) _solutions.Add(_sol1.Value);
                    if (_sol2.HasValue && _sol2.Value < 1 && _sol2.Value > 0) _solutions.Add(_sol2.Value);

                    Bezier.CubicGetRoots(_p0.y, _p1.y, _p2.y, _p3.y, out _sol1, out _sol2);
                    if (_sol1.HasValue && _sol1.Value < 1 && _sol1.Value > 0) _solutions.Add(_sol1.Value);
                    if (_sol2.HasValue && _sol2.Value < 1 && _sol2.Value > 0) _solutions.Add(_sol2.Value);

                    Bezier.CubicGetRoots(_p0.z, _p1.z, _p2.z, _p3.z, out _sol1, out _sol2);
                    if (_sol1.HasValue && _sol1.Value < 1 && _sol1.Value > 0) _solutions.Add(_sol1.Value);
                    if (_sol2.HasValue && _sol2.Value < 1 && _sol2.Value > 0) _solutions.Add(_sol2.Value);
                }

                // the list of new points of the offset curve
                List<Vector3[]> _newPointsCurve = new List<Vector3[]>();

                // So we're gonna need to split the starting curve at potentially more points
                // We do that by splitting in order and always taking the second split curve as the next
                // starting curve
                {
                    Vector3[] _first, _second = new Vector3[] { _p0, _p1, _p2, _p3 };
                    float _secondSize = 1f; // we need this to know how big the second curve is compared to the very first starting curve
                                           // so we can split at the right point
                    foreach(float sol in _solutions) {
                        Bezier.CubicSplitCurve(_second[0], _second[1], _second[2], _second[3], _secondSize * sol, out _first, out _second);

                        _secondSize *= (1f - sol);
                        _newPointsCurve.Add(_first);
                    }
                    _newPointsCurve.Add(_second); // no more calculations with the second curve, so we can add it to the list
                }

                // check whether each bezier curve is safe or not if not split it
                {
                    int _at = _newPointsCurve.Count - 1;
                
                    while (_at >= 0) {
                        Vector3[] _split1, _split2;
                        // where the curv t = 0.5f
                        Vector3 _zeroDotFive = Bezier.CubicGetPoint(_newPointsCurve[_at][0], _newPointsCurve[_at][1],
                            _newPointsCurve[_at][2], _newPointsCurve[_at][3], 0.5f);
                        // the center of the curve's 4 points
                        Vector3 _center = (_newPointsCurve[_at][0] + _newPointsCurve[_at][1] + _newPointsCurve[_at][2] + _newPointsCurve[_at][3]) / 4f;
                    
                        // if they are too far away
                        if (Vector3.Distance(_zeroDotFive, _center) > safeDist) {
                            // split curva at 0.5f
                            Bezier.CubicSplitCurve(_newPointsCurve[_at][0], _newPointsCurve[_at][1], _newPointsCurve[_at][2], _newPointsCurve[_at][3], 0.5f,
                                out _split1, out _split2);

                            // overwrite current curve
                            _newPointsCurve[_at] = _split2;
                            // add one before it
                            _newPointsCurve.Insert(_at, _split1);
                            // we need to check the _split too again as well, so add one to at
                            _at++;
                        } else {
                            // no problem with this curve, move on
                            _at--;
                        }
                    }
                }

                // scale 
                { 
                    Vector3 _pivot;

                    for (int k = 0; k < _newPointsCurve.Count; k++) {
                        // get pivot -> the intersection of the point normals at the start and end points
                        bool _doIntersect = Math3D.LineLineIntersection(
                            out _pivot,
                            _newPointsCurve[k][0],
                            (Quaternion.LookRotation(Vector3.right) * Bezier.CubicGetFirstDerivative(_newPointsCurve[k][0], _newPointsCurve[k][1], _newPointsCurve[k][2], _newPointsCurve[k][3], 0.01f)).normalized,
                            _newPointsCurve[k][3],
                            (Quaternion.LookRotation(Vector3.right) * Bezier.CubicGetFirstDerivative(_newPointsCurve[k][0], _newPointsCurve[k][1], _newPointsCurve[k][2], _newPointsCurve[k][3], 0.99f)).normalized
                        );

                        // scaling
                        if (!_doIntersect) { // it's essentially a line
                            // which way the right is from the line
                            Vector3 _whichWay = Vector3.Cross(_newPointsCurve[k][3] - _newPointsCurve[k][0], Vector3.up).normalized;
                        
                            for (int j = 0; j < _newPointsCurve[k].Length; j++)
                                _newPointsCurve[k][j] -= _whichWay * amount;
                        } else {
                            // cache this
                            Vector3 _middleDerivative = Bezier.CubicGetFirstDerivative(
                                    _newPointsCurve[k][0],
                                    _newPointsCurve[k][1],
                                    _newPointsCurve[k][2],
                                    _newPointsCurve[k][3], 0.5f);
                        
                            // which way is the middle point's right
                            Vector3 _curveMiddleRight = (Quaternion.LookRotation(Vector3.right) * _middleDerivative).normalized;
                            // which way is the middle point's left
                            Vector3 _curveMiddleLeft = (Quaternion.LookRotation(Vector3.left) * _middleDerivative).normalized;

                            for (int j = 0; j < _newPointsCurve[k].Length; j++) {
                                Vector3 _scaleDir = (_pivot - _newPointsCurve[k][j]).normalized;

                                // pivot is to the right
                                bool _isPivotToRight = Vector3.Angle(_scaleDir, _curveMiddleRight) <
                                    Vector3.Angle(_scaleDir, _curveMiddleLeft);

                                // move to position based on pivot direction
                                if (_isPivotToRight) {
                                    _newPointsCurve[k][j] += _scaleDir * amount;
                                } else {
                                    _newPointsCurve[k][j] -= _scaleDir * amount;
                                }
                            }
                    }
                    }
                }

                _newPointsSpline.AddRange(_newPointsCurve);

                _p0 = _p3;
            }

            _shiftedSpline.points = new Vector3[_newPointsSpline.Count * 3 + 1];
            _shiftedSpline.points[0] = _newPointsSpline[0][0];

            _shiftedSpline.modes = new BezierControlPointMode[_newPointsSpline.Count + 1];
            _shiftedSpline.modes[0] = BezierControlPointMode.Free;

            for (int i = 0; i < _newPointsSpline.Count; i++) {
                _shiftedSpline.modes[i + 1] = BezierControlPointMode.Free;

                for (int k = 1; k <= 3; k++)
                    _shiftedSpline.points[i * 3 + k] = _newPointsSpline[i][k];
            }

            return _shiftedSpline;
        }

        public void Clear() {
            points = new Vector3[0];
            modes = new BezierControlPointMode[0];
        }
        public void Reset() {
            points = new Vector3[] {
                new Vector3(0f, 0f, 0f),
                new Vector3(0f, 0f, 1f),
                new Vector3(0f, 0f, 2f),
                new Vector3(0f, 0f, 3f)
            };
            modes = new BezierControlPointMode[] {
                BezierControlPointMode.Free,
                BezierControlPointMode.Free
            };
        }
    }
}