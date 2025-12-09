using UnityEngine;

public class BossFightFacade : MonoBehaviour
{
    [SerializeField] private Boss boss;
    [SerializeField] private BossBulletPool bossBulletPool;
    [SerializeField] private BulletData bossBulletData;
    [SerializeField] private Transform firePoint;

    public void StartFight()
    {
        if (boss == null)
        {
            boss = FindFirstObjectByType<Boss>();
        }
        if (bossBulletPool == null)
        {
            bossBulletPool = BossBulletPool.Instance;
        }
    }

    public void StopFight()
    {
        if (boss != null)
        {
            boss.enabled = false;
        }
    }

    public void TriggerSwordAttack()
    {
        if (boss == null) return;
        boss.SwordAttack();
    }

    public void TriggerGunAttack()
    {
        if (boss == null) return;
        boss.GunAttack();
    }

    public void ShootFromCannon()
    {
        if (firePoint == null) firePoint = boss != null ? boss.transform : null;
        if (bossBulletPool == null || bossBulletData == null || firePoint == null) return;
        var obj = bossBulletPool.GetBullet(firePoint.position, firePoint.rotation);
        var bullet = obj != null ? obj.GetComponent<Bullet>() : null;
        if (bullet != null) bullet.Initialize(bossBulletData);
    }
}
