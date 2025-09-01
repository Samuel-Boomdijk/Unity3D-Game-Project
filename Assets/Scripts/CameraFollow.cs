using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameObject player;
    private Vector3 offset;

    public float rotationSpeed = 5f; // Speed of camera rotation
    private float wallDetectionDistance = 12f; // Distance from the camera to detect walls
    public LayerMask wallLayer;    // Layer for wall detection (assign in Inspector)
    private bool isRotated = false; // Tracks if the camera has rotated to avoid a wall

    private Vector3 cameraStartRotation;

    private void Start()
    {
        player = GameObject.FindWithTag("Player");

        transform.position = player.transform.position + new Vector3(-0.46f, 15.07f, -16.46f);
        offset = transform.position - player.transform.position;
    }

    private void LateUpdate()
    {
        // Perform a raycast from the camera to detect walls
        RaycastHit hit;
        Vector3 desiredPosition = player.transform.position + offset;

        if (Physics.Raycast(player.transform.position, offset.normalized, out hit, wallDetectionDistance, wallLayer))
        {
            // A wall is detected, rotate the camera to the opposite side
            if (!isRotated)
            {
                offset = Quaternion.Euler(0, 180, 0) * offset; // Flip the offset to the opposite side
                transform.rotation = Quaternion.Euler(36, 180, 0); // Flip the camera also (numbers are as desired)
                isRotated = true;
            }
            else
            {
                // No wall detected, restore the default camera position
                if (isRotated)
                {
                    offset = Quaternion.Euler(0, 180, 0) * offset; // Flip back to the default side
                    transform.rotation = Quaternion.Euler(36, 0, 0); // Flip the camera also (numbers are as desired)
                    isRotated = false;
                }
            }
        }

        //Finally set the camera's position
        transform.position = desiredPosition;
    }
}
