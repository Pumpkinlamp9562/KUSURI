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
        audio.vfxAudio.pitch = Random.Range(1,1.5f);
        audio.vfxAudio.PlayOneShot(audio.footstep, audio.footstep_v);
    }

    public void JumpSound()
    {
        audio.vfxAudio.pitch = Random.Range(1, 1.5f);
        audio.vfxAudio.PlayOneShot(audio.jump, audio.jump_v);
    }

    public void DrinkPotion()
    {
        audio.vfxAudio.pitch = Random.Range(1, 1.5f);
        audio.vfxAudio.PlayOneShot(audio.drinkPotion, audio.drinkPotion_v);
    }
}
