using System.Collections.Generic;
using UnityEngine;

public class AnomalySpawner : MonoBehaviour
{
    public static AnomalySpawner Instance;
    [SerializeField]
    private List<Anomaly> _anomalies = new();
    [HideInInspector]
    public Anomaly _anomaly;
    [SerializeField]
    private float _spawnInterval = 5f;
    public float SpawnTime { get; private set; }
    public List<Anomaly> GetAnomalies => _anomalies;
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SpawnTime = Random.Range(0f, 10f);
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        // call the anomaly spawn method
        InvokeRepeating(nameof(SpawnAnomaly), SpawnTime, _spawnInterval);
    }

    public void SpawnAnomaly()
    {
        if(_anomaly != null)
        {
            // If an anomaly is already spawned, do not spawn another one
            Debug.Log("Anomaly already spawned, skipping spawn.");
            return;
        }
        Anomaly anomaly = _anomalies[Random.Range(0, _anomalies.Count)];
        _anomaly = anomaly;
        // Instantiate the anomaly prefab at a random position
        Vector3 randomPosition = new(Random.Range(-10f, 10f), 2.7f, 20);
        Instantiate(anomaly, randomPosition, anomaly.transform.rotation);

        // Log the anomaly identifier
        Debug.Log($"Spawned Anomaly: {anomaly}");
    }
}
