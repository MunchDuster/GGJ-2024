using UnityEngine;

public class RandomAudioPlayer : MonoBehaviour
{
    public AudioSource source;
    public AudioClip[] clips;
    public void Play()
    {
        source.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }
}
