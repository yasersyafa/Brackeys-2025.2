using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class FishSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [SerializeField] private FishDatabase fishDatabase;
    [SerializeField] private Transform baitTransform; // Reference to the bait
    [SerializeField] private float spawnRadius = 15f; // How far from bait to spawn fish
    [SerializeField] private float fishSpeed = 3f; // How fast fish move toward bait
    [SerializeField] private float triggerDistance = 2f; // Distance to trigger reeling
    
    [Header("Spawn Timing")]
    [SerializeField] private float minSpawnDelay = 2f; // Minimum time before spawning
    [SerializeField] private float maxSpawnDelay = 8f; // Maximum time before spawning
    [SerializeField] private bool autoSpawn = true; // Automatically spawn fish when bait is in water
    [SerializeField] private bool allowMultipleFish = false; // Whether to allow multiple fish per cast
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool showGizmos = true;
    
    // Private variables
    private GameObject currentFishGameObject; // Current fish in scene
    private Fish currentFishData; // Current fish data
    private Coroutine spawnCoroutine;
    private bool isSpawningEnabled = false;
    private bool hasSpawnedForCurrentCast = false; // Track if fish has been spawned for current cast
    
    void OnEnable()
    {
        // Listen to bait events
        Bait.OnCaughtFish += OnFishReached;
        
        // Listen to reeling events to stop spawning
        Reeling.OnReelingStarted += OnReelingStarted;
        Reeling.OnReelingCompleted += OnReelingCompleted;
        
        // Listen to fish struggle events for rotation
        Reeling.OnFishStruggleStarted += OnFishStruggleStarted;
        Reeling.OnFishStruggleEnded += OnFishStruggleEnded;
    }
    
    void OnDisable()
    {
        Bait.OnCaughtFish -= OnFishReached;
        Reeling.OnReelingStarted -= OnReelingStarted;
        Reeling.OnReelingCompleted -= OnReelingCompleted;
        Reeling.OnFishStruggleStarted -= OnFishStruggleStarted;
        Reeling.OnFishStruggleEnded -= OnFishStruggleEnded;
        
        // Stop spawning coroutine
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        
        // Clean up DOTween animations
        if (currentFishGameObject != null)
        {
            DOTween.Kill(currentFishGameObject.transform);
            // Unparent fish before scene cleanup
            currentFishGameObject.transform.SetParent(null);
        }
    }
    
    void Start()
    {
        // Find bait if not assigned
        if (baitTransform == null)
        {
            var bait = FindFirstObjectByType<Bait>();
            if (bait != null)
                baitTransform = bait.transform;
        }
        
        // Validate fish database
        if (fishDatabase == null)
        {
            Debug.LogError("FishSpawner: No fish database assigned!");
            return;
        }
        
        var allFish = fishDatabase.GetAllFish();
        if (allFish == null || allFish.Count == 0)
        {
            Debug.LogError("FishSpawner: Fish database is empty!");
            return;
        }
    }
    
    void Update()
    {
        // Check if fish should start moving toward bait
        if (currentFishGameObject != null && baitTransform != null)
        {
            UpdateFishMovement();
        }
    }
    
    public void StartSpawning()
    {
        if (!isSpawningEnabled && baitTransform != null)
        {
            isSpawningEnabled = true;
            hasSpawnedForCurrentCast = false; // Reset spawn flag for new cast
            
            if (autoSpawn && spawnCoroutine == null)
            {
                spawnCoroutine = StartCoroutine(SpawnFishRoutine());
            }
            
            if (debugMode)
                Debug.Log("FishSpawner: Started spawning");
        }
    }
    
    public void StopSpawning()
    {
        isSpawningEnabled = false;
        hasSpawnedForCurrentCast = false; // Reset spawn flag
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        
        // Cleanup current fish
        if (currentFishGameObject != null)
        {
            // Restore physics before cleanup
            var fishRigidbody = currentFishGameObject.GetComponent<Rigidbody>();
            if (fishRigidbody != null)
            {
                fishRigidbody.isKinematic = false;
            }
            
            // Unparent before destroying
            currentFishGameObject.transform.SetParent(null);
            Destroy(currentFishGameObject);
            currentFishGameObject = null;
        }
        
        // Clear fish data
        currentFishData = null;
        
        // Clear fish data from FishManager
        var fishManager = FindFirstObjectByType<FishManager>();
        if (fishManager != null)
        {
            fishManager.ClearCurrentFish();
        }
        
        if (debugMode)
            Debug.Log("FishSpawner: Stopped spawning and cleared fish data");
    }
    
    private IEnumerator SpawnFishRoutine()
    {
        while (isSpawningEnabled)
        {
            // Wait random delay
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);
            
            // Check if we should still spawn (bait in water, no current fish, and haven't spawned for this cast)
            if (isSpawningEnabled && currentFishGameObject == null && !hasSpawnedForCurrentCast && baitTransform != null)
            {
                var bait = baitTransform.GetComponent<Bait>();
                if (bait != null && bait.IsInWater())
                {
                    SpawnFish();
                    if (!allowMultipleFish)
                    {
                        hasSpawnedForCurrentCast = true; // Mark that we've spawned for this cast
                    }
                }
            }
        }
    }
    
    private void SpawnFish()
    {
        if (fishDatabase == null || currentFishGameObject != null) return;
        
        // Select fish based on rarity
        FishData selectedFish = SelectFishByRarity();
        if (selectedFish == null)
        {
            Debug.LogWarning("FishSpawner: Failed to select fish!");
            return;
        }
        
        // Create fish data
        currentFishData = new Fish(selectedFish);
        
        // Spawn fish prefab
        if (selectedFish.fishPrefab != null)
        {
            // Calculate spawn position (around bait in a circle)
            Vector3 spawnPosition = GetRandomSpawnPosition();
            
            // Calculate rotation to face bait immediately
            Vector3 directionToBait = (baitTransform.position - spawnPosition).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(directionToBait);
            
            currentFishGameObject = Instantiate(selectedFish.fishPrefab, spawnPosition, lookRotation);
            
            // Setup fish component if it exists
            var fishComponent = currentFishGameObject.GetComponent<FishBehavior>();
            if (fishComponent != null)
            {
                fishComponent.Initialize(currentFishData, baitTransform);
            }
            
            if (debugMode)
                Debug.Log($"FishSpawner: Spawned {selectedFish.fishName} at {spawnPosition}, facing bait at {baitTransform.position}");
        }
        else
        {
            Debug.LogWarning($"FishSpawner: Fish {selectedFish.fishName} has no prefab assigned!");
        }
    }
    
    private FishData SelectFishByRarity()
    {
        if (fishDatabase == null) return null;
        
        var allFish = fishDatabase.GetAllFish();
        if (allFish.Count == 0) return null;
        
        // Calculate total rarity weights
        // Common: 70%, Rare: 20%, Legendary: 10%
        float random = Random.Range(0f, 100f);
        
        List<FishData> commonFish = fishDatabase.GetFishByRarity(FishRarity.Common);
        List<FishData> rareFish = fishDatabase.GetFishByRarity(FishRarity.Rare);
        List<FishData> legendaryFish = fishDatabase.GetFishByRarity(FishRarity.Legendary);
        
        // Select based on rarity chance
        if (random < 70f && commonFish.Count > 0) // 70% for common
        {
            return commonFish[Random.Range(0, commonFish.Count)];
        }
        else if (random < 90f && rareFish.Count > 0) // 20% for rare (70-90)
        {
            return rareFish[Random.Range(0, rareFish.Count)];
        }
        else if (legendaryFish.Count > 0) // 10% for legendary (90-100)
        {
            return legendaryFish[Random.Range(0, legendaryFish.Count)];
        }
        
        // Fallback to any available fish
        return allFish[Random.Range(0, allFish.Count)];
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        if (baitTransform == null) return Vector3.zero;
        
        // Spawn in a circle around the bait
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(spawnRadius * 0.7f, spawnRadius);
        
        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * distance,
            0f, // Keep at same Y level as bait (underwater)
            Mathf.Sin(angle) * distance
        );
        
        return baitTransform.position + offset;
    }
    
    private void UpdateFishMovement()
    {
        if (currentFishGameObject == null || baitTransform == null) return;
        
        // Get fish behavior component
        var fishBehavior = currentFishGameObject.GetComponent<FishBehavior>();
        if (fishBehavior == null) return;
        
        // Only move fish if it's not hooked
        if (!fishBehavior.CanMove()) return;
        
        // Calculate distance to bait
        float distance = Vector3.Distance(currentFishGameObject.transform.position, baitTransform.position);
        
        // Check if fish reached trigger distance
        if (distance <= triggerDistance)
        {
            TriggerFishReaching();
            return;
        }
        
        // Move fish toward bait (this is handled by FishBehavior now, but keep for manual movement)
        Vector3 direction = (baitTransform.position - currentFishGameObject.transform.position).normalized;
        currentFishGameObject.transform.position += direction * fishSpeed * Time.deltaTime;
        
        // Rotate fish to face bait
        currentFishGameObject.transform.LookAt(baitTransform.position);
    }
    
    private void TriggerFishReaching()
    {
        if (currentFishData == null) return;
        
        // Instead of destroying fish, position it in front of bait
        PositionFishAtBait();
        
        // Notify FishManager about the new fish
        var fishManager = FindFirstObjectByType<FishManager>();
        if (fishManager != null)
        {
            fishManager.SetCurrentFish(currentFishData);
        }
        
        // Trigger reeling start
        var bait = baitTransform.GetComponent<Bait>();
        if (bait != null)
        {
            bait.StartReeling();
        }
        
        if (debugMode)
            Debug.Log($"FishSpawner: Fish {currentFishData.GetDisplayName()} reached bait - starting reeling!");
    }
    
    private void PositionFishAtBait()
    {
        if (currentFishGameObject == null || baitTransform == null) return;
        
        // Disable fish physics so it can be properly parented
        var fishRigidbody = currentFishGameObject.GetComponent<Rigidbody>();
        if (fishRigidbody != null)
        {
            fishRigidbody.isKinematic = true; // Make kinematic so it follows parent transform
            fishRigidbody.linearVelocity = Vector3.zero;
            fishRigidbody.angularVelocity = Vector3.zero;
        }
        
        // Parent fish to bait so it follows bait movement
        currentFishGameObject.transform.SetParent(baitTransform);
        
        // Set local position relative to bait (x=0, y=0, z=0)
        Vector3 localPosition = Vector3.zero;
        currentFishGameObject.transform.localPosition = localPosition;

        // Set initial rotation when caught (x=0, y=90, z=0)
        Vector3 caughtRotation = new Vector3(0f, 180f, 0f);
        currentFishGameObject.transform.localRotation = Quaternion.Euler(caughtRotation);
        
        // Use state machine to mark fish as hooked
        var fishBehavior = currentFishGameObject.GetComponent<FishBehavior>();
        if (fishBehavior != null)
        {
            var stateMachine = fishBehavior.GetStateMachine();
            if (stateMachine != null)
            {
                stateMachine.ForceHook();
            }
        }
        
        if (debugMode)
            Debug.Log($"FishSpawner: Parented fish to bait at local position {localPosition}, rotation {caughtRotation}");
        
        // Test if parenting worked
        if (debugMode && currentFishGameObject.transform.parent == baitTransform)
        {
            Debug.Log($"FishSpawner: Parenting successful! Fish parent: {currentFishGameObject.transform.parent.name}");
        }
    }
    
    // Event handlers
    private void OnFishReached()
    {
        // Fish has been hooked, but don't destroy it - it's now positioned at bait
        if (debugMode)
            Debug.Log("FishSpawner: Fish reached bait and is now hooked");
    }
    
    private void OnReelingStarted()
    {
        // Stop spawning while reeling
        isSpawningEnabled = false;
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
    
    private void OnReelingCompleted()
    {
        // Cleanup fish when reeling completes
        if (currentFishGameObject != null)
        {
            // Clean up any DOTween animations on the fish
            DOTween.Kill(currentFishGameObject.transform);
            
            // Restore physics before unparenting if needed
            var fishRigidbody = currentFishGameObject.GetComponent<Rigidbody>();
            if (fishRigidbody != null)
            {
                fishRigidbody.isKinematic = false; // Restore physics
            }
            
            // Unparent fish before destroying
            currentFishGameObject.transform.SetParent(null);
            
            Destroy(currentFishGameObject);
            currentFishGameObject = null;
        }
        
        // Clear fish data
        currentFishData = null;
        
        // Clear fish data from FishManager
        var fishManager = FindFirstObjectByType<FishManager>();
        if (fishManager != null)
        {
            fishManager.ClearCurrentFish();
        }
        
        // Restart spawning if bait is still in water
        if (baitTransform != null)
        {
            var bait = baitTransform.GetComponent<Bait>();
            if (bait != null && bait.IsInWater())
            {
                StartSpawning();
            }
        }
    }
    
    private void OnFishStruggleStarted()
    {
        // Rotate fish to struggle position when struggle starts (x=0, y=-90, z=0)
        if (currentFishGameObject != null)
        {
            Vector3 struggleRotation = Vector3.zero;

            // Smooth rotation using DOTween
            currentFishGameObject.transform.DOLocalRotate(struggleRotation, 0.5f)
                .SetEase(Ease.OutQuart);
            
            if (debugMode)
                Debug.Log($"FishSpawner: Fish struggling - rotated to {struggleRotation}");
        }
    }
    
    private void OnFishStruggleEnded()
    {
        // Rotate fish back to caught position when struggle ends (x=0, y=180, z=0)
        if (currentFishGameObject != null)
        {
            Vector3 endRotation = new Vector3(0f, 180f, 0f);
            
            // Smooth rotation using DOTween
            currentFishGameObject.transform.DOLocalRotate(endRotation, 0.3f)
                .SetEase(Ease.OutQuart);
            
            if (debugMode)
                Debug.Log($"FishSpawner: Fish struggle ended - rotated to {endRotation}");
        }
    }
    
    // Public methods
    public void ForceSpawnFish()
    {
        if (currentFishGameObject == null)
        {
            SpawnFish();
        }
    }
    
    public Fish GetCurrentFish()
    {
        return currentFishData;
    }
    
    public bool HasFishSpawned()
    {
        return currentFishGameObject != null;
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (!showGizmos || baitTransform == null) return;
        
        // Draw spawn radius
        Gizmos.color = Color.yellow;
        DrawWireCircle(baitTransform.position, spawnRadius);
        
        // Draw trigger distance
        Gizmos.color = Color.red;
        DrawWireCircle(baitTransform.position, triggerDistance);
        
        // Draw line to current fish
        if (currentFishGameObject != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(baitTransform.position, currentFishGameObject.transform.position);
        }
    }
    
    private void DrawWireCircle(Vector3 center, float radius)
    {
        int segments = 32;
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + Vector3.forward * radius;
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Sin(angle) * radius, 0, Mathf.Cos(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}
