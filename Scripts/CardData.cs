using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCardData", menuName = "Cards/CardData")]
public class CardData : ScriptableObject
{
    public Sprite icon;
    public Animals animal;
    public AnimalShape shape;
    public uint[] scores;
};