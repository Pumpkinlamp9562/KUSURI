using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemCraft : MonoBehaviour
{
    [SerializeField]
    int craftSecond = 3;
    [SerializeField]
    float bubbleTarget = 0;
    [SerializeField]
    List<string> craft = new List<string>();
    List<string> type = new List<string>();
    List<Image> image = new List<Image>();
    public Sprite craftPotion;
    public bool crafting;

    GameManager manager;

    private void Awake()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    private void Start()
    {
        craft.Clear();
        type.Clear();
        image.Clear();
    }

    //定義分類
    public void ItemTypeRealizeButtonOnClick(string name)
    {
        manager.uiSetting.craftAnim.SetBool("Play", false);
        StopCoroutine(BubbleLerp());
        manager.uiSetting.potion.color = new Color(1, 1, 1, 0);
        if (manager.uiSetting.craftON)
        {
            craft.Add(name);

            switch (name)
            {
                case "fruit":
                    LineUp(name);
                    if (manager.input.previousControlScheme == "Gamepad")
                    {
                        if ((craft[0] == "fruit" && craft.Count < 3) || (craft[0] != "fruit" && craft.Count < 2))
                        {
                            if (manager.ui.itemButtons[2].interactable && manager.ui.itemButtons[2].enabled)
                                manager.uiNav.UISelectedUpdate(manager.ui.itemButtons[2].gameObject);
                            else if (manager.ui.itemButtons[3].interactable && manager.ui.itemButtons[3].enabled)
                                manager.uiNav.UISelectedUpdate(manager.ui.itemButtons[3].gameObject);
                            else if (manager.ui.itemButtons[1].interactable && manager.ui.itemButtons[1].enabled)
                                manager.uiNav.UISelectedUpdate(manager.ui.itemButtons[1].gameObject);
                            else
                            {
                                StopCoroutine(OtherCraftDone(craftSecond));
                                StopCoroutine(CraftDoneNoFruit(craftSecond));
                                StartCoroutine(CraftDone(craftSecond));
                            }
                        }
                    }
                    break;
                case "herb":
                    if (manager.input.previousControlScheme == "Gamepad")
                    {
                        if ((craft[0] == "fruit" && craft.Count < 3) || (craft[0] != "fruit" && craft.Count < 2))
                        {
                            if (manager.ui.itemButtons[4].interactable && manager.ui.itemButtons[4].enabled)
                                manager.uiNav.UISelectedUpdate(manager.ui.itemButtons[4].gameObject);
                            else if (manager.ui.itemButtons[5].interactable && manager.ui.itemButtons[5].enabled)
                                manager.uiNav.UISelectedUpdate(manager.ui.itemButtons[5].gameObject);
                            else
                            {
                                StopCoroutine(OtherCraftDone(craftSecond));
                                StopCoroutine(CraftDoneNoFruit(craftSecond));
                                StartCoroutine(CraftDone(craftSecond));
                            }
                        }
                    }
                    break;
                case "mineral":
                    if (manager.input.previousControlScheme == "Gamepad")
                    {
                        if ((craft[0] == "fruit" && craft.Count < 3) || (craft[0] != "fruit" && craft.Count < 2))
                        {
                            if (manager.ui.itemButtons[2].interactable && manager.ui.itemButtons[2].enabled)
                                manager.uiNav.UISelectedUpdate(manager.ui.itemButtons[2].gameObject);
                            else if (manager.ui.itemButtons[3].interactable && manager.ui.itemButtons[3].enabled)
                                manager.uiNav.UISelectedUpdate(manager.ui.itemButtons[3].gameObject);
                            else if (manager.ui.itemButtons[1].interactable && manager.ui.itemButtons[1].enabled)
                                manager.uiNav.UISelectedUpdate(manager.ui.itemButtons[1].gameObject);
                            else
                            {
                                StopCoroutine(OtherCraftDone(craftSecond));
                                StopCoroutine(CraftDoneNoFruit(craftSecond));
                                StartCoroutine(CraftDone(craftSecond));
                            }
                        }
                    }
                    break;
            }
        }
    }
    

    void FindButton()
    {
        for(int i = 0; i < 6; i++)
            if(manager.ui.itemButtons[i].interactable && manager.ui.itemButtons[i].enabled && manager.input.previousControlScheme == "Gamepad")
                manager.uiNav.UISelectedUpdate(manager.ui.itemButtons[i].gameObject);
    }

    //定義名稱
    public void ItemNameAddButtonOnClick(string name)
    {
        manager.uiSetting.craftAnim.SetBool("Play", false);
        if (manager.uiSetting.craftON)
        {
            type.Add(name);
            LineUp(name);
            switch (name)
            {
                case "timeHerb":
                    manager.ui.ItemPotionInteractable(1, 2, false);
                    break;
                case "scaleHerb":
                    manager.ui.ItemPotionInteractable(1, 3, false);
                    break;
                case "lightHerb":
                    manager.ui.ItemPotionInteractable(2, 3, false);
                    break;
                case "bigMine":
                    manager.ui.itemButtons[5].interactable = false;
                    break;
                case "smallMine":
                    manager.ui.itemButtons[4].interactable = false;
                    break;
            }
        }
    }

    void LineUp(string name)
    {
        for(int i = 0; i < manager.uiSetting.lines.Length; i++)
        {
            if (name == manager.uiSetting.lines[i].name) manager.uiSetting.lines[i].IsCrafting(
                new Vector2(manager.uiSetting.lines[i].gameObject.GetComponent<RectTransform>().anchoredPosition.x, 0));
        }
    }
 
    public void Check(Image buttonImage)
    {
        image.Add(buttonImage);
        buttonImage.color = Color.gray;
        if (craft.Count > 0)
        {
            if (craft[0] == "fruit")
            {
                bubbleTarget = 0.334f * craft.Count;
                StopCoroutine(BubbleLerp());
                StartCoroutine(BubbleLerp());
                if (craft.Count >= 3)
                {
                    if ((craft[1] == "mineral" || craft[1] == "herb") && (craft[2] == "mineral" || craft[2] == "herb"))
                    {
                        StopCoroutine(OtherCraftDone(craftSecond));
                        StartCoroutine(OtherCraftDone(craftSecond));  
                    }
                }
            }
            else
            if (craft[0] != "fruit")
            {
                bubbleTarget = 0.5f * craft.Count;
                StopCoroutine(BubbleLerp());
                StartCoroutine(BubbleLerp());
                manager.ui.itemButtons[0].interactable = false;
                if (craft.Count == 2)
                {
                    if ((craft[0] == "mineral" || craft[0] == "herb") && (craft[1] == "mineral" || craft[1] == "herb"))
                    {
                        StopCoroutine(CraftDoneNoFruit(craftSecond));
                        StartCoroutine(CraftDoneNoFruit(craftSecond));
                    }
                }
            }
            //有兩個重複
            for (int i = 0; i < craft.Count; i++)
            {
                for (int j = 1; j < craft.Count; j++)
                {
                    if (i != j && craft[i] == craft[j])
                    {
                        Clear();
                        Debug.Log("Craft Fail");
                    }
                }
            }
            //第二三是水果
            for (int i = 1; i < craft.Count; i++)
            {
                if (craft[i] == "fruit")
                {
                    Clear();
                    Debug.Log("Craft Fail");
                }
            }
        }
    }
    //Clear All
    public void Clear()
    {
        manager.uiSetting.potion.color = new Color(1, 1, 1, 0);
        manager.uiSetting.craftAnim.SetBool("Play", false);
        for (int i = 0; i < manager.uiSetting.lines.Length; i++)
        {
            manager.uiSetting.lines[i].IsCrafting(
                new Vector2(manager.uiSetting.lines[i].gameObject.GetComponent<RectTransform>().anchoredPosition.x, manager.uiSetting.lines[i].posY));
        }
        StopAllCoroutines();
        for (int i = 0; i < image.Count; i++)
            image[i].color = Color.white;
        craft.Clear();
        type.Clear();
        image.Clear();
        manager.uiSetting.craftBubble.fillAmount = 0f;
        for (int i = 0; i < manager.uiSetting.lines.Length; i++)
        {
            manager.uiSetting.lines[i].IsCrafting(
                new Vector2(manager.uiSetting.lines[i].gameObject.GetComponent<RectTransform>().anchoredPosition.x, manager.uiSetting.lines[i].posY));
        }
        for (int i = 0; i < 6; i++)
        {
            manager.ui.itemButtons[i].enabled = true;
            if (manager.ui.itemUI[i].text != "0")
                manager.ui.itemButtons[i].interactable = true;
        }
        if ( manager.input.previousControlScheme == "Gamepad")
        {
            if (manager.uiSetting.craftON && !manager.uiSetting.backpackON)
            {
                if (manager.ui.itemButtons[2].interactable && manager.ui.itemButtons[2].enabled)
                {
                    manager.uiNav.UISelectedUpdate(manager.ui.itemButtons[2].gameObject);
                }
                else
                    FindButton();
            }
        }
        crafting = false;
        manager.ui.UI_Update();
    }

    IEnumerator CraftDoneNoFruit(int second)
    {
        crafting = true;
        if (type.Count > 1)
        {
            if (type[0] == "lightHerb" && type[1] == "bigMine" || type[1] == "lightHerb" && type[0] == "bigMine")
            {
                craftPotion = manager.ui.itemButtons[12].GetComponent<Image>().sprite;
                manager.item.p_lightBigAdd();
                manager.item.lightHerbUse();
                manager.item.bigMineUse();
            }
            if (type[0] == "lightHerb" && type[1] == "smallMine" || type[1] == "lightHerb" && type[0] == "smallMine")
            {
                craftPotion = manager.ui.itemButtons[13].GetComponent<Image>().sprite;
                manager.item.p_lightSmallAdd();
                manager.item.lightHerbUse();
                manager.item.smallMineUse();
            }
            if (type[0] == "scaleHerb" && type[1] == "bigMine" || type[1] == "scaleHerb" && type[0] == "bigMine")
            {
                craftPotion = manager.ui.itemButtons[14].GetComponent<Image>().sprite;
                manager.item.p_scaleBigAdd();
                manager.item.scaleHerbUse();
                manager.item.bigMineUse();
            }
            if (type[0] == "scaleHerb" && type[1] == "smallMine" || type[1] == "scaleHerb" && type[0] == "smallMine")
            {
                craftPotion = manager.ui.itemButtons[15].GetComponent<Image>().sprite;
                manager.item.p_scaleSmallAdd();
                manager.item.scaleHerbUse();
                manager.item.smallMineUse();
            }
            if (type[0] == "timeHerb" && type[1] == "bigMine" || type[1] == "timeHerb" && type[0] == "bigMine")
            {
                craftPotion = manager.ui.itemButtons[16].GetComponent<Image>().sprite;
                manager.item.p_timeBigAdd(); ;
                manager.item.timeHerbUse();
                manager.item.bigMineUse();
            }
            if (type[0] == "timeHerb" && type[1] == "smallMine" || type[1] == "timeHerb" && type[0] == "smallMine")
            {
                craftPotion = manager.ui.itemButtons[17].GetComponent<Image>().sprite;
                manager.item.p_timeSmallAdd();
                manager.item.timeHerbUse();
                manager.item.smallMineUse();
            }
        }

        yield return new WaitForSeconds(second);
        Clear();
    }
    
    IEnumerator OtherCraftDone(int second)
    {
        crafting = true;
        if (type.Count > 1)
        {
            if (type[0] == "lightHerb" && type[1] == "bigMine" || type[1] == "lightHerb" && type[0] == "bigMine")
            {
                craftPotion = manager.ui.itemButtons[6].GetComponent<Image>().sprite;
                manager.item.o_lightBigAdd();
                manager.item.fruitUse();
                manager.item.lightHerbUse();
                manager.item.bigMineUse();
            }
            if (type[0] == "lightHerb" && type[1] == "smallMine" || type[1] == "lightHerb" && type[0] == "smallMine")
            {
                craftPotion = manager.ui.itemButtons[7].GetComponent<Image>().sprite;
                manager.item.o_lightSmallAdd();
                manager.item.fruitUse();
                manager.item.lightHerbUse();
                manager.item.smallMineUse();
            }
            if (type[0] == "scaleHerb" && type[1] == "bigMine" || type[1] == "scaleHerb" && type[0] == "bigMine")
            { 
                craftPotion = manager.ui.itemButtons[8].GetComponent<Image>().sprite;
                manager.item.o_scaleBigAdd();
                manager.item.fruitUse();
                manager.item.scaleHerbUse();
                manager.item.bigMineUse();
            }
            if (type[0] == "scaleHerb" && type[1] == "smallMine" || type[1] == "scaleHerb" && type[0] == "smallMine")
            {
                craftPotion = manager.ui.itemButtons[9].GetComponent<Image>().sprite;
                manager.item.o_scaleSmallAdd();
                manager.item.fruitUse();
                manager.item.scaleHerbUse();
                manager.item.smallMineUse();
            }
            if (type[0] == "timeHerb" && type[1] == "bigMine" || type[1] == "timeHerb" && type[0] == "bigMine")
            {
                craftPotion = manager.ui.itemButtons[10].GetComponent<Image>().sprite;
                manager.item.o_timeBigAdd();
                manager.item.fruitUse();
                manager.item.timeHerbUse();
                manager.item.bigMineUse();
            }
            if (type[0] == "timeHerb" && type[1] == "smallMine" || type[1] == "timeHerb" && type[0] == "smallMine")
            {
                craftPotion = manager.ui.itemButtons[11].GetComponent<Image>().sprite;
                manager.item.o_timeSmallAdd();
                manager.item.fruitUse();
                manager.item.timeHerbUse();
                manager.item.smallMineUse();
            }
        }

        yield return new WaitForSeconds(second);
        Clear();
    }

    IEnumerator CraftDone(int second)
    {
        yield return new WaitForSeconds(second);
        Clear();
    }

    IEnumerator BubbleLerp()
    {
        while (manager.uiSetting.craftBubble.fillAmount != bubbleTarget)
        {
            if (bubbleTarget > 0.9f)
                manager.uiSetting.craftAnim.SetBool("Play",true);
            manager.uiSetting.craftBubble.fillAmount = Mathf.Lerp(manager.uiSetting.craftBubble.fillAmount, bubbleTarget, 0.1f);
            yield return null;
        }
    }
}
