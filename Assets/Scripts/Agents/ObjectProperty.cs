using System;
using UnityEngine;

// One entry on an InteractableObject — what need it fills, how well, and how long it takes.
// A fridge can have two of these: one for "food" and one for "drinkable".
[Serializable]
public class ObjectProperty
{
    public string Tag;               // "drinkable", "food", "sleepable", "toilet"
    public NeedType NeedFulfilled;
    public float FulfillmentAmount;  // 0-1
    public float UseDuration;        // real seconds
    public int Quality;              // higher = preferred (bed=10, couch=5, tap=3, fountain=2)
}
