using UnityEngine;

[CreateAssetMenu(fileName = "NewBulletData", menuName = "Flyweight/BulletData", order = 1)]
public class BulletData : ScriptableObject
{
    [Header("Bullet Shared Data (Flyweight)")]
    public float speed = 10f;
    public float lifeTime = 1.5f;
    public int damage = 1;
    public Sprite sprite;
    public AudioClip shootSound;
    public AudioClip hitSound;
    public float shootVolume = 1f;
    public float hitVolume = 1f;

    [Header("Owner Info")]
    public string ownerTag;
}
