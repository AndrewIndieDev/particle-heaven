using UnityEngine;

public class SoundEffect : MonoBehaviour
{
    [SerializeField] private AudioClip clipType;
    [SerializeField] private Vector2 volume = Vector2.one;
    [SerializeField] private Vector2 pitch = Vector2.one;

    public void Play()
    {
        AudioManager.PlaySound(clipType, EMixerGroup.SFX, Random.Range(pitch.x, pitch.y), Random.Range(volume.x, volume.y));
    }
}
