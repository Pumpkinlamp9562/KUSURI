using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioClipManager : MonoBehaviour
{
    [Header("SFX")]
    public AudioSource vfxAudio;
    [Range(1,2)]
    public float pitchRandom = 1.5f;
    [Header("Player")]
    public AudioClip pick;
    [Range(0,1)]
    public float pick_v = 1;
    public AudioClip footstep;
    [Range(0,1)]
    public float footstep_v = 1;
    public AudioClip jump;
    [Range(0, 1)]
    public float jump_v = 1;
    public AudioClip throwPotion;
    [Range(0, 1)]
    public float throwPotion_v = 1;
    public AudioClip drinkPotion;
    [Range(0, 1)]
    public float drinkPotion_v = 1;
    [Header("WhiteBlood")]
    public AudioClip cell;
    [Range(0, 1)]
    public float cell_v = 1;
    public AudioClip core;
    [Range(0, 1)]
    public float core_v = 1;
    [Header("Plant")]
    public AudioClip grow_big;
    [Range(0, 1)]
    public float grow_big_v = 1;
    public AudioClip growBack_big;
    [Range(0, 1)]
    public float growBack_big_v = 1;
    public AudioClip grow_small;
    [Range(0, 1)]
    public float grow_small_v = 1;
    public AudioClip growBack_small;
    [Range(0, 1)]
    public float growBack_small_v = 1;
    [Header("VFX")]
    public AudioClip fire;
    [Range(0, 1)]
    public float fire_v = 1;
    public AudioClip water;
    [Range(0, 1)]
    public float water_v = 1;
    public AudioClip mushroom;
    [Range(0, 1)]
    public float mushroom_v = 1;
    [Header("UI")]
    public AudioClip openUI;
    [Range(0, 1)]
    public float openUI_v = 1;
    public AudioClip button;
    [Range(0, 1)]
    public float button_v = 1;
    public AudioClip crafting;
    [Range(0, 1)]
    public float crafting_v = 1;
    public AudioClip craftDone;
    [Range(0, 1)]
    public float craftDone_v = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
