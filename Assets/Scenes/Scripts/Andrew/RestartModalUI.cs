using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RestartModalUI : MonoBehaviour
{
    [SerializeField] private GameObject modal;
    [SerializeField] private TextMeshProUGUI strengthStat;
    [SerializeField] private TextMeshProUGUI fortitudeStat;
    [SerializeField] private TextMeshProUGUI agilityStat;
    [SerializeField] private TextMeshProUGUI faithStat;
    [SerializeField] private TextMeshProUGUI healthStat;
    [SerializeField] private TextMeshProUGUI skillPointsText;
    [SerializeField] private TextMeshProUGUI skillPointsAvailableText;
    [SerializeField] private TextMeshProUGUI levelText;

    [SerializeField]
    private Color inactiveColor =
        new Color(0.4f, 0.4f, 0.4f, 0.45f);

    [SerializeField]
    private Color perfectColor =
        new Color(1f, 0.85f, 0.3f, 1f);

    public int skillPoints
    {
        get
        {
            stats baseStats = GameManager.Instance.player.baseStats;
            int totalStats = baseStats.maxHealth + baseStats.strength + baseStats.agility + baseStats.fortitude + baseStats.faith;
            int maxSkillPoints = GameManager.Instance.player.level + 15;
            int skillPoints = maxSkillPoints - totalStats;
            return skillPoints;
            
        }
    }
    public void Show()
    {
        UpdateStats();
        modal.SetActive(true);
    }

    public void Hide()
    {
        modal.SetActive(false);
    }

    public void UpdateStats()
    {
        strengthStat.text = GameManager.Instance.player.baseStats.strength.ToString();
        fortitudeStat.text = GameManager.Instance.player.baseStats.fortitude.ToString();
        agilityStat.text = GameManager.Instance.player.baseStats.agility.ToString();
        faithStat.text = ((float)GameManager.Instance.player.baseStats.faith/10).ToString() + "/1";

        if (GameManager.Instance.player.baseStats.faith >= 10)
        {
            faithStat.color = perfectColor;
        }
        else
        {
            faithStat.color = inactiveColor;
        }

        healthStat.text = GameManager.Instance.player.baseStats.maxHealth.ToString();
        skillPointsText.text = "Max Value Points: " + (GameManager.Instance.player.level + 15).ToString();
        skillPointsAvailableText.text = "Value Points Available: " + skillPoints.ToString();
        levelText.text = "Value Level: " + GameManager.Instance.player.level.ToString();
    }

    public void AddOneSkillPoint(string StatText)
    {
        if(skillPoints <= 0)
        {
            Debug.LogError("No skill points available to add.");
            return;
        }

        switch (StatText.ToLower())
        {
            case "strength":
                GameManager.Instance.player.baseStats.strength++;
                break;
            case "fortitude":
                GameManager.Instance.player.baseStats.fortitude++;
                break;
            case "agility":
                GameManager.Instance.player.baseStats.agility++;
                break;
            case "faith":
                if(GameManager.Instance.player.baseStats.faith < 10)
                    GameManager.Instance.player.baseStats.faith++;
                break;
            case "health":
                GameManager.Instance.player.baseStats.maxHealth++;
                GameManager.Instance.player.baseStats.health = GameManager.Instance.player.baseStats.maxHealth;
                break;

            default:
                Debug.LogError("Invalid stat text: " + StatText);
                break;
        }

        UpdateStats();
    }

    public void SubstractOneSkillPoint(string StatText)
    {
        switch (StatText.ToLower())
        {
            case "strength":
                if (GameManager.Instance.player.baseStats.strength > 0)
                    GameManager.Instance.player.baseStats.strength--;
                break;
            case "fortitude":
                if (GameManager.Instance.player.baseStats.fortitude > 0)
                    GameManager.Instance.player.baseStats.fortitude--;
                break;
            case "agility":
                if (GameManager.Instance.player.baseStats.agility > 0)
                    GameManager.Instance.player.baseStats.agility--;
                break;
            case "faith":
                if (GameManager.Instance.player.baseStats.faith > 0)
                    GameManager.Instance.player.baseStats.faith--;
                break;
            case "health":
                if (GameManager.Instance.player.baseStats.maxHealth > 1)
                {
                    GameManager.Instance.player.baseStats.maxHealth--;
                    GameManager.Instance.player.baseStats.health = GameManager.Instance.player.baseStats.maxHealth;
                }
                    
                break;
            default:
                Debug.LogError("Invalid stat text: " + StatText);
                break;
        }

        UpdateStats();
    }
}