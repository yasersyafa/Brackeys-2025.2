using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;


public class GeneratorQTE : MonoBehaviour
{
    [Header("UI References")]
    public GameObject qtePanel; // panel utama QTE (parent dari elemen QTE)
    public Slider progressBar;
    public Image qteBackground; // background QTE (RectTransform)
    public Image correctArea;   // area benar QTE (RectTransform)
    public Image movingPoint;   // titik bergerak QTE (RectTransform)

    [Header("Generator Objects (akan di-enable/disable)")]
    public GameObject[] generatorObjects;
    
    [Header("Audio Settings")]
    public Transform generatorAudioSource; // Reference GameObject for 3D audio positioning
    public float audioCheckInterval = 0.5f; // How often to check audio distance (seconds)
    public float maxAudioDistance = 10f; // Maximum distance to hear generator audio
    
    private bool isGeneratorAudioPlaying = false;
    private Coroutine audioCheckCoroutine;

    [Header("QTE Settings")]
    public float totalDuration = 15f;
    public float failPenalty = 1f;

    private float progressTime;
    private bool isQTEActive = false;
    private bool isGeneratorOn = true;
    private Coroutine qteCoroutine;
    private Coroutine progressCoroutine;
    private Coroutine generatorOnCoroutine;
    public static bool IsQTEActive = false;

    // Public property to check generator state for JumpscareSystem
    public bool IsGeneratorOn => isGeneratorOn;
    
    // Events for generator state changes
    public static event Action OnGeneratorTurnedOff;
    public static event Action OnGeneratorTurnedOn;

    [Header("Navigation Panel")]
    public GameObject navigationPanel;

        void Start()
        {
            if (qtePanel != null) qtePanel.SetActive(false);
            HideQTEElements();
            SetGeneratorState(true); // Generator ON saat awal
            if (generatorOnCoroutine != null) StopCoroutine(generatorOnCoroutine);
            generatorOnCoroutine = StartCoroutine(GeneratorOnTimer());
            
            // Start audio distance checking
            if (audioCheckCoroutine != null) StopCoroutine(audioCheckCoroutine);
            audioCheckCoroutine = StartCoroutine(AudioDistanceChecker());
        }

    void Update()
    {
        var player = FindFirstObjectByType<PlayerMovement>();
        if (player != null && !player.isGeneratorArea) return;
        if (!isQTEActive && !isGeneratorOn && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartQTE();
        }
    }

    public void StartQTE()
    {
        IsQTEActive = true;
        if (qtePanel != null) qtePanel.SetActive(true);
        if (navigationPanel != null) navigationPanel.SetActive(false);
        progressBar.value = 0f;
        progressTime = 0f;
        isQTEActive = true;
        if (progressCoroutine != null) StopCoroutine(progressCoroutine);
        progressCoroutine = StartCoroutine(ProgressRoutine());
        if (qteCoroutine != null) StopCoroutine(qteCoroutine);
        qteCoroutine = StartCoroutine(QTERoutine());
    }

    IEnumerator ProgressRoutine()
    {
        while (progressTime < totalDuration)
        {
            progressTime += Time.deltaTime;
            progressBar.value = progressTime / totalDuration;
            yield return null;
        }
        EndQTE();
    }

    IEnumerator QTERoutine()
    {
        while (progressTime < totalDuration)
        {
            float delay = UnityEngine.Random.Range(1, 4); // 1,2,3 detik
            HideQTEElements();
            yield return new WaitForSeconds(delay);
            yield return StartCoroutine(QTESession());
        }
    }

    IEnumerator QTESession()
    {
        // Randomize speed and width
        float[] speeds = { 130f, 150f, 170f };
        int[] widths = { 70, 120, 150 };
        float speed = speeds[UnityEngine.Random.Range(0, speeds.Length)];
        int width = widths[UnityEngine.Random.Range(0, widths.Length)];

        // Setup correct area (width random, posisi X tetap)
        RectTransform bgRect = qteBackground.rectTransform;
        RectTransform correctRect = correctArea.rectTransform;
        RectTransform pointRect = movingPoint.rectTransform;
        float bgWidth = bgRect.sizeDelta.x;
        float bgLeft = -bgWidth / 2f;
        float bgRight = bgWidth / 2f;

        correctRect.sizeDelta = new Vector2(width, correctRect.sizeDelta.y);
        correctRect.anchoredPosition = new Vector2(0, correctRect.anchoredPosition.y); // tetap di tengah

        // Setup moving point di kiri
        pointRect.anchoredPosition = new Vector2(bgLeft, pointRect.anchoredPosition.y);

        // Tampilkan elemen QTE
        ShowQTEElements();

        bool success = false;
        while (pointRect.anchoredPosition.x < bgRight)
        {
            // Gerakkan titik dari kiri ke kanan
            pointRect.anchoredPosition += new Vector2(speed * Time.deltaTime * bgWidth / 200f, 0); // normalisasi speed

            // Input: Space untuk QTE
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (IsInCorrectArea(pointRect, correctRect))
                {
                    success = true;
                }
                break;
            }
            yield return null;
        }

