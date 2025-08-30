using UnityEngine;

public class FishBehavior : MonoBehaviour
{
    [Header("Fish Settings")]
    [SerializeField] private float swimSpeed = 2f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private bool showDebugInfo = false;
    
    private Fish fishData;
    private Transform targetTransform; // Bait transform
    private bool isInitialized = false;
    private FishStateMachine stateMachine;
    
    void Awake()
    {
        // Get or add state machine
        stateMachine = GetComponent<FishStateMachine>();
        if (stateMachine == null)
        {
            stateMachine = gameObject.AddComponent<FishStateMachine>();
        }
    }
    
    void OnEnable()
    {
        // Listen to state machine events
        if (stateMachine != null)
        {
            FishStateMachine.OnStateChanged += OnStateChanged;
        }
    }
    
    void OnDisable()
    {
        FishStateMachine.OnStateChanged -= OnStateChanged;
    }
    
    public void Initialize(Fish fish, Transform baitTransform)
    {
        fishData = fish;
        targetTransform = baitTransform;
        isInitialized = true;
        
        // Apply fish size if available
        if (fishData?.fishData != null)
        {
            transform.localScale = Vector3.one * fishData.actualSize;
        }
        
        if (showDebugInfo)
            Debug.Log($"FishBehavior: Initialized {fishData?.GetDisplayName()}");
    }
    
    void Update()
    {
        if (!isInitialized || targetTransform == null || stateMachine == null) return;
        
        // Debug parenting status
        if (showDebugInfo && Time.frameCount % 60 == 0) // Log every second
        {
            bool isParented = transform.parent != null;
            string parentName = isParented ? transform.parent.name : "None";
            Vector3 localRot = transform.localRotation.eulerAngles;
            Debug.Log($"FishBehavior: Parented: {isParented}, Parent: {parentName}, State: {stateMachine.GetCurrentState()}");
            Debug.Log($"FishBehavior: Position: {transform.position}, Local Position: {transform.localPosition}, Local Rotation: {localRot}");
        }
        
        // Only move when in swimming state and not parented to bait
        if (stateMachine.CanMove() && transform.parent == null)
        {
            MoveTowardTarget();
        }
        // If parented but fish seems stuck, ensure local position is maintained
        else if (stateMachine.IsHooked() && transform.parent != null)
        {
            // Verify local position is correct (safety check) - new position (0, 0, 0)
            Vector3 expectedLocalPos = Vector3.zero;
            if (Vector3.Distance(transform.localPosition, expectedLocalPos) > 0.1f)
            {
                transform.localPosition = expectedLocalPos;
                if (showDebugInfo)
                    Debug.Log($"FishBehavior: Corrected local position to {expectedLocalPos}");
            }
        }
    }
    
    private void MoveTowardTarget()
    {
        // Calculate direction to target
        Vector3 direction = (targetTransform.position - transform.position).normalized;
        
        // Move toward target
        transform.position += direction * swimSpeed * Time.deltaTime;
        
        // Rotate to face target
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Only trigger when in swimming state to avoid re-triggering
        if (stateMachine == null || !stateMachine.IsInState(FishState.Swimming)) return;
        
        // Check if fish reached the bait
        if (other.CompareTag("Bait") || other.GetComponent<Bait>() != null)
        {
            if (showDebugInfo)
                Debug.Log($"FishBehavior: {fishData?.GetDisplayName()} reached bait!");
            
            // Transition to hooked state
            stateMachine.ForceHook();
        }
    }
    
    private void OnStateChanged(FishState oldState, FishState newState)
    {
        // Only respond to our own state machine
        if (stateMachine == null) return;
        
        if (showDebugInfo)
            Debug.Log($"FishBehavior: State changed from {oldState} to {newState}");
        
        // Handle state-specific behavior
        switch (newState)
        {
            case FishState.Hooked:
                // Stop movement, fish is now hooked
                break;
            case FishState.Caught:
                // Fish caught - could add celebration effects
                break;
            case FishState.Escaped:
                // Fish escaped - could add escape effects
                break;
        }
    }
    
    public Fish GetFishData()
    {
        return fishData;
    }
    
    public bool IsInitialized()
    {
        return isInitialized;
    }
    
    public FishStateMachine GetStateMachine()
    {
        return stateMachine;
    }
    
    public bool CanMove()
    {
        return stateMachine != null && stateMachine.CanMove();
    }
    
    public bool IsHooked()
    {
        return stateMachine != null && stateMachine.IsHooked();
    }
    
    public bool IsParentedToBait()
    {
        return transform.parent != null && transform.parent == targetTransform;
    }
}
