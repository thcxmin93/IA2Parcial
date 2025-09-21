using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// AGGREGATE: Combines total damage and kills data
[System.Serializable]
public class TurretStats
{
    public float totalDamage = 0f;
    public int kills = 0;
    public List<float> damageHistory = new List<float>();

    // TUPLE: Store damage and timestamp together
    public List<(float damage, float time)> damageTimeline = new List<(float, float)>();

    public float DamagePerKill => kills > 0 ? totalDamage / kills : totalDamage;
}

public class TurretController : MonoBehaviour
{
    [Header("Turret Settings")] public string turretName = "Turret";
    public float range = 5f;
    public float fireRate = 1f;

    public TurretStats stats = new TurretStats();
    private float lastShotTime = 0f;
    public bool isDebuffTurret;

    void Start()
    {
        if (string.IsNullOrEmpty(turretName))
        {
            turretName = $"Turret_{GetInstanceID()}";
        }

        StartCoroutine(AnalyzeDamageOverTime());
    }

    void Update()
    {
        if (Time.time - lastShotTime < 1f / fireRate)
            return;

        var allEnemies = FindObjectsOfType<Enemy>(); //Agarra a todos los enemigos

        //Mar LINQ
        if (isDebuffTurret) //Desahbilitar la curacion
        {
            var closestBlockEnemies = allEnemies
                .OfType<BlockEnemy>() // Los cambia a block enemy siempre q peuda,  si es optro enemigo no
                .OrderBy(blockEnemy =>
                    Vector3.Distance(transform.position,
                        blockEnemy.transform
                            .position)) //Los ordena en base a la distancia, de la la torre a toddos los enemigoss, del mas cercano al mas lejano
                .FirstOrDefault(); //Agarrael primero, depsues de ordenarlos, si no hay niguno tira un null

            if (closestBlockEnemies != null)
            {
                closestBlockEnemies.StopHealth(); // Puede accederlo xq antes lo estamos casteando a blockEnemy
            }
        }
        //
        else
        {
            var targets = allEnemies
                .Where(enemy => enemy.IsAlive)
                .Where(enemy => Vector3.Distance(transform.position, enemy.transform.position) < range)
                .OrderBy(enemy => Vector3.Distance(transform.position, enemy.transform.position))
                .ToList();

            if (targets.Any())
            {
                AttackTarget(targets.First());
                lastShotTime = Time.time;
            }
        }
    }

    void AttackTarget(Enemy target)
    {
        float damage = 20f;
        bool enemyWasAlive = target.health > 0;

        target.health -= damage;

        // UPDATE AGGREGATE
        stats.totalDamage += damage;
        stats.damageHistory.Add(damage);

        // TUPLE: Store damage with timestamp
        stats.damageTimeline.Add((damage, Time.time));

        if (enemyWasAlive && target.health <= 0)
        {
            stats.kills++;
        }

        Debug.Log(
            $"[{turretName}] Total Damage: {stats.totalDamage}, Kills: {stats.kills}, Damage Per Kill: {stats.DamagePerKill:F1}");
    }

    // TIME-SLICING FUNCTION with TUPLE usage
    IEnumerator AnalyzeDamageOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            if (stats.damageTimeline.Count > 0)
            {
                int batchSize = 10;
                float totalAnalyzed = 0f;
                int batches = 0;

                for (int i = 0; i < stats.damageTimeline.Count; i += batchSize)
                {
                    // Process tuples in batches
                    var batch = stats.damageTimeline.Skip(i).Take(batchSize);

                    // Use tuple values - damage and time
                    float batchDamage = batch.Sum(tuple => tuple.damage);
                    float timeSpan = batch.Any() ? batch.Last().time - batch.First().time : 0f;

                    totalAnalyzed += batchDamage;
                    batches++;

                    // Show tuple data usage
                    if (timeSpan > 0)
                    {
                        float dps = batchDamage / timeSpan;
                        Debug.Log($"[{turretName}] Batch DPS: {dps:F1} (Damage: {batchDamage}, Time: {timeSpan:F1}s)");
                    }

                    yield return null;
                }
            }
        }
    }

    //LINQ
    public float CalculateTurretEfficiency() // Agarra las stasts de la torreta
    {
        var recentDamage = stats.damageTimeline
            .Skip(Mathf.Max(0, stats.damageTimeline.Count - 10)) // agarra los ultimos 10 da;os q hizo
            .OrderBy(tuple => tuple.time) // Los ordena en base al tiempo
            .ToDictionary(tuple => tuple.time, tuple => tuple.damage); // Los hace diccionario

        if (recentDamage.Count == 0) return 0f;

        float totalRecentDamage = recentDamage.Values.Sum();
        float timeSpan = recentDamage.Keys.Max() - recentDamage.Keys.Min();

        float efficiency = timeSpan > 0 ? totalRecentDamage / timeSpan : totalRecentDamage;

        Debug.Log($"[{turretName}] Current Efficiency: {efficiency:F2} DPS");
        return efficiency;
    }
}