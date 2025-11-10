using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Audio;

public class TowerAudio : MonoBehaviour
{
    public AudioClip towerShoot;
    public AudioClip towerDeath;
    public AudioSource towerAudioSource;

    public void PlaySFX(AudioClip clip)
    {
        towerAudioSource.PlayOneShot(clip);
        pitch: towerAudioSource.pitch = Random.Range(0.95f, 1.05f);
    }
}
