using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour {
    AudioSource m_MyAudioSource;
    bool m_Play = true;
    public int trackIndex;
    public List<AudioClip> musicTracks;
	// Use this for initialization
	void Start () {
        m_MyAudioSource = GetComponent<AudioSource>();
        m_MyAudioSource.playOnAwake = false;
        m_MyAudioSource.loop = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (!m_MyAudioSource.isPlaying && m_Play)
        {
            m_MyAudioSource.clip = musicTracks[trackIndex];
            m_MyAudioSource.Play();
            if (trackIndex < musicTracks.Count - 2)
                trackIndex++;
            else
                trackIndex = 0;
        }
	}
}
