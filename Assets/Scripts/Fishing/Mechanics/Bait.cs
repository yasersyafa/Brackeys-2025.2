using UnityEngine;
using System;

public class Bait : MonoBehaviour
{
    [Header("Water Physics")]
    [SerializeField] private float waterDrag = 5f;

    [Header("Reeling")]
    [SerializeField] private Transform reelingTarget; // Reference to the fishing rod or connection point

    [Header("Reeling Force Settings")]
    [SerializeField] private float baseReelingForce = 1f; // Base horizontal force when spinning
    [SerializeField] private float maxReelingForce = 8f; // Maximum additional force from fast spinning
    [SerializeField] private float baseUpwardForce = 0.5f; // Base upward force
    [SerializeField] private float maxUpwardForce = 5f; // Maximum additional upward force
    [SerializeField] private float rotationBoostForce = 5f; // Extra force when completing a rotation
    [SerializeField] private float rotationBoostUpForce = 3f; // Extra upward force when completing a rotation

    [Header("Fish Struggle Settings")]
    [SerializeField] private float struggleForce = 3f; // Continuous force during struggle
    [SerializeField] private float struggleUpwardForce = 1f; // Upward force during struggle

    private Rigidbody rb;
    private bool isInWater = false;
    private bool hasEnteredWater = false;
    private float originalDrag;
    private float originalAngularDrag;
    private bool isBeingReeled = false;
    private bool hasStartedReeling = false; // Prevent multiple reeling starts
    private bool isFishStruggling = false; // Track if fish is currently struggling
    private Reeling reelingScript;

    // Events
    public static event Action OnCaughtFish;

    void OnEnable()
    {
        Reeling.OnRotationCompleted += OnRotationCompleted;
        Reeling.OnReelingStarted += OnReelingStarted;
        Reeling.OnReelingCompleted += OnReelingCompleted;
        Reeling.OnFishStruggleStarted += OnFishStruggleStarted;
        Reeling.OnFishStruggleEnded += OnFishStruggleEnded;
    }

    void OnDisable()
    {
        Reeling.OnRotationCompleted -= OnRotationCompleted;
        Reeling.OnReelingStarted -= OnReelingStarted;
        Reeling.OnReelingCompleted -= OnReelingCompleted;
        Reeling.OnFishStruggleStarted -= OnFishStruggleStarted;
        Reeling.OnFishStruggleEnded -= OnFishStruggleEnded;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Store original drag values
        originalDrag = rb.linearDamping;
        originalAngularDrag = rb.angularDamping;
    }

    void Start()
    {
        // Initially disable the bait
        gameObject.SetActive(false);

        // Find the Reeling script in the scene
        reelingScript = FindFirstObjectByType<Reeling>();
        if (reelingScript == null)
        {
            Debug.LogWarning("Bait: No Reeling script found in scene!");
        }
    }

    void FixedUpdate()
    {
        if (isInWater)
        {
            ApplyWaterPhysics();
        }

        // Handle continuous reeling based on spinning
        if (isBeingReeled && reelingScript != null)
        {
            if (isFishStruggling)
            {
                // Apply continuous struggle force to push bait away (into -Z direction)
                HandleContinuousStruggle();
            }
            else
            {
                // Normal reeling behavior
                HandleContinuousReeling();
            }
            
            // Debug bait movement when reeling
            if (Time.fixedTime % 1f < Time.fixedDeltaTime) // Log every second
            {
                Debug.Log($"Bait: Position {transform.position}, IsReeling: {isBeingReeled}, IsStruggling: {isFishStruggling}, HasChildren: {transform.childCount}");
            }
        }
    }

