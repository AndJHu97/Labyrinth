using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BattleMechanics 
{
    public static int CalculateDamage(int attackerStrength, int defenderFortitude, int agility, string npcName, bool isPlayerAttacking)
    {
        float hitChance = CalculateHitChance(agility);
        float randomValue = Random.Range(0f, 100f);

        if (randomValue > hitChance)
        {
            Debug.Log(isPlayerAttacking ? "Player missed!" : "NPC missed!");
            GameManager.Instance.LogDisplayTextsToRecentLog(new List<string> { isPlayerAttacking ? "You missed!" : npcName + " missed!" });
            return 0; // Missed attack
        }
        else
        {
            int damage = System.Math.Max(0, attackerStrength - defenderFortitude);
            Debug.Log(isPlayerAttacking ? "You attacked " + npcName + " for " + damage : npcName + " attacked you for " + damage);
            
            GameManager.Instance.LogDisplayTextsToRecentLog(new List<string> { isPlayerAttacking ? "You attacked " + npcName + " for " + damage : npcName + " attacked you for " + damage });

            return damage;
        }
    }

    private static float CalculateHitChance(int agility)
    {
        return 100f * (1f - 0.7f * Mathf.Exp(-0.1f * agility));
    }

}
