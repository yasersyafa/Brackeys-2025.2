using Assets.Scripts.ScriptableObjects;
using UnityEngine;

public class Anomaly : MonoBehaviour
{
    // do not forget to assign this script into anomaly prefab
    [SerializeField]
    private AnomalySO _anomalySO;
    private float _anomalyTrigger => _anomalySO.anomalyTrigger;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        
    }
}
