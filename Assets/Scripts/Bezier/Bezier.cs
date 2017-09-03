using UnityEngine;

namespace Bezier {
    /// <summary>
    /// Class used for static bezier mathsss
    /// </summary>
    public static class BezierMath {
        /// <summary>
        /// Returns a point along the quadratic bezier curve at <paramref name="t"/>
        /// </summary>
        /// <param name="t">Clamped to [0;1]</param>
        public static Vector3 QuadraticGetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
            t = Mathf.Clamp01(t);
            float _oneMinusT = 1f - t;
            return _oneMinusT * _oneMinusT * p0 +
                   2f * _oneMinusT * t * p1 +
                   t * t * p2;
        }
        /// <summary>
        /// Returns the first derivative of the quadratic bezier curve at <paramref name="t"/>
        /// </summary>
        /// <param name="t">Clamped to [0;1]</param>
        public static Vector3 QuadraticGetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
            t = Mathf.Clamp01(t);
            return 2f * (1f - t) * (p1 - p0) +
                   2f * t * (p2 - p1);
        }


        /// <summary>
        /// Returns the point along the bezier curve at <paramref name="t"/>
        /// </summary>
        /// <param name="t">Clamped to [0;1]</param>
        public static Vector3 CubicGetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
            t = Mathf.Clamp01(t);
            float _oneMinusT = 1f - t;
            return _oneMinusT * _oneMinusT * _oneMinusT * p0 +
                   3f * _oneMinusT * _oneMinusT * t * p1 +
                   3f * _oneMinusT * t * t * p2 +
                   t * t * t * p3;
        }
        /// <summary>
        /// Returns the first derivative of the given bezier curve at <paramref name="t"/>
        /// </summary>
        /// <param name="t">Clamped to [0;1]</param>
        public static Vector3 CubicGetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
            t = Mathf.Clamp01(t);
            float _oneMinusT = 1f - t;
            return 3f * _oneMinusT * _oneMinusT * (p1 - p0) +
                   6f * _oneMinusT * t * (p2 - p1) +
                   3f * t * t * (p3 - p2);
        }
        /// <summary>
        /// Returns the roots of the given axis of the cubic bezier curve
        /// </summary>
        /// <param name="sol1">Null if it has no solution</param>
        /// <param name="sol2">Null if it either has no or only one solution</param>
        public static void CubicGetRoots(float c0, float c1, float c2, float c3, out float? sol1, out float? sol2) {
            float a = 3 * (-c0 + 3 * c1 - 3 * c2 + c3);
            float b = 6 * (c0 - 2 * c1 + c2);
            float c = 3 * (c1 - c0);

            float D = b * b - 4 * a * c;
            if (D < 0) {
                sol1 = sol2 = null;
            } else if (D == 0) {
                sol1 = -b / (2 * a);
                sol2 = null;
            } else {
                D = Mathf.Sqrt(D);
                sol1 = (-b + D) / (2 * a);
                sol2 = (-b - D) / (2 * a);
            }
        }
        /// <summary>
        /// Returns the roots of the given bezier curve for all axis
        /// </summary>
        /// <param name="sol1">Has null in axis if it has no solution</param>
        /// <param name="sol2">Has null in axis if it either has no or only one solution</param>
        public static void CubicGetRoots(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
                out System.Tuple<float?, float?, float?> sol1, out System.Tuple<float?, float?, float?> sol2) {
            float? _solNmbX1, _solNmbX2;
            float? _solNmbY1, _solNmbY2;
            float? _solNmbZ1, _solNmbZ2;

            CubicGetRoots(p0.x, p1.x, p2.x, p3.x, out _solNmbX1, out _solNmbX2);
            CubicGetRoots(p0.x, p1.x, p2.x, p3.x, out _solNmbY1, out _solNmbY2);
            CubicGetRoots(p0.x, p1.x, p2.x, p3.x, out _solNmbZ1, out _solNmbZ2);

            // output
            sol1 = new System.Tuple<float?, float?, float?>(_solNmbX1, _solNmbY1, _solNmbZ1);
            sol2 = new System.Tuple<float?, float?, float?>(_solNmbX2, _solNmbY2, _solNmbZ2);

        }
        /// <summary>
        /// Splits a cubic bezier curve then returns the two new cubic bezier curves
        /// </summary>
        /// <param name="split1">Array of length 4. Will be reset</param>
        /// <param name="split2">Array of length 4. Will be reset</param>
        public static void CubicSplitCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t,
                out Vector3[] split1, out Vector3[] split2) {
            // https://stackoverflow.com/questions/18655135/divide-bezier-curve-into-two-equal-halves

            t = Mathf.Clamp01(t);
            float _oneMinusT = 1 - t;

            split1 = new Vector3[4]; // A, E, H, K
            split2 = new Vector3[4]; // K, J, G ,D

            Vector3 F = _oneMinusT * p1 + t * p2; // B, C -> F

            split1[0] = p0; // A
            split1[1] = _oneMinusT * p0 + t * p1; // A, B -> E
            split1[2] = _oneMinusT * split1[1] + t * F; // E, F -> H

            split2[3] = p3; // D
            split2[2] = _oneMinusT * p2 + t * p3; // C, D -> G
            split2[1] = _oneMinusT * F + t * split2[2]; // F, G -> J

            split1[3] = split2[0] = _oneMinusT * split1[2] + t * split2[1]; // H, J -> K
        }

    }
}