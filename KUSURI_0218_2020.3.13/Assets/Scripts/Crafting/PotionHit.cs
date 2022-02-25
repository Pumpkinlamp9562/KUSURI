using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionHit : MonoBehaviour
{
    public GameManager manager;
    LayerMask detectLayer;
    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        detectLayer = manager.GetComponent<PotionMouseOn>().rayLayer;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (detectLayer == (detectLayer | (1 << other.gameObject.layer)) && other.tag != "IKLookAt")
        {
            manager.audios.vfxAudio.pitch = Random.Range(1, 1.5f);
            manager.audios.vfxAudio.PlayOneShot(manager.audios.throwPotion, manager.audios.throwPotion_v);
            if (other.GetComponent<ItemPotionUse>() != null)
                OtherPotionSwitch(other.gameObject, true); //減少藥水庫存並成功使用藥水
            else
                OtherPotionSwitch(other.gameObject, false); //減少藥水庫存但使用藥水失敗
            Destroy(gameObject);
            Resources.UnloadUnusedAssets();
        }
    }

    public void Cancel()
    {
        Destroy(gameObject);
        Resources.UnloadUnusedAssets();
    }

    public void OtherPotionSwitch(GameObject other, bool success)
    {
        if (success)
        {
            ItemPotionUse item;
            item = other.GetComponent<ItemPotionUse>();
            switch (manager.ui.potion)
            {
                case "lightbig":
                    item.o_lightBig();
                    manager.ui.ItemPotionInteractable(6, 7, true);
                    break;
                case "lightsmall":
                    item.o_lightSmall();
                    manager.ui.ItemPotionInteractable(6, 7, true);
                    break;
                case "scalebig":
                    item.o_scaleBig();
                    manager.ui.ItemPotionInteractable(9, 8, true);
                    break;
                case "scalesmall":
                    item.o_scaleSmall();
                    manager.ui.ItemPotionInteractable(9, 8, true);
                    break;
                case "timebig":
                    item.o_timeBig();
                    manager.ui.ItemPotionInteractable(11, 10, true);
                    break;
                case "timesmall":
                    item.o_timeSmall();
                    manager.ui.ItemPotionInteractable(11, 10, true);
                    break;
                default:
                    Debug.Log("Here is no sclected potion!");
                    break;
            }
        }
        else
        {
            switch (manager.ui.potion)
            {
                case "lightbig":
                    manager.ui.o_lightBig();
                    manager.ui.ItemPotionInteractable(6, 7, true);
                    break;
                case "lightsmall":
                    manager.ui.o_lightSmall();
                    manager.ui.ItemPotionInteractable(6, 7, true);
                    break;
                case "scalebig":
                    manager.ui.o_scaleBig();
                    manager.ui.ItemPotionInteractable(9, 8, true);
                    break;
                case "scalesmall":
                    manager.ui.o_scaleSmall();
                    manager.ui.ItemPotionInteractable(9, 8, true);
                    break;
                case "timebig":
                    manager.ui.o_timeBig();
                    manager.ui.ItemPotionInteractable(11, 10, true);
                    break;
                case "timesmall":
                    manager.ui.o_timeSmall();
                    manager.ui.ItemPotionInteractable(11, 10, true);
                    break;
                default:
                    Debug.Log("Here is no sclected potion!");
                    break;
            }
        }
        manager.ui.UI_Update();
    }
}
