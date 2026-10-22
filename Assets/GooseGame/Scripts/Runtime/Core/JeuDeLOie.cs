using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fait avancer le jeu : lancer le dé, déplacer le pion, appliquer une case spéciale et vérifier la victoire.
/// </summary>
public sealed class JeuDeLOie : MonoBehaviour
{
    [Header("Objets du jeu")]
    [SerializeField] private Plateau plateau;
    [SerializeField] private De de;
    [SerializeField] private Transform pion;

    [Header("Interface")]
    [SerializeField] private Button boutonLancer;
    [SerializeField] private Button boutonRejouer;
    [SerializeField] private Text texteResultat;
    [SerializeField] private Text texteMessage;
    [SerializeField] private Text texteCase;
    [SerializeField] private GameObject panneauVictoire;

    [Header("Rythme du tour")]
    [SerializeField, Min(0.01f)] private float dureeParCase = 0.16f;
    [SerializeField, Min(0f)] private float pauseArrivee = 0.3f;
    [SerializeField, Min(0f)] private float pauseResultat = 0.5f;

    [Header("Messages affichés")]
    [SerializeField] private string messageAttente = "À vous de jouer.";
    [SerializeField] private string messageEffet = "Case spéciale : {0}";
    [SerializeField] private string messageVictoire = "Vous avez gagné !";

    // L'index commence à zéro : la première case de la liste est le départ.
    private int indexActuel;
    private bool tourEnCours;

    /// <summary>Prépare le plateau et relie les boutons à leurs méthodes.</summary>
    private void OnEnable()
    {
        tourEnCours = false;
        indexActuel = 0;

        if (boutonLancer != null)
        {
            boutonLancer.onClick.RemoveListener(LancerDe);
            boutonLancer.onClick.AddListener(LancerDe);
            boutonLancer.interactable = true;
        }

        if (boutonRejouer != null)
        {
            boutonRejouer.onClick.RemoveListener(Rejouer);
            boutonRejouer.onClick.AddListener(Rejouer);
            boutonRejouer.gameObject.SetActive(false);
        }

        if (panneauVictoire != null)
            panneauVictoire.SetActive(false);

        PlacerPion();
        if (texteMessage != null) texteMessage.text = messageAttente;
        if (texteResultat != null) texteResultat.text = "Résultat : —";
    }

    /// <summary>Retire les listeners pour éviter les abonnements en double.</summary>
    private void OnDisable()
    {
        if (boutonLancer != null) boutonLancer.onClick.RemoveListener(LancerDe);
        if (boutonRejouer != null) boutonRejouer.onClick.RemoveListener(Rejouer);
    }

    /// <summary>Commence un tour si les objets indispensables sont bien assignés.</summary>
    public void LancerDe()
    {
        if (tourEnCours || plateau == null || plateau.NombreCases < 2 || de == null || pion == null)
            return;

        StartCoroutine(JouerTour());
    }

    /// <summary>Remet le pion au départ pour permettre une nouvelle partie.</summary>
    public void Rejouer()
    {
        if (tourEnCours || plateau == null || plateau.NombreCases == 0)
            return;

        indexActuel = 0;
        if (panneauVictoire != null) panneauVictoire.SetActive(false);
        if (boutonRejouer != null) boutonRejouer.gameObject.SetActive(false);
        if (boutonLancer != null) boutonLancer.interactable = true;
        if (texteResultat != null) texteResultat.text = "Résultat : —";
        if (texteMessage != null) texteMessage.text = messageAttente;
        PlacerPion();
    }

    // Un seul tour : lancer, avancer, appliquer au plus un effet, puis vérifier la victoire.
    private IEnumerator JouerTour()
    {
        tourEnCours = true;
        if (boutonLancer != null) boutonLancer.interactable = false;
        if (texteMessage != null) texteMessage.text = "Le dé roule…";

        yield return de.Lancer();
        int resultat = de.DerniereValeur;
        if (resultat <= 0)
        {
            TerminerTour(false);
            yield break;
        }

        if (texteResultat != null) texteResultat.text = "Résultat : " + resultat;
        if (pauseResultat > 0f) yield return new WaitForSeconds(pauseResultat);

        yield return DeplacerPion(resultat);
        if (pauseArrivee > 0f) yield return new WaitForSeconds(pauseArrivee);

        // On n'applique pas d'effet supplémentaire si le pion a déjà gagné.
        if (indexActuel < plateau.NombreCases - 1)
            yield return AppliquerEffet();

        TerminerTour(indexActuel >= plateau.NombreCases - 1);
    }

    // Une valeur positive avance, une valeur négative fait reculer.
    private IEnumerator DeplacerPion(int nombreDeCases)
    {
        int direction = nombreDeCases >= 0 ? 1 : -1;
        int nombreDePas = Mathf.Abs(nombreDeCases);

        for (int pas = 0; pas < nombreDePas; pas++)
        {
            int prochainIndex = Mathf.Clamp(indexActuel + direction, 0, plateau.NombreCases - 1);
            if (prochainIndex == indexActuel)
                yield break;

            Vector3 positionDepart = pion.position;
            Vector3 positionArrivee = plateau.PositionCase(prochainIndex);
            float tempsEcoule = 0f;
            float duree = Mathf.Max(0.01f, dureeParCase);

            while (tempsEcoule < duree)
            {
                tempsEcoule += Time.deltaTime;
                float progression = Mathf.Clamp01(tempsEcoule / duree);
                pion.position = Vector3.Lerp(positionDepart, positionArrivee, progression);
                yield return null;
            }

            indexActuel = prochainIndex;
            pion.position = positionArrivee;
            if (texteCase != null) texteCase.text = "Case : " + (indexActuel + 1);

            if (indexActuel == plateau.NombreCases - 1)
                yield break;
        }
    }

    // L'effet est une donnée simple : son nom et son déplacement viennent de l'asset de la case.
    private IEnumerator AppliquerEffet()
    {
        Case caseArrivee = plateau.ObtenirCase(indexActuel);
        if (caseArrivee == null || caseArrivee.Effet == null)
            yield break;

        if (texteMessage != null)
            texteMessage.text = string.Format(messageEffet, caseArrivee.Effet.Nom);

        yield return DeplacerPion(caseArrivee.Effet.Deplacement);
    }

    /// <summary>Replace le pion sur la case actuellement sélectionnée.</summary>
    private void PlacerPion()
    {
        if (plateau == null || pion == null || plateau.NombreCases == 0)
            return;

        indexActuel = Mathf.Clamp(indexActuel, 0, plateau.NombreCases - 1);
        pion.position = plateau.PositionCase(indexActuel);
        if (texteCase != null) texteCase.text = "Case : " + (indexActuel + 1);
    }

    // Réactive les commandes si la partie n'est pas terminée.
    private void TerminerTour(bool victoire)
    {
        if (texteMessage != null)
            texteMessage.text = victoire ? messageVictoire : messageAttente;
        if (panneauVictoire != null) panneauVictoire.SetActive(victoire);
        if (boutonRejouer != null) boutonRejouer.gameObject.SetActive(victoire);
        if (boutonLancer != null) boutonLancer.interactable = !victoire;
        tourEnCours = false;
    }
}
