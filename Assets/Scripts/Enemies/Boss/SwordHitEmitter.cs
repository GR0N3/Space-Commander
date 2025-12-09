using UnityEngine;

public class SwordHitEmitter : MonoBehaviour
{
    [SerializeField] private int damageOverride = 0;
    [SerializeField] private string targetTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!HasTag(other, targetTag)) return;
        ApplySwordDamage(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        var other = collision.collider;
        if (!HasTag(other, targetTag)) return;
        ApplySwordDamage(other);
    }

    void ApplySwordDamage(Collider other)
    {
        var boss = GetComponentInParent<Boss>();
        int dmg = damageOverride > 0 ? damageOverride : (boss != null ? boss.SwordDamage : 0);
        if (dmg <= 0) return;
        var life = other.GetComponent<EntityLife>();
        if (life == null) life = other.GetComponentInParent<EntityLife>();
        if (life != null)
        {
            life.TakeDamage(dmg);
        }
        else
        {
            var ph = other.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(dmg);
        }
    }

    bool HasTag(Collider other, string tag)
    {
        if (other == null) return false;
        if (other.CompareTag(tag)) return true;
        Transform t = other.transform;
        if (t != null)
        {
            if (t.parent != null && t.parent.CompareTag(tag)) return true;
            if (t.root != null && t.root.CompareTag(tag)) return true;
        }
        return false;
    }
}
