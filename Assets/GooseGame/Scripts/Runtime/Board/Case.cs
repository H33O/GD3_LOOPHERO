using UnityEngine;

/// <summary>
/// Une case du parcours avec un déplacement simple appliqué à l'arrivée du pion.
/// </summary>
public sealed class Case : MonoBehaviour
{
    // 0 : aucun effet, positif : avancer, négatif : reculer.
    [SerializeField] private int deplacement;

    /// <summary>Déplacement appliqué une seule fois après l'arrivée sur cette case.</summary>
    public int Deplacement => deplacement;
}
