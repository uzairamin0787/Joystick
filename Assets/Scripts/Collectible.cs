using UnityEngine;

public class Collectible : MonoBehaviour
{
    public ParticleSystem collectParticles;
    public ScoreManager scoreManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Collectible Collected!");

            if (collectParticles != null)
            {
                collectParticles.transform.position = transform.position;
                collectParticles.Play();
            }

            if (scoreManager != null)
                scoreManager.AddScore();

            Destroy(gameObject);
        }
    }
}