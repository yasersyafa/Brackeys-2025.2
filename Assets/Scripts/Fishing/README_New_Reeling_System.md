# Updated Fishing System - Dynamic Reeling

## Overview
The fishing system has been updated with a new formula-based fish generation and a dynamic, continuous reeling mechanic that responds to real-time mouse spinning instead of discrete word completion.

## Fish System Changes

### New Formula Implementation
Based on the provided formula:
- **Weight Calculation**: `w = rd + r - 1` (where rd = random decimal 0-1, r = rarity value)
- **Strength Calculation**: `s = r * (w / 10)`
- **HP Calculation**: `hp = 100 * (1 + s)`

### Rarity Distribution
- **Common**: 70% chance (rarity value = 1)
- **Rare**: 20% chance (rarity value = 3)  
- **Legendary**: 10% chance (rarity value = 5)

### Updated Files
- `FishData.cs`: New formula methods (`GetCalculatedWeight()`, `GetCalculatedStrength()`, `GetFinalHP()`)
- `Fish.cs`: Updated to use rotations instead of words, new damage calculation
- `FishDatabase.cs`: Maintains the new drop rate structure
- `FishManager.cs`: Updated to handle rotation events instead of word events
- `FishInventory.cs`: Updated to track rotations instead of words

## Reeling System Changes

### New Circular Spinning Mechanic
Replaced the typing system with an OSU-like circular spinning mechanic:

#### Core Features
- **Mouse-based spinning**: Players drag mouse in circles to reel fish
- **Clockwise rotation detection**: Only clockwise spins count toward progress
- **Continuous movement**: Bait moves dynamically based on real-time spin speed
- **Visual feedback**: Wheel rotates and UI updates in real-time

#### Reeling Physics
- **Continuous Force Application**: Bait receives force proportional to spin speed
- **Dynamic Scaling**: Faster spinning = stronger reeling force
- **Realistic Movement**: Force applied every FixedUpdate() for smooth motion
- **Distance-based Targeting**: Bait moves toward the `reelingTarget` transform

### Technical Implementation

#### Bait.cs Changes
```csharp
// New continuous reeling system
private void HandleContinuousReeling()
{
    float spinSpeed = reelingScript.GetSpinSpeed();
    bool isSpinning = reelingScript.IsSpinning();
    
    if (isSpinning && spinSpeed > 0f)
    {
        // Force scales with spin speed (0-300°/s)
        float reelForce = baseReelForce + (maxSpinForce * normalizedSpinSpeed);
        ApplyReelingForce(reelForce, reelUpForce);
    }
}
```

#### Reeling.cs Features
- **Spin Speed Detection**: Tracks mouse movement angular velocity
- **Direction Validation**: Ensures clockwise rotation
- **Progress Tracking**: Counts completed 360° rotations
- **UI Integration**: Real-time progress bar and instruction updates

### UI Components
The new reeling system requires:
- **Reeling Canvas**: Overlay canvas for the spinning interface
- **Wheel Transform**: Visual wheel that rotates with mouse movement
- **Progress Bar**: Shows rotation completion progress
- **Progress Text**: Displays "Rotations: X / Y"
- **Instruction Text**: Dynamic feedback on spinning state

### Events System
```csharp
// New events for continuous feedback
public static event Action OnRotationCompleted;     // Full 360° rotation
public static event Action<float> OnRotationProgress; // 0-1 progress
public static event Action OnReelingStarted;        // Reeling begins
public static event Action OnReelingCompleted;      // Reeling ends
```

## Setup Instructions

### 1. Fish Database Setup
- Create fish ScriptableObjects with the new formula in mind
- Set appropriate rarity values (Common=1, Rare=3, Legendary=5)
- Adjust `minRotationsToReel` and `maxRotationsToReel` for rotation counts

### 2. Reeling UI Setup
- Use the `ReelingUISetup` helper script to create the UI
- Assign UI references in the `Reeling` script inspector
- Ensure the reeling canvas is set to Screen Space - Overlay

### 3. Bait Configuration
- Set the `reelingTarget` to your fishing rod or player transform
- The bait will automatically find the `Reeling` script in the scene
- Adjust force values in `HandleContinuousReeling()` for desired feel

## Benefits of the New System

### Improved Gameplay
- **More Engaging**: Active mouse spinning vs passive typing
- **Realistic Feel**: Continuous movement mimics real fishing reels
- **Skill-based**: Faster, more consistent spinning = better results
- **Visual Feedback**: Clear indication of spinning effectiveness

### Technical Advantages
- **Smooth Physics**: Force applied every frame for fluid motion
- **Scalable Difficulty**: Spin speed requirements can vary by fish
- **Modular Design**: Easy to tweak force curves and feedback
- **Event-driven**: Clean separation between spinning and fish systems

## Customization Options

### Force Tuning
- `baseReelForce`: Minimum force when spinning slowly
- `maxSpinForce`: Additional force at maximum spin speed  
- `reelUpForce`: Upward force to lift bait from water

### Spin Detection
- `minSpinSpeed`: Minimum degrees/second to register as spinning
- `rotationTolerance`: Allowable direction variance
- `spinDecay`: How quickly spin momentum fades

### Fish Difficulty
- Adjust rotation requirements via fish data
- Scale force effectiveness by fish strength/rarity
- Add resistance or escape mechanics based on fish stats

This new system provides a much more engaging and realistic fishing experience while maintaining the core progression and collection mechanics of the original design.
