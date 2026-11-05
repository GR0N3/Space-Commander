using NUnit.Framework;
using UnityEditor;
using UnityEngine;

[TestFixture]
[Category("PowerUps")]
[Category("TransicionDeEstados")]
public class TC016_WeaponProgressionAndDronesTests
{
    private GameObject playerObject;
    private PlayerFacade playerFacade;
    private PlayerPowerUpManager powerUpManager;
    private GameObject droneL;
    private GameObject droneR;
    private GameObject drone3;
    private GameObject drone4;

    private PowerUpType cadenceLvl1;
    private PowerUpType dronesLvl1;
    private PowerUpType dronesLvl2;
    private PowerUpType dronesLvl3;
    private PowerUpType cadenceLvl2;
    private bool createdRuntimeAssets;

    [SetUp]
    public void SetUp()
    {
        cadenceLvl1 = AssetDatabase.LoadAssetAtPath<PowerUpType>("Assets/Scripts/Objects/CadenceLvl1.asset");
        dronesLvl1 = AssetDatabase.LoadAssetAtPath<PowerUpType>("Assets/Scripts/Objects/DronesLvl1.asset");
        dronesLvl2 = AssetDatabase.LoadAssetAtPath<PowerUpType>("Assets/Scripts/Objects/DronesLvl2.asset");
        dronesLvl3 = AssetDatabase.LoadAssetAtPath<PowerUpType>("Assets/Scripts/Objects/DronesLvl3.asset");
        cadenceLvl2 = AssetDatabase.LoadAssetAtPath<PowerUpType>("Assets/Scripts/Objects/CadenceLvl2.asset");

        if (cadenceLvl1 == null || dronesLvl1 == null || dronesLvl2 == null || dronesLvl3 == null || cadenceLvl2 == null)
        {
            createdRuntimeAssets = true;

            cadenceLvl1 = ScriptableObject.CreateInstance<PowerUpType>();
            cadenceLvl1.powerUpName = "Cadence LVL1";
            cadenceLvl1.weaponUpgradeMode = WeaponUpgradeMode.FireRateOnly;
            cadenceLvl1.fireRateMultiplier = 1.35f;

            dronesLvl1 = ScriptableObject.CreateInstance<PowerUpType>();
            dronesLvl1.powerUpName = "Drones Lvl1";
            dronesLvl1.weaponUpgradeMode = WeaponUpgradeMode.DronesFixedSides;
            dronesLvl1.fireRateMultiplier = 1.35f;

            dronesLvl2 = ScriptableObject.CreateInstance<PowerUpType>();
            dronesLvl2.powerUpName = "Drones Lvl2";
            dronesLvl2.weaponUpgradeMode = WeaponUpgradeMode.DronesOrbiting2;
            dronesLvl2.fireRateMultiplier = 1.45f;
            dronesLvl2.orbitRadius = 3.5f;
            dronesLvl2.orbitSpeed = 60f;

            dronesLvl3 = ScriptableObject.CreateInstance<PowerUpType>();
            dronesLvl3.powerUpName = "Drones Lvl3";
            dronesLvl3.weaponUpgradeMode = WeaponUpgradeMode.DronesOrbiting4;
            dronesLvl3.fireRateMultiplier = 1.45f;
            dronesLvl3.orbitRadius = 5f;
            dronesLvl3.orbitSpeed = 60f;

            cadenceLvl2 = ScriptableObject.CreateInstance<PowerUpType>();
            cadenceLvl2.powerUpName = "Cadence LVL2";
            cadenceLvl2.weaponUpgradeMode = WeaponUpgradeMode.FireRateLvl2;
            cadenceLvl2.fireRateMultiplier = 2.0f;
        }

        playerObject = new GameObject("Player_TC016");
        playerObject.tag = "Player";

        var powerUpsRoot = new GameObject("PowerUps");
        powerUpsRoot.transform.SetParent(playerObject.transform);

        droneL = new GameObject("Turret_L");
        droneL.transform.SetParent(powerUpsRoot.transform);
        droneL.SetActive(false);

        droneR = new GameObject("Turret_R");
        droneR.transform.SetParent(powerUpsRoot.transform);
        droneR.SetActive(false);

        drone3 = new GameObject("Turret_3");
        drone3.transform.SetParent(powerUpsRoot.transform);
        drone3.SetActive(false);

        drone4 = new GameObject("Turret_4");
        drone4.transform.SetParent(powerUpsRoot.transform);
        drone4.SetActive(false);

        powerUpManager = playerObject.AddComponent<PlayerPowerUpManager>();
        powerUpManager.Drone_L = droneL;
        powerUpManager.Drone_R = droneR;
        powerUpManager.Drone_3 = drone3;
        powerUpManager.Drone_4 = drone4;
        powerUpManager.weaponProgressionOrder = new PowerUpType[]
        {
            cadenceLvl1,
            dronesLvl1,
            dronesLvl2,
            dronesLvl3,
            cadenceLvl2
        };

        playerFacade = playerObject.AddComponent<PlayerFacade>();
        playerFacade.powerUpManager = powerUpManager;
    }

