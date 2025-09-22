using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemyBlueprint
    {
        public string name;
        public int reward;
    }

    public List<GameObject> enemyPrefabs;
    public Transform spawnPoint;
    public List<EnemyBlueprint> enemyList;

    // TUPLA : Lista de enemigos spawneados con su posición y recompensa
    public List<(GameObject enemy, Vector3 spawnPosition, int reward)> spawnedEnemyData = new List<(GameObject, Vector3, int)>();

    [Header("Wave Setup")] public int enemiesPerWave = 20;
    public int minReward = 5;
    public int maxReward = 21;
    public int minRewardFilter = 10;

    public int batchSize = 1;
    public float perSpawnDelay = 1f;

    [Header("Path")] public List<Transform> waypoints = new List<Transform>();
    public float enemySpeed = 3f;

    void Start()
    {
        // Generator
        var rawWave = Enumerable.Range(1, enemiesPerWave)
            .Select(i => new EnemyBlueprint
            {
                name = $"Enemy_{i}",
                reward = Random.Range(minReward, maxReward)
            })
            .ToList();

        // LINQ Where
        var filtered = rawWave.Where(e => e.reward >= minRewardFilter);

        // LINQ OrderByDescending
        var ordered = filtered.OrderByDescending(e => e.reward);

        // LINQ ToList
        enemyList = ordered.ToList();

        // Aggregate (actual: suma total de recompensas)
        int totalGold = enemyList.Aggregate(0, (acum, e) => acum + e.reward);
        Debug.Log($"Wave generada: {enemyList.Count} enemigos | Oro total esperado: {totalGold}");

        // Time slicing
        StartCoroutine(SpawnInBatches(enemyList, batchSize, perSpawnDelay));

        InvokeRepeating(nameof(AnalyzeWaveComposition), 3f, 10f);
        InvokeRepeating(nameof(AnalyzeSpawnedEnemyPositions), 5f, 8f); // NUEVO: Analiza posiciones usando tuplas
    }

    IEnumerator SpawnInBatches(List<EnemyBlueprint> wave, int chunk, float delay)
    {
        int spawned = 0;
        while (spawned < wave.Count)
        {
            var slice = wave.Skip(spawned).Take(chunk).ToList();

            foreach (var data in slice)
            {
                Vector3 pos = spawnPoint ? spawnPoint.position : Vector3.zero;

                var go = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Count)], pos, Quaternion.identity);
                go.name = data.name;

                var be = go.GetComponent<Enemy>();
                if (be != null)
                    be.reward = data.reward;

                var follower = go.GetComponent<EnemyFollowPath>();
                if (follower == null)
                    follower = go.AddComponent<EnemyFollowPath>();

                follower.Init(waypoints, enemySpeed);

                // TUPLA MAR: Guarda el enemigo, su posición de spawn y recompensa
                spawnedEnemyData.Add((go, pos, data.reward));

                Debug.Log($"SPAWN {go.name} → {data.reward} Gold at position {pos}");

                if (delay > 0f)
                    yield return new WaitForSeconds(delay);
                else
                    yield return null;
            }

            spawned += slice.Count;
        }
    }

    //  MAR LINQ
    public void AnalyzeWaveComposition()
    {
        var allSpawnedEnemies = FindObjectsOfType<Enemy>().Select(e => e.gameObject).ToList();
        //Busca a los enemigos, transforema todos los datos a gameobjects de enemigos, y depsues los hace una lista

        var enemyAnalysis = allSpawnedEnemies
            .Where(go => go.GetComponent<Enemy>() != null) // filtra a todo lo q tenga componente enemy
            .OrderByDescending(go =>
                go.GetComponent<Enemy>().reward) // ordena de mayor a  menor, en base a su recompensa
            .ToArray(); // los hace un array

        if (enemyAnalysis.Length > 0)
        {
            var topEnemy = enemyAnalysis[0].GetComponent<Enemy>();
            UICanvas.Instance.SetEnemiesAnalyzed($"Total enemies analyzed: {enemyAnalysis.Length}\n" +
                                                 $"Highest value target: {enemyAnalysis[0].name} with {topEnemy.reward} gold");
        }
        else
        {
            UICanvas.Instance.SetEnemiesAnalyzed("No enemy to analyzed");
        }
    }

    // FUNCIÓN CON TUPLAS : Analiza posiciones de spawn y recompensas
    public void AnalyzeSpawnedEnemyPositions()
    {
        if (spawnedEnemyData.Count == 0) return;

        // Filtrar solo enemigos que aún existen
        var activeEnemyData = spawnedEnemyData
            .Where(tuple => tuple.enemy != null) // Filtrar enemigos que no fueron destruidos
            .ToList();

        if (activeEnemyData.Count > 0)
        {
            // Usar las tuplas para análisis
            var highValueEnemies = activeEnemyData
                .Where(tuple => tuple.reward > 15) // Usar tupla.reward
                .OrderBy(tuple => Vector3.Distance(Vector3.zero, tuple.spawnPosition)) // Usar tupla.spawnPosition
                .ToList();

            if (highValueEnemies.Any())
            {
                var closestHighValue = highValueEnemies.First();
                Debug.Log($"[Wave Analysis] High-value enemy closest to origin: {closestHighValue.enemy.name} " +
                         $"with {closestHighValue.reward} gold, spawned at {closestHighValue.spawnPosition}");
            }

            // Estadísticas usando tuplas
            var avgReward = activeEnemyData.Average(tuple => tuple.reward);
            float avgDistanceFromOrigin = (float)activeEnemyData.Average(tuple => Vector3.Distance(Vector3.zero, tuple.spawnPosition));

            Debug.Log($"[Spawn Stats] Active enemies: {activeEnemyData.Count}, " +
                     $"Avg reward: {avgReward:F1}, Avg spawn distance: {avgDistanceFromOrigin:F1}");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!spawnPoint) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnPoint.position, 0.25f);
    }

    //Gizmos del path
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        Gizmos.color = Color.cyan;
        foreach (var wp in waypoints)
        {
            if (wp != null)
                Gizmos.DrawSphere(wp.position, 0.2f);
        }

        Gizmos.color = Color.green;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
}