        HideQTEElements();

        if (!success)
        {
            progressTime = Mathf.Max(0, progressTime - failPenalty);
        }
        // Optionally, beri feedback sukses/gagal di sini
    }

    bool IsInCorrectArea(RectTransform point, RectTransform area)
    {
        float pointX = point.anchoredPosition.x;
        float areaX = area.anchoredPosition.x;
        float areaW = area.sizeDelta.x;
        return pointX >= areaX - areaW / 2f && pointX <= areaX + areaW / 2f;
    }

    void HideQTEElements()
    {
        if (qteBackground != null) qteBackground.gameObject.SetActive(false);
        if (correctArea != null) correctArea.gameObject.SetActive(false);
        if (movingPoint != null) movingPoint.gameObject.SetActive(false);
    }

    void ShowQTEElements()
    {
        if (qteBackground != null) qteBackground.gameObject.SetActive(true);
        if (correctArea != null) correctArea.gameObject.SetActive(true);
        if (movingPoint != null) movingPoint.gameObject.SetActive(true);
    }

    void EndQTE()
    {
        IsQTEActive = false;
        isQTEActive = false;
        qtePanel.SetActive(false);
        HideQTEElements();
        if (navigationPanel != null) navigationPanel.SetActive(true);
        if (progressCoroutine != null) StopCoroutine(progressCoroutine);
        if (qteCoroutine != null) StopCoroutine(qteCoroutine);

        // Nyalakan generator
        SetGeneratorState(true);
        if (generatorOnCoroutine != null) StopCoroutine(generatorOnCoroutine);
        generatorOnCoroutine = StartCoroutine(GeneratorOnTimer());
    }

    void SetGeneratorState(bool on)
    {
        bool wasOn = isGeneratorOn;
        isGeneratorOn = on;
        
        if (generatorObjects != null)
        {
            foreach (var obj in generatorObjects)
            {
                if (obj != null) obj.SetActive(on);
            }
        }
        
        // Handle 3D audio with generator state changes
        if (AudioManager.Instance != null && generatorAudioSource != null)
        {
            if (on)
            {
                // Generator turned ON - the AudioDistanceChecker will handle starting the audio based on proximity
                // Reset audio state
                isGeneratorAudioPlaying = false;
                Debug.Log("Generator turned ON - Audio will be managed by distance checker");
            }
            else
            {
                // Generator turned OFF - stop loop and play shutdown sound
                AudioManager.Instance.KillAudioSfx(7); // Stop looping generator sound
                AudioManager.Instance.PlaySound3D(5, 0.6f, generatorAudioSource); // Play generator shutdown sound
                isGeneratorAudioPlaying = false;
                Debug.Log("Generator turned OFF - Audio stopped");
            }
        }
        
        // Fire events when state changes
        if (wasOn && !on)
        {
            OnGeneratorTurnedOff?.Invoke();
            Debug.Log("Generator turned OFF - Event fired");
        }
        else if (!wasOn && on)
        {
            OnGeneratorTurnedOn?.Invoke();
            Debug.Log("Generator turned ON - Event fired");
        }
    }

    IEnumerator GeneratorOnTimer()
    {
        // Durasi random 1-2 menit
        float duration = UnityEngine.Random.Range(60f, 121f);
        yield return new WaitForSeconds(duration);
        SetGeneratorState(false);
    }
    
    IEnumerator AudioDistanceChecker()
    {
        while (true)
        {
            yield return new WaitForSeconds(audioCheckInterval);
            
            // Only check if generator is on and we have audio source
            if (isGeneratorOn && generatorAudioSource != null && AudioManager.Instance != null)
            {
                var player = FindFirstObjectByType<PlayerMovement>();
                if (player != null)
                {
                    float distance = Vector3.Distance(player.transform.position, generatorAudioSource.position);
                    
                    // If player is within range and audio not playing, start it
                    if (distance <= maxAudioDistance && !isGeneratorAudioPlaying)
                    {
                        AudioManager.Instance.PlaySfxLoop3D(7, 0.4f, generatorAudioSource);
                        isGeneratorAudioPlaying = true;
                        Debug.Log($"Generator audio started - Distance: {distance:F1}");
                    }
                    // If player is out of range and audio is playing, stop it
                    else if (distance > maxAudioDistance && isGeneratorAudioPlaying)
                    {
                        AudioManager.Instance.KillAudioSfx(7);
                        isGeneratorAudioPlaying = false;
                        Debug.Log($"Generator audio stopped - Distance: {distance:F1}");
                    }
                }
            }
            // If generator is off, ensure audio is stopped
            else if (!isGeneratorOn && isGeneratorAudioPlaying)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.KillAudioSfx(7);
                }
                isGeneratorAudioPlaying = false;
            }
        }
    }
}
