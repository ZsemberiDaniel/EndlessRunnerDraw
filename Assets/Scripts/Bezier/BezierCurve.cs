using UnityEngine;

namespace Bezier {
    public class BezierCurve : MonoBehaviour {

        [HideInInspector]
        public Vector3[] points;

        /// <summary>
        /// Return point on this bezier curve at <paramref name="t"/>
        /// </summary>
        /// <param name="t">Clamped to [0..1]</param>
        public Vector3 GetPoint(float t) {
            t = Mathf.Clamp01(t);
            return transform.TransformPoint(BezierMath.CubicGetPoint(points[0], points[1], points[2], points[3], t));
        }

        /// <summary>
        /// Returns the velocity of this bezier curve at <paramref name="t"/>
        /// </summary>
        /// <param name="t">Clamped to [0..1]</param>
        public Vector3 GetVelocity(float t) {
            t = Mathf.Clamp01(t);
            return transform.TransformPoint(BezierMath.CubicGetFirstDerivative(points[0], points[1], points[2], points[3], t)) -
                transform.position; // Because it produces a velocity vector and not a point,
            // it should not be affected by the position of the curve, so we subtract that after transforming.
        }

        /// <summary>
        /// Returns the direction of this bezier curve at <paramref name="t"/> (basically the normalized velocity)
        /// </summary>
        /// <param name="t">Clamped to [0..1]</param>
        public Vector3 GetDirection(float t) {
            t = Mathf.Clamp01(t);
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
}