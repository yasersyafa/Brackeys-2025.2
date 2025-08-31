using System.Collections;
using UnityEngine;

public class JumpscareSystem : MonoBehaviour
{
    public static JumpscareSystem Instance;
    
    [Header("Jumpscare Settings")]
    [SerializeField] private float anomalyJumpscarePresent = 8f; // Will be randomized between 8-15f for anomaly
    [SerializeField] private float generatorJumpscarePresent = 8f; // Will be randomized between 8-15f for generator
    [SerializeField] private JumpscareAnimation jumpscareAnimation;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // Private variables to track states
    private float anomalyTimer = 0f;
    private bool isAnomalyPresent = false;
    private bool isGeneratorOn = true;
    private float generatorOffTimer = 0f;
    private bool jumpscareTriggeredForAnomaly = false;
    private bool jumpscareTriggeredForGenerator = false;
    
    // Coroutines for timers
    private Coroutine anomalyTimerCoroutine;
    private Coroutine generatorTimerCoroutine;
    
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        
        // Randomize jumpscare present time between 8-15 seconds for both conditions
        anomalyJumpscarePresent = Random.Range(8f, 15f);
        generatorJumpscarePresent = Random.Range(8f, 15f);
        
        if (enableDebugLogs)
        {
            Debug.Log($"JumpscareSystem: Anomaly jumpscare time set to {anomalyJumpscarePresent:F1} seconds");
            Debug.Log($"JumpscareSystem: Generator jumpscare time set to {generatorJumpscarePresent:F1} seconds");
        }
    }

    void Start()
    {
        // Find JumpscareAnimation if not assigned
        if (jumpscareAnimation == null)
        {
            jumpscareAnimation = FindFirstObjectByType<JumpscareAnimation>();
            if (jumpscareAnimation == null)
            {
                Debug.LogError("JumpscareSystem: No JumpscareAnimation found in scene!");
            }
        }
        
        // Subscribe to events
        AnomalySpawner.OnAnomalySpawned += OnAnomalySpawned;
        AnomalySpawner.OnAnomalyDestroyed += OnAnomalyDestroyed;
        GeneratorQTE.OnGeneratorTurnedOff += OnGeneratorTurnedOff;
        GeneratorQTE.OnGeneratorTurnedOn += OnGeneratorTurnedOn;
        JumpscareAnimation.OnJumpscareStart += OnJumpscareTriggered;
        
        if (enableDebugLogs)
            Debug.Log("JumpscareSystem: Event subscriptions complete");
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        AnomalySpawner.OnAnomalySpawned -= OnAnomalySpawned;
        AnomalySpawner.OnAnomalyDestroyed -= OnAnomalyDestroyed;
        GeneratorQTE.OnGeneratorTurnedOff -= OnGeneratorTurnedOff;
        GeneratorQTE.OnGeneratorTurnedOn -= OnGeneratorTurnedOn;
        JumpscareAnimation.OnJumpscareStart -= OnJumpscareTriggered;
    }

    #region Event Handlers
    
    private void OnAnomalySpawned()
    {
        if (enableDebugLogs)
            Debug.Log("JumpscareSystem: Anomaly spawned - starting timer");
        
        isAnomalyPresent = true;
        jumpscareTriggeredForAnomaly = false;
        
        // Play looping anomaly present sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfxLoop(1, 0.5f); // Play looping anomaly sound as SFX
        }
        
        // Stop existing timer if running
        if (anomalyTimerCoroutine != null)
        {
            StopCoroutine(anomalyTimerCoroutine);
        }
        
        // Start new timer
        anomalyTimerCoroutine = StartCoroutine(AnomalyTimerCoroutine());
    }
    
    private void OnAnomalyDestroyed()
    {
        if (enableDebugLogs)
            Debug.Log("JumpscareSystem: Anomaly destroyed - stopping timer");
        
        isAnomalyPresent = false;
        anomalyTimer = 0f;
        jumpscareTriggeredForAnomaly = false;
        
        // Stop anomaly present sound and play destroy sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.KillAudioSfx(1); // Stop anomaly sound (as SFX)
            AudioManager.Instance.PlaySound(2, 0.7f); // Play anomaly destroy sound
        }
        
        // Stop timer
        if (anomalyTimerCoroutine != null)
        {
            StopCoroutine(anomalyTimerCoroutine);
            anomalyTimerCoroutine = null;
        }
    }
    
    private void OnGeneratorTurnedOff()
    {
        if (enableDebugLogs)
            Debug.Log("JumpscareSystem: Generator turned off - starting timer");
        
        isGeneratorOn = false;
        jumpscareTriggeredForGenerator = false;
        
        // Stop existing timer if running
        if (generatorTimerCoroutine != null)
        {
            StopCoroutine(generatorTimerCoroutine);
        }
        
        // Start new timer
        generatorTimerCoroutine = StartCoroutine(GeneratorTimerCoroutine());
    }
    
    private void OnGeneratorTurnedOn()
    {
        if (enableDebugLogs)
            Debug.Log("JumpscareSystem: Generator turned on - stopping timer");
        
        isGeneratorOn = true;
        generatorOffTimer = 0f;
        jumpscareTriggeredForGenerator = false;
        
        // Stop timer
        if (generatorTimerCoroutine != null)
        {
            StopCoroutine(generatorTimerCoroutine);
            generatorTimerCoroutine = null;
        }
    }
    
    private void OnJumpscareTriggered()
    {
        if (enableDebugLogs)
            Debug.Log("JumpscareSystem: Jumpscare was triggered!");
    }
    
    #endregion

    #region Timer Coroutines
    
    private IEnumerator AnomalyTimerCoroutine()
    {
        anomalyTimer = 0f;
        
        while (anomalyTimer < anomalyJumpscarePresent && !jumpscareTriggeredForAnomaly)
        {
            anomalyTimer += Time.deltaTime;
            yield return null;
        }
        
        // Time's up! Trigger jumpscare
        if (!jumpscareTriggeredForAnomaly)
        {
            jumpscareTriggeredForAnomaly = true;
            TriggerJumpscare("Anomaly has been present too long!", JumpscareType.Anomaly);
        }
    }
    
    private IEnumerator GeneratorTimerCoroutine()
    {
        generatorOffTimer = 0f;
        
        while (generatorOffTimer < generatorJumpscarePresent && !jumpscareTriggeredForGenerator)
        {
            generatorOffTimer += Time.deltaTime;
            yield return null;
        }
        
        // Time's up! Trigger jumpscare
        if (!jumpscareTriggeredForGenerator)
        {
            jumpscareTriggeredForGenerator = true;
            TriggerJumpscare("Generator has been off too long!", JumpscareType.Generator);
        }
    }
    
    #endregion

    // Enum to identify which type of jumpscare was triggered
    public enum JumpscareType
    {
        Anomaly,
        Generator
    }

    private void TriggerJumpscare(string reason, JumpscareType type)
    {
        if (jumpscareAnimation == null)
        {
            Debug.LogError("JumpscareSystem: Cannot trigger jumpscare - no JumpscareAnimation assigned!");
            return;
        }
        
        if (enableDebugLogs)
            Debug.Log($"JumpscareSystem: Triggering {type} jumpscare! Reason: {reason}");
        
        // Trigger the jumpscare animation
        jumpscareAnimation.JumpscareAnim();
        
        // Generate new random jumpscare present time ONLY for the specific type that triggered
        if (type == JumpscareType.Anomaly)
        {
            anomalyJumpscarePresent = Random.Range(8f, 15f);
            if (enableDebugLogs)
                Debug.Log($"JumpscareSystem: Next anomaly jumpscare time set to {anomalyJumpscarePresent:F1} seconds");
        }
        else if (type == JumpscareType.Generator)
        {
            generatorJumpscarePresent = Random.Range(8f, 15f);
            if (enableDebugLogs)
                Debug.Log($"JumpscareSystem: Next generator jumpscare time set to {generatorJumpscarePresent:F1} seconds");
        }
    }

    /// <summary>
    /// Manually trigger a jumpscare (for testing or special events)
    /// </summary>
    public void ManualTriggerJumpscare()
    {
        TriggerJumpscare("Manual trigger", JumpscareType.Anomaly);
    }

    /// <summary>
    /// Manually trigger a specific type of jumpscare
    /// </summary>
    public void ManualTriggerJumpscare(JumpscareType type)
    {
        TriggerJumpscare($"Manual {type} trigger", type);
    }

    /// <summary>
    /// Reset all timers (useful when scene changes or game resets)
    /// </summary>
    public void ResetTimers()
    {
        // Stop all running coroutines
        if (anomalyTimerCoroutine != null)
        {
            StopCoroutine(anomalyTimerCoroutine);
            anomalyTimerCoroutine = null;
        }
        if (generatorTimerCoroutine != null)
        {
            StopCoroutine(generatorTimerCoroutine);
            generatorTimerCoroutine = null;
        }
        
        anomalyTimer = 0f;
        generatorOffTimer = 0f;
        isAnomalyPresent = false;
        isGeneratorOn = true;
        jumpscareTriggeredForAnomaly = false;
        jumpscareTriggeredForGenerator = false;
        anomalyJumpscarePresent = Random.Range(8f, 15f);
        generatorJumpscarePresent = Random.Range(8f, 15f);
        
        if (enableDebugLogs)
        {
            Debug.Log("JumpscareSystem: All timers reset");
            Debug.Log($"JumpscareSystem: New anomaly jumpscare time: {anomalyJumpscarePresent:F1}s");
            Debug.Log($"JumpscareSystem: New generator jumpscare time: {generatorJumpscarePresent:F1}s");
        }
    }

    /// <summary>
    /// Get current status for debugging
    /// </summary>
    public string GetStatus()
    {
        string anomalyStatus = isAnomalyPresent ? $"Present (Timer: {anomalyTimer:F1}s / {anomalyJumpscarePresent:F1}s)" : "Not Present";
        string generatorStatus = isGeneratorOn ? "On" : $"Off (Timer: {generatorOffTimer:F1}s / {generatorJumpscarePresent:F1}s)";
        
        return $"Anomaly: {anomalyStatus}\n" +
               $"Generator: {generatorStatus}\n" +
               $"Anomaly Jumpscare Triggered: {jumpscareTriggeredForAnomaly}\n" +
               $"Generator Jumpscare Triggered: {jumpscareTriggeredForGenerator}";
    }

    // Optional: Display debug info in scene view
    void OnGUI()
    {
        if (!enableDebugLogs) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 250));
        GUILayout.Label("=== JUMPSCARE SYSTEM DEBUG (EVENT-BASED) ===");
        GUILayout.Label(GetStatus());
        
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Manual Anomaly Jumpscare"))
        {
            ManualTriggerJumpscare(JumpscareType.Anomaly);
        }
        if (GUILayout.Button("Manual Generator Jumpscare"))
        {
            ManualTriggerJumpscare(JumpscareType.Generator);
        }
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("Reset All Timers"))
        {
            ResetTimers();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("=== EVENT TEST BUTTONS ===");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Simulate Anomaly Spawn"))
        {
            OnAnomalySpawned();
        }
        if (GUILayout.Button("Simulate Anomaly Destroy"))
        {
            OnAnomalyDestroyed();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Simulate Generator Off"))
        {
            OnGeneratorTurnedOff();
        }
        if (GUILayout.Button("Simulate Generator On"))
        {
            OnGeneratorTurnedOn();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.EndArea();
    }
}
