using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("Ranking")]
[Category("ParticionDeEquivalencias")]
public class TC020_HighScoreRankingTests
{
    private GameObject managerObject;
    private HighScoreManager highScoreManager;

    [SetUp]
    public void SetUp()
    {
        managerObject = new GameObject("HighScoreManager_TC020");
        highScoreManager = managerObject.AddComponent<HighScoreManager>();

        var instanceField = typeof(HighScoreManager).GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
        if (instanceField != null)
        {
            instanceField.SetValue(null, highScoreManager);
        }

        highScoreManager.Clear();
    }

    [TearDown]
    public void TearDown()
    {
        if (highScoreManager != null)
        {
            highScoreManager.Clear();
        }
        var instanceField = typeof(HighScoreManager).GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
        if (instanceField != null)
        {
            instanceField.SetValue(null, null);
        }
        if (managerObject != null)
        {
            Object.DestroyImmediate(managerObject);
        }
    }

    [Test]
    [Description("TC-020: Verificar el registro de puntajes en el Ranking con normalización de iniciales a 3 letras en mayúsculas y ordenamiento descendente.")]
    public void TC020_RegistrarPuntajesEnRanking_NormalizaInicialesA3MayusculasYOrdenaDeMayorAMenor()
    {
        Assert.AreEqual(0, highScoreManager.GetEntries().Count, "Precondición: la tabla de High Scores inicia vacía.");

        // Paso 1: Registrar puntaje de 500 con iniciales en minúscula e incompletas ("ab")
        int rankFirstInsert = highScoreManager.Add("ab", 500);

        // Paso 2: Registrar un segundo puntaje mayor de 1200 con iniciales largas ("jugador")
        int rankSecondInsert = highScoreManager.Add("jugador", 1200);

        // Paso 3: Consultar la lista del ranking
        IReadOnlyList<HighScoreManager.Entry> entries = highScoreManager.GetEntries();

        Assert.AreEqual(1, rankFirstInsert, "Al insertar el 1er puntaje en tabla vacía, su rango inicial debe ser 1.");
        Assert.AreEqual(1, rankSecondInsert, "Al insertar un puntaje superior (1200 > 500), debe ocupar el rango 1.");
        Assert.AreEqual(2, entries.Count, "La tabla debe contener exactamente 2 registros.");

        // Verificar orden descendente (1200 primero, 500 segundo) y normalización de iniciales
        Assert.AreEqual(1200, entries[0].score, "El primer puesto debe corresponder al puntaje mayor (1200).");
        Assert.AreEqual("JUG", entries[0].initials,
            "Las iniciales largas ('jugador') deben truncarse y convertirse a 3 letras mayúsculas ('JUG').");

        Assert.AreEqual(500, entries[1].score, "El segundo puesto debe corresponder al puntaje menor (500).");
        Assert.AreEqual("AB ", entries[1].initials,
            "Las iniciales cortas ('ab') deben pasarse a mayúsculas y rellenarse con espacio hasta 3 caracteres ('AB ').");
    }

    [TestCase("", "AAA", TestName = "TC020_PE_InicialesVacias_SeReemplazanPorAAA")]
    [TestCase(null, "AAA", TestName = "TC020_PE_InicialesNulas_SeReemplazanPorAAA")]
    [TestCase("x", "X  ", TestName = "TC020_PE_Iniciales1CaracterMinuscula_NormalizaAXMasDosEspacios")]
    [TestCase("ez", "EZ ", TestName = "TC020_PE_Iniciales2Caracteres_NormalizaAEZMasUnEspacio")]
    [TestCase("abc", "ABC", TestName = "TC020_PE_InicialesExactas3Caracteres_NormalizaAABC")]
    [TestCase("spacecommander", "SPA", TestName = "TC020_PE_InicialesMasDe3Caracteres_TruncaASPA")]
    public void TC020_ParticionEquivalencias_NormalizacionDeIniciales(string rawInitials, string expectedNormalized)
    {
        highScoreManager.Clear();

        highScoreManager.Add(rawInitials, 1000);
        IReadOnlyList<HighScoreManager.Entry> entries = highScoreManager.GetEntries();

        Assert.AreEqual(1, entries.Count);
        Assert.AreEqual(3, entries[0].initials.Length, "Las iniciales normalizadas siempre deben tener longitud 3.");
        Assert.AreEqual(expectedNormalized, entries[0].initials);
    }

    [Test]
    [Description("TC-020 (Análisis de Valores Límite en TopScoresCollection): Verificar capacidad máxima de 10 récords y descarte de puntajes bajos.")]
    public void TC020_CapacidadMaximaTop10_DescartaPuntajeMenorYRechazaNoClasificados()
    {
        for (int i = 1; i <= 10; i++)
        {
            highScoreManager.Add("P" + i, i * 100); // 100, 200, ..., 1000
        }

        Assert.AreEqual(10, highScoreManager.GetEntries().Count, "La capacidad máxima debe ser de 10 entradas.");
        Assert.AreEqual(1000, highScoreManager.GetEntries()[0].score);
        Assert.AreEqual(100, highScoreManager.GetEntries()[9].score);

        // Un puntaje menor al 10° puesto (50 < 100) no debe clasificar (retorna -1)
        int rankRejected = highScoreManager.Add("LOW", 50);
        Assert.AreEqual(-1, rankRejected, "Un puntaje que no supera al 10° lugar debe retornar rango -1.");
        Assert.AreEqual(10, highScoreManager.GetEntries().Count);
        Assert.AreEqual(100, highScoreManager.GetEntries()[9].score);

        // Un puntaje nuevo de 1500 entra en el puesto 1 y desplaza fuera del Top 10 al puntaje de 100
        int rankTop = highScoreManager.Add("TOP", 1500);
        Assert.AreEqual(1, rankTop);
        Assert.AreEqual(10, highScoreManager.GetEntries().Count, "La lista nunca debe superar los 10 elementos.");
        Assert.AreEqual(1500, highScoreManager.GetEntries()[0].score);
        Assert.AreEqual(200, highScoreManager.GetEntries()[9].score, "El antiguo 10° puesto (100) debe haber sido desplazado.");
    }
}
