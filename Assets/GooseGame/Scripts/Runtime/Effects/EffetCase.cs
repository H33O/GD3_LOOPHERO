using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Jeu de l'oie/Effet de case")]
public sealed class EffetCase : ScriptableObject
{
    [SerializeField, FormerlySerializedAs("effectName"), FormerlySerializedAs("nom")] private string nom;
    [SerializeField, FormerlySerializedAs("relativeOffset"), FormerlySerializedAs("deplacement")] private int deplacement;

    public string Nom => nom;
    public int Deplacement => deplacement;
}
