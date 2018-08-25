using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioDelay : MonoBehaviour {
    AudioSource player;
    public AudioClip[] sounds;
    public float minTime;
    public float maxTime;

    void Start () {
        player = GetComponent<AudioSource>();
        StartCoroutine("SoundLoop");
    }

    IEnumerator SoundLoop()
    {
        while (true)
        {
            float time = Random.Range(minTime, maxTime);
            Debug.Log(time);
            yield return new WaitForSeconds(time);
            if (!player.isPlaying)
            {
                if (sounds.Length > 0)
                {
                    player.clip = sounds[Random.Range(0, sounds.Length)];
                }
                player.Play();
            }
        }
    }
}
