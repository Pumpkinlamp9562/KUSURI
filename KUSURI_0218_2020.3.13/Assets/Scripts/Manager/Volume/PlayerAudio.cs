using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    AudioClipManager audio;
    // Start is called before the first frame update
    void Start()
    {
        audio = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().audios;
    }

    public void FootStep()
    {
        audio.vfxAudio.PlayOneShot(audio.footstep);
    }

    public void JumpSound()
    {
        audio.vfxAudio.PlayOneShot(audio.jump);
    }
}
