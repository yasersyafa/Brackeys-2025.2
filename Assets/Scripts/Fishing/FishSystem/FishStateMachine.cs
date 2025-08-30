using UnityEngine;
using System;

public enum FishState
{
    Spawning,       // Fish is being spawned
    Swimming,       // Fish is swimming toward bait
    Hooked,         // Fish has been hooked and is positioned at bait
    Struggling,     // Fish is struggling during reeling
    Caught,         // Fish has been successfully caught
    Escaped         // Fish has escaped
}

public class FishStateMachine : MonoBehaviour
{
    [Header("Current State")]
    [SerializeField] private FishState currentState = FishState.Spawning;
    [SerializeField] private bool debugMode = false;
    
    // State change events
    public static event Action<FishState, FishState> OnStateChanged; // oldState, newState
    public static event Action OnFishHooked;
    public static event Action OnFishCaught;
    public static event Action OnFishEscaped;
    
    private FishState previousState;
    
    void Start()
    {
        // Initialize state
        ChangeState(FishState.Swimming);
    }
    
    void OnEnable()
    {
        // Listen to reeling events
        Reeling.OnFishStruggleStarted += OnStruggleStarted;
        Reeling.OnFishStruggleEnded += OnStruggleEnded;
        
        // Listen to fish events
        Fish.OnFishCaught += OnFishCaughtEvent;
        Fish.OnFishEscaped += OnFishEscapedEvent;
    }
    
    void OnDisable()
    {
        Reeling.OnFishStruggleStarted -= OnStruggleStarted;
        Reeling.OnFishStruggleEnded -= OnStruggleEnded;
        Fish.OnFishCaught -= OnFishCaughtEvent;
        Fish.OnFishEscaped -= OnFishEscapedEvent;
    }
    
    public void ChangeState(FishState newState)
    {
        if (currentState == newState) return; // No change needed
        
        // Validate state transitions
        if (!IsValidTransition(currentState, newState))
        {
            if (debugMode)
                Debug.LogWarning($"FishStateMachine: Invalid transition from {currentState} to {newState}");
            return;
        }
        
        // Exit current state
        ExitState(currentState);
        
        // Store previous state
        previousState = currentState;
        currentState = newState;
        
        // Enter new state
        EnterState(newState);
        
        // Notify state change
        OnStateChanged?.Invoke(previousState, currentState);
        
        if (debugMode)
            Debug.Log($"FishStateMachine: {previousState} → {currentState}");
    }
    
    private bool IsValidTransition(FishState from, FishState to)
    {
        return (from, to) switch
        {
            // From Spawning
            (FishState.Spawning, FishState.Swimming) => true,
            
            // From Swimming
            (FishState.Swimming, FishState.Hooked) => true,
            (FishState.Swimming, FishState.Escaped) => true,
            
            // From Hooked
            (FishState.Hooked, FishState.Struggling) => true,
            (FishState.Hooked, FishState.Caught) => true,
            (FishState.Hooked, FishState.Escaped) => true,
            
            // From Struggling
            (FishState.Struggling, FishState.Hooked) => true,
            (FishState.Struggling, FishState.Caught) => true,
            (FishState.Struggling, FishState.Escaped) => true,
            
            // Terminal states (Caught/Escaped) - no transitions allowed
            (FishState.Caught, _) => false,
            (FishState.Escaped, _) => false,
            
            // Any other transition is invalid
            _ => false
        };
    }
    
    private void ExitState(FishState state)
    {
        switch (state)
        {
            case FishState.Swimming:
                // Stop movement
                break;
            case FishState.Hooked:
                // Clean up hook effects
                break;
            case FishState.Struggling:
                // Stop struggle effects
                break;
        }
    }
    
    private void EnterState(FishState state)
    {
        switch (state)
        {
            case FishState.Swimming:
                // Enable movement toward bait
                break;
            case FishState.Hooked:
                // Fish is now hooked, disable movement
                OnFishHooked?.Invoke();
                break;
            case FishState.Struggling:
                // Apply struggle effects
                break;
            case FishState.Caught:
                // Fish caught successfully
                OnFishCaught?.Invoke();
                break;
            case FishState.Escaped:
                // Fish escaped
                OnFishEscaped?.Invoke();
                break;
        }
    }
    
    // Event handlers
    private void OnStruggleStarted()
    {
        if (currentState == FishState.Hooked)
        {
            ChangeState(FishState.Struggling);
        }
    }
    
    private void OnStruggleEnded()
    {
        if (currentState == FishState.Struggling)
        {
            ChangeState(FishState.Hooked);
        }
    }
    
    private void OnFishCaughtEvent(Fish fish)
    {
        if (currentState == FishState.Hooked || currentState == FishState.Struggling)
        {
            ChangeState(FishState.Caught);
        }
    }
    
    private void OnFishEscapedEvent(Fish fish)
    {
        if (currentState != FishState.Caught) // Can escape from any state except caught
        {
            ChangeState(FishState.Escaped);
        }
    }
    
    // Public methods
    public FishState GetCurrentState()
    {
        return currentState;
    }
    
    public FishState GetPreviousState()
    {
        return previousState;
    }
    
    public bool IsInState(FishState state)
    {
        return currentState == state;
    }
    
    public bool CanMove()
    {
        return currentState == FishState.Swimming;
    }
    
    public bool IsHooked()
    {
        return currentState == FishState.Hooked || currentState == FishState.Struggling;
    }
    
    public bool IsActive()
    {
        return currentState != FishState.Caught && currentState != FishState.Escaped;
    }
    
    public void ForceHook()
    {
        if (currentState == FishState.Swimming)
        {
            ChangeState(FishState.Hooked);
        }
    }
    
    public void ForceCatch()
    {
        if (IsHooked())
        {
            ChangeState(FishState.Caught);
        }
    }
    
    public void ForceEscape()
    {
        if (IsActive())
        {
            ChangeState(FishState.Escaped);
        }
    }
}