    [TearDown]
    public void TearDown()
    {
        if (droneL != null) Object.DestroyImmediate(droneL);
        if (droneR != null) Object.DestroyImmediate(droneR);
        if (drone3 != null) Object.DestroyImmediate(drone3);
        if (drone4 != null) Object.DestroyImmediate(drone4);
        if (playerObject != null) Object.DestroyImmediate(playerObject);

        if (createdRuntimeAssets)
        {
            if (cadenceLvl1 != null) Object.DestroyImmediate(cadenceLvl1);
            if (dronesLvl1 != null) Object.DestroyImmediate(dronesLvl1);
            if (dronesLvl2 != null) Object.DestroyImmediate(dronesLvl2);
            if (dronesLvl3 != null) Object.DestroyImmediate(dronesLvl3);
            if (cadenceLvl2 != null) Object.DestroyImmediate(cadenceLvl2);
        }
    }

    [Test]
    [Description("TC-016: Verificar la transición progresiva de niveles de arma (0 -> 1 -> 2 -> 3) al recoger power-ups de armamento.")]
    public void TC016_RecogerTresMejorasDeArma_ProgresaNiveles1A3YActivaDronesFijosYOrbitantes()
    {
        Assert.AreEqual(0, powerUpManager.WeaponLevel, "Precondición: WeaponLevel inicial debe ser 0.");
        Assert.AreEqual(1f, powerUpManager.GetFireRateMultiplier(), 0.001f, "Precondición: multiplicador base debe ser 1.0.");
        Assert.IsFalse(droneL.activeSelf, "Precondición: Drone_L debe iniciar desactivado.");
        Assert.IsFalse(droneR.activeSelf, "Precondición: Drone_R debe iniciar desactivado.");

        // Paso 1: Recoger primer power-up de arma (CadenceLvl1 -> Nivel 1)
        playerFacade.CollectPickup(cadenceLvl1);

        Assert.AreEqual(1, powerUpManager.WeaponLevel, "Tras el 1er power-up, WeaponLevel debe pasar a 1.");
        Assert.AreEqual(1.35f, powerUpManager.GetFireRateMultiplier(), 0.01f,
            "En nivel 1 (CadenceLvl1), el multiplicador de cadencia debe subir a 1.35.");
        Assert.IsFalse(droneL.activeSelf, "En nivel 1 aún no deben activarse los drones.");
        Assert.IsFalse(droneR.activeSelf, "En nivel 1 aún no deben activarse los drones.");

        // Paso 2: Recoger segundo power-up de arma (DronesLvl1 -> Nivel 2: 2 drones fijos laterales)
        playerFacade.CollectPickup(dronesLvl1);

        Assert.AreEqual(2, powerUpManager.WeaponLevel, "Tras el 2do power-up, WeaponLevel debe pasar a 2.");
        Assert.IsTrue(droneL.activeSelf, "En nivel 2 (DronesLvl1), Drone_L debe estar activo.");
        Assert.IsTrue(droneR.activeSelf, "En nivel 2 (DronesLvl1), Drone_R debe estar activo.");
        Assert.IsFalse(drone3.activeSelf, "En nivel 2, Drone_3 debe permanecer inactivo.");
        Assert.IsFalse(drone4.activeSelf, "En nivel 2, Drone_4 debe permanecer inactivo.");
        var orbitLAtLvl2 = droneL.GetComponent<DroneOrbit>();
        Assert.IsTrue(orbitLAtLvl2 == null || !orbitLAtLvl2.enabled,
            "En nivel 2 los drones deben ser fijos a los costados (DroneOrbit deshabilitado).");
        Assert.AreEqual(-2.2f, droneL.transform.localPosition.x, 0.01f);
        Assert.AreEqual(2.2f, droneR.transform.localPosition.x, 0.01f);

        // Paso 3: Recoger tercer power-up de arma (DronesLvl2 -> Nivel 3: 2 drones orbitando)
        playerFacade.CollectPickup(dronesLvl2);

        Assert.AreEqual(3, powerUpManager.WeaponLevel, "Tras el 3er power-up, WeaponLevel debe pasar a 3.");
        Assert.IsTrue(droneL.activeSelf, "En nivel 3 (DronesLvl2), Drone_L debe seguir activo.");
        Assert.IsTrue(droneR.activeSelf, "En nivel 3 (DronesLvl2), Drone_R debe seguir activo.");
        var orbitLAtLvl3 = droneL.GetComponent<DroneOrbit>();
        var orbitRAtLvl3 = droneR.GetComponent<DroneOrbit>();
        Assert.IsNotNull(orbitLAtLvl3, "Drone_L debe tener componente DroneOrbit en nivel 3.");
        Assert.IsNotNull(orbitRAtLvl3, "Drone_R debe tener componente DroneOrbit en nivel 3.");
        Assert.IsTrue(orbitLAtLvl3.enabled, "En nivel 3, Drone_L debe estar orbitando (DroneOrbit.enabled == true).");
        Assert.IsTrue(orbitRAtLvl3.enabled, "En nivel 3, Drone_R debe estar orbitando (DroneOrbit.enabled == true).");
    }

