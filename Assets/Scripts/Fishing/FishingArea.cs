using UnityEngine;
using UnityEngine.InputSystem;

public class FishingArea : MonoBehaviour
{
    [Header("Bait Casting")]
    [SerializeField] private GameObject baitGameObject;
    [SerializeField] private Transform castPoint;
    [SerializeField] private float castForce = 15f;
    [SerializeField] private float castAngle = 45f;

    [Header("UI References")]
    [SerializeField] private GameObject baitUI;

    private Bait baitScript;
    private bool isBaitCast = false;

    void Start()
    {
        // Get the bait script component
        if (baitGameObject != null)
        {
            baitScript = baitGameObject.GetComponent<Bait>();
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
            if (!isBaitCast && baitGameObject != null)
            {
                CastBait();
            }
            else if (isBaitCast)
            {
                // If bait is already cast, reel it back in
                ReelInBait();
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
        // 1. Set active the bait gameObject
        baitGameObject.SetActive(true);
        baitUI.SetActive(true);
        
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

    private void ReelingBait()
    {
        // Pass a string argument for completedWord, e.g., an empty string or a relevant word
        baitScript.ReelingMove("");
    }

    private void ReelInBait()
    {
        if (baitScript != null)
        {
            baitScript.ResetBait();
        }

        baitGameObject.SetActive(false);
        baitUI.SetActive(false);
        isBaitCast = false;
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
