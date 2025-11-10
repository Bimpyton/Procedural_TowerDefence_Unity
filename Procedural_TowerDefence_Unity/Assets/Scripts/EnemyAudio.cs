using UnityEngine;

public class EnemyAudio : MonoBehaviour
{    
    public AudioClip enemyShoot;
    public AudioClip enemyDeath;
    public AudioSource enemyAudioSource;

     public void PlaySFX(AudioClip clip)
    {
        enemyAudioSource.PlayOneShot(clip);
        pitch: enemyAudioSource.pitch = Random.Range(0.95f, 1.05f);
    }
}
