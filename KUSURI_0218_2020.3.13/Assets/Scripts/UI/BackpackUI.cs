using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackpackUI : MonoBehaviour
{
    public GameObject currentSelected;
    public Button[] itemButtons;
    public Text[] itemUI = new Text[18];

    [HideInInspector]
    public string potion;
    GameManager manager;
    SaveSetting Save;

    private void Awake()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        Save = manager.save;
        for (int i = 0; i < itemButtons.Length; i++)
        {
            itemUI[i] = itemButtons[i].gameObject.GetComponentInChildren<Text>();
        }
    }

    public void UI_Update()
    {
        itemUI[0].text = Save.backpack.fruit.ToString();
        itemUI[1].text = Save.backpack.lightHerb.ToString();
        itemUI[2].text = Save.backpack.scaleHerb.ToString();
        itemUI[3].text = Save.backpack.timeHerb.ToString();
        itemUI[4].text = Save.backpack.bigMine.ToString();
        itemUI[5].text = Save.backpack.smallMine.ToString();
        itemUI[6].text = Save.backpack.o_lightBig.ToString();
        itemUI[7].text = Save.backpack.o_lightSmall.ToString();
        itemUI[8].text = Save.backpack.o_scaleBig.ToString();
        itemUI[9].text = Save.backpack.o_scaleSmall.ToString();
        itemUI[10].text = Save.backpack.o_timeBig.ToString();
        itemUI[11].text = Save.backpack.o_timeSmall.ToString();
        itemUI[12].text = Save.backpack.p_lightBig.ToString();
        itemUI[13].text = Save.backpack.p_lightSmall.ToString();
        itemUI[14].text = Save.backpack.p_scaleBig.ToString();
        itemUI[15].text = Save.backpack.p_scaleSmall.ToString();
        itemUI[16].text = Save.backpack.p_timeBig.ToString();
        itemUI[17].text = Save.backpack.p_timeSmall.ToString();

        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (itemUI[i].text != "0")
            {
                if (i <= 5)
                {
                    if (!manager.craft.crafting)
                    {
                        itemButtons[i].interactable = true;
                        itemUI[i].color = Color.white;
                    }
                }
                else
                {
                    if (itemButtons[i].GetComponentInChildren<CoolDownTimer>())
                    {
                        if (!itemButtons[i].GetComponentInChildren<CoolDownTimer>().count)
                        {
                            itemButtons[i].interactable = true;
                            itemUI[i].color = Color.white;
                        }
                    }
                }
            }
            else
            {
                itemUI[i].color = Color.gray;
                itemButtons[i].interactable = false;
            }
        }

        manager.save.BackPackSave();
    }

    public void CoolDown(int num1, int num2, bool tf, GameObject target)
    {
        if(target.GetComponent<PlayerManager>() != null)
        {
            for (int i = 0; i < manager.player.potionState.Count; i++)
            {
                /*if (manager.player.potionState[i] == PlayerManager.State.timeSmall)
                {
                    itemButtons[num1].GetComponentInChildren<CoolDownTimer>().potionTime = manager.player.use.potionTime*2;
                    itemButtons[num2].GetComponentInChildren<CoolDownTimer>().potionTime = manager.player.use.potionTime*2;
                }
                else
                {*/
                    itemButtons[num1].GetComponentInChildren<CoolDownTimer>().potionTime = manager.player.use.potionTime;
                    itemButtons[num2].GetComponentInChildren<CoolDownTimer>().potionTime = manager.player.use.potionTime;
                //}
            }
        }
        if(target.GetComponent<ItemPotionUse>() != null)
        {
            itemButtons[num1].GetComponentInChildren<CoolDownTimer>().potionTime = target.GetComponent<ItemPotionUse>().potionTime;
            itemButtons[num2].GetComponentInChildren<CoolDownTimer>().potionTime = target.GetComponent<ItemPotionUse>().potionTime;
        }
        itemButtons[num1].interactable = tf;
        itemButtons[num2].interactable = tf;
        itemButtons[num1].GetComponentInChildren<CoolDownTimer>().count = !tf;
        itemButtons[num2].GetComponentInChildren<CoolDownTimer>().count = !tf;
        if (manager.input.previousControlScheme == "Gamepad" && manager.uiSetting.backpackON)
        {
            if (tf) manager.uiNav.UISelectedUpdate(currentSelected);
        }

        UI_Update();
    }

    public void ButtonOnClick(string p) //Thrown Potion Active
    {
        potion = p;
        switch (potion)
        {
            case "lightbig":
                manager.mouse.GetPotion(0);
                ItemPotionInteractable(6, 7, false);
                break;
            case "lightsmall":
                manager.mouse.GetPotion(1);
                ItemPotionInteractable(6, 7, false);
                break;
            case "scalebig":
                manager.mouse.GetPotion(2);
                ItemPotionInteractable(9, 8, false);
                break;
            case "scalesmall":
                manager.mouse.GetPotion(3);

                ItemPotionInteractable(8, 9, false);
                break;
            case "timebig":
                manager.mouse.GetPotion(4);
                ItemPotionInteractable(11, 10, false);
                break;
            case "timesmall":
                manager.mouse.GetPotion(5);
                ItemPotionInteractable(10, 11, false);
                break;
            default:
                Debug.Log("Here is no sclected potion!");
                break;
        }
        manager.mouse.mouseOn = true;
        manager.cam.PotionCamera(1);
    }

    public void ItemPotionInteractable(int num1, int num2, bool tf)
    {
        if (tf)
        {
            if (itemButtons[num1].GetComponentInChildren<CoolDownTimer>().count == !tf)
            {
                itemButtons[num1].interactable = tf;
                if (manager.input.previousControlScheme == "Gamepad")
                    manager.uiNav.UISelectedUpdate(currentSelected);
            }
            if (itemButtons[num2].GetComponentInChildren<CoolDownTimer>().count == !tf)
            {
                itemButtons[num2].interactable = tf;
                if (manager.input.previousControlScheme == "Gamepad")
                    manager.uiNav.UISelectedUpdate(currentSelected);
            }
        }
        else
        {
            itemButtons[num1].interactable = tf;
            itemButtons[num2].interactable = tf;
        }
    }
    public void BackPackCurrentSelectedUpdate(GameObject button)
    {
        if (manager.input.previousControlScheme == "Gamepad")
        {
            if (button.GetComponent<Button>().interactable)
            {
                currentSelected = button;
                manager.uiNav.UISelectedUpdate(button);
            }
            else
            {
                for (int i = 6; i < itemButtons.Length; i++)
                {
                    if (itemButtons[i].interactable)
                    {
                        manager.uiNav.UISelectedUpdate(itemButtons[i].gameObject);
                        currentSelected = itemButtons[i].gameObject;
                    }
                }
            }
        }
    }

    //Player Potion Active
    public void p_lightBig()
    {
        if (Save.backpack.p_lightBig <= 0)
            Save.backpack.p_lightBig = 0;
        else
            Save.backpack.p_lightBig -= 1;
        UI_Update();
    }
    public void p_lightSmall()
    {
        if (Save.backpack.p_lightSmall <= 0)
            Save.backpack.p_lightSmall = 0;
        else
            Save.backpack.p_lightSmall -= 1;
        UI_Update();
    }
    public void p_scaleBig()
    {
        if (Save.backpack.p_scaleBig <= 0)
            Save.backpack.p_scaleBig = 0;
        else
            Save.backpack.p_scaleBig -= 1;
        UI_Update();
    }

    public void p_scaleSmall()
    {
        if (Save.backpack.p_scaleSmall <= 0)
            Save.backpack.p_scaleSmall = 0;
        else
            Save.backpack.p_scaleSmall -= 1;
        UI_Update();
    }

    public void p_timeBig()
    {
        if (Save.backpack.p_timeBig <= 0)
            Save.backpack.p_timeBig = 0;
        else
            Save.backpack.p_timeBig -= 1;
        UI_Update();
    }
    public void p_timeSmall()
    {
        if (Save.backpack.p_timeSmall <= 0)
            Save.backpack.p_timeSmall = 0;
        else
            Save.backpack.p_timeSmall -= 1;
        UI_Update();
    }
    public void o_lightBig()
    {
        if (Save.backpack.o_lightBig <= 0)
            Save.backpack.o_lightBig = 0;
        else
            Save.backpack.o_lightBig -= 1;
        UI_Update();
    }
    public void o_lightSmall()
    {
        if (Save.backpack.o_lightSmall <= 0)
            Save.backpack.o_lightSmall = 0;
        else
            Save.backpack.o_lightSmall -= 1;
        UI_Update();
    }
    public void o_scaleBig()
    {
        if (Save.backpack.o_scaleBig <= 0)
            Save.backpack.o_scaleBig = 0;
        else
            Save.backpack.o_scaleBig -= 1;
        UI_Update();
    }
    public void o_scaleSmall()
    {
        if (Save.backpack.o_scaleSmall <= 0)
            Save.backpack.o_scaleSmall = 0;
        else
            Save.backpack.o_scaleSmall -= 1;
        UI_Update();
    }
    public void o_timeBig()
    {
        if (Save.backpack.o_timeBig <= 0)
            Save.backpack.o_timeBig = 0;
        else
            Save.backpack.o_timeBig -= 1;
        UI_Update();
    }
    public void o_timeSmall()
    {
        if (Save.backpack.o_timeSmall <= 0)
            Save.backpack.o_timeSmall = 0;
        else
            Save.backpack.o_timeSmall -= 1;
        UI_Update();
    }
}
