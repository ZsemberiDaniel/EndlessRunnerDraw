using System.Collections.Generic;
using UnityEngine;

public class InputMonoBehaviour: MonoBehaviour {

    /// <summary>
    /// The time between the taking of samples
    /// </summary>
    private const float sampleTime = 0.001f;
    private float sampleTimer = 0f;
    
    private List<Vector2> points = new List<Vector2>();

    [SerializeField]
    private LineRenderer playerLineRenderer; 
    [SerializeField]
    private LineRenderer templateLinerederer;

    private void Update() {
        #if UNITY_ANDROID
        if (Input.touchCount == 1) {
            sampleTimer += Time.deltaTime;

            Touch touch = Input.GetTouch(0);

            switch (touch.phase) {
                case TouchPhase.Began:
                    points.Clear();
                    break;
                case TouchPhase.Moved:
                    if (sampleTimer >= sampleTime) {
                        sampleTimer = 0f;

                        points.Add(touch.position);
                    }
                    break;
                case TouchPhase.Ended:
                    ResampleListToEquidistantPoints();
                    break;
            }
        }
        #endif
        #if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0)) {
            points.Clear();
        }
        if (Input.GetMouseButton(0)) {
            sampleTimer += Time.deltaTime;

            if (sampleTimer >= sampleTime) {
                sampleTimer = 0f;

                points.Add(Input.mousePosition);
            }
        }
        if (Input.GetMouseButtonUp(0)) {
            points = ShapeRecognizer.DoEverything(points);
            playerLineRenderer.positionCount = 0;
            playerLineRenderer.positionCount = points.Count;

            for (int i = 0; i < points.Count; i++) {
                playerLineRenderer.SetPosition(i, points[i] * Camera.main.pixelHeight * 0.5f);
            }

            var template = Templates.GetBestMatch(points);
            templateLinerederer.positionCount = 0;
            templateLinerederer.positionCount = template.points.Count;

            for (int i = 0; i < template.points.Count; i++) {
                templateLinerederer.SetPosition(i, template.points[i] * Camera.main.pixelHeight * 0.5f);
            }

            Debug.Log(template.name);
        }
        #endif
        Update_();
	}

    

    /// <summary>
    /// This is the update method from the MonoBehaviour class
    /// </summary>
    protected virtual void Update_() { }
}
