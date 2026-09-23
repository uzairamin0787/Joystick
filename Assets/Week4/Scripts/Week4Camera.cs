using UnityEngine;
namespace MysticJungle
{
    public class Week4Camera : MonoBehaviour
    {
        public Transform player;
        Vector3 velocity;
        void LateUpdate()
        {
            if (!player) return;
            transform.position = Vector3.SmoothDamp(transform.position, player.position + new Vector3(0, 5.5f, -8), ref velocity, .16f, Mathf.Infinity, Time.unscaledDeltaTime);
            transform.LookAt(player.position + new Vector3(0, 1.2f, 3));
        }
    }
}
