using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BattleMechanics 
{
    public static int CalculateDamage(int attackerStrength, int defenderFortitude)
    {
        return System.Math.Max(0, attackerStrength - defenderFortitude);
    }

    public static float CalculateHitChance(int agility)
    {
        return 100f * (1f - 0.7f * Mathf.Exp(-0.1f * agility));
    }
}
