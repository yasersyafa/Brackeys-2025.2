using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Enhanced helper script to create the new Reeling UI with all required components:
/// - SpinWheel (main spinning wheel)
/// - ProgressFill (rotation progress)
/// - ReelConditionFill (reel stress indicator)
/// - SpinningNeedle (clock-like needle)
/// Add this to a Canvas GameObject and click "Setup Enhanced Reeling UI" in the context menu
/// </summary>
public class ReelingUISetup : MonoBehaviour
{
    [Header("Auto-Setup")]
    [SerializeField] private bool createSpinWheel = true;
    [SerializeField] private bool createProgressFill = true;
    [SerializeField] private bool createReelConditionFill = true;
    [SerializeField] private bool createSpinningNeedle = true;
    [SerializeField] private bool createProgressText = true;
    [SerializeField] private bool createInstructionText = true;
    
    [Header("Wheel Settings")]
    [SerializeField] private float wheelSize = 300f;
    [SerializeField] private Color wheelColor = Color.white;
    [SerializeField] private Color progressColor = Color.green;
    [SerializeField] private Color conditionSafeColor = Color.green;
    [SerializeField] private Color needleColor = Color.red;
    
    [ContextMenu("Setup Enhanced Reeling UI")]
    public void SetupEnhancedReelingUI()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("This script must be on a Canvas GameObject!");
            return;
        }
        
        // Ensure Canvas is set to Screen Space - Overlay
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // High sorting order to appear on top
        
        // Create main wheel container (this will be the ReelingWheel reference)
        GameObject wheelContainer = new GameObject("ReelingWheel");
        wheelContainer.transform.SetParent(transform, false);
        
        RectTransform wheelContainerRect = wheelContainer.AddComponent<RectTransform>();
        wheelContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
        wheelContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
        wheelContainerRect.anchoredPosition = Vector2.zero;
        wheelContainerRect.sizeDelta = new Vector2(wheelSize, wheelSize);
        
        if (createSpinWheel)
        {
            // Create main spinning wheel
            GameObject spinWheelObj = new GameObject("SpinWheel");
            spinWheelObj.transform.SetParent(wheelContainer.transform, false);
            
            Image wheelImage = spinWheelObj.AddComponent<Image>();
            wheelImage.color = wheelColor;
            
            RectTransform wheelRect = spinWheelObj.GetComponent<RectTransform>();
            wheelRect.anchorMin = Vector2.zero;
            wheelRect.anchorMax = Vector2.one;
            wheelRect.offsetMin = Vector2.zero;
            wheelRect.offsetMax = Vector2.zero;
            
            Debug.Log("SpinWheel created - assign this to the Reeling script");
        }
        
        if (createProgressFill)
        {
            // Create progress fill (circular slider for rotation progress)
            GameObject progressFillObj = new GameObject("ProgressFill");
            progressFillObj.transform.SetParent(wheelContainer.transform, false);
            
            Slider progressSlider = progressFillObj.AddComponent<Slider>();
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
            progressSlider.interactable = false; // Read-only display
            
            RectTransform progressRect = progressFillObj.GetComponent<RectTransform>();
            progressRect.anchorMin = Vector2.zero;
            progressRect.anchorMax = Vector2.one;
            progressRect.offsetMin = new Vector2(20, 20); // Slightly smaller than wheel
            progressRect.offsetMax = new Vector2(-20, -20);
            
            // Create background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(progressFillObj.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            bgImage.type = Image.Type.Filled;
            bgImage.fillMethod = Image.FillMethod.Radial360;
            
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            progressSlider.targetGraphic = bgImage;
            
            // Create fill area
            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(progressFillObj.transform, false);
            
            RectTransform fillAreaRect = fillAreaObj.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;
            
            // Create fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = progressColor;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Radial360;
            fillImage.fillOrigin = 2; // Top
            
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            
            progressSlider.fillRect = fillRect;
            
            Debug.Log("ProgressFill Slider created - assign this to the Reeling script");
        }
        
        if (createReelConditionFill)
        {
            // Create reel condition fill (vertical slider on the side)
            GameObject conditionBarContainer = new GameObject("ReelCondition Container");
            conditionBarContainer.transform.SetParent(transform, false);
            
            RectTransform conditionContainerRect = conditionBarContainer.AddComponent<RectTransform>();
            conditionContainerRect.anchorMin = new Vector2(0.85f, 0.3f);
            conditionContainerRect.anchorMax = new Vector2(0.9f, 0.7f);
            conditionContainerRect.offsetMin = Vector2.zero;
            conditionContainerRect.offsetMax = Vector2.zero;
            
            // Create reel condition slider
            GameObject conditionFillObj = new GameObject("ReelConditionFill");
            conditionFillObj.transform.SetParent(conditionBarContainer.transform, false);
            
            Slider conditionSlider = conditionFillObj.AddComponent<Slider>();
            conditionSlider.direction = Slider.Direction.BottomToTop; // Vertical slider
            conditionSlider.minValue = 0f;
            conditionSlider.maxValue = 1f;
            conditionSlider.value = 0f;
            conditionSlider.interactable = false; // Read-only display
            
            RectTransform conditionSliderRect = conditionFillObj.GetComponent<RectTransform>();
            conditionSliderRect.anchorMin = Vector2.zero;
            conditionSliderRect.anchorMax = Vector2.one;
            conditionSliderRect.offsetMin = Vector2.zero;
            conditionSliderRect.offsetMax = Vector2.zero;
            
            // Create background
            GameObject conditionBgObj = new GameObject("Background");
            conditionBgObj.transform.SetParent(conditionFillObj.transform, false);
            
            Image conditionBgImage = conditionBgObj.AddComponent<Image>();
            conditionBgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            RectTransform conditionBgRect = conditionBgObj.GetComponent<RectTransform>();
            conditionBgRect.anchorMin = Vector2.zero;
            conditionBgRect.anchorMax = Vector2.one;
            conditionBgRect.offsetMin = Vector2.zero;
            conditionBgRect.offsetMax = Vector2.zero;
            
            conditionSlider.targetGraphic = conditionBgImage;
            
            // Create fill area
            GameObject conditionFillAreaObj = new GameObject("Fill Area");
            conditionFillAreaObj.transform.SetParent(conditionFillObj.transform, false);
            
            RectTransform conditionFillAreaRect = conditionFillAreaObj.GetComponent<RectTransform>();
            conditionFillAreaRect.anchorMin = Vector2.zero;
            conditionFillAreaRect.anchorMax = Vector2.one;
            conditionFillAreaRect.offsetMin = Vector2.zero;
            conditionFillAreaRect.offsetMax = Vector2.zero;
            
            // Create fill
            GameObject conditionFillInnerObj = new GameObject("Fill");
            conditionFillInnerObj.transform.SetParent(conditionFillAreaObj.transform, false);
            
            Image conditionFillImage = conditionFillInnerObj.AddComponent<Image>();
            conditionFillImage.color = conditionSafeColor;
            
            RectTransform conditionFillRect = conditionFillInnerObj.GetComponent<RectTransform>();
            conditionFillRect.anchorMin = Vector2.zero;
            conditionFillRect.anchorMax = Vector2.one;
            conditionFillRect.offsetMin = Vector2.zero;
            conditionFillRect.offsetMax = Vector2.zero;
            
            conditionSlider.fillRect = conditionFillRect;
            
            // Add label
            GameObject conditionLabelObj = new GameObject("Condition Label");
            conditionLabelObj.transform.SetParent(conditionBarContainer.transform, false);
            
            TextMeshProUGUI conditionLabel = conditionLabelObj.AddComponent<TextMeshProUGUI>();
            conditionLabel.text = "REEL\nSTRESS";
            conditionLabel.fontSize = 12;
            conditionLabel.color = Color.white;
            conditionLabel.alignment = TextAlignmentOptions.Center;
            
            RectTransform conditionLabelRect = conditionLabelObj.GetComponent<RectTransform>();
            conditionLabelRect.anchorMin = new Vector2(0f, -0.3f);
            conditionLabelRect.anchorMax = new Vector2(1f, 0f);
            conditionLabelRect.offsetMin = Vector2.zero;
            conditionLabelRect.offsetMax = Vector2.zero;
            
            Debug.Log("ReelConditionFill Slider created - assign this to the Reeling script");
        }
        
        if (createSpinningNeedle)
        {
            // Create spinning needle (anchored at center, pointing outward like clock hand)
            GameObject needleObj = new GameObject("SpinningNeedle");
            needleObj.transform.SetParent(wheelContainer.transform, false);
            
            Image needleImage = needleObj.AddComponent<Image>();
            needleImage.color = needleColor;
            
            RectTransform needleRect = needleObj.GetComponent<RectTransform>();
            needleRect.anchorMin = new Vector2(0.5f, 0.5f); // Center anchor
            needleRect.anchorMax = new Vector2(0.5f, 0.5f); // Center anchor
            needleRect.pivot = new Vector2(0.5f, 0f); // Pivot at bottom (base of needle)
            needleRect.anchoredPosition = Vector2.zero; // Centered on wheel
            needleRect.sizeDelta = new Vector2(4f, wheelSize * 0.4f); // Thin needle, extends outward
            
            Debug.Log("SpinningNeedle created - assign this to the Reeling script");
        }
        
        if (createProgressText)
        {
            // Create progress text
            GameObject progressTextObj = new GameObject("Progress Text");
            progressTextObj.transform.SetParent(transform, false);
            
            TextMeshProUGUI progressText = progressTextObj.AddComponent<TextMeshProUGUI>();
            progressText.text = "Rotations: 0 / 5";
            progressText.fontSize = 24;
            progressText.color = Color.white;
            progressText.alignment = TextAlignmentOptions.Center;
            
            RectTransform progressTextRect = progressTextObj.GetComponent<RectTransform>();
            progressTextRect.anchorMin = new Vector2(0.2f, 0.8f);
            progressTextRect.anchorMax = new Vector2(0.8f, 0.9f);
            progressTextRect.offsetMin = Vector2.zero;
            progressTextRect.offsetMax = Vector2.zero;
        }
        
        if (createInstructionText)
        {
            // Create instruction text
            GameObject instructionTextObj = new GameObject("Instruction Text");
            instructionTextObj.transform.SetParent(transform, false);
            
            TextMeshProUGUI instructionText = instructionTextObj.AddComponent<TextMeshProUGUI>();
            instructionText.text = "Move mouse in circles clockwise to reel in the fish!";
            instructionText.fontSize = 18;
            instructionText.color = Color.yellow;
            instructionText.alignment = TextAlignmentOptions.Center;
            
            RectTransform instructionTextRect = instructionTextObj.GetComponent<RectTransform>();
            instructionTextRect.anchorMin = new Vector2(0.1f, 0.1f);
            instructionTextRect.anchorMax = new Vector2(0.9f, 0.2f);
            instructionTextRect.offsetMin = Vector2.zero;
            instructionTextRect.offsetMax = Vector2.zero;
        }
        
        Debug.Log("Enhanced Reeling UI setup completed!");
        Debug.Log("Please assign the following to your Reeling script:");
        Debug.Log("- ReelingWheel: 'ReelingWheel' RectTransform (parent container)");
        Debug.Log("- SpinWheel: 'SpinWheel' GameObject");
        Debug.Log("- ProgressFill: 'ProgressFill' Slider component");
        Debug.Log("- ReelConditionFill: 'ReelConditionFill' Slider component"); 
        Debug.Log("- SpinningNeedle: 'SpinningNeedle' RectTransform");
    }
    
    [ContextMenu("Find Reeling Script and Show Assignment Instructions")]
    public void ShowAssignmentInstructions()
    {
        Reeling reelingScript = FindFirstObjectByType<Reeling>();
        if (reelingScript == null)
        {
            Debug.LogWarning("No Reeling script found in the scene!");
            return;
        }
        
        Debug.Log("Found Reeling script. Please manually assign these UI references:");
        Debug.Log("1. Reeling Canvas: " + gameObject.name);
        Debug.Log("2. Reeling Wheel: Find 'ReelingWheel' RectTransform (parent container)");
        Debug.Log("3. Spin Wheel: Find 'SpinWheel' child object");
        Debug.Log("4. Progress Fill: Find 'ProgressFill' Slider component");
        Debug.Log("5. Reel Condition Fill: Find 'ReelConditionFill' Slider component");
        Debug.Log("6. Spinning Needle: Find 'SpinningNeedle' RectTransform");
        Debug.Log("7. Progress Text: Find 'Progress Text' TextMeshProUGUI");
        Debug.Log("8. Instruction Text: Find 'Instruction Text' TextMeshProUGUI");
    }
}
