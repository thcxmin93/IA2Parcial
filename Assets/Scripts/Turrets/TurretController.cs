using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

 
[System.Serializable]
public class TurretStats
{
    public float totalDamage = 0f;
    public int kills = 0;
    public List<float> damageHistory = new List<float>();

 
    public List<(float damage, float time)> damageTimeline = new List<(float, float)>();

    public float DamagePerKill => kills > 0 ? totalDamage / kills : totalDamage;
}

public class TurretController : MonoBehaviour
{
    [Header("Turret Settings")] public string turretName = "Turret";
    public float range = 5f;
    public float fireRate = 1f;
    public TMP_Text tuttetDPS;
    public TurretStats stats = new TurretStats();
    private float lastShotTime = 0f;
    public bool isDebuffTurret;

    void Start()
    {
        CalculateTurretEfficiency();
        if (string.IsNullOrEmpty(turretName))
        {
            turretName = $"Turret_{GetInstanceID()}";
        }

        StartCoroutine(AnalyzeDamageOverTime());
        StartCoroutine(DamageProjectionGenerator()); 
        InvokeRepeating(nameof(CalculateTurretEfficiency), 8f, 8f);
        InvokeRepeating(nameof(GeneratePerformanceReport), 10f, 10f);
        InvokeRepeating(nameof(CalculateAdvancedStats), 12f, 12f);
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
        //LINQ NACHO: primero ve si esta vivo, despues calcula la distancia, los ordena por cercania y los manda a una lista
        {
            var targets = allEnemies
                .Where(enemy => enemy.IsAlive)
                .Where(enemy => Vector3.Distance(transform.position, enemy.transform.position) < range)
                .OrderBy(enemy => Vector3.Distance(transform.position, enemy.transform.position))
                .ToList();

            if (targets.Any())
            //ataca al primero de la lista
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

       
        stats.totalDamage += damage;
        stats.damageHistory.Add(damage);

        // TUPLA: agrega el saño y el tiempo
        stats.damageTimeline.Add((damage, Time.time));

        if (enemyWasAlive && target.health <= 0)
        {
            stats.kills++;
        }

        Debug.Log(
            $"[{turretName}] Total Damage: {stats.totalDamage}, Kills: {stats.kills}, Damage Per Kill: {stats.DamagePerKill:F1}");
    }

    // TIME-SLICING Y TUPLA
    IEnumerator AnalyzeDamageOverTime()
    {
        while (true)
        //Lo mantiene siempre loopeando 
        {
            yield return new WaitForSeconds(5f);

            // TIME SLICING NACHO: solo se hyace una vez cada 5 seg

            if (stats.damageTimeline.Count > 0)
            //Pregunta si hice daño
            {
                int batchSize = 10;
                //cantidad de objetos que voy a revisar por analisis
                float totalAnalyzed = 0f;
                //suma para saber cuantos analice
                int batches = 0;
                //cantidad de grupos que voy analiazndo

                for (int i = 0; i < stats.damageTimeline.Count; i += batchSize)

                {
                    // divide los objetos en grupos de 10, saltaendo lo que ya reviso (agarra el siguiente grupo de 10 posible)
                    var batch = stats.damageTimeline.Skip(i).Take(batchSize);

                    // TUPLAS NACHO: daño y Tiempo 
                    float batchDamage = batch.Sum(tuple => tuple.damage);
                    //calcula cuanto daño se hizo en el grupo de 10
                    float timeSpan = batch.Any() ? batch.Last().time - batch.First().time : 0f;
                    // si hya algun objeto en el batch devuelve el margen entre tiempos, si no hay objetos devuelve 0
                    totalAnalyzed += batchDamage;
                    //le suma al daño total, el daño del batch actual
                    batches++;

                    // Muestro lo que calcule con las tuplas en consola
                    if (timeSpan > 0)
                    //si en algun lapso hice daño, calcula el dps
                    {
                        float dps = batchDamage / timeSpan;
                        Debug.Log($"[{turretName}] Batch DPS: {dps:F1} (Damage: {batchDamage}, Time: {timeSpan:F1}s)");
                    }

                    yield return null;
                    //pasa al sig frame
                }
            }
        }
    }

    //LINQ
    public void CalculateTurretEfficiency() // Agarra las stasts de la torreta
    {
        var recentDamage = stats.damageTimeline
            .Skip(Mathf.Max(0, stats.damageTimeline.Count - 10)) // agarra los ultimos 10 da;os q hizo
            .OrderBy(tuple => tuple.time) // Los ordena en base al tiempo
            .ToDictionary(tuple => tuple.time, tuple => tuple.damage); // Los hace diccionario

        if (recentDamage.Count == 0) return;

        float totalRecentDamage = recentDamage.Values.Sum();
        float timeSpan = recentDamage.Keys.Max() - recentDamage.Keys.Min();

        float efficiency = timeSpan > 0 ? totalRecentDamage / timeSpan : totalRecentDamage;

        Debug.Log($"[{turretName}] Current Efficiency: {efficiency:F2} DPS");
        tuttetDPS.text = $"{efficiency:F2} DPS";
    }

    // TIPO ANÓNIMO NACHO: Genera reporte de acción de disparo
    public void GeneratePerformanceReport()
    {
        if (stats.damageTimeline.Count == 0) return;

        // TIPO ANÓNIMO: Información sobre la acción de disparo
        var shootingReport = stats.damageTimeline
            .TakeLast(20) // Últimos 20 disparos
            .Select(tuple => new
            {
                Damage = tuple.damage,
                Time = tuple.time,
                IsHighDamage = tuple.damage > 15f,
                TimeSinceStart = tuple.time - stats.damageTimeline.First().time
            })
            .Where(report => report.IsHighDamage)
            .OrderBy(report => report.Time)
            .ToList();

        if (shootingReport.Any())
        //levanta el daño para dar la performance,dando un resumen general de las estadisticas de los ataques.
        {
            var summary = new
            {
                TurretName = turretName,
                HighDamageShots = shootingReport.Count,
                TotalHighDamage = shootingReport.Sum(s => s.Damage),
                AverageTime = shootingReport.Average(s => s.TimeSinceStart),
                Performance = shootingReport.Count > 10 ? "Excellent" : "Good"
            };

            Debug.Log($"[Performance Report] {summary.TurretName}: {summary.HighDamageShots} high-damage shots, " +
                     $"Total: {summary.TotalHighDamage}, Performance: {summary.Performance}");
        }
    }

    // AGGREGATE NACHO: Calcula estadísticas complejas usando Aggregate
    public void CalculateAdvancedStats()
    {
        if (stats.damageHistory.Count == 0) return;

        //AGGREGATE: Combina múltiples cálculos en una sola operación
        var complexStats = stats.damageHistory
            .TakeLast(15) // Últimos 15 disparos
            .Aggregate(
                new { TotalDamage = 0f, MaxDamage = 0f, ShotCount = 0, DamageVariation = 0f }, // Valor inicial
                (accumulator, damage) => new
                {
                    TotalDamage = accumulator.TotalDamage + damage,
                    MaxDamage = damage > accumulator.MaxDamage ? damage : accumulator.MaxDamage,
                    ShotCount = accumulator.ShotCount + 1,
                    DamageVariation = accumulator.DamageVariation + Mathf.Abs(damage - 20f) // Variación del daño base
                },
                result => new // Transformación final
                {
                    AverageDamage = result.ShotCount > 0 ? result.TotalDamage / result.ShotCount : 0f,
                    MaxDamage = result.MaxDamage,
                    Consistency = result.ShotCount > 0 ? 100f - (result.DamageVariation / result.ShotCount * 5f) : 100f,
                    TotalShots = result.ShotCount
                }
            );

        Debug.Log($"[Advanced Stats - {turretName}] Avg: {complexStats.AverageDamage:F1}, " +
                 $"Max: {complexStats.MaxDamage:F1}, Consistency: {complexStats.Consistency:F1}%, " +
                 $"Shots: {complexStats.TotalShots}");
    }

    // GENERATOR NACHO: Genera proyecciones de daño futuro de forma lazy
    IEnumerator DamageProjectionGenerator()
    {
        while (true)
        {
            yield return new WaitForSeconds(7f); // Cada 7 segundos genera proyección

            if (stats.damageHistory.Count >= 5) // Necesita al menos 5 disparos para proyectar
            {
                // Generator que calcula daño proyectado para los próximos 5 disparos
                var projections = GenerateFutureDamageProjections()
                    .Take(5) // Solo los primeros 5
                    .ToList(); // Consume el generator

                if (projections.Any())
                {
                    float totalProjected = projections.Sum();
                    float avgProjected = projections.Average();

                    Debug.Log($"[Damage Generator - {turretName}] Next 5 shots projected: " +
                             $"Total: {totalProjected:F1}, Avg: {avgProjected:F1}");
                }
            }

            yield return null; // Yield para no bloquear el frame
        }
    }

    // GENERATOR HELPER: Genera valores de daño proyectado infinitamente
    IEnumerable<float> GenerateFutureDamageProjections()
    {
        float baseDamage = 20f;
        var recentDamages = stats.damageHistory.TakeLast(5); // Últimos 5 para calcular tendencia

        float trend = recentDamages.Any() ? recentDamages.Average() : baseDamage;
        int shotNumber = 1;

        while (true) // Generator infinito
        {
            // Proyecta daño basado en tendencia + variación por shot
            float projectedDamage = trend + (shotNumber * 0.5f) - 2.5f; // Pequeña variación
            projectedDamage = Mathf.Max(projectedDamage, baseDamage * 0.8f); // Mínimo 80% del daño base

            yield return projectedDamage; // Genera el siguiente valor
            shotNumber++;

            if (shotNumber > 100) shotNumber = 1; // Reset para evitar números muy grandes
        }
    }
}