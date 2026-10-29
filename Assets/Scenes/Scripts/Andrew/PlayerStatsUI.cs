
using TMPro;
using UnityEngine;


public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private TMP_Text statsText;

    void Update()
    {
        if (player == null || statsText == null) return;

        var stats = player.totalStats;

        statsText.text =
            $"Level: {player.level} | " +
            $"EXP: {player.experiencePoints} | " +
            $"HP: {stats.health}/{stats.maxHealth} | " +
            $"STR: {stats.strength} | " +
            $"AGI: {stats.agility} | " +
            $"FOR: {stats.fortitude} | " +
            $"Faith: {(float)stats.faith / 10}/1";
    }
}

