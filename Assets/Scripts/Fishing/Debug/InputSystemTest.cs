using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Simple test script to verify Input System is working correctly
/// Add this to a GameObject with a TextMeshProUGUI component to see mouse input
/// </summary>
public class InputSystemTest : MonoBehaviour
{
    private TextMeshProUGUI debugText;
    
    void Start()
    {
        debugText = GetComponent<TextMeshProUGUI>();
        if (debugText == null)
        {
            debugText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (debugText == null)
        {
            Debug.LogWarning("InputSystemTest: No TextMeshProUGUI component found!");
        }
    }
    
    void Update()
    {
        if (debugText == null) return;
        
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            debugText.text = "No mouse detected!";
            return;
        }
        
        Vector2 mousePos = mouse.position.ReadValue();
        bool leftPressed = mouse.leftButton.isPressed;
        bool rightPressed = mouse.rightButton.isPressed;
        
        debugText.text = $"Mouse Input Test\n" +
                        $"Position: {mousePos.x:F0}, {mousePos.y:F0}\n" +
                        $"Left Button: {(leftPressed ? "PRESSED" : "Released")}\n" +
                        $"Right Button: {(rightPressed ? "PRESSED" : "Released")}\n" +
                        $"Input System: Working ✓";
    }
}
