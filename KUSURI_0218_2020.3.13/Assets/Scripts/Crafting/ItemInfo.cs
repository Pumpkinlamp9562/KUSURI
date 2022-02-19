using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInfo : MonoBehaviour
{
    //Use for UI
    public enum ItemCategory
    {
        None,
        Herb,
        Fruit,
        Mine
    }
    public enum HerbType
    {
        None,
        Emission,
        Time,
        Scale
    }

    public enum MineType
    {
        None,
        Big,
        Small
    }

    //Type Select
    public ItemCategory type = ItemCategory.None;

    //Type Info Show
    [HideInInspector]
    public HerbType herb;
    [HideInInspector]
    public MineType mine;

    GameManager manager;
    Collider colli;
    MeshRenderer[] mesh;


    private void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        mesh = GetComponentsInChildren<MeshRenderer>(); //more than one
        colli = GetComponent<Collider>();
    }
    float respawnTime()
    {
        float time;
        time = ((manager.save.backpack.lightHerb + manager.save.backpack.timeHerb + manager.save.backpack.scaleHerb + 
            manager.save.backpack.fruit + manager.save.backpack.bigMine + manager.save.backpack.smallMine) / 10) + 2;
        return time;
    }
    public void ItemDespawn()
    {
        StartCoroutine(Despawn());
        respawnTime();
        StartCoroutine(Respawn());
    }
    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(1.05f);// count pick up time
        //Destory
        for (int i = 0; i < mesh.Length; i++)
            mesh[i].enabled = false;
    }
    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime());
        for (int i = 0; i < mesh.Length; i++)
            mesh[i].enabled = true;
    }
}
