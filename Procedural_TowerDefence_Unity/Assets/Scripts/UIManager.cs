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
    public TextMeshProUGUI waveText;

    [Header("Wave UI")]
    public GameObject startNextWaveButton;
    public TextMeshProUGUI countdownText;

    [Header("----- Pause Menu -----")]
    public GameObject pauseMenu;

    private PlayerManager playerManager;
    private WaveManager waveManager;

    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        waveManager = FindObjectOfType<WaveManager>();
        StartCoroutine(InitializeMainTower());
        UpdateUI();
        if (startNextWaveButton != null)
        {
            startNextWaveButton.SetActive(false);
        }
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
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
            waveText.text = $"{waveManager.GetCurrentWaveIndex()}";
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

    // Called by WaveManager when a wave ends
    public void ShowStartNextWaveButton(float countdown)
    {
        if (startNextWaveButton != null)
        {
            startNextWaveButton.SetActive(true);
        }
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
        nextWaveCountdown = countdown;
    }

    private float nextWaveCountdown = 5f;

    // Called by button OnClick
    public void OnStartNextWaveButtonClicked()
    {
        if (startNextWaveButton != null)
        {
            startNextWaveButton.SetActive(false);
        }
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }
        StartCoroutine(CountdownAndStartNextWave());
    }

    private IEnumerator CountdownAndStartNextWave()
    {
        float timer = nextWaveCountdown;
        while (timer > 0) 
        {
            if (countdownText != null)
            {
                countdownText.text = $"Wave {waveManager.GetCurrentWaveIndex() + 1} starting in {Mathf.CeilToInt(timer)}...";
            }
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
        if (waveManager != null)
        {
            waveManager.StartNextWave();
        }
    }

    private IEnumerator InitializeMainTower()
    {
        yield return new WaitForSeconds(1f);
        MainTower = GameObject.FindGameObjectWithTag("MainTower")?.GetComponent<MainTower>();
    }

    public void TogglePauseMenu()
    {
        if (pauseMenu != null)
        {
            bool isActive = pauseMenu.activeSelf;
            pauseMenu.SetActive(!isActive);
            Time.timeScale = isActive ? 1f : 0f; // Pause or resume game
        }
    }
}