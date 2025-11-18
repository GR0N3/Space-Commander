using UnityEngine;

public class LifeFacade : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("AudioSource para reproducir sonidos de muerte (opcional, se buscará automáticamente si no está asignado)")]
    public AudioSource audioSource;
    
    [Tooltip("Volumen del sonido de muerte (0-1)")]
    [Range(0f, 1f)]
    public float deathSoundVolume = 1f;

    [Header("Efectos Visuales")]
    [Tooltip("Prefab de explosión por defecto (se usa si LifeType no tiene uno asignado)")]
    public GameObject defaultExplosionPrefab;

    [Header("Sistema de Puntuación")]
    [Tooltip("Objeto que maneja la puntuación: recibirá SendMessage('AddScore', points)")]
    public GameObject scoreManager;

    [Header("Game Over")]
    public GameObject gameOverUI;
    public string menuSceneName = "Menu";
    public bool pauseOnPlayerDeath = true;
    private bool gameOverTriggered;

    [Header("Configuración Global")]
    public bool useGlobalAudio = false;

    private static LifeFacade instance;

    public static LifeFacade Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Object.FindFirstObjectByType<LifeFacade>();
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && useGlobalAudio)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }
    public void HandleDeath(LifeType lifeType, Vector3 position, GameObject entityGameObject = null)
    {
        if (lifeType == null)
        {
            return;
        }
        PlayDeathSound(lifeType, position);
        SpawnDeathEffect(lifeType, position);

        int points = lifeType.pointsOnDeath;
        if (points <= 0 && entityGameObject != null)
        {
            var n = entityGameObject.name;
            if (!string.IsNullOrEmpty(n))
            {
                if (n.Contains("Enemy00") || n.Contains("NaveEnemigaAlien")) points = 100;
                else if (n.Contains("Enemy01") || n.Contains("NaveEnemigaAlien2")) points = 350;
            }
        }
        if (points > 0)
        {
            AddScore(points);
        }
        if (entityGameObject != null)
        {
            if (entityGameObject.CompareTag("Player"))
            {
                entityGameObject.SetActive(false);
                if (!gameOverTriggered)
                {
                    gameOverTriggered = true;
                    if (pauseOnPlayerDeath)
                    {
                        Time.timeScale = 0f;
                        AudioListener.pause = true;
                    }
                    if (gameOverUI != null) gameOverUI.SetActive(true);
                    StartCoroutine(WaitAnyKeyThenMenu());
                }
            }
            else
            {
                Destroy(entityGameObject);
            }
        }
    }
    private void PlayDeathSound(LifeType lifeType, Vector3 position)
    {
        if (lifeType.deathSound == null) return;

        AudioSource sourceToUse = audioSource;
        float vol = Mathf.Clamp01(deathSoundVolume) * Mathf.Clamp01(lifeType.deathSoundVolume);

        if (sourceToUse == null && !useGlobalAudio)
        {
            GameObject tempAudio = new GameObject("TempDeathAudio");
            tempAudio.transform.position = position;
            sourceToUse = tempAudio.AddComponent<AudioSource>();
            sourceToUse.spatialBlend = 1f;
            sourceToUse.maxDistance = 50f;
            sourceToUse.PlayOneShot(lifeType.deathSound, vol);
            Destroy(tempAudio, lifeType.deathSound.length + 0.1f);
        }
        else if (sourceToUse != null)
        {
            if (useGlobalAudio)
            {
                sourceToUse.PlayOneShot(lifeType.deathSound, vol);
            }
            else
            {
                AudioSource.PlayClipAtPoint(lifeType.deathSound, position, vol);
            }
        }
    }
    private void SpawnDeathEffect(LifeType lifeType, Vector3 position)
    {
        GameObject effectPrefab = lifeType.deathEffectPrefab != null 
            ? lifeType.deathEffectPrefab 
            : defaultExplosionPrefab;

        if (effectPrefab != null)
        {
            Instantiate(effectPrefab, position, Quaternion.identity);
        }
    }
    private void AddScore(int points)
    {
        if (points <= 0) return;
        var service = ScoreManager.Instance as IScoreService;
        if (service != null)
        {
            service.AddScore(points);
            return;
        }
        if (scoreManager != null)
        {
            var sm = scoreManager.GetComponent<ScoreManager>();
            if (sm != null) sm.AddScore(points);
        }
    }
    public static void HandleEntityDeath(LifeType lifeType, Vector3 position, GameObject entityGameObject = null)
    {
        LifeFacade facade = Instance;
        if (facade != null)
        {
            facade.HandleDeath(lifeType, position, entityGameObject);
        }
        else
        {
            GameObject tempFacade = new GameObject("TempLifeFacade");
            LifeFacade temp = tempFacade.AddComponent<LifeFacade>();
            temp.HandleDeath(lifeType, position, entityGameObject);
            Destroy(tempFacade, 1f);
        }
    }

    private System.Collections.IEnumerator WaitAnyKeyThenMenu()
    {
        while (!Input.anyKeyDown) { yield return null; }
        AudioListener.pause = false;
        Time.timeScale = 1f;
        SceneTransition.LoadScene(menuSceneName);
    }
}

