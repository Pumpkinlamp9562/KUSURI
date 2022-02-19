using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickUp : MonoBehaviour
{
    public float pickUpCoolDown = 2f;
    bool playerIn;
    bool canPick = true;
    GameObject itemObject;
    public GameObject newItemObject;
    GameManager manager;

    private void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }
    private void Update()
    {
        if ( playerIn && canPick && itemObject != null)
        {
            newItemObject = itemObject;

            if (manager.input.pick)
            {
                ItemInfo item = newItemObject.gameObject.GetComponent<ItemInfo>();
                item.ItemDespawn();
                switch (item.type)
                {
                    case ItemInfo.ItemCategory.Herb:
                        manager.item.IsHerb(item);
                        break;
                    case ItemInfo.ItemCategory.Fruit:
                        manager.item.IsFruit();
                        break;
                    case ItemInfo.ItemCategory.Mine:
                        manager.item.IsMine(item);
                        break;
                    case ItemInfo.ItemCategory.None:
                        Debug.Log(newItemObject.gameObject.name + "ItemCategory need Set Up");
                        break;
                }
                //Write In UI
                manager.ui.UI_Update();
                //Player Ik + PlayerPickUpAnimation Mask
                manager.player.anim.PickUp();
                itemObject = null;
                StartCoroutine(PickUpWait());
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        //Item Save In Database
        if (other.gameObject.GetComponent<ItemInfo>() != null && other.gameObject.GetComponent<MeshRenderer>().enabled && canPick)
        {
            playerIn = true;
            itemObject = other.gameObject;
            manager.player.anim.target = newItemObject;
        }
        if (other.gameObject.GetComponent<ItemInfo>() != null && !other.gameObject.GetComponent<MeshRenderer>().enabled)
        {
            manager.player.anim.target = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<ItemInfo>() != null && other.gameObject.GetComponent<MeshRenderer>().enabled && canPick)
        {
            playerIn = true;
            itemObject = other.gameObject;
            manager.player.anim.target = newItemObject;
        }
        if (other.gameObject.GetComponent<ItemInfo>() != null && !other.gameObject.GetComponent<MeshRenderer>().enabled)
        {
            manager.player.anim.target = null;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<ItemInfo>() != null)
        {
            playerIn = false;
            manager.player.anim.target = null;
        }
    }

    IEnumerator PickUpWait()
    {
        canPick = false;
        yield return new WaitForSeconds(pickUpCoolDown);
        canPick = true;
    }
}
