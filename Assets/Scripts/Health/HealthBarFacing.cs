using UnityEngine;

public class HealthBarFacing : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main; // Get the main camera
    }

    private void LateUpdate()
    {
        // Ensure the health bar faces the camera without flipping
        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0; // Keep the health bar upright
        transform.rotation = Quaternion.LookRotation(cameraForward);
    }
}
