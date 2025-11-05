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
        playerManager = Object.FindFirstObjectByType<PlayerManager>();
        waveManager = Object.FindFirstObjectByType<WaveManager>();
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

            goldText.text = $"{playerManager.Gold}G";
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

    public void PulseGoldText()
    {
        StartCoroutine(PulseTextCoroutine(goldText));
    }

    private IEnumerator PulseTextCoroutine(TextMeshProUGUI textElement)
    {
        // Define the absolute default scale for UI
        Vector3 defaultScale = Vector3.one;
        Vector3 targetScale = defaultScale * 1.5f;
        float duration = 0.2f;
        float elapsed = 0f;

        // Scale up (Start from current scale to make transition smooth if overlapping)
        Vector3 currentScale = textElement.transform.localScale;
        while (elapsed < duration)
        {
            textElement.transform.localScale = Vector3.Lerp(currentScale, targetScale, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            Color gold = new Color(1f, 0.84f, 0f);
            textElement.color = gold;
            yield return null;
        }
        textElement.transform.localScale = targetScale;

        // Scale down
        elapsed = 0f;
        while (elapsed < duration)
        {
            // Scale back to the fixed default scale (Vector3.one)
            textElement.transform.localScale = Vector3.Lerp(targetScale, defaultScale, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            Color white = Color.white;
            textElement.color = white;
            yield return null;
        }
        
        // Ensure the text ends exactly at the default scale
        textElement.transform.localScale = defaultScale;
    }
}