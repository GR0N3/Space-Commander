using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("IAEnemiga")]
[Category("TransicionDeEstados")]
public class TC018_EnemyAIStateTransitionTests
{
    private const float VisionRange = 10f;
    private const float AlertDuration = 1f;

    private GameObject enemyObject;
    private EnemyController enemyController;
    private GameObject playerObject;
    private GameObject patrolPointA;
    private GameObject patrolPointB;
    private GameObject bulletPrefab;

    [SetUp]
    public void SetUp()
    {
        playerObject = new GameObject("Player_TC018");
        playerObject.tag = "Player";
        playerObject.transform.position = new Vector3(0f, 0f, 15f); // Inicialmente fuera de rango (15 u > 10 u)

        patrolPointA = new GameObject("PatrolPointA_TC018");
        patrolPointA.transform.position = new Vector3(-5f, 0f, 0f);
        patrolPointB = new GameObject("PatrolPointB_TC018");
        patrolPointB.transform.position = new Vector3(5f, 0f, 0f);

        bulletPrefab = new GameObject("EnemyBulletPrefab_TC018");
        bulletPrefab.AddComponent<SphereCollider>();
        bulletPrefab.AddComponent<Rigidbody>();
        bulletPrefab.AddComponent<Bullet>();
        bulletPrefab.SetActive(false);

        enemyObject = new GameObject("EnemyAI_TC018");
        enemyObject.tag = "Enemy";
        enemyObject.transform.position = Vector3.zero;
        enemyObject.transform.rotation = Quaternion.identity;

        enemyController = enemyObject.AddComponent<EnemyController>();
        enemyController.player = playerObject.transform;
        enemyController.visionRange = VisionRange;
        enemyController.alertDuration = AlertDuration;
        enemyController.rotationSpeed = 20f;
        enemyController.patrolPoints = new Transform[] { patrolPointA.transform, patrolPointB.transform };
        enemyController.restrictToPatrolArea = false;
        enemyController.enableRandomRotation = false;
        enemyController.useBulletPool = false;
        enemyController.projectilePrefab = bulletPrefab;
        enemyController.firePoint = enemyObject.transform;

        var bulletData = ScriptableObject.CreateInstance<BulletData>();
        bulletData.damage = 15;
        bulletData.ownerTag = "Enemy";
        enemyController.enemyBulletData = bulletData;

        enemyController.CurrentState = new PatrollingState();
        enemyController.CurrentState.EnterState(enemyController);
    }

    [TearDown]
    public void TearDown()
    {
        if (enemyController != null && enemyController.enemyBulletData != null)
        {
            Object.DestroyImmediate(enemyController.enemyBulletData);
        }
        foreach (var b in Object.FindObjectsByType<Bullet>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (b != null && b.gameObject != bulletPrefab)
            {
                Object.DestroyImmediate(b.gameObject);
            }
        }
        if (enemyObject != null) Object.DestroyImmediate(enemyObject);
        if (playerObject != null) Object.DestroyImmediate(playerObject);
        if (patrolPointA != null) Object.DestroyImmediate(patrolPointA);
        if (patrolPointB != null) Object.DestroyImmediate(patrolPointB);
        if (bulletPrefab != null) Object.DestroyImmediate(bulletPrefab);
    }

    [Test]
    [Description("TC-018: Verificar la transición de estados PatrollingState -> AlertState -> AttackingState al entrar el jugador en el rango de visión (<= 10 u) durante 1 s.")]
    public void TC018_JugadorEntraEnRangoDeVisionYPermanece1Segundo_EnemigoTransicionaDePatrolAAlertYAttack()
    {
        // Precondición: Jugador a 15 u (> 10 u) -> el enemigo permanece en PatrollingState
        Assert.IsFalse(enemyController.IsPlayerInSight(), "Precondición: a 15 u el jugador está fuera de visión.");
        enemyController.CurrentState.UpdateState(enemyController, 0.2f);
        Assert.That(enemyController.CurrentState, Is.InstanceOf<PatrollingState>(),
            "Fuera del rango de visión, el enemigo debe mantenerse en PatrollingState.");

        // Paso 1: Acercar al jugador a menos de 10 unidades (8 u en eje +Z, alineado al frente del enemigo)
        playerObject.transform.position = new Vector3(0f, 0f, 8f);
        Assert.IsTrue(enemyController.IsPlayerInSight(), "A 8 u (<= 10 u), IsPlayerInSight() debe ser true.");

        enemyController.CurrentState.UpdateState(enemyController, 0.1f);
        Assert.That(enemyController.CurrentState, Is.InstanceOf<AlertState>(),
            "Al detectar al jugador dentro de las 10 u, el enemigo debe transicionar a AlertState.");

        // Paso 2: Mantenerse dentro del rango durante 1 s (alertDuration = 1.0 s)
        enemyController.CurrentState.UpdateState(enemyController, 0.5f);
        Assert.That(enemyController.CurrentState, Is.InstanceOf<AlertState>(),
            "Antes de cumplirse 1 s en alerta (0.5 s), el enemigo debe seguir en AlertState.");

        enemyController.CurrentState.UpdateState(enemyController, 0.6f);
        Assert.That(enemyController.CurrentState, Is.InstanceOf<AttackingState>(),
            "Tras cumplirse 1 s alineado hacia el jugador, el enemigo debe transicionar a AttackingState.");
    }

    [Test]
    [Description("TC-018 (Transición inversa): Si el jugador sale del rango de visión (> 10 u) estando en Alert o Attacking, regresa a PatrollingState.")]
    public void TC018_JugadorSaleDelRangoDeVision_EnemigoRegresaAPatrollingState()
    {
        playerObject.transform.position = new Vector3(0f, 0f, 8f);
        enemyController.CurrentState.UpdateState(enemyController, 0.1f);
        Assert.That(enemyController.CurrentState, Is.InstanceOf<AlertState>());

        // El jugador se aleja a 12 unidades (> 10 u)
        playerObject.transform.position = new Vector3(0f, 0f, 12f);
        enemyController.CurrentState.UpdateState(enemyController, 0.1f);

        Assert.That(enemyController.CurrentState, Is.InstanceOf<PatrollingState>(),
            "Cuando el jugador sale del rango de visión (> 10 u), la IA debe regresar a PatrollingState.");
    }

    [TestCase(10.01f, false, TestName = "TC018_AVL_FueraDeRango_10_01u_NoDetecta")]
    [TestCase(10.00f, true, TestName = "TC018_AVL_LimiteExactoVision_10_00u_DetectaYPasaAAlert")]
    [TestCase(9.99f, true, TestName = "TC018_AVL_DentroDeRango_9_99u_DetectaYPasaAAlert")]
    public void TC018_AnalisisValoresLimite_FronteraRangoDeVision10Unidades(float distanceZ, bool expectedAlertState)
    {
        playerObject.transform.position = new Vector3(0f, 0f, distanceZ);

        enemyController.CurrentState.UpdateState(enemyController, 0.1f);

        Assert.AreEqual(expectedAlertState, enemyController.CurrentState is AlertState);
    }
}
