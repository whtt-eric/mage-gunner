using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public Transform player; // set in unity

    private Camera _camera;

    void Awake() {
        _camera = Camera.main;
    }

    void Update() {
        _camera.transform.position = new Vector3(player.position.x, player.position.y, _camera.transform.position.z);
    }

    public static Vector3 clampPosition(Vector3 worldPosition) {
        Camera camera = Camera.main;
        Vector3 screenPosition = camera.WorldToScreenPoint(worldPosition);

        if (screenPosition.x < 0f) {
            Vector3 leftmostPoint = new Vector3(0f, screenPosition.y, screenPosition.z);
            worldPosition = camera.ScreenToWorldPoint(leftmostPoint);

        } else if (screenPosition.x > camera.pixelWidth) {
            Vector3 rightmostPoint = new Vector3(camera.pixelWidth, screenPosition.y, screenPosition.z);
            worldPosition = camera.ScreenToWorldPoint(rightmostPoint);
        }

        if (screenPosition.y < 0f) {
            Vector3 bottommostPoint = new Vector3(screenPosition.x, 0f, screenPosition.z);
            worldPosition = camera.ScreenToWorldPoint(bottommostPoint);

        } else if (screenPosition.y > camera.pixelHeight) {
            Vector3 topmostPoint = new Vector3(screenPosition.x, camera.pixelHeight, screenPosition.z);
            worldPosition = camera.ScreenToWorldPoint(topmostPoint);
        }

        return worldPosition;
    }
}
