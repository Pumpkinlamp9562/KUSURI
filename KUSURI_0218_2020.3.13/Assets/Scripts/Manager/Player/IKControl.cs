using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKControl : MonoBehaviour
{
    GameManager manager;
    public float maxDistance;
    float a1 = 0;
    float a2 = 1;
    float b1 = 0.1f;
    [Range(0,1)]
    public float ikWeight = 0.5f;
    private void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        maxDistance = gameObject.GetComponent<BoxCollider>().size.z + gameObject.GetComponent<BoxCollider>().center.z;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<ItemInfo>() != null || other.gameObject.tag == "IKLookAt")
            manager.player.anim.target = other.gameObject;
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<ItemInfo>() != null || other.gameObject.tag == "IKLookAt")
        {
            manager.player.anim.target = other.gameObject;
            manager.player.anim.distance = Vector3.Distance(gameObject.transform.position, manager.player.anim.target.transform.position);
            manager.player.anim.LookiKWeight = ((1-(manager.player.anim.distance / maxDistance)) - a1)/(a2 - a1)*(ikWeight - b1) + b1;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == manager.player.anim.target)
            manager.player.anim.target = null;
    }

}
