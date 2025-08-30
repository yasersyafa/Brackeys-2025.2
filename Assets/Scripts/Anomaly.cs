using System.Collections;
using System.Threading.Tasks;
using Assets.Scripts.ScriptableObjects;
using UnityEngine;

public class Anomaly : MonoBehaviour
{
    // do not forget to assign this script into anomaly prefab
    [SerializeField]
    private AnomalySO _anomalySO;
    private Vector3 _target;
    private float _anomalyTrigger;
    private bool _isAlive = true;
    private Vector3 _startPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _target = new(0, transform.position.y, 0);
        _startPosition = transform.position;
        _anomalyTrigger = _anomalySO.anomalyTrigger;
    }

    private void Update()
    {
        if (!_isAlive) return;

        _anomalyTrigger -= Time.deltaTime;
        if (_target != null && _anomalyTrigger > 0)
        {
            float progress = 1f - (_anomalyTrigger / _anomalySO.anomalyTrigger);
            transform.position = Vector3.Lerp(_startPosition, _target, progress);
        }

        if (_anomalyTrigger <= 0f)
        {
            transform.position = _target;
        }
    }

    public void DestroyAnomaly()
    {
        if (!_isAlive) return;
        StartCoroutine(SinkAndDestroy(0.2f));
    }

    private IEnumerator SinkAndDestroy(float duration)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.down * 2f; // turun 2 satuan Y
        float time = 0;

        while (time < duration)
        {
            float t = time / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            time += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
        _isAlive = false;
        AnomalySpawner.Instance._anomaly = null;
        Destroy(gameObject);
    }
}
