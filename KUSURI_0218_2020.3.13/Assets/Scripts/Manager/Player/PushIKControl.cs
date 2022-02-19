using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushIKControl : MonoBehaviour
{
    GameManager manager;
    [HideInInspector]
    public float maxDistance;

    private void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        maxDistance = gameObject.GetComponent<BoxCollider>().size.z + gameObject.GetComponent<BoxCollider>().center.z;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Rigidbody>() != null && other.gameObject.layer == 9)
            manager.player.anim.pushTarget = other.gameObject;
        if(manager.player.anim.pushTarget != null)
        {
            if (manager.player.anim.pushTarget.GetComponent<MeshRenderer>() != null)
                if (!manager.player.anim.pushTarget.GetComponent<MeshRenderer>().enabled)
                    manager.player.anim.pushTarget = null;
            if (manager.player.anim.pushTarget.GetComponent<SkinnedMeshRenderer>() != null)
                if (!manager.player.anim.pushTarget.GetComponent<SkinnedMeshRenderer>().enabled)
                    manager.player.anim.pushTarget = null;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<Rigidbody>() != null && other.gameObject.layer == 9)
        {
            manager.player.anim.pushTarget = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == manager.player.anim.pushTarget)
            manager.player.anim.pushTarget = null;
    }
}
