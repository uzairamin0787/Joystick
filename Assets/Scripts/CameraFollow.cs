using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0f, 5f, -8f);

    [Header("Camera Follow")]
    public float smoothSpeed = 8f;

    private void FixedUpdate()
    {
        if (player == null)
            return;

        Vector3 targetPosition = player.position + offset;

        float t = 1f - Mathf.Exp(-smoothSpeed * Time.fixedDeltaTime);
        transform.position = Vector3.Lerp(transform.position, targetPosition, t);
    }
}