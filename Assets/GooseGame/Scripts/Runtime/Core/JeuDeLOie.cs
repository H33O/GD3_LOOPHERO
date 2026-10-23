using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Orchestre les tours, le déplacement sur le parcours et la fin de partie.
/// </summary>
public sealed class JeuDeLOie : MonoBehaviour
{
    private const string TexteResultatInitial = "Résultat : —";
    private const string PrefixeResultat = "Résultat : ";
    private const string PrefixeCase = "Case : ";
    private const string TexteVictoire = "Victoire !";
    private const string TexteDeplacement = "Le pion avance…";

    [Header("Objets du jeu")]
    [SerializeField] private List<Case> cases = new List<Case>();
    [SerializeField, FormerlySerializedAs("pionComposant")] private Pion pion;
    [SerializeField] private CompteurVictoires compteurVictoires;

    [Header("Interface")]
    [SerializeField] private Button boutonLancer;
    [SerializeField] private Button boutonRejouer;
    [SerializeField] private Text texteResultat;
    [SerializeField] private Text texteMessage;
    [SerializeField] private Text texteCase;
    [SerializeField] private Text texteCompteurVictoires;
    [SerializeField] private GameObject panneauVictoire;

    [Header("Règles du tour")]
    [SerializeField, Min(1)] private int valeurMin = 1;
    [SerializeField, Min(1)] private int valeurMax = 6;
    [SerializeField, Min(0f)] private float pauseAvantEffet = 0.5f;
    [SerializeField] private string messageAttente = "À vous de jouer.";

    private int indexActuel;
    private bool tourEnCours;

    /// <summary>Valide les références, relie les boutons et prépare une nouvelle partie.</summary>
    private void OnEnable()
    {
        if (!ReferencesValides())
        {
            enabled = false;
            return;
        }

        boutonLancer.onClick.RemoveListener(LancerDe);
        boutonLancer.onClick.AddListener(LancerDe);
        boutonRejouer.onClick.RemoveListener(Rejouer);
        boutonRejouer.onClick.AddListener(Rejouer);
        Rejouer();
    }

    /// <summary>Retire les listeners pour éviter les abonnements en double.</summary>
    private void OnDisable()
    {
        if (boutonLancer != null)
            boutonLancer.onClick.RemoveListener(LancerDe);
        if (boutonRejouer != null)
            boutonRejouer.onClick.RemoveListener(Rejouer);
    }

    /// <summary>Commence un tour en tirant un entier dans l'intervalle configuré.</summary>
    public void LancerDe()
    {
        if (tourEnCours || !ReferencesValides())
            return;

        StartCoroutine(JouerTour());
    }

    /// <summary>Arrête les déplacements en cours et replace le pion au départ sans effacer les victoires.</summary>
    public void Rejouer()
    {
        if (!ReferencesValides())
            return;

        StopAllCoroutines();
        tourEnCours = false;
        indexActuel = 0;
        pion.Teleporter(cases[indexActuel].transform.position);
        panneauVictoire.SetActive(false);
        boutonRejouer.gameObject.SetActive(false);
        boutonLancer.interactable = true;
        texteResultat.text = TexteResultatInitial;
        texteMessage.text = messageAttente;
        MettreAJourTexteCase();
        ActualiserCompteurVictoires();
    }

    private bool ReferencesValides()
    {
        if (pion == null)
            return SignalerReferenceManquante("pion");
        if (compteurVictoires == null)
            return SignalerReferenceManquante("compteurVictoires");
        if (boutonLancer == null)
            return SignalerReferenceManquante("boutonLancer");
        if (boutonRejouer == null)
            return SignalerReferenceManquante("boutonRejouer");
        if (texteResultat == null)
            return SignalerReferenceManquante("texteResultat");
        if (texteMessage == null)
            return SignalerReferenceManquante("texteMessage");
        if (texteCase == null)
            return SignalerReferenceManquante("texteCase");
        if (texteCompteurVictoires == null)
            return SignalerReferenceManquante("texteCompteurVictoires");
        if (panneauVictoire == null)
            return SignalerReferenceManquante("panneauVictoire");
        if (cases == null || cases.Count == 0)
            return SignalerReferenceManquante("cases (la liste est vide)");
        if (valeurMin < 1 || valeurMax < valeurMin)
        {
            Debug.LogError("JeuDeLOie : l'intervalle du tirage doit respecter 1 <= valeurMin <= valeurMax.", this);
            return false;
        }

        for (int index = 0; index < cases.Count; index++)
        {
            if (cases[index] == null)
            {
                Debug.LogError("JeuDeLOie : la case à l'index " + index + " est nulle.", this);
                return false;
            }
        }

        return true;
    }

    private bool SignalerReferenceManquante(string nomReference)
    {
        Debug.LogError("JeuDeLOie : la référence requise " + nomReference + " n'est pas assignée.", this);
        return false;
    }

    private IEnumerator JouerTour()
    {
        tourEnCours = true;
        boutonLancer.interactable = false;
        texteMessage.text = TexteDeplacement;

        int resultat = Random.Range(valeurMin, valeurMax + 1);
        texteResultat.text = PrefixeResultat + resultat;
        indexActuel = Mathf.Min(indexActuel + resultat, cases.Count - 1);
        yield return PlacerPion();

        if (pauseAvantEffet > 0f)
            yield return new WaitForSeconds(pauseAvantEffet);

        int indexApresEffet = Mathf.Clamp(indexActuel + cases[indexActuel].Deplacement, 0, cases.Count - 1);
        if (indexApresEffet != indexActuel)
        {
            indexActuel = indexApresEffet;
            yield return PlacerPion();
        }

        FinDeTour();
    }

    private Coroutine PlacerPion()
    {
        MettreAJourTexteCase();
        return pion.AllerVers(cases[indexActuel].transform.position);
    }

    private void MettreAJourTexteCase()
    {
        texteCase.text = PrefixeCase + (indexActuel + 1);
    }

    private void ActualiserCompteurVictoires()
    {
        texteCompteurVictoires.text = "Victoires : " + compteurVictoires.TotalVictoires;
    }

    private void FinDeTour()
    {
        bool victoire = indexActuel >= cases.Count - 1;
        if (victoire)
        {
            compteurVictoires.Incrementer();
            ActualiserCompteurVictoires();
            texteMessage.text = TexteVictoire;
            panneauVictoire.SetActive(true);
            boutonRejouer.gameObject.SetActive(true);
            boutonLancer.interactable = false;
            tourEnCours = true;
            return;
        }

        texteMessage.text = messageAttente;
        panneauVictoire.SetActive(false);
        boutonRejouer.gameObject.SetActive(false);
        boutonLancer.interactable = true;
        tourEnCours = false;
    }
}
