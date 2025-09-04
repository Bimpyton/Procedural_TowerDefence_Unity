using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public MainTower MainTower;
    public Image scoreProgressBar;
    public Image healthBar;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI skillPointsText;

    private PlayerManager playerManager;

    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        StartCoroutine(InitializeMainTower());
        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (playerManager != null)
        {
            // Assuming PlayerManager exposes xpToNextLevel
            float progress = 0f;
            if (playerManager.xp >= 0 && playerManager.xp < playerManager.xpToNextLevel)
            {
                progress = (float)playerManager.xp / playerManager.xpToNextLevel;
            }
            scoreProgressBar.fillAmount = progress;

            if (MainTower != null)
            {
                float healthPercent = MainTower.health / MainTower.maxHealth;
                healthBar.fillAmount = healthPercent;
                healthText.text = $"{MainTower.health} / {MainTower.maxHealth}";
            }

            goldText.text = $"Gold: {playerManager.Gold}";
            levelText.text = $"{playerManager.Level}";
            skillPointsText.text = $"Skill Points: {playerManager.SkillPoints}";
        }
    }

    public void CloseGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private IEnumerator InitializeMainTower()
    {
        yield return new WaitForSeconds(1f);
        MainTower = GameObject.FindGameObjectWithTag("MainTower")?.GetComponent<MainTower>();
    }
}