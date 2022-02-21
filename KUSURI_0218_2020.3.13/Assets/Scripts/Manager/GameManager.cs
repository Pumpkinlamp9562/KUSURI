using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Player")]
    public PlayerManager player;
    public ItemCraft craft;
    [Header("Setting")]
    public InputManager input;
    public PotionMouseOn mouse;
    public CameraFollow cam;
    public EffectManager effect;
    public ScenesManager scenes;
    [Header("Save")]
    public SaveSetting save;
    public ItemToSave item;
    [Header("UI")]
    public BackpackUI ui;
    public UIManager uiSetting;
    public UINavigationSkip uiNav;

    private void Awake()
    {
        if (PlayerPrefs.GetInt("ending", 0) == 1)
        {
            Home_Tent.ending = true;
        }
        else
        {
            Home_Tent.ending = false;
        }
    }
}
