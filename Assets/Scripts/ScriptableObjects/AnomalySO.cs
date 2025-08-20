using UnityEngine;

namespace Assets.Scripts.ScriptableObjects
{
    public enum AnomalyType
    {
        FromSea,
        FromShip
    }
    [CreateAssetMenu(fileName = "Anomaly", menuName = "Scriptable Objects/Anomaly")]
    public class AnomalySO : ScriptableObject
    {
        public GameObject anomalyPrefab;
        public string anomalyIdentifier;
        public float anomalyTrigger = 5f; // default 5 detik untuk trigger game over
        public AnomalyType anomalyType;
    }
}
