using System;
using UnityEngine;

public class LevelComponent : MonoBehaviour {

    [Tooltip("If true the middle lane will be expande to the other lanes")]
    [SerializeField]
    private bool expandMiddleLane = false;

    /// <summary>
    /// Used when expandMiddleLane is true. Should not be modified during runtime
    /// </summary>
    [SerializeField]
    private int laneCount = 3;

    [SerializeField]
    private float laneWidth = 2f;

    /// <summary>
    /// When we want the other lanes to be calculated this will be the middle lane
    /// </summary>
    [SerializeField]
    private BezierSpline defaultLane;

    [SerializeField]
    private BezierSpline[] lanes;

    public Vector3 GetPositionOnLane(float t, int lane) {
        return lanes[lane].GetPoint(t);
    }

    /// <summary>
    /// Calculates the other lanes (meaning other than middle) based on the laneCount, laneWidth
    /// and defaultLane class variables
    /// </summary>
    public void CalculateLanesBasedOnDefaultLane() {
        if (Application.isPlaying) return;
        
        Array.Resize(ref lanes, laneCount);

        int middleIndex = laneCount / 2;
        for (int i = 0; i < laneCount; i++) {
            // not the middle lane
            if (i != middleIndex) {
                if (lanes[i] != null) DestroyImmediate(lanes[i].gameObject);

                lanes[i] = defaultLane.GetShiftedSpline((i - middleIndex) * laneWidth);
                lanes[i].transform.parent = transform;
                lanes[i].color = UnityEngine.Random.ColorHSV();
            }
        }
        lanes[middleIndex] = defaultLane;
    }
	
}
