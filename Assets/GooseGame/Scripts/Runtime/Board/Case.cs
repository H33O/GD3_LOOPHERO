using UnityEngine;
using UnityEngine.Serialization;

public sealed class Case : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("effect")] private EffetCase effet;

    // Retourne l'effet configuré pour cette case.
    public EffetCase Effet => effet;
}
