using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : InputMonoBehaviour {

    private Rigidbody rb;

    [Header("Camera")]
    [SerializeField]
    private Camera cam;
    [Range(5, 90)]
    [SerializeField]
    private int cameraAngle = 45;
    [SerializeField]
    private float cameraDistance = 1f;

    [Header("Attributes")]
    [SerializeField]
    private float maxSpeed = 2;

    /// <summary>
    /// Reset every fixed update tick
    /// </summary>
    private Vector3 playerVelocity = new Vector3();

    /// <summary>
    /// On which lane the player is. 0 - middle, negative - left, positive - right
    /// </summary>
    private int lane = 0;
    private Coroutine laneMovingCoroutine;

    [SerializeField]
    private LevelComponent currentLevel;
    float levelProgrssion = 0f;
    
    
    
	private void Start() {
        rb = GetComponent<Rigidbody>();
	}

    private new void Update() {
        base.Update();
    }
    
    private void FixedUpdate() {
        UpdateCamera();

        MovePlayer();
        playerVelocity = new Vector3();
    }

    /// <summary>
    /// Moves the player's rigidbody
    /// </summary>
    private void MovePlayer() {
        levelProgrssion += Time.fixedDeltaTime / 20f;

        rb.MovePosition(currentLevel.GetPositionOnLane(levelProgrssion, lane));
    }
    
    /// <summary>
    /// Updates the position and rotation of the camera
    /// </summary>
    private void UpdateCamera() {
        Vector3 _pos = transform.position - cameraDistance * transform.forward;
        _pos.y = transform.position.y + Mathf.Tan(Mathf.Deg2Rad * cameraAngle) * cameraDistance;

        cam.transform.SetPositionAndRotation(_pos, Quaternion.LookRotation(transform.position - cam.transform.position));
    }

    public void MoveToLeftLane() {
        MoveToLane(lane - 1);
    }
    public void MoveToRightLane() {
        MoveToLane(lane + 1);
    }
    public void MoveToLane(int lane) {
        if (lane == this.lane) return;

        if (laneMovingCoroutine != null) StopCoroutine(laneMovingCoroutine);
        laneMovingCoroutine = StartCoroutine(CoroutineMoveToLane(lane));
    }
    private IEnumerator CoroutineMoveToLane(int lane) {
        while (true) {
            playerVelocity += transform.right * (lane - this.lane) * maxSpeed;

            yield return new WaitForFixedUpdate();
        }

        this.lane = lane;
        laneMovingCoroutine = null;
    }

    protected override void Swipe(Vector2 from, Vector2 to, SwipeType type) {
        switch (type) {
            case SwipeType.left:
                lane--;
                break;
            case SwipeType.right:
                lane++;
                break;
        }
    }
}