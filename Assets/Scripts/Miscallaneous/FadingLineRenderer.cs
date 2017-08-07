using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FadingLineRenderer : MonoBehaviour {

    private LineRenderer lineRenderer;

    private ExpandingArray<float> addedAt = new ExpandingArray<float>(32);

    [Tooltip("After how much time the points should disappear")]
    [SerializeField]
    private float fadeTime = 0.5f;
    
	void Start() {
        lineRenderer = GetComponent<LineRenderer>();

        if (fadeTime <= 0) {
            Debug.LogError("FadeTime should never ever be 0 or lower!", this);
        }
	}

    private void Update() {
        // If addedAt time is smaller or equal to this then that pos needs to go 
        float timeLimit = Time.time - fadeTime;
        int i;
        for (i = 0; i < lineRenderer.positionCount && addedAt[i] <= timeLimit && addedAt[i] != 0; i++);
        // So now i contains how many positions we need to delet from the start of the lineRenderer
        if (i > 0) DeletePositions(i);
    }

    /// <summary>
    /// Deletes count positions from the start of lineRenderer and addedAt array
    /// </summary>
    private void DeletePositions(int count) {
        for (int i = count; i < lineRenderer.positionCount; i++) {
            lineRenderer.SetPosition(i - count, lineRenderer.GetPosition(i));
            addedAt[i - count] = addedAt[i];
        }

        lineRenderer.positionCount -= count;
    }

    /// <summary>
    /// Add a point to the end of the linerenderer
    /// </summary>
    public void AddPoint(Vector2 point) {
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, point);
        addedAt[lineRenderer.positionCount - 1] = Time.time;
    }
    
    public void Reset() {
        lineRenderer.positionCount = 0;
        for (int i = 0; i < addedAt.Length; i++) addedAt[i] = 0;
    }
}
