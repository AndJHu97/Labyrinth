using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EmotionKey
{
    Anger,
    Sadness,
    Joy,
    Fear
}

public enum ConcentrationKey
{
    Right,
    Left,
    Up,
    Down,
    Straight_Ahead,
    Self
}

public enum MotorSensorKey
{
    Arm,
    Leg,
    Gut,
    Voice,
    Observe,
    Manipulate
}

[CreateAssetMenu(fileName = "New Player Intent", menuName = "Interactions/Player Intent")]
public class PlayerIntentSO : ScriptableObject
{
    public string name_;
    public string defaultNoninteractableText;
    public int netHealthImpact;
    public int netSelfHealthImpact;

    public EmotionKey emotionKey;
    public MotorSensorKey motorSensorKey;
}
