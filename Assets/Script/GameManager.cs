using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Configuration d'un type d'ennemi pour le spawn
[System.Serializable]
public class EnemySpawnConfig
{
    
    public GameObject enemyPrefab;
    [Range(0f, 100f)]
    public float spawnProbability = 50f; // Probabilité de spawn (en %)
    public bool isContinuous = false; // Pour les SHA : ennemi "parasite" qui ne compte pas dans la vague
    [Tooltip("Cooldown min/max pour le respawn continu (SHA)")]
    public float respawnCooldownMin = 2f;
    public float respawnCooldownMax = 5f;
}

// Configuration d'une vague
[System.Serializable]
public class WaveConfig
{
    public string waveName = "Vague";
    public int totalEnemies = 10; // Nombre total d'ennemis NON-CONTINUS à tuer pour finir la vague
    public int maxEnemiesAtOnce = 5; // Nombre max d'ennemis NON-CONTINUS en même temps
    public Transform spawnPoint; // Point de spawn des ennemis
    public List<EnemySpawnConfig> enemyConfigs = new List<EnemySpawnConfig>();
    [Tooltip("Délai avant de commencer la vague")]
    public float delayBeforeWave = 2f;
}
public class BonusCard
{
    public string id;
    public Sprite icon;
    public System.Action effect;
    public System.Func<bool> canAppear;
}
public class GameManager : MonoBehaviour
{
    public bool hasLaser = false;
    public bool laserUpgradeDuration = false;
    public GameObject GameOver;
    public GameObject pause;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int score;
    public Vector2 moveInput;
    public bool submitPressed;
    public GameObject BonusHUD;
    public PlayerController pc;
    public int EnemyNumber;
    public FinishLevel FinishLevel;
    public TextMeshProUGUI ScoreText;
    public Image ScoreBlank;
    public Image ScoreImage_Add;
    public Image ScoreNumber;
    public Image ScoreNumberAddBlank;
    public Sprite[] ImageList;
    public Sprite[] ImageAdd;
    private List<int> ScoreBonus = new List<int> { 50, 100, 150, 200, 250, 300, 350, 400, 450, 500 };
    public int selectedIndex = 0; // 0 = dmg, 1 = speed, 2 = bullet speed
    public Image[] BonusIcons; // UI (3 cartes affichées)
    private List<BonusCard> allCards = new List<BonusCard>();
    private List<BonusCard> currentCards = new List<BonusCard>();
    private float inputCooldown = 0.2f; // éviter que l'axe soit lu plusieurs fois trop vite
    private float lastInputTime;
    public TextMeshProUGUI scorefinaleTxT;
    [Header("Système de Vagues")]
    public List<WaveConfig> waves = new List<WaveConfig>();
    public Transform defaultSpawnPoint; // Point de spawn par défaut si non spécifié dans la vague
    
    private int currentWaveIndex = 0;
    private int enemiesKilledThisWave = 0; // Ennemis normaux tués
    private int enemiesSpawnedThisWave = 0; // Ennemis normaux spawnés
    private int currentNormalEnemiesAlive = 0; // Ennemis normaux en vie
    private bool waveInProgress = false;
    private WaveConfig currentWave;
    private List<GameObject> activeEnemies = new List<GameObject>(); // Ennemis normaux
    private List<GameObject> continuousEnemies = new List<GameObject>(); // Ennemis continus (parasites)
    public Sprite[] CardSprites; // assigner les images uniques de chaque carte dans l’inspecteur

