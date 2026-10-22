using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Suit le pion et fait orbiter la caméra à une distance réelle et réglable.
/// </summary>
public sealed class CameraOrbitPion : MonoBehaviour
{
    [Header("Cible")]
    // Le pion à suivre. La caméra reste volontairement indépendante de son échelle.
    [SerializeField] private Transform cible;
    // La caméra vise un point légèrement au-dessus du pivot du pion.
    [SerializeField, Min(0f)] private float hauteurCible = 0.8f;

    [Header("Vitesse de rotation")]
    // Vitesse de rotation des touches, en degrés par seconde.
    [SerializeField, Min(0f)] private float vitesseRotation = 90f;

    [Header("Limites d'inclinaison")]
    // Empêche la caméra de passer sous le plateau ou directement à la verticale.
    [SerializeField, Range(5f, 85f)] private float inclinaisonMinimum = 30f;
    [SerializeField, Range(5f, 85f)] private float inclinaisonMaximum = 70f;

    [Header("Distance de caméra")]
    // Distance en unités monde : contrairement à l'ancienne version, elle n'est pas multipliée par l'échelle du pion.
    [SerializeField, Min(0.1f)] private float distanceCamera = 18f;

    // Angles conservés d'une image à l'autre pour obtenir une rotation continue.
    private float angleHorizontal;
    private float inclinaison;

    private void Awake()
    {
        if (cible == null && transform.parent != null)
            cible = transform.parent;

        if (cible == null)
        {
            Debug.LogError("Assigne le pion dans le champ Cible de la caméra.", this);
            enabled = false;
            return;
        }

        // Calcule les angles à partir du cadrage réel dans la scène.
        Vector3 pointVise = cible.position + Vector3.up * hauteurCible;
        Vector3 positionRelative = transform.position - pointVise;
        distanceCamera = Mathf.Max(0.1f, distanceCamera);
        angleHorizontal = Mathf.Atan2(positionRelative.x, positionRelative.z) * Mathf.Rad2Deg;
        float distanceHorizontale = new Vector2(positionRelative.x, positionRelative.z).magnitude;
        inclinaison = Mathf.Atan2(positionRelative.y, distanceHorizontale) * Mathf.Rad2Deg;
        inclinaison = Mathf.Clamp(inclinaison, inclinaisonMinimum, inclinaisonMaximum);

        AppliquerPositionCamera();
    }

    private void LateUpdate()
    {
        if (cible == null)
            return;

        LireClavier();
        AppliquerPositionCamera();
    }

    // Les lettres AZERTY sont lues selon leurs touches physiques dans le nouveau Input System.
    // Q/D tournent autour du pion; Z/S règlent l'inclinaison.
    private void LireClavier()
    {
        Keyboard clavier = Keyboard.current;
        if (clavier == null)
            return;

        float mouvementHorizontal = 0f;
        if (clavier.aKey.isPressed) mouvementHorizontal += 1f; // Q sur AZERTY : sens inversé demandé.
        if (clavier.dKey.isPressed) mouvementHorizontal -= 1f; // D sur AZERTY : sens inversé demandé.

        float mouvementVertical = 0f;
        if (clavier.wKey.isPressed) mouvementVertical += 1f; // Z sur un clavier AZERTY.
        if (clavier.sKey.isPressed) mouvementVertical -= 1f; // S sur un clavier AZERTY.

        float changementAngle = vitesseRotation * Time.deltaTime;
        angleHorizontal += mouvementHorizontal * changementAngle;
        inclinaison += mouvementVertical * changementAngle;
        inclinaison = Mathf.Clamp(inclinaison, inclinaisonMinimum, inclinaisonMaximum);
    }

    // Positionne la caméra à une distance monde constante puis la fait regarder le pion.
    private void AppliquerPositionCamera()
    {
        Vector3 pointVise = cible.position + Vector3.up * hauteurCible;
        float angleHorizontalRadians = angleHorizontal * Mathf.Deg2Rad;
        float inclinaisonRadians = inclinaison * Mathf.Deg2Rad;

        Vector3 direction = new Vector3(
            Mathf.Sin(angleHorizontalRadians) * Mathf.Cos(inclinaisonRadians),
            Mathf.Sin(inclinaisonRadians),
            Mathf.Cos(angleHorizontalRadians) * Mathf.Cos(inclinaisonRadians));

        transform.position = pointVise + direction * distanceCamera;
        transform.rotation = Quaternion.LookRotation(pointVise - transform.position, Vector3.up);
    }

    // Garde les limites et la distance valides quand elles sont changées dans l'Inspector.
    private void OnValidate()
    {
        inclinaisonMaximum = Mathf.Max(inclinaisonMinimum, inclinaisonMaximum);
        distanceCamera = Mathf.Max(0.1f, distanceCamera);
        hauteurCible = Mathf.Max(0f, hauteurCible);
    }
}
