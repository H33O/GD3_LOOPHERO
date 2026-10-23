using UnityEngine;

/// <summary>
/// Stocke le total des victoires pour la session de jeu courante.
/// </summary>
[CreateAssetMenu(menuName = "Jeu de l'oie/Compteur de victoires")]
public sealed class CompteurVictoires : ScriptableObject
{
    [SerializeField] private int totalVictoires;

    /// <summary>Total des victoires comptabilisées pendant la session courante.</summary>
    public int TotalVictoires => totalVictoires;

    /// <summary>Ajoute une victoire au total en mémoire.</summary>
    public void Incrementer()
    {
        totalVictoires++;
    }
}
