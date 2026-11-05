using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("Movimiento")]
[Category("AnalisisDeValoresLimite")]
public class TC002_PlayerMovementBoundaryTests
{
    private const float MoveSpeed = 5f;
    private const float AreaMargin = 0.5f;
    private const float AreaSize = 20f;
    private const float MaxAllowedCoord = (AreaSize * 0.5f) - AreaMargin;

    private GameObject playerObject;
    private PlayerMovement playerMovement;
    private GameObject areaObject;
    private SpawnArea spawnArea;

    [SetUp]
    public void SetUp()
    {
        areaObject = new GameObject("ClampArea_TC002");
        areaObject.transform.position = Vector3.zero;
        spawnArea = areaObject.AddComponent<SpawnArea>();
        spawnArea.shape = SpawnArea.Shape.Rectangle;
        spawnArea.size = new Vector2(AreaSize, AreaSize);
        spawnArea.radius = AreaSize * 0.5f;

        playerObject = new GameObject("Player_TC002");
        playerObject.tag = "Player";
        playerObject.transform.position = Vector3.zero;
        playerObject.transform.rotation = Quaternion.identity;

        playerMovement = playerObject.AddComponent<PlayerMovement>();
        playerMovement.MoveSpeed = MoveSpeed;
        playerMovement.PlayerPadding = AreaMargin;
        playerMovement.ClampArea = spawnArea;
        playerMovement.ClampToArea = true;
    }

    [TearDown]
    public void TearDown()
    {
        if (playerObject != null) Object.DestroyImmediate(playerObject);
        if (areaObject != null) Object.DestroyImmediate(areaObject);
    }

    [Test]
    [Description("TC-002: Verificar que la nave se detiene a 0.5 unidades del borde del área y no lo atraviesa al seguir presionando W.")]
    public void TC002_MantenerWHastaElBordeY2SegundosMas_SeDetieneEnMargen05()
    {
        playerMovement.Move(1f, 2f);
        float zAtBorder = playerObject.transform.position.z;

        playerMovement.Move(1f, 2f);
        float zAfterExtraHold = playerObject.transform.position.z;

        Assert.AreEqual(MaxAllowedCoord, zAtBorder, 0.001f,
            "La nave debe detenerse exactamente a 0.5 unidades del borde superior (9.5 u).");
        Assert.AreEqual(MaxAllowedCoord, zAfterExtraHold, 0.001f,
            "Al seguir presionando W 2 s más, la nave no debe atravesar el límite de 9.5 u.");
    }

    [TestCase(9.4f, 9.4f, TestName = "TC002_AVL_BordeSuperior_VecinoInterno_9_4")]
    [TestCase(9.5f, 9.5f, TestName = "TC002_AVL_BordeSuperior_ValorLimiteExacto_9_5")]
    [TestCase(9.6f, 9.5f, TestName = "TC002_AVL_BordeSuperior_VecinoExternoInvalido_9_6_ClampedA9_5")]
    [TestCase(-9.4f, -9.4f, TestName = "TC002_AVL_BordeInferior_VecinoInterno_Menos9_4")]
    [TestCase(-9.5f, -9.5f, TestName = "TC002_AVL_BordeInferior_ValorLimiteExacto_Menos9_5")]
    [TestCase(-9.6f, -9.5f, TestName = "TC002_AVL_BordeInferior_VecinoExternoInvalido_Menos9_6_ClampedAMenos9_5")]
    public void TC002_AnalisisValoresLimite_FronterasDelArea(float targetZ, float expectedClampedZ)
    {
        playerObject.transform.position = new Vector3(0f, 0f, targetZ);

        playerMovement.Move(0f, 0f);

        Assert.AreEqual(expectedClampedZ, playerObject.transform.position.z, 0.001f);
    }

    [Test]
    [Description("TC-002 (Caja Blanca - Branch Coverage): Verificar el límite de margen 0.5 u cuando el área es circular.")]
    public void TC002_CajaBlanca_BranchCoverage_AreaCircular_RespetaMargen05()
    {
        spawnArea.shape = SpawnArea.Shape.Circle;
        spawnArea.radius = 10f;

        playerMovement.Move(1f, 4f);

        Assert.AreEqual(9.5f, playerObject.transform.position.z, 0.001f,
            "En área circular de radio 10 con margen 0.5, la distancia máxima al centro debe ser 9.5.");
    }
}
