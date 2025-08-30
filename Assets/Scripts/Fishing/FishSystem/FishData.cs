using UnityEngine;

[CreateAssetMenu(fileName = "New Fish", menuName = "Fishing/Fish Data")]
public class FishData : ScriptableObject
{
    [Header("Basic Info")]
    public string fishName;
    public FishRarity rarity;
    public Sprite fishIcon;
    public GameObject fishPrefab; // 3D model for the fish
    
    [Header("Stats")]
    public int baseHP = 100;
    public float strength = 1.0f; // Affects difficulty
    public float weight = 1.0f; // In kg
    public float size = 1.0f; // Visual size multiplier
    
    [Header("Fishing Properties")]
    [Range(1, 10)]
    public int minRotationsToReel = 3; // Minimum rotations to complete reeling
    [Range(1, 15)]
    public int maxRotationsToReel = 8; // Maximum rotations to complete reeling
    
    [Header("Description")]
    [TextArea(3, 5)]
    public string description;
    
    // Calculate final HP using new formula: hp = 100 * (1 + s)
    public int GetFinalHP()
    {
        float calculatedStrength = GetCalculatedStrength();
        return Mathf.RoundToInt(baseHP * (1f + calculatedStrength));
    }
    
    // Calculate spinning rotations needed to reel based on strength and rarity
    public int GetRotationsToReel()
    {
        float rarityMultiplier = GetRarityMultiplier();
        float strengthMultiplier = 1f + (strength - 1f) * 0.5f; // Strength affects rotation count
        
        int baseRotations = Random.Range(minRotationsToReel, maxRotationsToReel + 1); // Random rotations within range
        int finalRotations = Mathf.RoundToInt(baseRotations * rarityMultiplier * strengthMultiplier);
        
        return Mathf.Clamp(finalRotations, minRotationsToReel, maxRotationsToReel * 2);
    }
    
    // Calculate weight using new formula: w = random.randint(1, 4)
    public float GetCalculatedWeight()
    {
        System.Random rand = new System.Random();
        return rand.Next(1, 5); // Random integer between 1-4 inclusive
    }
    
    // Calculate strength using new formula: r = r * (w / 10)
    public float GetCalculatedStrength()
    {
        float rarityValue = GetRarityValue();
        float calculatedWeight = GetCalculatedWeight();
        return rarityValue * (calculatedWeight / 10f);
    }
    
    // Get rarity multiplier for HP and other calculations
    private float GetRarityMultiplier()
    {
        return rarity switch
        {
            FishRarity.Common => 1.0f,
            FishRarity.Rare => 1.6f,
            FishRarity.Legendary => 2.1f,
            _ => 1.0f
        };
    }
    
    // Get rarity value for new formula calculations
    private float GetRarityValue()
    {
        return rarity switch
        {
            FishRarity.Common => 1f,
            FishRarity.Rare => 3f,
            FishRarity.Legendary => 5f,
            _ => 1f
        };
    }
    
    // Get min and max HP range for this fish
    public Vector2Int GetHPRange()
    {
        int finalHP = GetFinalHP();
        int minHP = Mathf.RoundToInt(finalHP * 0.9f); // -10%
        int maxHP = Mathf.RoundToInt(finalHP * 1.1f); // +10%
        
        return new Vector2Int(minHP, maxHP);
    }
}

public enum FishRarity
{
    Common = 1,    // 70% spawn chance
    Rare = 3,      // 20% spawn chance  
    Legendary = 5  // 10% spawn chance
}
