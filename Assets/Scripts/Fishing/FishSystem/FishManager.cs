using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class FishManager : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private FishDatabase fishDatabase;
    
    [Header("Current Fish")]
    [SerializeField] private Fish currentFish;
    
    [Header("UI References")]
    [SerializeField] private GameObject fishUI;
    [SerializeField] private TextMeshProUGUI fishNameText;
    [SerializeField] private TextMeshProUGUI fishStatsText;
    [SerializeField] private Slider fishHealthBar;
    [SerializeField] private Image fishIcon;
    [SerializeField] private Image healthBarFill;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool showFishUI = true;
    
    // Events
    public static event Action<Fish> OnFishHooked;
    public static event Action<Fish> OnFishCaught;
    public static event Action<Fish> OnFishLost;
    public static event Action<Fish, int> OnFishTookDamage;
    
    void OnEnable()
    {
        // Listen to bait events
        Bait.OnCaughtFish += HandleFishHooked;
        
        // Listen to reeling rotation completion
        Reeling.OnRotationCompleted += HandleRotationCompleted;
        
        // Listen to fish events
        Fish.OnFishCaught += HandleFishCaughtEvent;
        Fish.OnFishEscaped += HandleFishEscapedEvent;
        Fish.OnFishDamaged += HandleFishDamagedEvent;
    }
    
    void OnDisable()
    {
        Bait.OnCaughtFish -= HandleFishHooked;
        Reeling.OnRotationCompleted -= HandleRotationCompleted;
        Fish.OnFishCaught -= HandleFishCaughtEvent;
        Fish.OnFishEscaped -= HandleFishEscapedEvent;
        Fish.OnFishDamaged -= HandleFishDamagedEvent;
    }
    
    void Start()
    {
        // Hide fish UI initially
        if (fishUI != null)
        {
            fishUI.SetActive(false);
        }
        
        // Validate database
        if (fishDatabase == null)
        {
            Debug.LogError("FishManager: No fish database assigned!");
        }
        else if (debugMode)
        {
            Debug.Log(fishDatabase.GetDatabaseStats());
        }
    }
    
    // Called when bait hooks a fish
    private void HandleFishHooked()
    {
        if (fishDatabase == null)
        {
            Debug.LogError("Cannot hook fish - no database assigned!");
            return;
        }
        
        // Generate a random fish
        FishData fishData = fishDatabase.GetRandomFish();
        if (fishData == null)
        {
            Debug.LogError("Failed to generate fish from database!");
            return;
        }
        
        // Create fish instance
        currentFish = new Fish(fishData);
        
        if (debugMode)
        {
            Debug.Log($"Fish hooked: {currentFish}");
        }
        
        // Update UI
        UpdateFishUI();
        
        // Show fish UI
        if (fishUI != null && showFishUI)
        {
            fishUI.SetActive(true);
        }
        
        // Notify other systems
        OnFishHooked?.Invoke(currentFish);
    }
    
    // Called when a rotation is completed during reeling
    private void HandleRotationCompleted()
    {
        if (currentFish == null || currentFish.isCaught) return;
        
        // Calculate damage based on fish stats
        int damage = currentFish.CalculateRotationDamage();
        
        // Apply damage to fish
        currentFish.TakeDamage(damage);
        
        if (debugMode)
        {
            Debug.Log($"Rotation completed! Dealt {damage} damage to {currentFish.GetDisplayName()}");
        }
        
        // Update UI
        UpdateFishUI();
    }
    
    // Handle fish caught event
    private void HandleFishCaughtEvent(Fish fish)
    {
        if (debugMode)
        {
            Debug.Log($"Fish caught: {fish}");
        }
        
        // Hide fish UI after a delay
        if (fishUI != null)
        {
            Invoke(nameof(HideFishUI), 2f);
        }
        
        // Notify other systems
        OnFishCaught?.Invoke(fish);
        
        // Clear current fish
        currentFish = null;
    }
    
    // Handle fish escaped event
    private void HandleFishEscapedEvent(Fish fish)
    {
        if (debugMode)
        {
            Debug.Log($"Fish escaped: {fish}");
        }
        
        // Hide fish UI
        if (fishUI != null)
        {
            fishUI.SetActive(false);
        }
        
        // Notify other systems
        OnFishLost?.Invoke(fish);
        
        // Clear current fish
        currentFish = null;
    }
    
    // Handle fish damaged event
    private void HandleFishDamagedEvent(Fish fish, int damage)
    {
        if (debugMode)
        {
            Debug.Log($"{fish.GetDisplayName()} took {damage} damage! HP: {fish.currentHP}/{fish.maxHP}");
        }
        
        // Update UI immediately
        UpdateFishUI();
        
        // Notify other systems
        OnFishTookDamage?.Invoke(fish, damage);
    }
    
    // Update the fish UI with current fish data
    private void UpdateFishUI()
    {
        if (currentFish == null) return;
        
        // Update fish name
        if (fishNameText != null)
        {
            fishNameText.text = $"{currentFish.GetDisplayName()} ({currentFish.GetRarityText()})";
            fishNameText.color = currentFish.GetRarityColor();
        }
        
        // Update fish stats
        if (fishStatsText != null)
        {
            fishStatsText.text = $"HP: {currentFish.currentHP}/{currentFish.maxHP}\n" +
                               $"Weight: {currentFish.actualWeight:F1}kg\n" +
                               $"Strength: {currentFish.actualStrength:F1}\n" +
                               $"Rotations to Reel: {currentFish.rotationsToReel}";
        }
        
        // Update health bar
        if (fishHealthBar != null)
        {
            fishHealthBar.value = currentFish.GetHealthPercentage();
        }
        
        // Update health bar color based on health percentage
        if (healthBarFill != null)
        {
            float healthPercent = currentFish.GetHealthPercentage();
            if (healthPercent > 0.6f)
                healthBarFill.color = Color.green;
            else if (healthPercent > 0.3f)
                healthBarFill.color = Color.yellow;
            else
                healthBarFill.color = Color.red;
        }
        
        // Update fish icon
        if (fishIcon != null && currentFish.fishData != null && currentFish.fishData.fishIcon != null)
        {
            fishIcon.sprite = currentFish.fishData.fishIcon;
        }
    }
    
    private void HideFishUI()
    {
        if (fishUI != null)
        {
            fishUI.SetActive(false);
        }
    }
    
    // Public methods for external access
    public Fish GetCurrentFish()
    {
        return currentFish;
    }
    
    public bool HasFishHooked()
    {
        return currentFish != null && !currentFish.isCaught;
    }
    
    // Clear current fish data (called when bait is reset)
    public void ClearCurrentFish()
    {
        if (debugMode && currentFish != null)
        {
            Debug.Log($"FishManager: Clearing current fish: {currentFish.GetDisplayName()}");
        }
        
        currentFish = null;
        
        // Hide fish UI
        if (fishUI != null)
        {
            fishUI.SetActive(false);
        }
    }
    
    // Force fish to escape (can be called by other systems)
    public void ForceFishEscape()
    {
        if (currentFish != null && !currentFish.isCaught)
        {
            currentFish.EscapeFish();
        }
    }
    
    // Set current fish (called by FishSpawner)
    public void SetCurrentFish(Fish fish)
    {
        currentFish = fish;
        
        if (debugMode && fish != null)
        {
            Debug.Log($"FishManager: Set current fish to {fish}");
        }
        
        // Update UI
        UpdateFishUI();
        
        // Show fish UI
        if (fishUI != null && showFishUI)
        {
            fishUI.SetActive(true);
        }
        
        // Trigger fish hooked event
        OnFishHooked?.Invoke(currentFish);
    }
    
    // Debug method to manually hook a specific fish
    [ContextMenu("Hook Random Fish")]
    public void DebugHookRandomFish()
    {
        if (Application.isPlaying)
        {
            HandleFishHooked();
        }
    }
    
    // Debug method to damage current fish
    [ContextMenu("Damage Current Fish")]
    public void DebugDamageCurrentFish()
    {
        if (Application.isPlaying && currentFish != null)
        {
            HandleRotationCompleted();
        }
    }
}
