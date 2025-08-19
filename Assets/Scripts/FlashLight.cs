using UnityEngine;
using UnityEngine.InputSystem;

public class FlashLight : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField]
    private Light _flashLight;
    [SerializeField]
    private float _range = 10f;
    [SerializeField]
    private LayerMask _detectableLayer;

    private InputSystem_Actions _inputActions;
    private bool _isFlashLightOn = false;
    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _inputActions.Player.Enable();
        _inputActions.Player.Interact.performed += HandleFlashLight;
        Debug.Log("Hello World");
    }

    private void OnDisable()
    {
        _inputActions.Player.Disable();
        _inputActions.Player.Interact.performed -= HandleFlashLight;
    }

    private void HandleFlashLight(InputAction.CallbackContext context)
    {
        _isFlashLightOn = !_isFlashLightOn;
        _flashLight.enabled = _isFlashLightOn;
        Debug.Log($"Flashlight is {_isFlashLightOn}");
    }
    private void Update()
    {
        if (_isFlashLightOn)
        {
            DetectObjectInFlashLight();
        }
    }

    private void DetectObjectInFlashLight()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _range, _detectableLayer);

        foreach (var hit in hits)
        {
            Vector3 dirToTarget = (hit.transform.position - _flashLight.transform.position).normalized;

            // cek sudut dengan arah senter
            float angle = Vector3.Angle(_flashLight.transform.forward, dirToTarget);

            if (angle < _flashLight.spotAngle / 2f) // dalam cone cahaya
            {
                Debug.Log($"Detected: {hit.gameObject.name}");
            }
        }
    }
}
