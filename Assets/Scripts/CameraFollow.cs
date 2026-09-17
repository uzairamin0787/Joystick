using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    public Vector3 offset = new Vector3(0f, 5f, -8f);

    public float smoothSpeed = 5f;

    private void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 targetPosition = player.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.LookAt(player);
    }
}