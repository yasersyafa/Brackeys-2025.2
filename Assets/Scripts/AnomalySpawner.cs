using System.Collections.Generic;
using Assets.Scripts.ScriptableObjects;
using UnityEngine;

public class AnomalySpawner : MonoBehaviour
{
    public List<AnomalySO> Anomalies { get; private set; } = new();
    public float SpawnInterval { get; private set; } = 5f;
    public float SpawnTime { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // shuffle the anomalies list
        ShuffleHelper.Shuffle(Anomalies);
        SpawnTime = Random.Range(0f, 10f);
    }
    void Start()
    {
        // call the anomaly spawn method
        InvokeRepeating(nameof(SpawnAnomaly), SpawnTime, SpawnInterval);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SpawnAnomaly(AnomalySO anomaly)
    {
        // Instantiate the anomaly prefab at a random position
        Vector3 randomPosition = new(Random.Range(-10f, 10f), 0, 20);
        Instantiate(anomaly.anomalyPrefab, randomPosition, Quaternion.identity);

        // Log the anomaly identifier
        Debug.Log($"Spawned Anomaly: {anomaly.anomalyIdentifier}");
    }
}
