using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartUI : MonoBehaviour
{
    public Button load;
    GameManager manager;

    public void loadButtonSet()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        if (manager.save.data.scenePlayed != "" && manager.save.data.scenePlayed != "Start_UI")
            load.interactable = true;
        else
            load.interactable = false;
    }
}
