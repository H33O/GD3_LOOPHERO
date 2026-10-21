using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public sealed class Plateau : MonoBehaviour
{
    [SerializeField, Min(2)] private int nombreCases = 50;
    [SerializeField] private List<Case> tiles = new List<Case>();
    [SerializeField] private GameObject prefabCase;
    [SerializeField] private Vector3 ecartCases = new Vector3(1.5f, 0f, 0f);
    [SerializeField, Min(0f)] private float hauteurPion = 0.52f;
    public int NombreCases => tiles != null ? tiles.Count : 0;

    // Retourne la case demandée, ou null si l'index est invalide.
    public Case ObtenirCase(int index) => tiles != null && index >= 0 && index < tiles.Count ? tiles[index] : null;

    // Retourne la position du pion au-dessus de la case demandée.
    public Vector3 PositionCase(int index)
    {
        Case cible = ObtenirCase(index);
        return cible != null ? cible.transform.position + Vector3.up * hauteurPion : transform.position;
    }

    // Génère ou redimensionne le parcours uniquement dans l'éditeur.
    [ContextMenu("Générer le plateau")]
    public void GenererPlateau()
    {
#if UNITY_EDITOR
        if (Application.isPlaying) return;
        if (tiles == null) tiles = new List<Case>();
        int cible = Mathf.Max(2, nombreCases);
        if (cible > tiles.Count && prefabCase == null) { Debug.LogError("Assignez le prefab de case.", this); return; }
        if (cible < tiles.Count && !EditorUtility.DisplayDialog("Réduire le plateau", "Les cases retirées et leurs effets seront supprimés.", "Supprimer", "Annuler")) return;
        Undo.RecordObject(this, "Redimensionner le plateau");
        while (tiles.Count > cible) { Case derniere = tiles[tiles.Count - 1]; tiles.RemoveAt(tiles.Count - 1); if (derniere != null) Undo.DestroyObjectImmediate(derniere.gameObject); }
        while (tiles.Count < cible)
        {
            GameObject objet = PrefabUtility.InstantiatePrefab(prefabCase, transform) as GameObject;
            if (objet == null) break;
            Case nouvelle = objet.GetComponent<Case>();
            if (nouvelle == null) { Undo.DestroyObjectImmediate(objet); break; }
            objet.transform.position = tiles.Count > 0 && tiles[tiles.Count - 1] != null ? tiles[tiles.Count - 1].transform.position + ecartCases : transform.position + ecartCases * tiles.Count;
            Undo.RegisterCreatedObjectUndo(objet, "Créer une case");
            tiles.Add(nouvelle);
        }
        nombreCases = tiles.Count;
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    // Dessine le chemin et l'ordre des cases dans la scène.
    private void OnDrawGizmos()
    {
        if (tiles == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] == null) continue;
            Vector3 position = tiles[i].transform.position;
            Gizmos.DrawSphere(position, 0.12f);
            if (i + 1 < tiles.Count && tiles[i + 1] != null) Gizmos.DrawLine(position, tiles[i + 1].transform.position);
        }
    }
}
