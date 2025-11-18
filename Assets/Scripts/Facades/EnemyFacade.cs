using UnityEngine;

[System.Obsolete("EnemyFacade está obsoleto. Usa EntityLife con LifeFacade en su lugar.")]
public class EnemyFacade : MonoBehaviour
{
    [Header("Explosion & Scoring")]
    public GameObject explosionPrefab;
    public int pointsOnDestroy = 100;
    public GameObject scoreTarget;

    public void DestroyEnemy()
    {
        
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (scoreTarget != null)
        {
            scoreTarget.SendMessage("AddScore", pointsOnDestroy, SendMessageOptions.DontRequireReceiver);
        }

        Destroy(gameObject);
    }
}
