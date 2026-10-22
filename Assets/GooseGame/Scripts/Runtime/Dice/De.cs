using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Tire une face au hasard et anime le modèle du dé jusqu'à cette face.
/// </summary>
public sealed class De : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("diceModel")] private Transform modele;
    [SerializeField] private List<int> valeurs = new List<int> { 1, 2, 3, 4, 5, 6 };
    [SerializeField] private List<Vector3> rotations = new List<Vector3>
    {
        Vector3.zero,
        new Vector3(0f, 0f, 90f),
        new Vector3(90f, 0f, 0f),
        new Vector3(-90f, 0f, 0f),
        new Vector3(0f, 0f, -90f),
        new Vector3(180f, 0f, 0f)
    };
    [SerializeField, Min(0.01f)] private float dureeLancer = 1f;
    [SerializeField, Min(1)] private int nombreDeRotations = 3;
    [SerializeField] private AnimationCurve courbe = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    /// <summary>Valeur du dernier lancer.</summary>
    public int DerniereValeur { get; private set; }

    /// <summary>Choisit une valeur et fait tourner le modèle avant de montrer le résultat.</summary>
    public IEnumerator Lancer()
    {
        // Évite de réutiliser par erreur le résultat du lancer précédent si le dé est mal configuré.
        DerniereValeur = 0;

        if (valeurs == null || valeurs.Count == 0)
        {
            Debug.LogError("Ajoutez au moins une valeur dans la liste du dé.", this);
            yield break;
        }

        // Par défaut, le premier enfant est le modèle visuel du dé.
        if (modele == null && transform.childCount > 0)
            modele = transform.GetChild(0);

        if (modele == null)
        {
            Debug.LogError("Assignez le modèle visuel du dé.", this);
            yield break;
        }

        int indexFace = Random.Range(0, valeurs.Count);
        DerniereValeur = valeurs[indexFace];

        Vector3 rotationFinale = rotations != null && rotations.Count > indexFace
            ? rotations[indexFace]
            : Vector3.zero;
        Quaternion cible = Quaternion.Euler(rotationFinale);
        Quaternion depart = modele.localRotation;
        Vector3 axeRotation = new Vector3(1f, 1f, 0.7f).normalized;
        float angleTotal = Mathf.Max(1, nombreDeRotations) * 360f;
        Quaternion fin = cible * Quaternion.Inverse(Quaternion.AngleAxis(angleTotal, axeRotation));
        float tempsEcoule = 0f;
        float duree = Mathf.Max(0.01f, dureeLancer);

        // La courbe rend le mouvement moins mécanique sans changer le résultat tiré.
        while (tempsEcoule < duree)
        {
            tempsEcoule += Time.deltaTime;
            float progression = Mathf.Clamp01(tempsEcoule / duree);
            float progressionLisse = courbe != null
                ? Mathf.Clamp01(courbe.Evaluate(progression))
                : progression;

            modele.localRotation = Quaternion.Slerp(depart, fin, progressionLisse)
                * Quaternion.AngleAxis(angleTotal * progressionLisse, axeRotation);
            yield return null;
        }

        modele.localRotation = cible;
    }
}
