using UnityEngine;
using UnityEngine.AI;
using Hellmade.Sound;

public class PlayerMovement : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform steerArea;
    public Transform flashlightArea;
    public Transform fishingArea;
    public Transform generatorArea;

    public enum Area { Steer, Flashlight, Fishing, Generator }
    public Area currentArea = Area.Steer;

    // Flag area aktif
    public bool isSteerArea => currentArea == Area.Steer;
    public bool isFlashlightArea => currentArea == Area.Flashlight;
    public bool isFishingArea => currentArea == Area.Fishing;
    public bool isGeneratorArea => currentArea == Area.Generator;

    [Header("Area Panels")]
    public GameObject panelSteer;
    public GameObject panelFlashlight;
    public GameObject panelFishing;
    public GameObject panelGenerator;

    private float targetYRotation = 0f;
    private bool isRotatingToTarget = false;
    public float smoothRotateSpeed = 360f; // derajat per detik

    [Header("Other Settings")]
    public GameObject flashlightRod;

    // Tambahan: flag idle di area flashlight
    private bool isIdleInFlashlightArea = false;
    
    // Audio variables for walking
    private bool wasMovingLastFrame = false;
    private float walkingAudioDelay = 0f;
    private const float WALKING_AUDIO_DELAY_TIME = 1f;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;
        MoveToArea(currentArea);
        wasMovingLastFrame = false; // Initialize walking audio state
        walkingAudioDelay = 0f; // Initialize walking audio delay
        
        // Don't stop ambient music, just ensure music (0) is playing
        if (AudioManager.Instance != null)
        {
            if (!AudioManager.Instance.IsMusicPlaying(0))
            {
                AudioManager.Instance.PlayMusic(0, 0.6f);
            }
        }
    }

    void Update()
    {
        // Update walking audio
        UpdateWalkingAudio();
        
        // Smooth rotasi manual setelah sampai tujuan
        if (isRotatingToTarget)
        {
            float currentY = transform.eulerAngles.y;
            float newY = Mathf.MoveTowardsAngle(currentY, targetYRotation, smoothRotateSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, newY, transform.eulerAngles.z);
            if (Mathf.Abs(Mathf.DeltaAngle(newY, targetYRotation)) < 0.5f)
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, targetYRotation, transform.eulerAngles.z);
                isRotatingToTarget = false;
                // Aktifkan panel area baru setelah sampai tujuan
                // Jangan aktifkan panel navigasi jika QTE sedang aktif
                if (!GeneratorQTE.IsQTEActive) {
                    if (panelSteer != null) panelSteer.SetActive(currentArea == Area.Steer);
                    if (panelFlashlight != null) panelFlashlight.SetActive(currentArea == Area.Flashlight);
                    if (panelFishing != null) panelFishing.SetActive(currentArea == Area.Fishing && !FishingArea.IsFishingActive);
                    if (panelGenerator != null) panelGenerator.SetActive(currentArea == Area.Generator);
                }
                if (flashlightRod != null) flashlightRod.SetActive(currentArea == Area.Flashlight);
            }
        }
        // Cek jika sudah sampai tujuan dan belum rotasi manual
        else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            float yRot = 0f;
            switch (currentArea)
            {
                case Area.Flashlight: yRot = 0f; break;
                case Area.Fishing: yRot = 180f; break;
                case Area.Generator: yRot = 90f; break;
                default: yRot = -90f; break;
            }
            targetYRotation = yRot;
            isRotatingToTarget = true;
        }
    }

    // Fungsi public untuk Button UI
    public void GoToSteer() { if (currentArea != Area.Steer) MoveToArea(Area.Steer); }
    public void GoToFlashlight() { if (currentArea != Area.Flashlight) MoveToArea(Area.Flashlight); }
    public void GoToFishing() { if (currentArea != Area.Fishing) MoveToArea(Area.Fishing); }
    public void GoToGenerator() { if (currentArea != Area.Generator) MoveToArea(Area.Generator); }

    void MoveToArea(Area area)
    {
        currentArea = area;
        Transform target = steerArea;
        if (area == Area.Flashlight) target = flashlightArea;
        else if (area == Area.Fishing) target = fishingArea;
        else if (area == Area.Generator) target = generatorArea;
        agent.SetDestination(target.position);
        isRotatingToTarget = false;
        if (flashlightRod != null) flashlightRod.SetActive(false);

        // Nonaktifkan semua panel saat mulai pindah area
        if (panelSteer != null) panelSteer.SetActive(false);
        if (panelFlashlight != null) panelFlashlight.SetActive(false);
        if (panelFishing != null) panelFishing.SetActive(false);
        if (panelGenerator != null) panelGenerator.SetActive(false);
    }
    
    private void UpdateWalkingAudio()
    {
        // Check if agent is currently moving
        bool isMoving = agent.hasPath && agent.remainingDistance > agent.stoppingDistance && agent.velocity.magnitude > 0.1f;
        
        // Update delay timer
        if (walkingAudioDelay > 0f)
        {
            walkingAudioDelay -= Time.deltaTime;
        }
        
        // Check if moving state changed
        if (isMoving && !wasMovingLastFrame)
        {
            // Started moving - play looping walking sound with delay
            if (walkingAudioDelay <= 0f && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfxLoop(12, 0.4f); // Play looping walking sound as SFX
                walkingAudioDelay = WALKING_AUDIO_DELAY_TIME; // Set delay before next play
            }
        }
        else if (!isMoving && wasMovingLastFrame)
        {
            // Stopped moving - kill the walking sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.KillAudioSfx(12); // Kill walking sound as SFX only
            }
            walkingAudioDelay = 0f; // Reset delay when stopping
        }
        
        // Update last frame state
        wasMovingLastFrame = isMoving;
    }
}