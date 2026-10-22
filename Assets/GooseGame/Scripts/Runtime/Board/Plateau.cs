using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Garde les cases du parcours dans leur ordre et donne leur position au pion.
/// </summary>
public sealed class Plateau : MonoBehaviour
{
    // L'ordre de cette liste définit l'ordre du parcours. Les cases sont placées dans la scène.
    [SerializeField] private List<Case> tiles = new List<Case>();

    // Le pion est légèrement au-dessus du centre de la case pour ne pas la traverser.
    [SerializeField, Min(0f)] private float hauteurPion = 0.52f;

    /// <summary>Nombre de cases réellement assignées à ce plateau.</summary>
    public int NombreCases => tiles != null ? tiles.Count : 0;

    /// <summary>Renvoie une case par son index, ou null si l'index n'existe pas.</summary>
    public Case ObtenirCase(int index)
    {
        if (tiles == null || index < 0 || index >= tiles.Count)
            return null;

        return tiles[index];
    }

    /// <summary>Renvoie la position au-dessus de la case utilisée par le pion.</summary>
    public Vector3 PositionCase(int index)
    {
        Case caseCible = ObtenirCase(index);
        if (caseCible == null)
            return transform.position;

        return caseCible.transform.position + Vector3.up * hauteurPion;
    }
}
