using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform[] topSpawnPoints;
    public Transform[] bottomSpawnPoints;

    [Header("Safe Platform Prefabs")]
    public GameObject[] safeMoveUpPlatformPrefabs;
    public GameObject[] safeMoveDownPlatformPrefabs;

    [Header("Trap Platform Prefabs")]
    public GameObject[] trapMoveUpPlatformPrefabs;
    public GameObject[] trapMoveDownPlatformPrefabs;

    [Header("Spawn Settings")]
    public float startSpawnInterval = 3f;
    public float minSpawnInterval = 1.5f;

    public int startPlatformPerWave = 2;
    public int maxPlatformPerWave = 4;

    [Header("Platform Move Settings")]
    public float startPlatformSpeed = 1.5f;
    public float maxPlatformSpeed = 4f;
    public float platformLifeTime = 10f;

    [Header("Difficulty")]
    public float speedIncreasePerSecond = 0.03f;
    public float intervalDecreasePerSecond = 0.02f;

    public float addPlatformAfterSeconds = 25f;
    public float addMorePlatformAfterSeconds = 50f;

    [Header("Trap Settings")]
    public float trapStartTime = 20f;
    public float trapChance = 0.2f;
    public float maxTrapChance = 0.4f;

    [Header("Anti Sandwich")]
    public bool lockColumnWhilePlatformAlive = true;

    [Tooltip("Kalau true, kolom sebelah platform juga ikut diblok sementara.")]
    public bool blockNeighborColumns = true;

    [Header("Debug")]
    public bool showDebug = true;

    private float gameTimer;
    private bool[] activeColumns;

    private void Start()
    {
        int columnCount = GetColumnCount();
        activeColumns = new bool[columnCount];

        StartCoroutine(SpawnRoutine());
    }

    private void Update()
    {
        gameTimer += Time.deltaTime;
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            SpawnWave();

            float currentInterval = GetCurrentSpawnInterval();
            yield return new WaitForSeconds(currentInterval);
        }
    }

    private void SpawnWave()
    {
        int platformCount = GetCurrentPlatformCount();

        // Minimal 1 platform aman di setiap wave.
        SpawnOnePlatform(true);

        for (int i = 1; i < platformCount; i++)
        {
            SpawnOnePlatform(false);
        }
    }

    private void SpawnOnePlatform(bool forceSafePlatform)
    {
        int columnIndex = GetRandomAvailableColumn();

        if (columnIndex == -1)
        {
            if (showDebug)
            {
                Debug.Log("[PlatformSpawner] Tidak ada kolom aman untuk spawn. Wave ini dilewati sebagian.");
            }

            return;
        }

        bool spawnFromTop = Random.value > 0.5f;

        Transform spawnPoint;
        Vector2 moveDirection;
        bool platformMoveUp;

        if (spawnFromTop)
        {
            spawnPoint = topSpawnPoints[columnIndex];

            // Kalau muncul dari atas, platform harus bergerak ke bawah.
            moveDirection = Vector2.down;
            platformMoveUp = false;
        }
        else
        {
            spawnPoint = bottomSpawnPoints[columnIndex];

            // Kalau muncul dari bawah, platform harus bergerak ke atas.
            moveDirection = Vector2.up;
            platformMoveUp = true;
        }

        GameObject prefabToSpawn = GetPlatformPrefab(forceSafePlatform, platformMoveUp);

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("[PlatformSpawner] Prefab platform belum diisi untuk arah ini.");
            return;
        }

        GameObject newPlatform = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);

        MovingPlatform movingPlatform = newPlatform.GetComponent<MovingPlatform>();

        if (movingPlatform != null)
        {
            movingPlatform.SetPlatformData(
                moveDirection,
                GetCurrentPlatformSpeed(),
                platformLifeTime,
                this,
                columnIndex
            );
        }

        LockColumn(columnIndex);

        if (showDebug)
        {
            string spawnSide = spawnFromTop ? "ATAS" : "BAWAH";
            string moveSide = platformMoveUp ? "NAIK" : "TURUN";
            string platformType = forceSafePlatform ? "SAFE" : "RANDOM";

            Debug.Log("[PlatformSpawner] Spawn " + platformType + " dari " + spawnSide +
                      " | Bergerak " + moveSide +
                      " | Kolom " + columnIndex +
                      " | Prefab: " + prefabToSpawn.name);
        }
    }

    private GameObject GetPlatformPrefab(bool forceSafePlatform, bool platformMoveUp)
    {
        GameObject[] safePrefabs;
        GameObject[] trapPrefabs;

        if (platformMoveUp)
        {
            safePrefabs = safeMoveUpPlatformPrefabs;
            trapPrefabs = trapMoveUpPlatformPrefabs;
        }
        else
        {
            safePrefabs = safeMoveDownPlatformPrefabs;
            trapPrefabs = trapMoveDownPlatformPrefabs;
        }

        if (forceSafePlatform)
        {
            return GetRandomPrefab(safePrefabs);
        }

        bool canSpawnTrap = gameTimer >= trapStartTime;
        bool spawnTrap = canSpawnTrap && Random.value <= GetCurrentTrapChance();

        if (spawnTrap && HasPrefab(trapPrefabs))
        {
            return GetRandomPrefab(trapPrefabs);
        }

        return GetRandomPrefab(safePrefabs);
    }

    private bool HasPrefab(GameObject[] prefabs)
    {
        return prefabs != null && prefabs.Length > 0;
    }

    private GameObject GetRandomPrefab(GameObject[] prefabs)
    {
        if (!HasPrefab(prefabs))
        {
            return null;
        }

        int randomIndex = Random.Range(0, prefabs.Length);
        return prefabs[randomIndex];
    }

    private void LockColumn(int columnIndex)
    {
        if (!lockColumnWhilePlatformAlive) return;
        if (columnIndex < 0 || columnIndex >= activeColumns.Length) return;

        activeColumns[columnIndex] = true;

        if (showDebug)
        {
            Debug.Log("[PlatformSpawner] Kolom " + columnIndex + " dikunci.");
        }
    }

    public void ReleaseColumn(int columnIndex)
    {
        if (!lockColumnWhilePlatformAlive) return;
        if (columnIndex < 0 || columnIndex >= activeColumns.Length) return;

        activeColumns[columnIndex] = false;

        if (showDebug)
        {
            Debug.Log("[PlatformSpawner] Kolom " + columnIndex + " dibuka lagi.");
        }
    }

    private int GetRandomAvailableColumn()
    {
        List<int> availableColumns = new List<int>();

        int columnCount = GetColumnCount();

        for (int i = 0; i < columnCount; i++)
        {
            if (IsColumnSafeToSpawn(i))
            {
                availableColumns.Add(i);
            }
        }

        if (availableColumns.Count == 0)
        {
            return -1;
        }

        int randomListIndex = Random.Range(0, availableColumns.Count);
        return availableColumns[randomListIndex];
    }

    private bool IsColumnSafeToSpawn(int columnIndex)
    {
        if (!lockColumnWhilePlatformAlive)
        {
            return true;
        }

        if (activeColumns[columnIndex])
        {
            return false;
        }

        if (blockNeighborColumns)
        {
            int leftColumn = columnIndex - 1;
            int rightColumn = columnIndex + 1;

            if (leftColumn >= 0 && activeColumns[leftColumn])
            {
                return false;
            }

            if (rightColumn < activeColumns.Length && activeColumns[rightColumn])
            {
                return false;
            }
        }

        return true;
    }

    private int GetCurrentPlatformCount()
    {
        int count = startPlatformPerWave;

        if (gameTimer >= addPlatformAfterSeconds)
        {
            count++;
        }

        if (gameTimer >= addMorePlatformAfterSeconds)
        {
            count++;
        }

        count = Mathf.Clamp(count, startPlatformPerWave, maxPlatformPerWave);

        return count;
    }

    private float GetCurrentSpawnInterval()
    {
        float interval = startSpawnInterval - (gameTimer * intervalDecreasePerSecond);
        interval = Mathf.Clamp(interval, minSpawnInterval, startSpawnInterval);

        return interval;
    }

    private float GetCurrentPlatformSpeed()
    {
        float speed = startPlatformSpeed + (gameTimer * speedIncreasePerSecond);
        speed = Mathf.Clamp(speed, startPlatformSpeed, maxPlatformSpeed);

        return speed;
    }

    private float GetCurrentTrapChance()
    {
        float extraChance = gameTimer * 0.002f;
        float currentTrapChance = trapChance + extraChance;

        return Mathf.Clamp(currentTrapChance, trapChance, maxTrapChance);
    }

    private int GetColumnCount()
    {
        if (topSpawnPoints == null || bottomSpawnPoints == null)
        {
            return 0;
        }

        return Mathf.Min(topSpawnPoints.Length, bottomSpawnPoints.Length);
    }
}