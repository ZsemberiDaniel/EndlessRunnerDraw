using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {

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

    
    
	private void Start() {
        rb = GetComponent<Rigidbody>();
	}
    
    private void Update() {
        #region Input



        // Detect swiping
        if (Input.touchCount == 1) {

        }

        #endregion

        MovePlayer();
        UpdateCamera();
	}

    private void MovePlayer() {
        rb.MovePosition(transform.position + transform.forward * maxSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// Updates the position and rotation of the camera
    /// </summary>
    private void UpdateCamera() {
        Vector3 _pos = transform.position - cameraDistance * transform.forward;
        _pos.y = transform.position.y + Mathf.Tan(Mathf.Deg2Rad * cameraAngle) * cameraDistance;

        cam.transform.SetPositionAndRotation(_pos, Quaternion.LookRotation(transform.position - cam.transform.position));
    }

    private enum SwipeDirection {
        left, right, up, down
    }
}