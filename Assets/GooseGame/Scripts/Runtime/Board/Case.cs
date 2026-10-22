using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Une case du plateau. Son effet est facultatif et configuré dans l'Inspector.
/// </summary>
public sealed class Case : MonoBehaviour
{
    // Conserve les effets enregistrés avant le renommage du champ « effect » en « effet ».
    [SerializeField, FormerlySerializedAs("effect")] private EffetCase effet;

    /// <summary>L'effet à appliquer quand le pion termine son déplacement sur cette case.</summary>
    public EffetCase Effet => effet;
}
