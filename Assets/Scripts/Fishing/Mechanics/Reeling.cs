using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.InputSystem;
using DG.Tweening;

public class Reeling : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject reelingCanvas;
    [SerializeField] private RectTransform reelingWheel; // Parent container that will scale down
    [SerializeField] private RectTransform spinningNeedle; // Needle that spins like a clock hand
    [SerializeField] private Slider progressFill; // Progress slider for rotation completion
    [SerializeField] private Slider reelStressFill; // Stress slider for reel stress
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Slider progressBar;
    
    [Header("Wheel Animation Settings")]
    [SerializeField] private float wheelScaleSpeed = 1f; // How fast the wheel scales down
    [SerializeField] private AnimationCurve wheelScaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f); // Scale curve for smooth animation
    [SerializeField] private float progressFillRate = 0.8f; // How smoothly the progress fill updates
    [SerializeField] private float progressDecayRate = 0.5f; // How fast progress decays when not spinning
    [SerializeField] private bool enableProgressDecay = true; // Whether to enable progress decay
    
    [Header("Wheel Settings")]
    //? [SerializeField] private float wheelRadius = 100f;
    //? [SerializeField] private float rotationSpeed = 180f; // degrees per second when spinning
    [SerializeField] private float minSpinSpeed = 90f; // minimum speed to register as spinning
    [SerializeField] private float spinDecay = 2f; // how quickly spin decays when not moving
    
    [Header("Progress Settings")]
    [SerializeField] private int targetRotations = 5;
    [SerializeField] private float rotationTolerance = 30f; // degrees of tolerance for direction changes
    
    [Header("Reel Stress Settings")]
    [SerializeField] private float maxReelStress = 100f; // Maximum stress the reel can handle
    [SerializeField] private float baseStressIncreaseRate = 15f; // Base stress increase rate (will be modified by fish weight)
    [SerializeField] private float baseStressDecreaseRate = 10f; // Base stress decrease rate (will become dynamic)
    [SerializeField] private float stressDangerThreshold = 80f; // When to warn player
    [SerializeField] private Color stressSafeColor = Color.green;
    [SerializeField] private Color stressWarningColor = Color.yellow;
    [SerializeField] private Color stressDangerColor = Color.red;
    
    [Header("Dynamic Stress Settings")]
    [SerializeField] private float stressDecreaseAcceleration = 2f; // How fast the decrease rate accelerates over time
    [SerializeField] private float maxStressDecreaseMultiplier = 5f; // Maximum multiplier for stress decrease rate
    
    [Header("Fish Struggle Settings")]
    [SerializeField] private float struggleChance = 0.15f; // 15% chance per second when reeling
    [SerializeField] private float struggeDuration = 2f; // How long the struggle lasts
    [SerializeField] private float struggleProgressDecayMultiplier = 2f; // Progress decays 2x faster during struggle
    
    [Header("Wheel Shake Settings")]
    [SerializeField] private float shakeIntensity = 10f; // How strong the shake is
    [SerializeField] private float shakeSpeed = 20f; // How fast the shake oscillates
    
    // Private variables
    private Camera playerCamera;
    private Vector2 lastMousePosition;
    private Vector2 wheelCenter;
    private float currentRotation = 0f;
    private float totalRotations = 0f;
    private float currentSpinSpeed = 0f;
    private bool isSpinning = false;
    private bool isReelingActive = false;
    
    // Reel stress variables
    private float currentReelStress = 0f;
    private bool reelBroken = false;
    
    // Needle tracking variables
    private float needleAngle = 0f; // Current angle of needle pointing toward mouse
    
    // Tracking rotation direction
    private bool clockwise = true;
    
    // DOTween sequences
    private Sequence wheelSpinSequence;
    private Sequence uiUpdateSequence;
    private Sequence wheelScaleSequence;
    private Sequence wheelShakeSequence; // For fish struggle shake animation
    
    // Smooth progress tracking
    private float smoothProgressFillValue = 1f; // For smooth progress fill interpolation
    private Vector2 originalWheelPosition; // Store original wheel position for shake
    
    // Fish stats tracking for dynamic mechanics
    private float currentFishWeight = 1f; // Current fish weight
    private float currentFishStrength = 1f; // Current fish strength
    private float stressNotSpinningTime = 0f; // Time since player stopped spinning (for dynamic decrease)
    
    // Dynamic calculated rates
    private float dynamicStressIncreaseRate = 15f; // Calculated from fish weight
    private float dynamicStressDecreaseRate = 10f; // Calculated dynamically
    private float dynamicProgressFillRate = 0.8f; // Calculated from fish strength
    
    // Fish struggle mechanics
    private bool isFishStruggling = false; // Is fish currently struggling
    private float struggleTimer = 0f; // Time remaining in current struggle
    private float lastStruggleCheck = 0f; // Time since last struggle chance check
    
    // Events
    public static event Action OnRotationCompleted;
    public static event Action<float> OnRotationProgress; // 0-1 progress
    public static event Action OnReelingStarted;
    public static event Action OnReelingCompleted;
    public static event Action OnFishStruggleStarted; // When fish starts struggling
    public static event Action OnFishStruggleEnded; // When fish stops struggling
    
    void Awake()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindFirstObjectByType<Camera>();
    }
    
    void OnEnable()
    {
        Bait.OnCaughtFish += StartReeling;
        FishManager.OnFishCaught += StopReeling;
        FishManager.OnFishLost += StopReeling;
    }
    
    void OnDestroy()
    {
        // Clean up DOTween sequences
        wheelSpinSequence?.Kill();
        uiUpdateSequence?.Kill();
        wheelScaleSequence?.Kill();
        wheelShakeSequence?.Kill();
        DOTween.Kill(this);
    }
    
    void OnDisable()
    {
        Bait.OnCaughtFish -= StartReeling;
        FishManager.OnFishCaught -= StopReeling;
        FishManager.OnFishLost -= StopReeling;
        
        // Clean up DOTween sequences when disabled
        wheelSpinSequence?.Kill();
        uiUpdateSequence?.Kill();
        wheelScaleSequence?.Kill();
        wheelShakeSequence?.Kill();
        DOTween.Kill(this);
    }
    
    void Update()
    {
        if (!isReelingActive) return;
        
        UpdateFishStruggle(); // Check for fish struggling
        
        // Always handle mouse input, but modify behavior during struggle
        HandleMouseInput();
        
        UpdateWheelRotation();
        UpdateReelStress();
        UpdateProgress();
        UpdateWheelScale(); // Update wheel scaling based on progress
        UpdateUI();
    }
    
    private void StartReeling()
    {
        isReelingActive = true;
        totalRotations = 0f;
        currentRotation = 0f;
        currentSpinSpeed = 0f;
        isSpinning = false; // Explicitly set to false
        currentReelStress = 0f; // Reset reel stress
        reelBroken = false; // Reset reel state
        needleAngle = 0f; // Reset needle angle
        
        // Initialize mouse position to current position to prevent false spinning detection
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            lastMousePosition = mouse.position.ReadValue();
            Debug.Log($"Reeling: Initialized mouse position at {lastMousePosition}");
        }
        
        // Get target rotations from current fish if possible
        var fishManager = FindFirstObjectByType<FishManager>();
        if (fishManager != null && fishManager.GetCurrentFish() != null)
        {
            var currentFish = fishManager.GetCurrentFish();
            targetRotations = currentFish.rotationsToReel;
            
            // Get fish stats for dynamic mechanics
            currentFishWeight = currentFish.actualWeight;
            currentFishStrength = currentFish.actualStrength;
            
            // Calculate dynamic rates based on fish stats
            // Fish weight affects stress increase rate: stressIncreaseRate = w × baseStressIncreaseRate
            dynamicStressIncreaseRate = currentFishWeight * baseStressIncreaseRate;
            
            // Fish strength affects progress fill rate: progressFillRate = s × baseProgressFillRate
            dynamicProgressFillRate = currentFishStrength * progressFillRate;
            
            // Initialize dynamic stress decrease rate
            dynamicStressDecreaseRate = baseStressDecreaseRate;
            stressNotSpinningTime = 0f; // Reset timing for dynamic decrease
            
            Debug.Log($"Fish Stats - Weight: {currentFishWeight:F2}, Strength: {currentFishStrength:F2}");
            Debug.Log($"Dynamic Rates - Stress Increase: {dynamicStressIncreaseRate:F2}, Progress Fill: {dynamicProgressFillRate:F2}");
        }
        
        // Show reeling UI
        if (reelingCanvas != null)
        {
            reelingCanvas.SetActive(true);
            
            // Calculate wheel center using UI coordinates instead of world-to-screen conversion
            if (reelingWheel != null)
            {
                // Get the canvas for proper coordinate conversion
                Canvas canvas = reelingCanvas.GetComponent<Canvas>();
                if (canvas != null)
                {
                    // Use direct screen position approach: get world corners and find center
                    Vector3[] corners = new Vector3[4];
                    reelingWheel.GetWorldCorners(corners);
                    Vector3 centerWorld = (corners[0] + corners[2]) * 0.5f; // Average of opposite corners
                    wheelCenter = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera ?? Camera.main, centerWorld);
                    
                    Debug.Log($"Reeling: Wheel center calculated at {wheelCenter} using UI corners");
                }
                else
                {
                    // Fallback to screen center if canvas not found
                    wheelCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                    Debug.Log($"Reeling: Using screen center fallback {wheelCenter}");
                }
            }
            else
            {
                // Fallback to screen center if wheel transform is not set
                wheelCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            }
        }
        
        // Initialize UI with DOTween
        InitializeUIAnimations();
        UpdateUI();
        
        OnReelingStarted?.Invoke();
    }
    
    private void StopReeling(Fish fish)
    {
        isReelingActive = false;
        
        // Kill any running DOTween sequences
        wheelSpinSequence?.Kill();
        uiUpdateSequence?.Kill();
        wheelScaleSequence?.Kill();
        wheelShakeSequence?.Kill();
        DOTween.Kill(this);
        
        // Clear all fish data
        ClearAllFishData("reeling stopped");
        
        // Hide reeling UI
        if (reelingCanvas != null)
        {
            reelingCanvas.SetActive(false);
        }
        
        OnReelingCompleted?.Invoke();
    }
    
    private void HandleMouseInput()
    {
        // Get mouse and pointer from new Input System
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mousePosition = mouse.position.ReadValue();
        
        // Always track mouse movement when reeling is active (no click required)
        if (isReelingActive)
        {
            // Ensure we have valid wheel center - recalculate if needed using UI approach
            if (wheelCenter == Vector2.zero && reelingWheel != null)
            {
                Canvas canvas = reelingCanvas?.GetComponent<Canvas>();
                if (canvas != null)
                {
                    Vector3[] corners = new Vector3[4];
                    reelingWheel.GetWorldCorners(corners);
                    Vector3 centerWorld = (corners[0] + corners[2]) * 0.5f;
                    wheelCenter = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera ?? Camera.main, centerWorld);
                }
                else
                {
                    wheelCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                }
            }
            
            // Skip calculation if wheel center is still invalid
            if (wheelCenter == Vector2.zero)
            {
                wheelCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            }
            
            // Check if this is the first frame or if mouse hasn't moved enough
            Vector2 mouseDelta = mousePosition - lastMousePosition;
            float mouseDeltaMagnitude = mouseDelta.magnitude;
            
            // Only process if mouse has moved enough (prevent false spinning on first frame)
            if (mouseDeltaMagnitude > 1f) // Minimum 1 pixel movement
            {
                // Calculate angle from wheel center
                Vector2 currentDirection = (mousePosition - wheelCenter).normalized;
                Vector2 lastDirection = (lastMousePosition - wheelCenter).normalized;
                
                // Calculate angular difference
                float angle = Vector2.SignedAngle(lastDirection, currentDirection);
                
                // Check if we're spinning in the right direction (clockwise = negative angle)
                if (angle < -rotationTolerance / 60f) // Convert tolerance to per-frame
                {
                    clockwise = true;
                    currentSpinSpeed = Mathf.Abs(angle) * 60f; // Convert to degrees per second
                    
                    if (currentSpinSpeed >= minSpinSpeed)
                    {
                        isSpinning = true;
                        
                        // Only add to rotation progress if fish isn't struggling
                        if (!isFishStruggling)
                        {
                            currentRotation += Mathf.Abs(angle);
                            
                            // Check for complete rotation
                            if (currentRotation >= 360f)
                            {
                                CompleteRotation();
                                currentRotation -= 360f;
                            }
                        }
                    }
                    else
                    {
                        isSpinning = false;
                    }
                }
                else if (angle > rotationTolerance / 60f)
                {
                    // Wrong direction (counter-clockwise)
                    clockwise = false;
                    currentSpinSpeed = 0f;
                    isSpinning = false;
                }
                else
                {
                    // Not enough movement to register as spinning
                    currentSpinSpeed = Mathf.Lerp(currentSpinSpeed, 0f, spinDecay * Time.deltaTime);
                    if (currentSpinSpeed < minSpinSpeed * 0.1f)
                    {
                        isSpinning = false;
                        currentSpinSpeed = 0f;
                    }
                }
            }
            else
            {
                // Mouse hasn't moved enough - decay spinning
                currentSpinSpeed = Mathf.Lerp(currentSpinSpeed, 0f, spinDecay * Time.deltaTime);
                if (currentSpinSpeed < minSpinSpeed * 0.1f)
                {
                    isSpinning = false;
                    currentSpinSpeed = 0f;
                }
            }
        }
        
        lastMousePosition = mousePosition;
    }
    
    private void UpdateWheelRotation()
    {
        // Remove rotation animation for spin wheel - keep it static
        // Only update needle to point toward mouse cursor (like a clock hand)
        UpdateNeedleTracking();
    }
    
    private void UpdateNeedleTracking()
    {
        if (spinningNeedle == null || wheelCenter == Vector2.zero) return;
        
        // Get current mouse position
        Mouse mouse = Mouse.current;
        if (mouse == null) return;
        
        Vector2 mousePosition = mouse.position.ReadValue();
        
        // Calculate angle from wheel center to mouse position
        Vector2 direction = mousePosition - wheelCenter;
        needleAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Adjust angle so needle points correctly (Unity UI rotation starts from right going counter-clockwise)
        needleAngle -= 90f; // Offset so needle points up when mouse is above center
        
        // Smoothly rotate needle to point toward mouse using DOTween
        spinningNeedle.DORotate(new Vector3(0, 0, needleAngle), 0.1f)
            .SetEase(Ease.OutQuart);
    }
    
    private void InitializeUIAnimations()
    {
        // Kill any existing sequences
        wheelSpinSequence?.Kill();
        uiUpdateSequence?.Kill();
        wheelScaleSequence?.Kill();
        wheelShakeSequence?.Kill();
        
        // Initialize needle position
        if (spinningNeedle != null)
        {
            needleAngle = 0f;
            spinningNeedle.rotation = Quaternion.Euler(0, 0, needleAngle);
        }
        
        // Initialize reel wheel scale and position
        if (reelingWheel != null)
        {
            reelingWheel.localScale = Vector3.one; // Start at full scale
            originalWheelPosition = reelingWheel.anchoredPosition; // Store original position
            reelingWheel.anchoredPosition = originalWheelPosition; // Reset position for shake
        }
        
        // Initialize progress fills
        if (progressFill != null)
        {
            progressFill.value = 1f; // Start at full (inverted)
            smoothProgressFillValue = 1f; // Initialize smooth value
        }
        
        if (reelStressFill != null)
        {
            reelStressFill.value = 0f;
        }
    }
    
    private void UpdateWheelScale()
    {
        if (reelingWheel == null) return;
        
        float currentProgress = totalRotations / targetRotations;
        
        // Use animation curve for smooth scaling from 1 to 0
        float targetScale = wheelScaleCurve.Evaluate(currentProgress);
        
        // Apply minimum scale constraint of 0.2
        targetScale = Mathf.Max(targetScale, 0.2f);
        
        // Smooth scale animation
        reelingWheel.DOScale(targetScale, wheelScaleSpeed)
            .SetEase(Ease.OutCubic);
    }
    
    private void CompleteRotation()
    {
        totalRotations += 1f;
        OnRotationCompleted?.Invoke();
        
        // Check if we've completed enough rotations
        if (totalRotations >= targetRotations)
        {
            // Catch the fish when target rotations reached
            var fishManager = FindFirstObjectByType<FishManager>();
            if (fishManager != null && fishManager.GetCurrentFish() != null)
            {
                fishManager.GetCurrentFish().CatchFish();
            }
            
            // Clear all fish data
            ClearAllFishData("target rotations reached");
            
            // Stop reeling system
            isReelingActive = false;
            
            // Kill any running DOTween sequences
            wheelSpinSequence?.Kill();
            uiUpdateSequence?.Kill();
            wheelScaleSequence?.Kill();
            wheelShakeSequence?.Kill();
            DOTween.Kill(this);
            
            // Hide reeling UI
            if (reelingCanvas != null)
            {
                reelingCanvas.SetActive(false);
            }
            
            // Reset bait after catching fish
            var fishingArea = FindFirstObjectByType<FishingArea>();
            if (fishingArea != null)
            {
                fishingArea.ResetBait();
            }
            
            // Notify that reeling is completed
            OnReelingCompleted?.Invoke();
            
            return;
        }
        
        UpdateProgressUI();
    }
    
    private void UpdateProgress()
    {
        // Calculate decay rate (normal or struggle enhanced)
        float currentDecayRate = progressDecayRate;
        if (isFishStruggling)
        {
            currentDecayRate *= struggleProgressDecayMultiplier; // 2x faster during struggle
        }
        
        // Apply progress decay when not spinning or when fish is struggling
        if (enableProgressDecay && (!isSpinning || isFishStruggling) && totalRotations > 0f)
        {
            float decayAmount = currentDecayRate * Time.deltaTime;
            totalRotations = Mathf.Max(0f, totalRotations - decayAmount);
            
            // Also decay current rotation progress
            currentRotation = Mathf.Max(0f, currentRotation - (decayAmount * 360f));
        }
        
        float progress = totalRotations / targetRotations;
        OnRotationProgress?.Invoke(Mathf.Clamp01(progress));
    }
    
    private void UpdateReelStress()
    {
        if (reelBroken) return;
        
        if (isSpinning)
        {
            // Debug: Log stress increase when it happens
            if (Time.time % 1f < Time.deltaTime) // Log every second
            {
                Debug.Log($"Reeling: Stress increasing - isSpinning: {isSpinning}, isStruggling: {isFishStruggling}, currentSpinSpeed: {currentSpinSpeed:F2}, currentReelStress: {currentReelStress:F2}");
            }
            
            // Increase stress when spinning using dynamic rate (fish weight affects this)
            float stressIncrease = dynamicStressIncreaseRate * Time.deltaTime;
            // Faster spinning = more stress
            float speedMultiplier = Mathf.Clamp01(currentSpinSpeed / 300f);
            stressIncrease *= (1f + speedMultiplier);
            
            // During fish struggle, stress increases faster as player fights against fish
            if (isFishStruggling)
            {
                stressIncrease *= 1.5f; // 50% more stress during struggle
            }
            
            currentReelStress += stressIncrease;
            
            // Reset stress decrease timing when spinning
            stressNotSpinningTime = 0f;
        }
        else
        {
            
            // Increase time not spinning for dynamic decrease rate
            stressNotSpinningTime += Time.deltaTime;
            
            // Calculate dynamic stress decrease rate (starts slow, gets faster over time)
            float decreaseMultiplier = 1f + (stressNotSpinningTime * stressDecreaseAcceleration);
            decreaseMultiplier = Mathf.Min(decreaseMultiplier, maxStressDecreaseMultiplier);
            dynamicStressDecreaseRate = baseStressDecreaseRate * decreaseMultiplier;
            
            // During fish struggle, stress decreases slower (fish is fighting back)
            if (isFishStruggling)
            {
                dynamicStressDecreaseRate *= 0.5f; // 50% slower decrease during struggle
            }
            
            // Decrease stress when not spinning using dynamic rate
            currentReelStress -= dynamicStressDecreaseRate * Time.deltaTime;
        }
        
        // Clamp stress
        currentReelStress = Mathf.Clamp(currentReelStress, 0f, maxReelStress);
        
        // Check if reel breaks
        if (currentReelStress >= maxReelStress && !reelBroken)
        {
            BreakReel();
        }
    }
    
    private void UpdateFishStruggle()
    {
        if (isFishStruggling)
        {
            // Update struggle timer
            struggleTimer -= Time.deltaTime;
            
            // During struggle, apply resistance to spinning (fish fights back)
            if (isSpinning && currentSpinSpeed > 0f)
            {
                // Fish resistance reduces spin speed during struggle
                float resistanceReduction = spinDecay * 3f * Time.deltaTime; // 3x faster decay during struggle
                currentSpinSpeed = Mathf.Max(0f, currentSpinSpeed - resistanceReduction);
                
                // If spin speed drops too low, stop spinning
                if (currentSpinSpeed < minSpinSpeed * 0.3f)
                {
                    isSpinning = false;
                    currentSpinSpeed = 0f;
                }
            }
            
            if (struggleTimer <= 0f)
            {
                // End struggle
                EndFishStruggle();
            }
        }
        else
        {
            // Check for new struggle (only when not struggling and reeling is active)
            lastStruggleCheck += Time.deltaTime;
            
            if (lastStruggleCheck >= 1f) // Check every second
            {
                lastStruggleCheck = 0f;
                
                // Random chance for fish to struggle
                if (UnityEngine.Random.Range(0f, 1f) < struggleChance)
                {
                    StartFishStruggle();
                }
            }
        }
    }
    
    private void StartFishStruggle()
    {
        if (isFishStruggling) return; // Already struggling
        
        isFishStruggling = true;
        struggleTimer = struggeDuration;
        
        // Start wheel shake animation
        StartWheelShake();
        
        Debug.Log("Fish started struggling!");
        OnFishStruggleStarted?.Invoke();
    }
    
    private void EndFishStruggle()
    {
        if (!isFishStruggling) return; // Not struggling
        
        isFishStruggling = false;
        struggleTimer = 0f;
        
        // Stop wheel shake animation
        StopWheelShake();
        
        Debug.Log("Fish stopped struggling!");
        OnFishStruggleEnded?.Invoke();
    }
    
    private void StartWheelShake()
    {
        if (reelingWheel == null) return;
        
        // Kill any existing shake
        wheelShakeSequence?.Kill();
        
        // Create manual shake animation using DOTween
        wheelShakeSequence = DOTween.Sequence();
        
        // Create a looping shake effect
        for (int i = 0; i < Mathf.RoundToInt(struggeDuration * shakeSpeed); i++)
        {
            Vector2 shakeOffset = new Vector2(
                UnityEngine.Random.Range(-shakeIntensity, shakeIntensity),
                UnityEngine.Random.Range(-shakeIntensity, shakeIntensity)
            );
            
            wheelShakeSequence.Append(
                DOTween.To(() => reelingWheel.anchoredPosition, 
                          x => reelingWheel.anchoredPosition = x, 
                          originalWheelPosition + shakeOffset, 
                          1f / shakeSpeed)
            );
        }
        
        wheelShakeSequence.SetLoops(-1, LoopType.Restart);
    }
    
    private void StopWheelShake()
    {
        if (reelingWheel == null) return;
        
        // Stop shake animation
        wheelShakeSequence?.Kill();
        
        // Smoothly return to original position
        DOTween.To(() => reelingWheel.anchoredPosition, 
                  x => reelingWheel.anchoredPosition = x, 
                  originalWheelPosition, 
                  0.3f)
            .SetEase(Ease.OutBounce);
    }
    
    private void BreakReel()
    {
        reelBroken = true;
        isReelingActive = false;
        
        Debug.Log("Reel broke! Fish escaped and bait reset!");
        
        // Trigger fish escape and clear fish data
        var fishManager = FindFirstObjectByType<FishManager>();
        if (fishManager != null)
        {
            if (fishManager.GetCurrentFish() != null)
            {
                fishManager.ForceFishEscape();
            }
        }
        
        // Clear all fish data
        ClearAllFishData("reel broke");

        var FishingArea = FindFirstObjectByType<FishingArea>();
        if (FishingArea != null)
        {
            FishingArea.ResetBait();
        }

        // Hide UI
        if (reelingCanvas != null)
        {
            reelingCanvas.SetActive(false);
        }
        
        OnReelingCompleted?.Invoke();
    }
    
    private void UpdateUI()
    {
        UpdateProgressUI();
        UpdateReelStressUI();
    }
    
    private void UpdateProgressUI()
    {   
        float targetProgress = totalRotations / targetRotations;
        
        if (progressBar != null)
        {
            DOTween.To(() => progressBar.value, x => progressBar.value = x, targetProgress, 0.5f)
                .SetEase(Ease.OutCubic);
        }
        
        // Update progress fill with smooth animation - inverted (1 to 0) with smoother interpolation
        if (progressFill != null)
        {
            float targetInvertedProgress = 1f - targetProgress; // Invert: start at 1, go to 0
            
            // Smooth interpolation for progress fill value using dynamic rate
            DOTween.To(() => smoothProgressFillValue, x => {
                smoothProgressFillValue = x;
                progressFill.value = x;
            }, targetInvertedProgress, dynamicProgressFillRate)
                .SetEase(Ease.OutCubic);
        }
        
        // Update progress text
        if (progressText != null)
        {
            int currentRots = Mathf.FloorToInt(totalRotations);
            string newText = $"Rotations: {currentRots} / {targetRotations}";
            progressText.text = newText;
        }
    }
    
    private void UpdateReelStressUI()
    {
        if (reelStressFill != null)
        {
            float stressPercentage = currentReelStress / maxReelStress;
            
            // Smooth animation for stress fill
            DOTween.To(() => reelStressFill.value, x => reelStressFill.value = x, stressPercentage, 0.4f)
                .SetEase(Ease.OutCubic);
            
            // Update color based on stress level by accessing the fill image
            Image fillImage = reelStressFill.fillRect?.GetComponent<Image>();
            if (fillImage != null)
            {
                Color targetColor;
                if (stressPercentage < 0.5f)
                {
                    targetColor = stressSafeColor;
                }
                else if (stressPercentage < (stressDangerThreshold / maxReelStress))
                {
                    targetColor = stressWarningColor;
                }
                else
                {
                    targetColor = stressDangerColor;
                }
                
                // Smooth color transition - longer duration for gradual change
                DOTween.To(() => fillImage.color, x => fillImage.color = x, targetColor, 0.8f)
                    .SetEase(Ease.OutCubic);
            }
        }
    }
    
    // Public methods for external access
    public bool IsReelingActive()
    {
        return isReelingActive;
    }
    
    public float GetProgress()
    {
        return totalRotations / targetRotations;
    }
    
    public int GetTargetRotations()
    {
        return targetRotations;
    }
    
    public float GetCurrentRotations()
    {
        return totalRotations;
    }
    
    public bool IsSpinning()
    {
        return isSpinning;
    }
    
    public float GetSpinSpeed()
    {
        return currentSpinSpeed;
    }
    
    public float GetReelStress()
    {
        return currentReelStress;
    }
    
    public float GetReelStressPercentage()
    {
        return currentReelStress / maxReelStress;
    }
    
    public bool IsReelBroken()
    {
        return reelBroken;
    }
    
    public float GetNeedleRotation()
    {
        return needleAngle;
    }
    
    public bool IsFishStruggling()
    {
        return isFishStruggling;
    }
    
    public float GetStruggleTimeRemaining()
    {
        return struggleTimer;
    }
    
    // Helper method to clear all fish data consistently
    private void ClearAllFishData(string context = "")
    {
        // Clear fish data from FishManager
        var fishManager = FindFirstObjectByType<FishManager>();
        if (fishManager != null)
        {
            fishManager.ClearCurrentFish();
        }
        
        // Clear fish data from FishSpawner
        var fishSpawner = FindFirstObjectByType<FishSpawner>();
        if (fishSpawner != null)
        {
            fishSpawner.StopSpawning(); // This will clear fish data
        }
        
        if (!string.IsNullOrEmpty(context))
        {
            Debug.Log($"Reeling: Cleared all fish data - {context}");
        }
    }
    
    // Debug methods
    [ContextMenu("Test Start Reeling")]
    public void DebugStartReeling()
    {
        if (Application.isPlaying)
        {
            StartReeling();
        }
    }
    
    [ContextMenu("Complete One Rotation")]
    public void DebugCompleteRotation()
    {
        if (Application.isPlaying && isReelingActive)
        {
            CompleteRotation();
        }
    }
}
