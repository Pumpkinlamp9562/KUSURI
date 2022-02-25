using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftPoition : MonoBehaviour
{
    GameManager manager;
    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }


    public void PotionDone()
    {
        if (manager.uiSetting.craftON)
        {
            manager.uiSetting.potion.sprite = manager.craft.craftPotion;
            manager.uiSetting.potion.color = new Color(1, 1, 1, 1);
            manager.audios.vfxAudio.pitch = 1;
            manager.audios.vfxAudio.PlayOneShot(manager.audios.craftDone, manager.audios.craftDone_v);
        }
    }
}
