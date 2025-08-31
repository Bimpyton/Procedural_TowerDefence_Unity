using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Image scoreProgressBar;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI skillPointsText;

    private PlayerManager playerManager;

    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
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
            // Assuming PlayerManager exposes scoreToNextLevel
            float progress = 0f;
            if (playerManager.Score >= 0 && playerManager.Score < playerManager.scoreToNextLevel)
            {
                progress = (float)playerManager.Score / playerManager.scoreToNextLevel;
            }
            scoreProgressBar.fillAmount = progress;

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
}