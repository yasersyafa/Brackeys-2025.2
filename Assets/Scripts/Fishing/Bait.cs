using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class Bait : MonoBehaviour
{    
    [Header("Water Physics")]
    [SerializeField] private float waterDrag = 5f;
    
    private Rigidbody rb;
    private bool isInWater = false;
    private bool hasEnteredWater = false;
    private float originalDrag;
    private float originalAngularDrag;
    
    // Events
    public static event Action OnCaughtFish;

    void OnEnable()
    {
        Reeling.OnWordCompleted += ReelingMove;
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
    }

    void FixedUpdate()
    {
        if (isInWater)
        {
            ApplyWaterPhysics();
        }
    }

    public void Cast(Vector3 direction, float force)
    {
        // Reset physics state
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isInWater = false;
        hasEnteredWater = false;
        
        // Reset drag to original values
        rb.linearDamping = originalDrag;
        rb.angularDamping = originalAngularDrag;
        
        // Apply casting force
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);
    }

    public void StartReeling()
    {
        OnCaughtFish?.Invoke();
    }
    
    public void ReelingMove(string completedWord)
    {
        float reelZForce = -20f;
        float reelYForce = 20f;

        // move the bait into z 0 by add force the bait's rb
        rb.AddForce(new Vector3(0, 0, reelZForce) * 10, ForceMode.Force);

        // move the bait into y up a little bit to possitive y
        rb.AddForce(new Vector3(0, reelYForce, 0) * 5, ForceMode.Force);
    }

    public void ResetBait()
    {
        // Reset position and disable
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isInWater = false;
        hasEnteredWater = false;

        // Reset drag values
        rb.linearDamping = originalDrag;
        rb.angularDamping = originalAngularDrag;

        gameObject.SetActive(false);
    }

    private void ApplyWaterPhysics()
    {
        Vector3 velocity = rb.linearVelocity;
        float targetDepth = -10f;
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
}