    [Test]
    [Description("TC-016 (Caja Blanca - Cobertura de Ramas del switch WeaponLevel): Verificar niveles 4, 5 y tope máximo sin degradación.")]
    public void TC016_TransicionCompletaHastaNivel5YTopeMaximo_Activa4DronesYCadenciaMaxima()
    {
        for (int i = 0; i < 4; i++)
        {
            playerFacade.CollectPickup(cadenceLvl1);
        }

        Assert.AreEqual(4, powerUpManager.WeaponLevel, "Tras 4 mejoras, WeaponLevel debe ser 4 (DronesLvl3).");
        Assert.IsTrue(droneL.activeSelf && droneR.activeSelf && drone3.activeSelf && drone4.activeSelf,
            "En nivel 4 deben estar activos los 4 drones orbitando.");

        playerFacade.CollectPickup(cadenceLvl2);
        Assert.AreEqual(5, powerUpManager.WeaponLevel, "Tras 5 mejoras, WeaponLevel debe llegar al nivel máximo 5.");
        Assert.AreEqual(2.0f, powerUpManager.GetFireRateMultiplier(), 0.01f,
            "En nivel 5 (CadenceLvl2), el multiplicador de cadencia debe alcanzar 2.0.");

        // Intentar recoger un 6to power-up no debe superar el nivel máximo 5 ni desactivar los 4 drones
        playerFacade.CollectPickup(cadenceLvl1);
        Assert.AreEqual(5, powerUpManager.WeaponLevel, "El nivel de arma no debe superar el tope máximo (5).");
        Assert.AreEqual(2.0f, powerUpManager.GetFireRateMultiplier(), 0.01f,
            "El multiplicador de cadencia no debe degradarse al recoger un power-up de nivel inferior.");
    }
}
