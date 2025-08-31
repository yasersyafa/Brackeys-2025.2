using UnityEngine;
using DG.Tweening;
using System;

public class JumpscareAnimation : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject jumpscarePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private float panelDisplayDuration = 3f;
    
    [Header("Camera Settings")]
    [SerializeField] private Camera[] targetCameras; // Array of cameras to shake
    [SerializeField] private Camera primaryCamera; // Fallback camera if array is empty
    
    [Header("Shake Settings")]
    [SerializeField] private float shakeIntensity = 2f;
    [SerializeField] private float shakeDuration = 1f;
    [SerializeField] private int shakeVibrato = 10;
    [SerializeField] private float shakeRandomness = 90f;
    [SerializeField] private bool fadeOut = true;
    [SerializeField] private bool useRotationShake = true; // Use rotation shake to keep center focused
    
    // Event for when jumpscare starts
    public static event Action OnJumpscareStart;
    
    private Vector3[] originalCameraPositions;
    private Vector3[] originalCameraRotations;
    private Camera[] activeCameras; // The cameras we're actually using

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize cameras
        SetupCameras();
        
        // Make sure jumpscare panel starts disabled
        if (jumpscarePanel != null)
        {
            jumpscarePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Setup cameras and store their original positions/rotations
    /// </summary>
    private void SetupCameras()
    {
        // Determine which cameras to use
        if (targetCameras != null && targetCameras.Length > 0)
        {
            // Filter out null cameras
            var validCameras = new Camera[targetCameras.Length];
            int validCount = 0;
            for (int i = 0; i < targetCameras.Length; i++)
            {
                if (targetCameras[i] != null)
                {
                    validCameras[validCount] = targetCameras[i];
                    validCount++;
                }
            }
            
            // Resize array to actual valid count
            activeCameras = new Camera[validCount];
            for (int i = 0; i < validCount; i++)
            {
                activeCameras[i] = validCameras[i];
            }
        }
        else
        {
            // Use primary camera or main camera as fallback
            Camera fallbackCamera = primaryCamera != null ? primaryCamera : Camera.main;
            if (fallbackCamera != null)
            {
                activeCameras = new Camera[] { fallbackCamera };
            }
            else
            {
                Debug.LogWarning("JumpscareAnimation: No cameras found!");
                activeCameras = new Camera[0];
                return;
            }
        }
        
        // Store original positions and rotations
        originalCameraPositions = new Vector3[activeCameras.Length];
        originalCameraRotations = new Vector3[activeCameras.Length];
        
        for (int i = 0; i < activeCameras.Length; i++)
        {
            originalCameraPositions[i] = activeCameras[i].transform.position;
            originalCameraRotations[i] = activeCameras[i].transform.eulerAngles;
        }
        
        Debug.Log($"JumpscareAnimation: Setup complete with {activeCameras.Length} camera(s)");
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /// <summary>
    /// Triggers a jumpscare animation with panel and camera shake
    /// </summary>
    public void JumpscareAnim()
    {
        AudioManager.Instance.PlaySound(8, 1f);
        // Fire the event first
        OnJumpscareStart?.Invoke();
        
        // Activate jumpscare panel first
        if (jumpscarePanel != null)
        {
            jumpscarePanel.SetActive(true);
            Debug.Log("Jumpscare panel activated!");
        }
        else
        {
            Debug.LogWarning("JumpscareAnimation: No jumpscare panel assigned!");
        }
        
        // Then start camera shake effect
        StartCameraShake();
        
        // Auto-hide panel after duration and show game over panel
        if (jumpscarePanel != null)
        {
            DOVirtual.DelayedCall(panelDisplayDuration, () => 
            {
                if (jumpscarePanel != null)
                {
                    jumpscarePanel.SetActive(false);
                    Debug.Log("Jumpscare panel deactivated!");
                }
                
                // Activate game over panel after jumpscare ends
                if (gameOverPanel != null)
                {
                    gameOverPanel.SetActive(true);
                    // freeze the game
                    Time.timeScale = 0f;
                    // stop all audio
                    AudioManager.Instance.StopAllAudio();
                    Debug.Log("Game Over panel activated after jumpscare!");
                }
            });
        }
        
        Debug.Log("Jumpscare animation started!");
    }
    
    /// <summary>
    /// Handles the camera shake part of the jumpscare
    /// </summary>
    private void StartCameraShake()
    {
        if (activeCameras == null || activeCameras.Length == 0)
        {
            Debug.LogWarning("JumpscareAnimation: No cameras available for shake animation!");
            return;
        }
        
        // Apply shake to all active cameras
        for (int i = 0; i < activeCameras.Length; i++)
        {
            if (activeCameras[i] == null) continue;
            
            Camera cam = activeCameras[i];
            int cameraIndex = i; // Capture index for closure
            
            // Kill any existing shake tweens to prevent conflicts
            cam.transform.DOKill();
            
            if (useRotationShake)
            {
                // Use rotation shake to keep the center point focused while creating shake effect
                cam.transform.DOShakeRotation(
                    shakeDuration,     // Duration of the shake
                    shakeIntensity,    // Strength/intensity of the shake (in degrees)
                    shakeVibrato,      // How much vibration (frequency)
                    shakeRandomness,   // Randomness of the shake direction (0-180)
                    fadeOut           // Whether to fade out the shake over time
                ).SetEase(Ease.OutQuad)
                 .OnComplete(() => 
                 {
                     // Ensure camera returns to original rotation when shake is complete
                     if (cameraIndex < originalCameraRotations.Length && cam != null)
                     {
                         cam.transform.eulerAngles = originalCameraRotations[cameraIndex];
                         Debug.Log($"Jumpscare rotation shake completed for camera {cameraIndex}!");
                     }
                 });
            }
            else
            {
                // Alternative: Use a smaller position shake that keeps the ghost mostly centered
                cam.transform.DOShakePosition(
                    shakeDuration,     // Duration of the shake
                    shakeIntensity * 0.1f,    // Much smaller position shake to stay centered
                    shakeVibrato,      // How much vibration (frequency)
                    shakeRandomness,   // Randomness of the shake direction (0-180)
                    fadeOut           // Whether to fade out the shake over time
                ).SetEase(Ease.OutQuad)
                 .OnComplete(() => 
                 {
                     // Ensure camera returns to original position when shake is complete
                     if (cameraIndex < originalCameraPositions.Length && cam != null)
                     {
                         cam.transform.position = originalCameraPositions[cameraIndex];
                         Debug.Log($"Jumpscare position shake completed for camera {cameraIndex}!");
                     }
                 });
            }
        }
    }
    
    /// <summary>
    /// Triggers jumpscare with custom shake parameters
    /// </summary>
    /// <param name="customIntensity">Custom shake intensity</param>
    /// <param name="customDuration">Custom shake duration</param>
    public void JumpscareAnim(float customIntensity, float customDuration)
    {
        // Activate jumpscare panel first
        if (jumpscarePanel != null)
        {
            jumpscarePanel.SetActive(true);
            Debug.Log("Jumpscare panel activated!");
        }
        
        // Start custom camera shake
        StartCustomCameraShake(customIntensity, customDuration);
        
        // Auto-hide panel after duration (use the longer of the two durations) and show game over panel
        float totalDuration = Mathf.Max(panelDisplayDuration, customDuration);
        if (jumpscarePanel != null)
        {
            DOVirtual.DelayedCall(totalDuration, () => 
            {
                if (jumpscarePanel != null)
                {
                    jumpscarePanel.SetActive(false);
                    Debug.Log("Jumpscare panel deactivated!");
                }
                
                // Activate game over panel after jumpscare ends
                if (gameOverPanel != null)
                {
                    gameOverPanel.SetActive(true);
                    Debug.Log("Game Over panel activated after custom jumpscare!");
                }
            });
        }
    }
    
    /// <summary>
    /// Handles custom camera shake with specified parameters
    /// </summary>
    private void StartCustomCameraShake(float customIntensity, float customDuration)
    {
        if (activeCameras == null || activeCameras.Length == 0)
        {
            Debug.LogWarning("JumpscareAnimation: No cameras available for shake animation!");
            return;
        }
        
        // Apply custom shake to all active cameras
        for (int i = 0; i < activeCameras.Length; i++)
        {
            if (activeCameras[i] == null) continue;
            
            Camera cam = activeCameras[i];
            int cameraIndex = i; // Capture index for closure
            
            // Kill any existing shake tweens
            cam.transform.DOKill();
            
            if (useRotationShake)
            {
                // Create rotation shake with custom parameters
                cam.transform.DOShakeRotation(
                    customDuration,
                    customIntensity,
                    shakeVibrato,
                    shakeRandomness,
                    fadeOut
                ).SetEase(Ease.OutQuad)
                 .OnComplete(() => 
                 {
                     if (cameraIndex < originalCameraRotations.Length && cam != null)
                     {
                         cam.transform.eulerAngles = originalCameraRotations[cameraIndex];
                         Debug.Log($"Custom jumpscare rotation shake completed for camera {cameraIndex}!");
                     }
                 });
            }
            else
            {
                // Create position shake with custom parameters (reduced intensity)
                cam.transform.DOShakePosition(
                    customDuration,
                    customIntensity * 0.1f,
                    shakeVibrato,
                    shakeRandomness,
                    fadeOut
                ).SetEase(Ease.OutQuad)
                 .OnComplete(() => 
                 {
                     if (cameraIndex < originalCameraPositions.Length && cam != null)
                     {
                         cam.transform.position = originalCameraPositions[cameraIndex];
                         Debug.Log($"Custom jumpscare position shake completed for camera {cameraIndex}!");
                     }
                 });
            }
        }
    }
    
    /// <summary>
    /// Manually show the jumpscare panel without camera shake
    /// </summary>
    public void ShowJumpscarePanel()
    {
        if (jumpscarePanel != null)
        {
            jumpscarePanel.SetActive(true);
            Debug.Log("Jumpscare panel manually activated!");
        }
    }
    
    /// <summary>
    /// Manually hide the jumpscare panel
    /// </summary>
    public void HideJumpscarePanel()
    {
        if (jumpscarePanel != null)
        {
            jumpscarePanel.SetActive(false);
            Debug.Log("Jumpscare panel manually deactivated!");
        }
    }
    
    /// <summary>
    /// Stops any ongoing shake animation and resets camera positions
    /// </summary>
    public void StopJumpscareAnimation()
    {
        gameOverPanel.SetActive(true);
        if (activeCameras != null)
        {
            for (int i = 0; i < activeCameras.Length; i++)
            {
                if (activeCameras[i] == null) continue;

                Camera cam = activeCameras[i];
                cam.transform.DOKill();

                // Reset both position and rotation to original values
                if (i < originalCameraPositions.Length && originalCameraPositions[i] != Vector3.zero)
                {
                    cam.transform.position = originalCameraPositions[i];
                }
                if (i < originalCameraRotations.Length && originalCameraRotations[i] != Vector3.zero)
                {
                    cam.transform.eulerAngles = originalCameraRotations[i];
                }
            }

            Debug.Log($"Jumpscare animation stopped for {activeCameras.Length} camera(s)!");
        }
    }
}
