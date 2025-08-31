    using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

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
    
    private bool _hasStartedSpawning = false;
    
    // Events for anomaly state changes
    public static event Action OnAnomalySpawned;
    public static event Action OnAnomalyDestroyed;
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Subscribe to scene loaded event to reset spawner when scene reloads
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from scene loaded event to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset the spawner when a new scene is loaded
        ResetSpawner();
    }
    
    void Start()
    {
        // Initialize spawning
        StartSpawning();
    }
    
    /// <summary>
    /// Starts the anomaly spawning process
    /// </summary>
    public void StartSpawning()
    {
        if (!_hasStartedSpawning)
        {
            SpawnTime = UnityEngine.Random.Range(0f, 10f);
            InvokeRepeating(nameof(SpawnAnomaly), SpawnTime, _spawnInterval);
            _hasStartedSpawning = true;
            Debug.Log("Anomaly spawning started!");
        }
    }
    
    /// <summary>
    /// Resets the spawner state for a new game
    /// </summary>
    public void ResetSpawner()
    {
        // Stop any existing spawning
        CancelInvoke(nameof(SpawnAnomaly));
        
        // Clear current anomaly
        if (_anomaly != null)
        {
            if (_anomaly.gameObject != null)
            {
                Destroy(_anomaly.gameObject);
            }
            _anomaly = null;
        }
        
        // Reset spawning state
        _hasStartedSpawning = false;
        
        // Start spawning again
        StartSpawning();
        
        Debug.Log("Anomaly spawner reset!");
    }

    public void SpawnAnomaly()
    {
        if(_anomaly != null)
        {
            // If an anomaly is already spawned, do not spawn another one
            Debug.Log("Anomaly already spawned, skipping spawn.");
            return;
        }
        Anomaly anomaly = _anomalies[UnityEngine.Random.Range(0, _anomalies.Count)];
        _anomaly = anomaly;
        // Instantiate the anomaly prefab at a random position
        Vector3 randomPosition = new(UnityEngine.Random.Range(-10f, 10f), 2.7f, 20);
        Instantiate(anomaly, randomPosition, anomaly.transform.rotation);

        // Fire the event
        OnAnomalySpawned?.Invoke();

        // Log the anomaly identifier
        Debug.Log($"Spawned Anomaly: {anomaly}");
    }
    
    /// <summary>
    /// Call this when an anomaly is destroyed
    /// </summary>
    public void NotifyAnomalyDestroyed()
    {
        OnAnomalyDestroyed?.Invoke();
        Debug.Log("Anomaly destroyed event fired");
    }
}
