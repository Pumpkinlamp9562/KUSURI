using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemToSave : MonoBehaviour
{
    GameManager manager;
    [SerializeField]
    int potionPerCraft = 3;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }
    //potion identifly
    public void IsHerb(ItemInfo item)
    {
        switch (item.herb)
        {
            case ItemInfo.HerbType.Emission:
                if (manager.save.backpack.lightHerb >= 20)
                    manager.save.backpack.lightHerb = 20;
                else
                {
                    manager.save.backpack.lightHerb += 1;
                    if(!manager.uiSetting.craftON)
                        manager.uiSetting.craftRedDot.enabled = true;
                }
                break;
            case ItemInfo.HerbType.Time:
                if (manager.save.backpack.timeHerb >= 20)
                    manager.save.backpack.timeHerb = 20;
                else
                {
                    manager.save.backpack.timeHerb += 1;
                    if (!manager.uiSetting.craftON)
                        manager.uiSetting.craftRedDot.enabled = true;
                }
                break;
            case ItemInfo.HerbType.Scale:
                if (manager.save.backpack.scaleHerb >= 20)
                    manager.save.backpack.scaleHerb = 20;
                else
                {
                    manager.save.backpack.scaleHerb += 1;
                    if (!manager.uiSetting.craftON)
                        manager.uiSetting.craftRedDot.enabled = true;
                }
                break;
            case ItemInfo.HerbType.None:
                Debug.Log(item.gameObject.name + " Herb Type need Set Up");
                break;
        }
    }
    public void IsFruit()
    {
        if (manager.save.backpack.fruit >= 20)
            manager.save.backpack.fruit = 20;
        else
        {
            manager.save.backpack.fruit += 1;
            if (!manager.uiSetting.craftON)
                manager.uiSetting.craftRedDot.enabled = true;
        }
    }
    public void IsMine(ItemInfo item)
    {
        switch (item.mine)
        {
            case ItemInfo.MineType.Big:
                if (manager.save.backpack.bigMine >= 20)
                    manager.save.backpack.bigMine = 20;
                else
                {
                    manager.save.backpack.bigMine += 1;
                    if (!manager.uiSetting.craftON)
                        manager.uiSetting.craftRedDot.enabled = true;
                }
                break;
            case ItemInfo.MineType.Small:
                if (manager.save.backpack.smallMine >= 20)
                    manager.save.backpack.smallMine = 20;
                else
                {
                    manager.save.backpack.smallMine += 1;
                    if (!manager.uiSetting.craftON)
                        manager.uiSetting.craftRedDot.enabled = true;
                }
                break;
            case ItemInfo.MineType.None:
                Debug.Log(item.gameObject.name + "Mineral Type need Set Up");
                break;
        }
    }

    //potionUse
    public void lightHerbUse()
    {
        if (manager.save.backpack.lightHerb <= 0)
            manager.save.backpack.lightHerb = 0;
        else
            manager.save.backpack.lightHerb -= 1;
        manager.ui.UI_Update();
    }
    public void scaleHerbUse()
    {
        if (manager.save.backpack.scaleHerb <= 0)
            manager.save.backpack.scaleHerb = 0;
        else
            manager.save.backpack.scaleHerb -= 1;
        manager.ui.UI_Update();
    }
    public void timeHerbUse()
    {
        if (manager.save.backpack.timeHerb <= 0)
            manager.save.backpack.timeHerb = 0;
        else
            manager.save.backpack.timeHerb -= 1;
        manager.ui.UI_Update();
    }
    public void bigMineUse()
    {
        if (manager.save.backpack.bigMine <= 0)
            manager.save.backpack.bigMine = 0;
        else
            manager.save.backpack.bigMine -= 1;
        manager.ui.UI_Update();
    }
    public void smallMineUse()
    {
        if (manager.save.backpack.smallMine <= 0)
            manager.save.backpack.smallMine = 0;
        else
            manager.save.backpack.smallMine -= 1;
        manager.ui.UI_Update();
    }
    public void fruitUse()
    {
        if (manager.save.backpack.fruit <= 0)
            manager.save.backpack.fruit = 0;
        else
            manager.save.backpack.fruit -= 1;
        manager.ui.UI_Update();
    }

    //potionAdd
    public void o_lightBigAdd()
    {
        if (!manager.uiSetting.backpackON)
            manager.uiSetting.backpackRedDot.enabled = true;
        if (manager.save.backpack.o_lightBig >= 20 || manager.save.backpack.o_lightBig + potionPerCraft >= 20)
            manager.save.backpack.o_lightBig = 20;
        else
            manager.save.backpack.o_lightBig += potionPerCraft;
        manager.ui.UI_Update();
    }
    public void o_lightSmallAdd()
    {
        if (!manager.uiSetting.backpackON)
            manager.uiSetting.backpackRedDot.enabled = true;
        if (manager.save.backpack.o_lightSmall >= 20 || manager.save.backpack.o_lightSmall + potionPerCraft >= 20)
            manager.save.backpack.o_lightSmall = 20;
        else
            manager.save.backpack.o_lightSmall += potionPerCraft;
        manager.ui.UI_Update();
    }
    public void o_scaleBigAdd()
    {
        if (!manager.uiSetting.backpackON)
            manager.uiSetting.backpackRedDot.enabled = true;
        if (manager.save.backpack.o_scaleBig >= 20 || manager.save.backpack.o_scaleBig + potionPerCraft >= 20)
            manager.save.backpack.o_scaleBig = 20;
        else
            manager.save.backpack.o_scaleBig += potionPerCraft;
        manager.ui.UI_Update();
    }
    public void o_scaleSmallAdd()
    {
        if (!manager.uiSetting.backpackON)
            manager.uiSetting.backpackRedDot.enabled = true;
        if (manager.save.backpack.o_scaleSmall >= 20 || manager.save.backpack.o_scaleSmall + potionPerCraft >= 20)
            manager.save.backpack.o_scaleSmall = 20;
        else
            manager.save.backpack.o_scaleSmall += potionPerCraft;
        manager.ui.UI_Update();
    }
    public void o_timeBigAdd()
    {
        if (!manager.uiSetting.backpackON)
            manager.uiSetting.backpackRedDot.enabled = true;
        if (manager.save.backpack.o_timeBig >= 20 || manager.save.backpack.o_timeBig + potionPerCraft >= 20)
            manager.save.backpack.o_timeBig = 20;
        else
            manager.save.backpack.o_timeBig += potionPerCraft;
        manager.ui.UI_Update();
    }
    public void o_timeSmallAdd()
    {
        if (!manager.uiSetting.backpackON)
            manager.uiSetting.backpackRedDot.enabled = true;
        if (manager.save.backpack.o_timeSmall >= 20 || manager.save.backpack.o_timeSmall + potionPerCraft >= 20)
            manager.save.backpack.o_timeSmall = 20;
        else
            manager.save.backpack.o_timeSmall += potionPerCraft;
        manager.ui.UI_Update();
    }
    public void p_lightBigAdd()
    {
        if (!manager.uiSetting.backpackON)
            manager.uiSetting.backpackRedDot.enabled = true;
        if (manager.save.backpack.p_lightBig >= 20 || manager.save.backpack.p_lightBig + potionPerCraft >= 20)
            manager.save.backpack.p_lightBig = 20;
        else
            manager.save.backpack.p_lightBig += potionPerCraft;
        manager.ui.UI_Update();
    }
    public void p_lightSmallAdd()
    {
        if (!manager.uiSetting.backpackON)
            manager.uiSetting.backpackRedDot.enabled = true;
        if (manager.save.backpack.p_lightSmall >= 20 || manager.save.backpack.p_lightSmall + potionPerCraft >= 20)
            manager.save.backpack.p_lightSmall = 20;
        else
            manager.save.backpack.p_lightSmall += potionPerCraft;
        manager.ui.UI_Update();
    }
    public void p_scaleBigAdd()
    {
        if (!manager.uiSetting.backpackON)
            manager.uiSetting.backpackRedDot.enabled = true;
        if (manager.save.backpack.p_scaleBig >= 20 || manager.save.backpack.p_scaleBig + potionPerCraft >= 20)
            manager.save.backpack.p_scaleBig = 20;
        else
            manager.save.backpack.p_scaleBig += potionPerCraft;
        manager.ui.UI_Update();
    }
    public void p_scaleSmallAdd()
    {
        if (!manager.uiSetting.backpackON)
            manager.uiSetting.backpackRedDot.enabled = true;
        if (manager.save.backpack.p_scaleSmall >= 20 || manager.save.backpack.p_scaleSmall + potionPerCraft >= 20)
            manager.save.backpack.p_scaleSmall = 20;
        else
            manager.save.backpack.p_scaleSmall += potionPerCraft;
        manager.ui.UI_Update();
    }
    public void p_timeBigAdd()
    {
        if (!manager.uiSetting.backpackON)
            manager.uiSetting.backpackRedDot.enabled = true;
        if (manager.save.backpack.p_timeBig >= 20 || manager.save.backpack.p_timeBig + potionPerCraft >= 20)
            manager.save.backpack.p_timeBig = 20;
        else
            manager.save.backpack.p_timeBig += potionPerCraft;
        manager.ui.UI_Update();
    }
    public void p_timeSmallAdd()
    {
        if (!manager.uiSetting.backpackON)
            manager.uiSetting.backpackRedDot.enabled = true;
        if (manager.save.backpack.p_timeSmall >= 20 || manager.save.backpack.p_timeSmall + potionPerCraft >= 20)
            manager.save.backpack.p_timeSmall = 20;
        else
            manager.save.backpack.p_timeSmall += potionPerCraft;
        manager.ui.UI_Update();
    }
}
