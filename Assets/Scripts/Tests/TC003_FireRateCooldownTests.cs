using NUnit.Framework;
using UnityEditor;
using UnityEngine;

[TestFixture]
[Category("Disparo")]
[Category("AnalisisDeValoresLimite")]
public class TC003_FireRateCooldownTests
{
    private const float FireCooldownSeconds = 0.5f;
    private const float HoldTimeSeconds = 3.0f;

    private GameObject playerObject;
    private PlayerFacade playerFacade;
    private BulletShoot bulletShoot;
    private IWeapon weaponInterface;
    private ObjectPool bulletObjectPool;
    private GameObject bulletPrefab;
    private BulletData playerBulletData;
    private bool createdRuntimeBulletData;

    [SetUp]
    public void SetUp()
    {
        bulletPrefab = new GameObject("BulletPrefab_TC003");
        bulletPrefab.AddComponent<SphereCollider>();
        bulletPrefab.AddComponent<Rigidbody>();
        bulletPrefab.AddComponent<Bullet>();
        bulletPrefab.SetActive(false);

        playerBulletData = AssetDatabase.LoadAssetAtPath<BulletData>("Assets/Scripts/Objects/PlayerBulletData.asset");
        if (playerBulletData == null)
        {
            playerBulletData = ScriptableObject.CreateInstance<BulletData>();
            playerBulletData.damage = 50;
            playerBulletData.speed = 50f;
            playerBulletData.lifeTime = 5f;
            playerBulletData.ownerTag = "Player";
            createdRuntimeBulletData = true;
        }

        playerObject = new GameObject("Player_TC003");
        playerObject.tag = "Player";

        bulletObjectPool = playerObject.AddComponent<ObjectPool>();
        bulletObjectPool.ObjPrefab = bulletPrefab;
        bulletObjectPool.PoolSize = 20;

        bulletShoot = playerObject.AddComponent<BulletShoot>();
        bulletShoot.ObjectPool = bulletObjectPool;
        bulletShoot.BulletData = playerBulletData;
        bulletShoot.BaseFireRate = FireCooldownSeconds;
        bulletShoot.FirePoint = playerObject.transform;
        weaponInterface = bulletShoot;

        playerFacade = playerObject.AddComponent<PlayerFacade>();
        playerFacade.playerShooting = bulletShoot;
        playerFacade.weapon = bulletShoot;
    }

    [TearDown]
    public void TearDown()
    {
        if (bulletObjectPool != null && bulletObjectPool.Pool != null)
        {
            foreach (var b in bulletObjectPool.Pool)
            {
                if (b != null) Object.DestroyImmediate(b);
            }
        }
        if (playerObject != null) Object.DestroyImmediate(playerObject);
        if (bulletPrefab != null) Object.DestroyImmediate(bulletPrefab);
        if (createdRuntimeBulletData && playerBulletData != null) Object.DestroyImmediate(playerBulletData);
    }

    [Test]
    [Description("TC-003: Verificar que al mantener presionado el botón izquierdo durante 3 s con cooldown de 0.5 s se disparan entre 6 y 7 balas.")]
    public void TC003_MantenerBotonIzquierdo3Segundos_DisparaEntre6Y7Balas()
    {
        int shotsFired = 0;
        const int totalFrames = 60;
        float dt = HoldTimeSeconds / totalFrames;

        for (int i = 0; i < totalFrames; i++)
        {
            if (bulletShoot.SimulateUpdate(true, dt))
            {
                shotsFired++;
            }
        }

        int activeBulletsInPool = 0;
        foreach (var b in bulletObjectPool.Pool)
        {
            if (b != null && b.activeSelf) activeBulletsInPool++;
        }

        Assert.IsNotNull(weaponInterface, "BulletShoot debe implementar la interfaz IWeapon.");
        Assert.AreEqual(shotsFired, activeBulletsInPool,
            "Cada disparo debe activar una instancia de Bullet desde el ObjectPool.");
        Assert.That(shotsFired, Is.InRange(6, 7),
            $"Se esperaban entre 6 y 7 balas en 3 s con cooldown de 0.5 s, pero se dispararon {shotsFired}.");
    }

    [TestCase(0.49f, false, TestName = "TC003_AVL_CooldownVecinoInferior_0_49s_NoDispara")]
    [TestCase(0.50f, true, TestName = "TC003_AVL_CooldownValorLimiteExacto_0_50s_SiDispara")]
    [TestCase(0.51f, true, TestName = "TC003_AVL_CooldownVecinoSuperior_0_51s_SiDispara")]
    public void TC003_AnalisisValoresLimite_VecinosDelCooldown05s(float elapsedAfterFirstShot, bool expectedSecondShot)
    {
        bool firstShot = bulletShoot.SimulateUpdate(true, 0f);
        Assert.IsTrue(firstShot, "El primer disparo en t = 0 debe ejecutarse.");

        bool secondShot = bulletShoot.SimulateUpdate(true, elapsedAfterFirstShot);

        Assert.AreEqual(expectedSecondShot, secondShot);
    }

    [Test]
    [Description("TC-003 (Caja Blanca - Condition Coverage): Evaluar las 4 combinaciones de botón presionado y cooldown.")]
    public void TC003_CajaBlanca_ConditionCoverage_BotonYCooldown()
    {
        Assert.IsFalse(bulletShoot.SimulateUpdate(false, 0.1f));
        Assert.IsTrue(bulletShoot.SimulateUpdate(true, 0.1f));
        Assert.IsFalse(bulletShoot.SimulateUpdate(true, 0.1f));
        Assert.IsFalse(bulletShoot.SimulateUpdate(false, 0.1f));
    }
}
