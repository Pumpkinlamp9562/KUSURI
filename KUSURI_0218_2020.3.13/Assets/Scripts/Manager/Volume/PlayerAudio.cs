using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    GameManager manager;
    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    public void FootStep()
    {
        if (manager.player.move.grounded)
        {
            manager.audios.vfxAudio.pitch = Random.Range(1, manager.audios.pitchRandom);
            manager.audios.vfxAudio.PlayOneShot(manager.audios.footstep, manager.audios.footstep_v);
        }
    }

    public void JumpSound()
    {
        manager.audios.vfxAudio.pitch = Random.Range(1, manager.audios.pitchRandom);
        manager.audios.vfxAudio.PlayOneShot(manager.audios.jump, manager.audios.jump_v);
    }

    public void DrinkPotion()
    {
        manager.audios.vfxAudio.pitch = Random.Range(1, manager.audios.pitchRandom);
        manager.audios.vfxAudio.PlayOneShot(manager.audios.drinkPotion, manager.audios.drinkPotion_v);
    }
}