    public void Cast(Vector3 direction, float force)
    {
        // Reset physics state
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isInWater = false;
        hasEnteredWater = false;
        hasStartedReeling = false; // Reset reeling flag for new cast
        isFishStruggling = false; // Reset struggle state for new cast
        
        // Reset drag to original values
        rb.linearDamping = originalDrag;
        rb.angularDamping = originalAngularDrag;
        
        // Apply casting force
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);
    }
    
    #region Reeling Methods
    public void StartReeling()
    {
        // Prevent multiple reeling starts
        if (hasStartedReeling) return;
        
        hasStartedReeling = true;
        OnCaughtFish?.Invoke();
    }

    // Event handlers for the new reeling system
    private void OnReelingStarted()
    {
        isBeingReeled = true;
        Debug.Log("Bait: Reeling started - fish hooked!");
    }
    
    private void OnReelingCompleted()
    {
        isBeingReeled = false;
        hasStartedReeling = false; // Reset for next fish
        isFishStruggling = false; // Reset struggle state
        Debug.Log("Bait: Reeling completed!");
    }    private void OnRotationCompleted()
    {
        // Optional: Add extra force when a full rotation is completed
        if (isBeingReeled)
        {
            ApplyRotationBoost();
        }
    }

    private void OnFishStruggleStarted()
    {
        if (isBeingReeled)
        {
            isFishStruggling = true;
            Debug.Log("Bait: Fish started struggling - applying continuous force away!");
        }
    }

    private void OnFishStruggleEnded()
    {
        if (isBeingReeled)
        {
            isFishStruggling = false;
            Debug.Log("Bait: Fish stopped struggling - back to normal reeling");
        }
    }

    private void HandleContinuousReeling()
    {
        if (reelingScript == null || !reelingScript.IsReelingActive()) return;

        // Get current spin speed from the reeling script
        float spinSpeed = reelingScript.GetSpinSpeed();
        bool isSpinning = reelingScript.IsSpinning();

        if (isSpinning && spinSpeed > 0f)
        {
            // Calculate reeling force based on spin speed (using configurable values)
            float normalizedSpinSpeed = Mathf.Clamp01(spinSpeed / 300f); // Normalize spin speed (300 deg/s = max)

            float reelForce = baseReelingForce + (maxReelingForce * normalizedSpinSpeed);
            float reelUpForce = baseUpwardForce + (maxUpwardForce * normalizedSpinSpeed);

            // Apply continuous reeling force
            ApplyReelingForce(reelForce, reelUpForce);

            //! Debug info
            // if (Time.fixedTime % 0.5f < Time.fixedDeltaTime) // Log every 0.5 seconds
            // {
            //     Debug.Log($"Continuous Reeling - Spin Speed: {spinSpeed:F1}°/s, Force: {reelForce:F1}, Up: {reelUpForce:F1}");
            // }
        }
    }

    private void HandleContinuousStruggle()
    {
        // Apply continuous force to push bait away during fish struggle
        Vector3 struggleDirection = Vector3.back; // -Z direction (farther away)
        
        // Apply continuous struggle force
        rb.AddForce(struggleDirection * struggleForce, ForceMode.Force);
        
        // Add some upward force to make the struggle more dynamic
        rb.AddForce(Vector3.up * struggleUpwardForce, ForceMode.Force);
        
        // Debug info for struggle movement
        if (Time.fixedTime % 0.5f < Time.fixedDeltaTime) // Log every 0.5 seconds
        {
            Debug.Log($"Fish Struggling - Continuous force: {struggleForce}, Position: {transform.position.z:F2}");
        }
    }

    private void ApplyReelingForce(float horizontalForce, float upwardForce)
    {
        if (reelingTarget != null)
        {
            // Calculate direction from bait to the reeling target
            Vector3 directionToTarget = (reelingTarget.position - transform.position).normalized;

            // Apply force towards the target (reduced from previous to make it continuous)
            rb.AddForce(directionToTarget * horizontalForce, ForceMode.Force);

            // Add some upward force
            rb.AddForce(Vector3.up * upwardForce, ForceMode.Force);
        }
        else
        {
            // Fallback: use relative movement towards origin (0,0,0) and up
            Vector3 directionToOrigin = (Vector3.zero - transform.position).normalized;

            // Apply horizontal force towards origin
            Vector3 horizontalDirection = new Vector3(directionToOrigin.x, 0, directionToOrigin.z).normalized;
            rb.AddForce(horizontalDirection * horizontalForce, ForceMode.Force);

            // Add upward force
            rb.AddForce(Vector3.up * upwardForce, ForceMode.Force);
        }
    }


    private void ApplyRotationBoost()
    {
        // Apply a stronger force when a full rotation is completed (using configurable values)
        ApplyReelingForce(rotationBoostForce, rotationBoostUpForce);

        Debug.Log($"Rotation completed - applying boost! Force: {rotationBoostForce}, Up: {rotationBoostUpForce}");
    }

    public void SetReelingTarget(Transform target)
    {
        reelingTarget = target;
    }
    #endregion

    public void ResetBait()
    {
        // Reset position and disable
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isInWater = false;
        hasEnteredWater = false;
        isBeingReeled = false; // Reset reeling state
        hasStartedReeling = false; // Reset reeling start flag
        isFishStruggling = false; // Reset struggle state
        
        // Reset drag values
        rb.linearDamping = originalDrag;
        rb.angularDamping = originalAngularDrag;
        
        // Stop fish spawning
        var fishSpawner = FindFirstObjectByType<FishSpawner>();
        if (fishSpawner != null)
        {
            fishSpawner.StopSpawning();
            Debug.Log("Bait: Reset - stopped fish spawning");
        }
        
        gameObject.SetActive(false);
    }
    private void ApplyWaterPhysics()
    {
        Vector3 velocity = rb.linearVelocity;
        float targetDepth = -7f;
        float currentY = transform.position.y;

        // If reached target depth, stop Y movement
        if (currentY <= targetDepth)
        {
            rb.linearVelocity = new Vector3(velocity.x, 0f, velocity.z);
            transform.position = new Vector3(transform.position.x, targetDepth, transform.position.z);
            return;
        }

        // Smoothly decrease Y velocity towards 0 as it approaches target depth
        if (velocity.y < 0) // Only if sinking
        {
            // Calculate how close we are to target (0 = at target, 1 = far from target)
            float distanceToTarget = Mathf.Abs(currentY - targetDepth);
            float maxDistance = 15f; // Assume max distance for normalization
            float normalizedDistance = Mathf.Clamp01(distanceToTarget / maxDistance);

            // Gradually reduce velocity - closer to target = slower velocity
            float targetYVelocity = velocity.y * normalizedDistance * 0.98f; // Smooth reduction

            rb.linearVelocity = new Vector3(velocity.x, targetYVelocity, velocity.z);
        }
    }


    #region Trigger Events

    void OnTriggerEnter(Collider other)
    {
        // Check if bait hits water
        if (other.CompareTag("Water") && !hasEnteredWater)
        {
            isInWater = true;
            hasEnteredWater = true;

            // Reduce initial impact velocity when hitting water
            Vector3 velocity = rb.linearVelocity;
            rb.linearVelocity = new Vector3(velocity.x * 0.7f, velocity.y * 0.5f, velocity.z * 0.7f);

            // Increase drag immediately upon entering water
            rb.linearDamping = waterDrag;
            rb.angularDamping = waterDrag * 0.5f;

            // Notify FishSpawner that bait is in water
            var fishSpawner = FindFirstObjectByType<FishSpawner>();
            if (fishSpawner != null)
            {
                fishSpawner.StartSpawning();
                Debug.Log("Bait: Entered water - started fish spawning");
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Ensure we stay in water state while overlapping with water
        if (other.CompareTag("Water"))
        {
            isInWater = true;
        }
    }

    public bool IsInWater()
    {
        return isInWater;
    }

    public bool HasEnteredWater()
    {
        return hasEnteredWater;
    }

    public bool IsBeingReeled()
    {
        return isBeingReeled;
    }

    public float GetDistanceToTarget()
    {
        if (reelingTarget == null) return float.MaxValue;
        return Vector3.Distance(transform.position, reelingTarget.position);
    }
    
    public bool HasHookedFish()
    {
        return transform.childCount > 0;
    }
    
    public Transform GetHookedFish()
    {
        return transform.childCount > 0 ? transform.GetChild(0) : null;
    }
    #endregion
}
