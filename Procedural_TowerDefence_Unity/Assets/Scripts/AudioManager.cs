using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [Header("----- AUDIO SOURCES -----")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("----- MUSIC CLIPS -----")]
    public AudioClip mainmenuMusic;
    public string mainMenuName = "MainMenuScene";
    public AudioClip gameMusic;
    public string gameSceneName = "GameScene";
    public AudioClip endGameMusic;
    public string endGameSceneName = "EndGameScene";

    [Header("----- SFX CLIPS -----")]
    
    public AudioClip towerPlacement;
    public AudioClip towerUpgrade;
    public AudioClip buttonClick;
    public AudioClip cancelAction;
    public AudioClip cantAfford;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        CheckSceneMusic(SceneManager.GetActiveScene().name);
    }

    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
        pitch: sfxSource.pitch = Random.Range(0.95f, 1.05f);
    }
    
    public void CheckSceneMusic(string sceneName)
    {
        if (sceneName == mainMenuName)
        {
            PlayMusic(mainmenuMusic);
        }
        else if (sceneName == gameSceneName)
        {
            PlayMusic(gameMusic);
        }
        else if (sceneName == endGameSceneName)
        {
            PlayMusic(endGameMusic);
        }
    }
}
