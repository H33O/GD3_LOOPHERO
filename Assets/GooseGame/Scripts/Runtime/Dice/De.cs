using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public sealed class De : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("diceModel")] private Transform modele;
    [SerializeField] private List<int> valeurs = new List<int> { 1, 2, 3, 4, 5, 6 };
    [SerializeField] private List<Vector3> rotations = new List<Vector3> { Vector3.zero, new Vector3(0f, 0f, 90f), new Vector3(90f, 0f, 0f), new Vector3(-90f, 0f, 0f), new Vector3(0f, 0f, -90f), new Vector3(180f, 0f, 0f) };
    [SerializeField, Min(0.01f)] private float dureeLancer = 1f;
    [SerializeField, Min(1)] private int nombreDeRotations = 3;
    [SerializeField] private AnimationCurve courbe = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public int DerniereValeur { get; private set; }

    // Anime le dé, tire un résultat et rétablit l'orientation de la face choisie.
    public IEnumerator Lancer()
    {
        if (valeurs == null || valeurs.Count == 0) yield break;
        if (modele == null && transform.childCount > 0) modele = transform.GetChild(0);
        if (modele == null) { Debug.LogError("Assignez le modèle visuel du dé.", this); yield break; }
        int face = Random.Range(0, valeurs.Count);
        DerniereValeur = valeurs[face];
        Quaternion cible = Quaternion.Euler(rotations != null && rotations.Count > face ? rotations[face] : Vector3.zero);
        Quaternion depart = modele.localRotation;
        Vector3 axe = new Vector3(1f, 1f, 0.7f).normalized;
        float angle = Mathf.Max(1, nombreDeRotations) * 360f;
        Quaternion fin = cible * Quaternion.Inverse(Quaternion.AngleAxis(angle, axe));
        float temps = 0f, duree = Mathf.Max(0.01f, dureeLancer);
        while (temps < duree)
        {
            temps += Time.deltaTime;
            float t = Mathf.Clamp01(temps / duree);
            float lisse = courbe != null ? Mathf.Clamp01(courbe.Evaluate(t)) : t;
            modele.localRotation = Quaternion.Slerp(depart, fin, lisse) * Quaternion.AngleAxis(angle * lisse, axe);
            yield return null;
        }
        modele.localRotation = cible;
    }
}
