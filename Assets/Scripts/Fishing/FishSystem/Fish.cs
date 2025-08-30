using UnityEngine;
using System;

[Serializable]
public class Fish
{
    [Header("Instance Data")]
    public FishData fishData;
    public int currentHP;
    public int maxHP;
    public int rotationsToReel;
    public bool isCaught = false;
    
    [Header("Runtime Stats")]
    public float actualWeight; // Calculated weight using new formula
    public float actualSize; // Randomized size within fish data range
    public float actualStrength; // Calculated strength using new formula
    
    // Events
    public static event Action<Fish> OnFishCaught;
    public static event Action<Fish> OnFishEscaped;
    public static event Action<Fish, int> OnFishDamaged; // Fish, damage dealt
    
    // Constructor
    public Fish(FishData data)
    {
        fishData = data;
        InitializeFish();
    }
    
    private void InitializeFish()
    {
        if (fishData == null) return;
        
        // Calculate HP with some randomization using new formula
        Vector2Int hpRange = fishData.GetHPRange();
        maxHP = UnityEngine.Random.Range(hpRange.x, hpRange.y + 1);
        currentHP = maxHP;
        
        // Calculate rotations needed to reel
        rotationsToReel = fishData.GetRotationsToReel();
        
        // Use new formula calculations
        actualWeight = fishData.GetCalculatedWeight();
        actualStrength = fishData.GetCalculatedStrength();
        
        // Randomize size slightly
        float sizeVariation = UnityEngine.Random.Range(0.9f, 1.1f);
        actualSize = fishData.size * sizeVariation;
    }
    
    // Deal damage to the fish (when rotations are completed successfully)
    public void TakeDamage(int damage)
    {
        if (isCaught) return;
        
        currentHP = Mathf.Max(0, currentHP - damage);
        OnFishDamaged?.Invoke(this, damage);
        
        if (currentHP <= 0)
        {
            CatchFish();
        }
    }
    
    // Successfully catch the fish
    public void CatchFish()
    {
        if (isCaught) return;
        
        isCaught = true;
        currentHP = 0;
        OnFishCaught?.Invoke(this);
    }
    
    // Fish escapes (timeout, too many mistakes, etc.)
    public void EscapeFish()
    {
        if (isCaught) return;
        
        OnFishEscaped?.Invoke(this);
    }
    
    // Calculate damage based on rotation completion (can be modified by difficulty)
    public int CalculateRotationDamage()
    {
        // Base damage is percentage of max HP
        float baseDamagePercent = 100f / rotationsToReel; // Divide max HP by rotations needed
        int baseDamage = Mathf.RoundToInt(maxHP * (baseDamagePercent / 100f));
        
        // Add some randomization (-20% to +20%)
        float damageVariation = UnityEngine.Random.Range(0.8f, 1.2f);
        int finalDamage = Mathf.RoundToInt(baseDamage * damageVariation);
        
        return Mathf.Max(1, finalDamage); // Minimum 1 damage
    }
    
    // Get health percentage
    public float GetHealthPercentage()
    {
        if (maxHP <= 0) return 0f;
        return (float)currentHP / maxHP;
    }
    
    // Check if fish is in critical health (low HP, might escape)
    public bool IsInCriticalHealth()
    {
        return GetHealthPercentage() <= 0.25f; // 25% or less
    }
    
    // Get display info for UI
    public string GetDisplayName()
    {
        return fishData != null ? fishData.fishName : "Unknown Fish";
    }
    
    public string GetRarityText()
    {
        if (fishData == null) return "Unknown";
        
        return fishData.rarity switch
        {
            FishRarity.Common => "Common",
            FishRarity.Rare => "Rare", 
            FishRarity.Legendary => "Legendary",
            _ => "Unknown"
        };
    }
    
    public Color GetRarityColor()
    {
        if (fishData == null) return Color.white;
        
        return fishData.rarity switch
        {
            FishRarity.Common => Color.white,
            FishRarity.Rare => Color.blue,
            FishRarity.Legendary => Color.yellow,
            _ => Color.white
        };
    }
    
    // Get info string for debugging/display
    public override string ToString()
    {
        return $"{GetDisplayName()} ({GetRarityText()}) - HP: {currentHP}/{maxHP} - Weight: {actualWeight:F1}kg";
    }
}
