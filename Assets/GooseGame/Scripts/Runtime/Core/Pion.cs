using System.Collections;
using UnityEngine;

/// <summary>
/// Déplace le pion entre deux positions sans connaître les cases ni les règles du jeu.
/// </summary>
public sealed class Pion : MonoBehaviour
{
    [SerializeField, Min(0f)] private float hauteurPion = 0.52f;
    [SerializeField, Min(0.01f)] private float dureeDeplacement = 0.4f;

    /// <summary>Démarre le déplacement lissé du pion vers une position de case.</summary>
    public Coroutine AllerVers(Vector3 position)
    {
        return StartCoroutine(DeplacerVers(position + Vector3.up * hauteurPion));
    }

    /// <summary>Arrête les déplacements en cours et place immédiatement le pion sur une case.</summary>
    public void Teleporter(Vector3 position)
    {
        StopAllCoroutines();
        transform.position = position + Vector3.up * hauteurPion;
    }

    private IEnumerator DeplacerVers(Vector3 positionCible)
    {
        Vector3 positionDepart = transform.position;
        float duree = Mathf.Max(0.01f, dureeDeplacement);
        float tempsEcoule = 0f;

        while (tempsEcoule < duree)
        {
            tempsEcoule += Time.deltaTime;
            float progression = Mathf.Clamp01(tempsEcoule / duree);
            float progressionLisse = Mathf.SmoothStep(0f, 1f, progression);
            transform.position = Vector3.Lerp(positionDepart, positionCible, progressionLisse);
            yield return null;
        }

        transform.position = positionCible;
    }
}
