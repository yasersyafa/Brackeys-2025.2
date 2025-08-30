using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public Button resumeButton;
    public Button settingButton;
    public Button exitButton;
    private bool isPaused = false;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        // Tambahkan efek hover DOTween ke button
        AddButtonHoverEffect(resumeButton);
        AddButtonHoverEffect(exitButton);
        AddButtonHoverEffect(settingButton);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!isPaused)
                Pause();
            else
                Resume();
        }
    }

    public void Pause()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("_MainMenu");
    }

    private void AddButtonHoverEffect(Button button)
    {
        if (button == null) return;
        var image = button.GetComponent<Image>();
        if (image == null) return;
        image.enabled = false; // default: nonaktif
        var trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((_) => image.enabled = true);
        trigger.triggers.Add(entryEnter);
        var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        entryExit.callback.AddListener((_) => image.enabled = false);
        trigger.triggers.Add(entryExit);
    }
}
