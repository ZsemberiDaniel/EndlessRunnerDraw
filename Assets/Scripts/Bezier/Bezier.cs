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

}
