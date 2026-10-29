
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
        var gm = GameManager.Instance;

        int round = gm != null ? gm.roundNumber : 1;
        int step = gm != null ? gm.actionStep : 0;
        int totalSteps = gm != null ? gm.totalActionStep : 0;

        statsText.text =
            $"Round: {round} | Step: {step}/{totalSteps} | " +
            $"Level: {player.level} | " +
            $"EXP: {player.experiencePoints} | " +
            $"HP: {stats.health}/{stats.maxHealth} | " +
            $"STR: {stats.strength} | " +
            $"AGI: {stats.agility} | " +
            $"FOR: {stats.fortitude} | " +
            $"Faith: {(float)stats.faith / 10}/1";
    }
}
