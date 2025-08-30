using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

[Serializable]
public class CaughtFish
{
    public string fishName;
    public FishRarity rarity;
    public float weight;
    public float size;
    public DateTime caughtTime;
    public int rotationsUsed; // How many rotations it took to catch
    
    public CaughtFish(Fish fish, int rotationsToComplete)
    {
        fishName = fish.fishData.fishName;
        rarity = fish.fishData.rarity;
        weight = fish.actualWeight;
        size = fish.actualSize;
        caughtTime = DateTime.Now;
        rotationsUsed = rotationsToComplete;
    }
}

public class FishInventory : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private List<CaughtFish> caughtFish = new List<CaughtFish>();
    
    [Header("Statistics")]
    [SerializeField] private int totalFishCaught = 0;
    [SerializeField] private int commonCaught = 0;
    [SerializeField] private int rareCaught = 0;
    [SerializeField] private int legendaryCaught = 0;
    
    [Header("Records")]
    [SerializeField] private float heaviestFishWeight = 0f;
    [SerializeField] private string heaviestFishName = "";
    [SerializeField] private float largestFishSize = 0f;
    [SerializeField] private string largestFishName = "";
    
    // Events
    public static event Action<CaughtFish> OnFishAddedToInventory;
    public static event Action<FishInventory> OnInventoryUpdated;
    
    void OnEnable()
    {
        FishManager.OnFishCaught += HandleFishCaught;
    }
    
    void OnDisable()
    {
        FishManager.OnFishCaught -= HandleFishCaught;
    }
    
    void Start()
    {
        LoadInventory();
    }
    
    private void HandleFishCaught(Fish fish)
    {
        if (fish == null || fish.fishData == null) return;
        
        // Create caught fish record
        CaughtFish caughtFishRecord = new CaughtFish(fish, fish.rotationsToReel);
        
        // Add to inventory
        AddFishToInventory(caughtFishRecord);
        
        Debug.Log($"Added to inventory: {caughtFishRecord.fishName} ({caughtFishRecord.weight:F1}kg)");
    }
    
    public void AddFishToInventory(CaughtFish fish)
    {
        caughtFish.Add(fish);
        UpdateStatistics(fish);
        SaveInventory();
        
        OnFishAddedToInventory?.Invoke(fish);
        OnInventoryUpdated?.Invoke(this);
    }
    
    private void UpdateStatistics(CaughtFish fish)
    {
        totalFishCaught++;
        
        // Update rarity counts
        switch (fish.rarity)
        {
            case FishRarity.Common:
                commonCaught++;
                break;
            case FishRarity.Rare:
                rareCaught++;
                break;
            case FishRarity.Legendary:
                legendaryCaught++;
                break;
        }
        
        // Update weight record
        if (fish.weight > heaviestFishWeight)
        {
            heaviestFishWeight = fish.weight;
            heaviestFishName = fish.fishName;
        }
        
        // Update size record
        if (fish.size > largestFishSize)
        {
            largestFishSize = fish.size;
            largestFishName = fish.fishName;
        }
    }
    
    // Get fish by species name
    public List<CaughtFish> GetFishByName(string fishName)
    {
        return caughtFish.Where(f => f.fishName.Equals(fishName, StringComparison.OrdinalIgnoreCase)).ToList();
    }
    
    // Get fish by rarity
    public List<CaughtFish> GetFishByRarity(FishRarity rarity)
    {
        return caughtFish.Where(f => f.rarity == rarity).ToList();
    }
    
    // Get unique species caught
    public List<string> GetUniqueSpeciesCaught()
    {
        return caughtFish.Select(f => f.fishName).Distinct().ToList();
    }
    
    // Get count of specific fish species
    public int GetSpeciesCount(string fishName)
    {
        return caughtFish.Count(f => f.fishName.Equals(fishName, StringComparison.OrdinalIgnoreCase));
    }
    
    // Get total weight of all caught fish
    public float GetTotalWeight()
    {
        return caughtFish.Sum(f => f.weight);
    }
    
    // Get average weight
    public float GetAverageWeight()
    {
        if (caughtFish.Count == 0) return 0f;
        return GetTotalWeight() / caughtFish.Count;
    }
    
    // Get heaviest fish of a species
    public CaughtFish GetHeaviestOfSpecies(string fishName)
    {
        var fishOfSpecies = GetFishByName(fishName);
        return fishOfSpecies.OrderByDescending(f => f.weight).FirstOrDefault();
    }
    
    // Get inventory statistics string
    public string GetInventoryStats()
    {
        return $"=== FISHING STATISTICS ===\n" +
               $"Total Fish Caught: {totalFishCaught}\n" +
               $"Common: {commonCaught} | Rare: {rareCaught} | Legendary: {legendaryCaught}\n" +
               $"Unique Species: {GetUniqueSpeciesCaught().Count}\n" +
               $"Total Weight: {GetTotalWeight():F1}kg\n" +
               $"Average Weight: {GetAverageWeight():F1}kg\n" +
               $"Heaviest Fish: {heaviestFishName} ({heaviestFishWeight:F1}kg)\n" +
               $"Largest Fish: {largestFishName} ({largestFishSize:F1}x size)";
    }
    
    // Get species collection progress
    public string GetCollectionProgress()
    {
        var uniqueSpecies = GetUniqueSpeciesCaught();
        var progressText = "=== COLLECTION PROGRESS ===\n";
        
        foreach (var species in uniqueSpecies.OrderBy(s => s))
        {
            int count = GetSpeciesCount(species);
            var heaviest = GetHeaviestOfSpecies(species);
            progressText += $"{species}: {count} caught (best: {heaviest.weight:F1}kg)\n";
        }
        
        return progressText;
    }
    
    // Save/Load functionality (basic PlayerPrefs implementation)
    private void SaveInventory()
    {
        try
        {
            string jsonData = JsonUtility.ToJson(new SerializableInventory(this));
            PlayerPrefs.SetString("FishInventory", jsonData);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save inventory: {e.Message}");
        }
    }
    
    private void LoadInventory()
    {
        try
        {
            if (PlayerPrefs.HasKey("FishInventory"))
            {
                string jsonData = PlayerPrefs.GetString("FishInventory");
                var loadedInventory = JsonUtility.FromJson<SerializableInventory>(jsonData);
                loadedInventory.ApplyToInventory(this);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load inventory: {e.Message}");
        }
    }
    
    // Public getters for UI
    public int TotalFishCaught => totalFishCaught;
    public int CommonCaught => commonCaught;
    public int RareCaught => rareCaught;
    public int LegendaryCaught => legendaryCaught;
    public float HeaviestFishWeight => heaviestFishWeight;
    public string HeaviestFishName => heaviestFishName;
    public float LargestFishSize => largestFishSize;
    public string LargestFishName => largestFishName;
    public List<CaughtFish> AllCaughtFish => new List<CaughtFish>(caughtFish);
    
    // Clear inventory (for testing/reset)
    [ContextMenu("Clear Inventory")]
    public void ClearInventory()
    {
        caughtFish.Clear();
        totalFishCaught = 0;
        commonCaught = 0;
        rareCaught = 0;
        legendaryCaught = 0;
        heaviestFishWeight = 0f;
        heaviestFishName = "";
        largestFishSize = 0f;
        largestFishName = "";
        
        SaveInventory();
        OnInventoryUpdated?.Invoke(this);
    }
}

// Helper class for JSON serialization
[Serializable]
public class SerializableInventory
{
    public List<CaughtFish> caughtFish;
    public int totalFishCaught;
    public int commonCaught;
    public int rareCaught;
    public int legendaryCaught;
    public float heaviestFishWeight;
    public string heaviestFishName;
    public float largestFishSize;
    public string largestFishName;
    
    public SerializableInventory(FishInventory inventory)
    {
        caughtFish = inventory.AllCaughtFish;
        totalFishCaught = inventory.TotalFishCaught;
        commonCaught = inventory.CommonCaught;
        rareCaught = inventory.RareCaught;
        legendaryCaught = inventory.LegendaryCaught;
        heaviestFishWeight = inventory.HeaviestFishWeight;
        heaviestFishName = inventory.HeaviestFishName;
        largestFishSize = inventory.LargestFishSize;
        largestFishName = inventory.LargestFishName;
    }
    
    public void ApplyToInventory(FishInventory inventory)
    {
        // Use reflection or direct field access to restore data
        // This is a simple implementation - in production you'd want more robust loading
        foreach (var fish in caughtFish)
        {
            inventory.AddFishToInventory(fish);
        }
    }
}
