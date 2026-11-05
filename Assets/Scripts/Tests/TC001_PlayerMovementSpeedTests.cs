using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("Movimiento")]
[Category("ParticionDeEquivalencias")]
public class TC001_PlayerMovementSpeedTests
{
    private const float ExpectedMoveSpeed = 5f;
    private const float HoldDurationSeconds = 2f;
    private const float ExpectedDisplacement = 10f;

    private GameObject playerObject;
    private PlayerMovement playerMovement;

    [SetUp]
    public void SetUp()
    {
        playerObject = new GameObject("Player_TC001");
        playerObject.tag = "Player";
        playerObject.transform.position = Vector3.zero;
        playerObject.transform.rotation = Quaternion.identity;

        playerMovement = playerObject.AddComponent<PlayerMovement>();
        playerMovement.MoveSpeed = ExpectedMoveSpeed;
        playerMovement.ClampToArea = false;
    }

    [TearDown]
    public void TearDown()
    {
        if (playerObject != null)
        {
            Object.DestroyImmediate(playerObject);
        }
    }

    [Test]
    [Description("TC-001: Verificar que al mantener W durante 2s a 5 u/s la nave avanza 10 unidades y se detiene al soltar W.")]
    public void TC001_MantenerWDurante2Segundos_Desplaza10Unidades()
    {
        Vector3 initialPosition = playerObject.transform.position;

        const int steps = 20;
        float dt = HoldDurationSeconds / steps;
        for (int i = 0; i < steps; i++)
        {
            playerMovement.Move(1f, dt);
        }

        Vector3 positionAfterHold = playerObject.transform.position;

        playerMovement.Move(0f, 1f);
        Vector3 finalPosition = playerObject.transform.position;

        float distanceMoved = Vector3.Distance(initialPosition, positionAfterHold);
        Assert.AreEqual(ExpectedDisplacement, distanceMoved, 0.01f,
            "La nave debe desplazarse 10 unidades (5 u/s * 2 s) al mantener W.");
        Assert.AreEqual(ExpectedDisplacement, positionAfterHold.z, 0.01f,
            "El avance hacia adelante (+Z) debe ser de 10 unidades.");
        Assert.AreEqual(positionAfterHold, finalPosition,
            "Al soltar W (input = 0), la nave no debe seguir desplazándose.");
    }

    [TestCase(1f, 2f, 10f, TestName = "TC001_PE_ClaseValida_AvanceW_DesplazaPositivo10")]
    [TestCase(-1f, 2f, -10f, TestName = "TC001_PE_ClaseValida_RetrocesoS_DesplazaNegativo10")]
    [TestCase(0f, 2f, 0f, TestName = "TC001_PE_ClaseInactiva_SinTecla_DesplazaCero")]
    public void TC001_ParticionEquivalencias_EjeVertical(float verticalInput, float seconds, float expectedZ)
    {
        playerMovement.Move(verticalInput, seconds);

        Assert.AreEqual(expectedZ, playerObject.transform.position.z, 0.01f);
    }
}
