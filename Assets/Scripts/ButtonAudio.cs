using UnityEngine;

public class ButtonAudio : MonoBehaviour {
    new public KMAudio audio;
    public AudioClip[] clips = new AudioClip[6];

    [SerializeField]
    private int id;

    public void PlaySound()
    {
        audio.PlaySoundAtTransform(clips[id].name, transform);
    }
}
