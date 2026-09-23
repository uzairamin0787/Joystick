using UnityEngine;
namespace MysticJungle
{
    public class Week4Pickup : MonoBehaviour
    {
        public bool finish;
        [System.NonSerialized] public Week4Runner route;
        [System.NonSerialized] public long routeKey;
        public ParticleSystem effect;
        Vector3 origin;
        void OnEnable() { origin = transform.localPosition; }
        public void ResetOrigin() { origin = transform.localPosition; }
        void Update() { if (!finish) { transform.Rotate(0, 85 * Time.deltaTime, 0, Space.World); transform.localPosition = origin + Vector3.up * Mathf.Sin(Time.time * 2 + origin.z) * .12f; } }
        void OnTriggerEnter(Collider other)
        {
            if (!other.GetComponentInParent<Week4Player>() || !Week4Game.Instance.Playing) return;
            if (finish) Week4Game.Instance.Finish();
            else { if (route) route.MarkCollected(routeKey); Week4Game.Instance.Collect(); if (effect) { effect.transform.position = transform.position; effect.Play(); } gameObject.SetActive(false); }
        }
    }
}
