using UnityEngine;

public static class Bezier {

    public static Vector3 QuadraticGetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
        t = Mathf.Clamp01(t);
        float _oneMinusT = 1f - t;
        return _oneMinusT * _oneMinusT * p0 +
               2f * _oneMinusT * t * p1 +
               t * t * p2;
    }

    public static Vector3 QuadraticGetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
        return 2f * (1f - t) * (p1 - p0) +
               2f * t * (p2 - p1);
    }

    public static Vector3 CubicGetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
        t = Mathf.Clamp01(t);
        float _oneMinusT = 1f - t;
        return _oneMinusT * _oneMinusT * _oneMinusT * p0 +
               3f * _oneMinusT * _oneMinusT * t * p1 +
               3f * _oneMinusT * t * t * p2 +
               t * t * t * p3;
    }
    public static Vector3 CubicGetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
        t = Mathf.Clamp01(t);
        float _oneMinusT = 1f - t;
        return 3f * _oneMinusT * _oneMinusT * (p1 - p0) +
               6f * _oneMinusT * t * (p2 - p1) +
               3f * t * t * (p3 - p2);
    }
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
    public static void CubicSplitCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, out Vector3[] split1, out Vector3[] split2) {
        // https://stackoverflow.com/questions/18655135/divide-bezier-curve-into-two-equal-halves

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
