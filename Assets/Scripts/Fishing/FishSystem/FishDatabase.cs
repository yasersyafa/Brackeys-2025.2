using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

[CreateAssetMenu(fileName = "Fish Database", menuName = "Fishing/Fish Database")]
public class FishDatabase : ScriptableObject
{
    [Header("Fish Collections")]
    public List<FishData> commonFish = new List<FishData>();
    public List<FishData> rareFish = new List<FishData>();
    public List<FishData> legendaryFish = new List<FishData>();
    
    [Header("Drop Rates (Based on image)")]
    [Range(0f, 100f)]
    [Tooltip("Codfish + Mackerel = 70%")]
    public float commonDropRate = 70f;
    
    [Range(0f, 100f)]
    [Tooltip("Anglerfish + Oarfish = 20%")]
    public float rareDropRate = 20f;
    
    [Range(0f, 100f)]
    [Tooltip("Deadfish + Biblically Anglerfish = 10%")]
    public float legendaryDropRate = 10f;
    
    // Get all fish of a specific rarity
    public List<FishData> GetFishByRarity(FishRarity rarity)
    {
        return rarity switch
        {
            FishRarity.Common => commonFish,
            FishRarity.Rare => rareFish,
            FishRarity.Legendary => legendaryFish,
            _ => new List<FishData>()
        };
    }
    
    // Get all fish in the database
    public List<FishData> GetAllFish()
    {
        var allFish = new List<FishData>();
        allFish.AddRange(commonFish);
        allFish.AddRange(rareFish);
        allFish.AddRange(legendaryFish);
        return allFish;
    }
    
    // Validate that drop rates add up to 100%
    public bool ValidateDropRates()
    {
        float total = commonDropRate + rareDropRate + legendaryDropRate;
        return Mathf.Approximately(total, 100f);
    }
    
    // Get a fish based on normalized drop rates
    public FishData GetRandomFish()
    {
        if (!ValidateDropRates())
        {
            Debug.LogWarning("Drop rates don't add up to 100%! Using default rates.");
        }
        
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        
        // Determine rarity first
        FishRarity selectedRarity;
        if (randomValue < commonDropRate)
        {
            selectedRarity = FishRarity.Common;
        }
        else if (randomValue < commonDropRate + rareDropRate)
        {
            selectedRarity = FishRarity.Rare;
        }
        else
        {
            selectedRarity = FishRarity.Legendary;
        }
        
        // Get fish list for the selected rarity
        List<FishData> availableFish = GetFishByRarity(selectedRarity);
        
        if (availableFish.Count == 0)
        {
            Debug.LogWarning($"No fish available for rarity: {selectedRarity}. Falling back to common fish.");
            availableFish = commonFish;
        }
        
        if (availableFish.Count == 0)
        {
            Debug.LogError("No fish available in database!");
            return null;
        }
        
        // Return random fish from available list
        int randomIndex = UnityEngine.Random.Range(0, availableFish.Count);
        return availableFish[randomIndex];
    }
    
    // Get fish by name (useful for testing/debugging)
    public FishData GetFishByName(string fishName)
    {
        return GetAllFish().FirstOrDefault(fish => 
            fish.fishName.Equals(fishName, StringComparison.OrdinalIgnoreCase));
    }
    
    // Get statistics about the database
    public string GetDatabaseStats()
    {
        int totalFish = GetAllFish().Count;
        return $"Fish Database Stats:\n" +
               $"Common: {commonFish.Count}\n" +
               $"Rare: {rareFish.Count}\n" +
               $"Legendary: {legendaryFish.Count}\n" +
               $"Total: {totalFish}\n" +
               $"Drop Rates: {commonDropRate}% / {rareDropRate}% / {legendaryDropRate}%";
    }
}