    void Update()
    {
        if (pc.hp<=0) { StartCoroutine(StopGame()); }
        // Vérification du score pour le pop-up de bonus
        if (ScoreBonus.Count > 0 && score >= ScoreBonus[0])
        {
            BonusPopUp();
            ScoreBonus.RemoveAt(0);
        }

        // Vérification de la fin de niveau (toutes les vagues terminées)
        if (!waveInProgress && currentWaveIndex >= waves.Count && waves.Count > 0)
        {
            FinishLevel.Stop();
        }

        // Mise à jour du texte du score
        ScoreText.text = score.ToString();
        scorefinaleTxT.text = "Score :" + score.ToString();
        // Gestion du HUD de bonus
        if (BonusHUD.activeSelf)
        {
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                moveInput = gamepad.leftStick.ReadValue(); // stick gauche ou D-pad
                submitPressed = gamepad.buttonSouth.wasPressedThisFrame; // bouton A / Cross
            }

            // Navigation gauche/droite avec cooldown
            if (Time.unscaledTime - lastInputTime > inputCooldown)
            {
                if (moveInput.x > 0.5f)
                {
                    selectedIndex = (selectedIndex + 1) % BonusIcons.Length;
                    lastInputTime = Time.unscaledTime;
                }
                else if (moveInput.x < -0.5f)
                {
                    selectedIndex = (selectedIndex - 1 + BonusIcons.Length) % BonusIcons.Length;
                    lastInputTime = Time.unscaledTime;
                }
            }

            // Surbrillance de l'icône sélectionnée
            for (int i = 0; i < BonusIcons.Length; i++)
            {
                BonusIcons[i].color = (i == selectedIndex) ? Color.green : Color.white;
            }

            // Validation du bonus
            if (submitPressed)
            {
                ApplyBonus(selectedIndex);
            }
        }
    }

    void ApplyBonus(int index)
    {
        if (index < 0 || index >= currentCards.Count)
            return;

        currentCards[index].effect.Invoke();

        BonusHUD.SetActive(false);
        Time.timeScale = 1;
    }


    void Start()
    {
            score = 0;

        allCards.Add(new BonusCard
        {
            id = "Damage",
            icon = CardSprites[0], // unique image
            effect = () => pc.dmg *= 1.5f,
            canAppear = null
        });

        allCards.Add(new BonusCard
        {
            id = "Speed",
            icon = CardSprites[1],
            effect = () => pc.speed += 2f,
            canAppear = null
        });

        allCards.Add(new BonusCard
        {
            id = "BulletSpeed",
            icon = CardSprites[2],
            effect = () => pc.bulletSpeed += 10f,
            canAppear = () => !pc.Laser
        });

        allCards.Add(new BonusCard
        {
            id = "Laser",
            icon = CardSprites[3],
            effect = () =>
            {
                pc.Laser = true;
                pc.LaserLength = 3f; // durée initiale
            },
            canAppear = () => !pc.Laser
        });

        allCards.Add(new BonusCard
        {
            id = "BulletMult",
            icon = CardSprites[4],
            effect = () => pc.AddMult() ,
            canAppear = () => !pc.Laser
        });
        allCards.Add(new BonusCard
        {
            id = "LaserLength",
            icon = CardSprites[5],
            effect = () => pc.LaserLength += 0.2f,
            canAppear = () => pc.Laser
        });
        allCards.Add(new BonusCard
        {
            id = "HP",
            icon = CardSprites[6], // unique image
            effect = () => pc.hp = 5,
            canAppear = null
        });


        // ➕ plus tard
        // allCards.Add(new BonusCard { ... });


        score = 0;
        ScoreNumber.sprite = ImageList[0];
        ScoreImage_Add.gameObject.SetActive(false);
        StartCoroutine(EventLoop());
        
        // Démarrer la première vague
        if (waves.Count > 0)
        {
            StartCoroutine(StartWave(0));
        }
    }

    // ==================== SYSTÈME DE VAGUES ====================

    IEnumerator StartWave(int waveIndex)
    {
        if (waveIndex >= waves.Count)
        {
            Debug.Log("Toutes les vagues sont terminées !");
            yield break;
        }

        currentWaveIndex = waveIndex;
        currentWave = waves[waveIndex];
        enemiesKilledThisWave = 0;
        enemiesSpawnedThisWave = 0;
        currentNormalEnemiesAlive = 0;
        waveInProgress = true;
        activeEnemies.Clear();
        // Note: on ne clear pas continuousEnemies ici, ils seront détruits à la fin de la vague précédente

        Debug.Log($"=== Début de la {currentWave.waveName} (Vague {waveIndex + 1}/{waves.Count}) ===");
        Debug.Log($"Ennemis total: {currentWave.totalEnemies}, Max simultanés: {currentWave.maxEnemiesAtOnce}");

        // Délai avant la vague
        yield return new WaitForSeconds(currentWave.delayBeforeWave);

        // Spawn des ennemis continus d'abord
        SpawnContinuousEnemies();

        // Spawn initial des ennemis normaux (jusqu'à maxEnemiesAtOnce)
        int initialSpawnCount = Mathf.Min(currentWave.maxEnemiesAtOnce, currentWave.totalEnemies);
        for (int i = 0; i < initialSpawnCount; i++)
        {
            SpawnNormalEnemy();
            yield return new WaitForSeconds(0.3f); // Petit délai entre chaque spawn
        }
    }

    // Spawn tous les ennemis continus configurés pour cette vague
    void SpawnContinuousEnemies()
    {
        foreach (var config in currentWave.enemyConfigs)
        {
            if (config.isContinuous && config.enemyPrefab != null)
            {
                SpawnContinuousEnemy(config);
            }
        }
    }

    // Spawn un ennemi continu (parasite)
    void SpawnContinuousEnemy(EnemySpawnConfig config)
    {
        if (!waveInProgress) return;

        Transform spawnPoint = currentWave.spawnPoint != null ? currentWave.spawnPoint : defaultSpawnPoint;
        if (spawnPoint == null)
        {
            Debug.LogError("Aucun point de spawn défini !");
            return;
        }

        GameObject enemy = Instantiate(config.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        continuousEnemies.Add(enemy);

        // Configurer l'ennemi
        SetupEnemy(enemy, config);

        Debug.Log($"Ennemi continu spawné: {config.enemyPrefab.name}");
    }

    // Spawn un ennemi normal (compte dans la vague)
    void SpawnNormalEnemy()
    {
        if (currentWave == null || enemiesSpawnedThisWave >= currentWave.totalEnemies)
            return;

        // Sélectionner un ennemi NON-CONTINU basé sur les probabilités
        EnemySpawnConfig selectedConfig = SelectNormalEnemyByProbability();
        if (selectedConfig == null || selectedConfig.enemyPrefab == null)
        {
            Debug.LogWarning("Aucun ennemi normal configuré pour cette vague !");
            return;
        }

        Transform spawnPoint = currentWave.spawnPoint != null ? currentWave.spawnPoint : defaultSpawnPoint;
        if (spawnPoint == null)
        {
            Debug.LogError("Aucun point de spawn défini !");
            return;
        }

        GameObject enemy = Instantiate(selectedConfig.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        enemiesSpawnedThisWave++;
        currentNormalEnemiesAlive++;
        EnemyNumber = currentNormalEnemiesAlive;
        activeEnemies.Add(enemy);

        // Configurer l'ennemi
        SetupEnemy(enemy, selectedConfig);

        Debug.Log($"Ennemi normal spawné: {selectedConfig.enemyPrefab.name} ({enemiesSpawnedThisWave}/{currentWave.totalEnemies})");
    }

    void SetupEnemy(GameObject enemy, EnemySpawnConfig config)
    {
        // Configuration pour Enemy de base (inclut gameManager)
        Enemy baseEnemy = enemy.GetComponent<Enemy>();
        if (baseEnemy != null)
        {
            baseEnemy.playerController = pc;
            baseEnemy.gameManager = this; // Assigner le gameManager à tous les ennemis
        }

        // Configuration pour SHAEnnemy
        SHAEnnemy shaEnemy = enemy.GetComponent<SHAEnnemy>();
        if (shaEnemy != null)
        {
            if (pc != null)
                shaEnemy.SetTarget(pc.transform);
        }

        // Configuration pour Oppenheimer
        Oppenheimer oppenheimer = enemy.GetComponent<Oppenheimer>();
        if (oppenheimer != null)
        {
            if (pc != null)
                oppenheimer.SetTarget(pc.transform);
        }

        // Ajouter le listener de mort via SHALife
        SHALife lifeComponent = enemy.GetComponent<SHALife>();
        if (lifeComponent == null)
        {
            lifeComponent = enemy.AddComponent<SHALife>();
        }
        
        // Capturer la config pour le callback
        EnemySpawnConfig capturedConfig = config;
        lifeComponent.OnDeath += () => OnEnemyDeath(enemy, capturedConfig);
    }

    // Sélectionne uniquement parmi les ennemis NON-CONTINUS
    EnemySpawnConfig SelectNormalEnemyByProbability()
    {
        if (currentWave.enemyConfigs.Count == 0)
            return null;

        // Filtrer les ennemis non-continus
        List<EnemySpawnConfig> normalConfigs = new List<EnemySpawnConfig>();
        foreach (var config in currentWave.enemyConfigs)
        {
            if (!config.isContinuous)
                normalConfigs.Add(config);
        }

        if (normalConfigs.Count == 0)
            return null;

        // Calculer la somme totale des probabilités
        float totalProbability = 0f;
        foreach (var config in normalConfigs)
        {
            totalProbability += config.spawnProbability;
        }

        if (totalProbability <= 0f)
            return normalConfigs[0];

        // Tirer un nombre aléatoire
        float randomValue = Random.Range(0f, totalProbability);
        float cumulative = 0f;

        foreach (var config in normalConfigs)
        {
            cumulative += config.spawnProbability;
            if (randomValue <= cumulative)
            {
                return config;
            }
        }

        return normalConfigs[0];
    }

    void OnEnemyDeath(GameObject enemy, EnemySpawnConfig config)
    {
        // Si c'est un ennemi continu (parasite)
        if (config.isContinuous)
        {
            if (continuousEnemies.Contains(enemy))
            {
                continuousEnemies.Remove(enemy);
            }

            // Respawn seulement si la vague est encore en cours
            if (waveInProgress)
            {
                float cooldown = Random.Range(config.respawnCooldownMin, config.respawnCooldownMax);
                StartCoroutine(RespawnContinuousAfterCooldown(config, cooldown));
            }

            Debug.Log("Ennemi continu tué ! (ne compte pas dans la vague)");
            return;
        }

        // Sinon c'est un ennemi normal
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }

        currentNormalEnemiesAlive--;
        enemiesKilledThisWave++;
        EnemyNumber = currentNormalEnemiesAlive;

        Debug.Log($"Ennemi normal tué ! ({enemiesKilledThisWave}/{currentWave.totalEnemies})");

        // Vérifier si la vague est terminée
        if (enemiesKilledThisWave >= currentWave.totalEnemies)
        {
            OnWaveComplete();
            return;
        }

        // Spawn un remplaçant si on n'a pas atteint le total
        if (enemiesSpawnedThisWave < currentWave.totalEnemies && 
            currentNormalEnemiesAlive < currentWave.maxEnemiesAtOnce)
        {
            SpawnNormalEnemy();
        }
    }

    IEnumerator RespawnContinuousAfterCooldown(EnemySpawnConfig config, float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        
        // Vérifier que la vague est encore en cours
        if (waveInProgress)
        {
            SpawnContinuousEnemy(config);
        }
    }

    void OnWaveComplete()
    {
        waveInProgress = false;
        Debug.Log($"=== {currentWave.waveName} TERMINÉE ! ===");

        // Détruire tous les ennemis continus restants
        DestroyContinuousEnemies();

        // Passer à la vague suivante
        currentWaveIndex++;
        if (currentWaveIndex < waves.Count)
        {
            StartCoroutine(StartWave(currentWaveIndex));
        }
        else
        {
            Debug.Log("=== TOUTES LES VAGUES TERMINÉES ! VICTOIRE ! ===");
        }
    }

    void DestroyContinuousEnemies()
    {
        foreach (GameObject enemy in continuousEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        continuousEnemies.Clear();
        Debug.Log("Tous les ennemis continus ont été détruits.");
    }

    // Méthode publique pour notifier la mort d'un ennemi (appelée par les ennemis)
    public void NotifyEnemyDeath()
    {
        // Cette méthode peut être utilisée par les ennemis qui n'ont pas SHALife
        // La logique principale est dans OnEnemyDeath via SHALife
    }

    // ==================== AUTRES MÉTHODES ====================

    public void AddScore(int adds)
    {
        StartCoroutine(AnimAddScore(adds));
    }

    void BonusPopUp()
    {
        pause.SetActive(false);
        BonusHUD.SetActive(true);
        Time.timeScale = 0;

        GenerateRandomCards();
    }
    void GenerateRandomCards()
    {
        currentCards.Clear();

        // On ne garde que les cartes disponibles
        List<BonusCard> pool = allCards
            .Where(card => card.canAppear == null || card.canAppear())
            .ToList();

        for (int i = 0; i < BonusIcons.Length; i++)
        {
            if (pool.Count == 0) break;

            int rand = Random.Range(0, pool.Count);
            BonusCard card = pool[rand];
            pool.RemoveAt(rand);

            currentCards.Add(card);
            BonusIcons[i].sprite = card.icon;
        }

        selectedIndex = 0;
    }




    public void addDmg()
    {
        pc.dmg *= 1.5f;
        BonusHUD.SetActive(false);
        Time.timeScale = 1;
    }
    
    public void addSpeed()
    {
        pc.speed += 2f;
        BonusHUD.SetActive(false);
        Time.timeScale = 1;
    }
    
    public void addbulletSpeed()
    {
        pc.bulletSpeed += 10f;
        BonusHUD.SetActive(false);
        Time.timeScale = 1;
    }
    
    IEnumerator EventLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(15f);
            AddScore(10);
        }
    }
    
    IEnumerator AnimAddScore(int adds)
    {
        ScoreImage_Add.gameObject.SetActive(true);
        
        // Vérifier que l'index ne dépasse pas la taille du tableau
        int addIndex = Mathf.Clamp(adds / 10, 0, ImageAdd.Length - 1);
        if (ImageAdd.Length > 0)
            ScoreNumberAddBlank.sprite = ImageAdd[addIndex];
        
        yield return new WaitForSeconds(1.5f);
        score += adds;
        
        // Vérifier que l'index ne dépasse pas la taille du tableau
        int scoreIndex = Mathf.Clamp(score / 10, 0, ImageList.Length - 1);
        if (scoreIndex >= ImageList.Length)  scoreIndex = ImageList.Length; 
        if (ImageList.Length > 0)ScoreNumber.sprite = ImageList[scoreIndex];
            
        ScoreImage_Add.gameObject.SetActive(false);

        yield return new WaitForSeconds(1.2f);
    }

    IEnumerator StopGame()
    {
        GameOver.SetActive(true);
        yield return new WaitForSeconds(1.2f);
        SceneManager.LoadScene("Menu");
        yield return new WaitForSeconds(1.2f);
    }
}
