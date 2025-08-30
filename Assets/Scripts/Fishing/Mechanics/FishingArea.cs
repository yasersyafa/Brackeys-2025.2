using UnityEngine;
using UnityEngine.InputSystem;

public class FishingArea : MonoBehaviour
{

    [Header("Bait Casting")]
    [SerializeField] private GameObject baitGameObject;
    [SerializeField] private GameObject ropeGameObject;
    [SerializeField] private Transform castPoint;
    [SerializeField] private float castForce = 15f;
    [SerializeField] private float castAngle = 45f;

    [Header("UI References")]
    public GameObject panelFishing;
    [SerializeField] private GameObject baitUI;

    private Bait baitScript;
    private bool isBaitCast = false;
    public static bool IsFishingActive = false;

    void Start()
    {
        // Get the bait script component
        if (baitGameObject != null)
        {
            baitScript = baitGameObject.GetComponent<Bait>();
            
            // Set the reeling target to the cast point (fishing rod)
            if (baitScript != null)
            {
                baitScript.SetReelingTarget(castPoint);
            }
        }

        // If no cast point is assigned, use this transform
        if (castPoint == null)
        {
            castPoint = transform;
        }

        baitUI.SetActive(isBaitCast);
    }

    void Update()
    {
        // 3. Simple space input using new Input System
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // Hanya izinkan cast jika player di area Fishing
            var player = FindFirstObjectByType<PlayerMovement>();
            if (player != null && !player.isFishingArea) return;
            if (!isBaitCast && baitGameObject != null)
            {
                CastBait();
            }
            else if (isBaitCast)
            {
                // If bait is already cast, reel it back in
                // ResetBait();
            }
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            baitScript?.StartReeling();
        }
        
        // Optional: Check if bait landed in water
        if (isBaitCast && baitScript != null && baitScript.IsInWater())
        {
            // You can add auto-reel logic here if needed
        }
    }

    private void CastBait()
    {
        // 1. Set active the bait gameObject\
        panelFishing.SetActive(false);
        baitGameObject.SetActive(true);
        ropeGameObject.SetActive(true);
        baitUI.SetActive(true);
        IsFishingActive = true;
        
        // Position bait at cast point
        baitGameObject.transform.position = castPoint.position;
        
        // 2. Shoot the bait gameObject
        Vector3 castDirection = CalculateCastDirection();
        
        if (baitScript != null)
        {
            baitScript.Cast(castDirection, castForce);
        }
        
        isBaitCast = true;
        Debug.Log("Bait cast!");
    }

    public void ResetBait()
    {
        if (baitScript != null)
        {
            baitScript.ResetBait();
        }

        baitGameObject.SetActive(false);
        ropeGameObject.SetActive(false);
        panelFishing.SetActive(true);
        baitUI.SetActive(false);
        isBaitCast = false;
        IsFishingActive = false;
        Debug.Log("Bait reeled in!");
    }

    private Vector3 CalculateCastDirection()
    {
        // Calculate forward direction with an upward angle
        Vector3 forward = transform.forward;
        float angleInRadians = castAngle * Mathf.Deg2Rad;
        
        // Create direction vector with upward arc
        Vector3 direction = new Vector3(
            forward.x,
            Mathf.Sin(angleInRadians),
            forward.z
        ).normalized;
        
        return direction;
    }
}
