using System.Collections.Generic;
using UnityEngine;

public abstract class InputMonoBehaviour : MonoBehaviour {

    /// <summary>
    /// The time between the taking of samples
    /// </summary>
    [Header("Draw input")]
    [SerializeField]
    protected float sampleTime = 0.001f;
    private float sampleTimer = 0f;
    
    [SerializeField]
    protected TemplateGroup templateGroup;

    [SerializeField]
    protected FadingLineRenderer fadingLineRenderer;

    [SerializeField]
    private LineRenderer tempLineRenderer;

    // the drawn line's points
    private List<Vector2> points = new List<Vector2>();
    private List<Vector2> processedPoints = new List<Vector2>();

    protected void Update() {
#if UNITY_ANDROID
        if (Input.touchCount == 1) {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase) {
                case TouchPhase.Began:
                    TouchStarted(touch.position);
                    break;
                case TouchPhase.Moved:
                    TouchMoved(touch.position, Time.deltaTime);
                    break;
                case TouchPhase.Ended:
                    TouchEnded(touch.position);
                    break;
            }
        }
#endif
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0)) TouchStarted(Input.mousePosition);
        if (Input.GetMouseButton(0)) TouchMoved(Input.mousePosition, Time.deltaTime);
        if (Input.GetMouseButtonUp(0)) TouchEnded(Input.mousePosition);
#endif
	}

    /// <summary> 
    /// Called when a touch was started
    /// </summary>
    private void TouchStarted(Vector2 pos) {
        points.Clear();
        fadingLineRenderer.Reset();
    }
    /// <summary>
    /// Called every time the started touch is moved
    /// </summary>
    private void TouchMoved(Vector2 pos, float delta) {
        sampleTimer += delta;

        if (sampleTimer >= sampleTime) {
            sampleTimer = 0f;

            points.Add(pos);

            fadingLineRenderer.AddPoint(pos - Camera.main.pixelRect.size / 2f);
        }
    }
    /// <summary>
    /// Called when the touch ended
    /// </summary>
    private void TouchEnded(Vector2 pos) {
        processedPoints = ShapeRecognizer.DoEverything(points);

        tempLineRenderer.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++) tempLineRenderer.SetPosition(i, points[i] * Camera.main.pixelHeight / 2f);

        var _template = ShapeRecognizer.GetBestMatch(processedPoints, templateGroup);
        
        // Recognize swipe if it's in the template provided by user
        if (_template.Name == "Swipe") {
            Swipe(points[0], points[points.Count - 1]);
        }
    }

    #region Swipe
    private void Swipe(Vector2 from, Vector2 to) {
        float _dX = to.x - from.x;
        float _dY = to.y - from.y;
        SwipeType _type;
        
        // Horizontal swipe
        if (Mathf.Abs(_dX) > Mathf.Abs(_dY)) {
            if (_dX < 0) _type = SwipeType.left;
            else _type = SwipeType.right;
        } else { // Vertical swipe
            if (_dY < 0) _type = SwipeType.down;
            else _type = SwipeType.up;
        }

        Swipe(from, to, _type);
    }
    protected virtual void Swipe(Vector2 from, Vector2 to, SwipeType type) { }

    protected enum SwipeType {
        up, down, right, left
    }
    #endregion
}