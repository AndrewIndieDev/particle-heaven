using System;
using System.Collections;
using UnityEngine;

public class DestroyAudioWhenFinished : MonoBehaviour
{
    AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(DestroyAudio());
    }

    private IEnumerator DestroyAudio()
    {
        yield return new WaitForSeconds(1.0f);
        while (audioSource != null && audioSource.time != 0f && audioSource.time < audioSource.clip.length)
        {
            yield return null;
        }
        Destroy(gameObject);
    }
}
