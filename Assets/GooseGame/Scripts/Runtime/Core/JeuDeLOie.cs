using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public sealed class JeuDeLOie : MonoBehaviour
{
    [SerializeField] private Plateau plateau;
    [SerializeField] private De de;
    [SerializeField] private Transform pion;
    [SerializeField] private Button boutonLancer;
    [SerializeField] private Button boutonRejouer;
    [SerializeField] private Text texteResultat;
    [SerializeField] private Text texteMessage;
    [SerializeField] private Text texteCase;
    [SerializeField] private GameObject panneauVictoire;
    [SerializeField, Min(0.01f)] private float dureeParCase = 0.16f;
    [SerializeField, Min(0f)] private float pauseArrivee = 0.3f;
    [SerializeField, Min(0f)] private float pauseResultat = 0.5f;
    [SerializeField] private string messageAttente = "À vous de jouer.";
    [SerializeField] private string messageEffet = "Case spéciale : {0}";
    [SerializeField] private string messageVictoire = "Vous avez gagné !";
    private int indexActuel;
    private bool tourEnCours;

    // Branche les boutons et initialise les affichages avant de jouer.
    private void OnEnable()
    {
        tourEnCours = false;
        if (boutonLancer != null) { boutonLancer.onClick.RemoveListener(LancerDe); boutonLancer.onClick.AddListener(LancerDe); boutonLancer.interactable = true; }
        if (boutonRejouer != null) { boutonRejouer.onClick.RemoveListener(Rejouer); boutonRejouer.onClick.AddListener(Rejouer); boutonRejouer.gameObject.SetActive(false); }
        if (panneauVictoire != null) panneauVictoire.SetActive(false);
        indexActuel = 0;
        PlacerPion();
        if (texteMessage != null) texteMessage.text = messageAttente;
        if (texteResultat != null) texteResultat.text = "Résultat : —";
    }

    // Détache les clics des boutons lorsque le contrôleur est désactivé.
    private void OnDisable()
    {
        if (boutonLancer != null) boutonLancer.onClick.RemoveListener(LancerDe);
        if (boutonRejouer != null) boutonRejouer.onClick.RemoveListener(Rejouer);
    }

    // Démarre un tour si la scène contient toutes les références requises.
    public void LancerDe()
    {
        if (tourEnCours || plateau == null || plateau.NombreCases < 2 || de == null || pion == null || boutonLancer == null) return;
        StartCoroutine(JouerTour());
    }

    // Replace le pion au départ et permet de rejouer sans recharger la scène.
    public void Rejouer()
    {
        if (tourEnCours || plateau == null || plateau.NombreCases == 0) return;
        indexActuel = 0;
        if (panneauVictoire != null) panneauVictoire.SetActive(false);
        if (boutonRejouer != null) boutonRejouer.gameObject.SetActive(false);
        if (boutonLancer != null) boutonLancer.interactable = true;
        if (texteResultat != null) texteResultat.text = "Résultat : —";
        if (texteMessage != null) texteMessage.text = messageAttente;
        PlacerPion();
    }

    // Exécute le tour complet avec un seul effet de case au maximum.
    private IEnumerator JouerTour()
    {
        tourEnCours = true;
        boutonLancer.interactable = false;
        if (texteMessage != null) texteMessage.text = "Le dé roule…";
        yield return de.Lancer();
        int resultat = de.DerniereValeur;
        if (resultat <= 0) { FinirTour(false); yield break; }
        if (texteResultat != null) texteResultat.text = "Résultat : " + resultat;
        if (pauseResultat > 0f) yield return new WaitForSeconds(pauseResultat);
        yield return DeplacerPion(resultat);
        if (pauseArrivee > 0f) yield return new WaitForSeconds(pauseArrivee);
        if (indexActuel < plateau.NombreCases - 1) yield return AppliquerEffet();
        FinirTour(indexActuel >= plateau.NombreCases - 1);
    }

    // Met à jour la victoire, le panneau et l'état des boutons.
    private void FinirTour(bool victoire)
    {
        if (texteMessage != null) texteMessage.text = victoire ? messageVictoire : messageAttente;
        if (panneauVictoire != null) panneauVictoire.SetActive(victoire);
        if (boutonRejouer != null) boutonRejouer.gameObject.SetActive(victoire);
        if (boutonLancer != null) boutonLancer.interactable = !victoire;
        tourEnCours = false;
    }

    // Déplace le pion case par case et le garde dans les limites du plateau.
    private IEnumerator DeplacerPion(int nombreDeCases)
    {
        int sens = nombreDeCases >= 0 ? 1 : -1;
        for (int i = 0; i < Mathf.Abs(nombreDeCases); i++)
        {
            int prochainIndex = Mathf.Clamp(indexActuel + sens, 0, plateau.NombreCases - 1);
            if (prochainIndex == indexActuel) yield break;
            Vector3 depart = pion.position;
            Vector3 arrivee = plateau.PositionCase(prochainIndex);
            float temps = 0f;
            float duree = Mathf.Max(0.01f, dureeParCase);
            while (temps < duree)
            {
                temps += Time.deltaTime;
                pion.position = Vector3.Lerp(depart, arrivee, Mathf.Clamp01(temps / duree));
                yield return null;
            }
            indexActuel = prochainIndex;
            pion.position = arrivee;
            if (texteCase != null) texteCase.text = "Case : " + indexActuel;
            if (indexActuel == plateau.NombreCases - 1) yield break;
        }
    }

    // Applique une fois l'effet configuré sur la case d'arrivée.
    private IEnumerator AppliquerEffet()
    {
        Case arrivee = plateau.ObtenirCase(indexActuel);
        if (arrivee == null || arrivee.Effet == null) yield break;
        if (texteMessage != null) texteMessage.text = string.Format(messageEffet, arrivee.Effet.Nom);
        yield return DeplacerPion(arrivee.Effet.Deplacement);
    }

    // Place le pion sur sa case courante et met à jour le compteur.
    private void PlacerPion()
    {
        if (plateau == null || pion == null || plateau.NombreCases == 0) return;
        indexActuel = Mathf.Clamp(indexActuel, 0, plateau.NombreCases - 1);
        pion.position = plateau.PositionCase(indexActuel);
        if (texteCase != null) texteCase.text = "Case : " + indexActuel;
    }
}