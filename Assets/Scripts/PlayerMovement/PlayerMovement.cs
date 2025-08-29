using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform steerArea;
    public Transform flashlightArea;
    public Transform fishingArea;
    public Transform generatorArea;

    public enum Area { Steer, Flashlight, Fishing, Generator }
    public Area currentArea = Area.Steer;

    [Header("Area Panels")]
    public GameObject panelSteer;
    public GameObject panelFlashlight;
    public GameObject panelFishing;
    public GameObject panelGenerator;

    private float targetYRotation = 0f;
    private bool isRotatingToTarget = false;
    public float smoothRotateSpeed = 360f; // derajat per detik

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;
        MoveToArea(currentArea);
    }

    void Update()
    {
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
                if (panelSteer != null) panelSteer.SetActive(currentArea == Area.Steer);
                if (panelFlashlight != null) panelFlashlight.SetActive(currentArea == Area.Flashlight);
                if (panelFishing != null) panelFishing.SetActive(currentArea == Area.Fishing);
                if (panelGenerator != null) panelGenerator.SetActive(currentArea == Area.Generator);
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

        // Nonaktifkan semua panel saat mulai pindah area
        if (panelSteer != null) panelSteer.SetActive(false);
        if (panelFlashlight != null) panelFlashlight.SetActive(false);
        if (panelFishing != null) panelFishing.SetActive(false);
        if (panelGenerator != null) panelGenerator.SetActive(false);
    }
}