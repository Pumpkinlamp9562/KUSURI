using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    GameManager manager;
    public bool debug;
    public List<Collider> ragdoll = new List<Collider>();
    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        CollectColliders();
        SetRagdoll(false);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (debug)
            Debug.Log(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "SavePoint")
        {
            manager.save.Save(gameObject.transform.position);
            manager.save.levelSave.UpdateLevelObjectsChild(other.gameObject);
        }
        if (other.gameObject.tag == "DeadArea") manager.player.Dead();

        if (other.gameObject.tag == "NextScene") manager.scenes.ChangeScene(other.gameObject.name);

        if (other.gameObject.layer == 7)//Camera
        {
            manager.cam.playerInOtherCamArea = true;
            manager.cam.target = other.GetComponentsInChildren<Transform>()[1];
        }
        if (other.gameObject.layer == 17)//Camera
        {
            manager.cam.playerInOtherFollowCamArea = true;
            manager.cam.followOffset = other.GetComponentsInChildren<Transform>()[1].localPosition;
            manager.cam.followRotate = other.GetComponentsInChildren<Transform>()[1].localRotation;
        }
        if(other.gameObject.tag == "Cloud")
        {
            gameObject.transform.parent = other.gameObject.transform;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 4)//Water
        {
            float playerhead = manager.player.transform.position.y + manager.player.GetComponent<Collider>().bounds.size.y;
            float waterHeight = other.gameObject.GetComponent<WaterFloat>().waterLevel;
            if (waterHeight > playerhead) manager.player.Dead();
        }
        if (!manager.cam.playerInOtherCamArea)
        {
            if (other.gameObject.layer == 7)//Camera
            {
                manager.cam.playerInOtherCamArea = true;
                manager.cam.target = other.GetComponentsInChildren<Transform>()[1];
            }
            if (other.gameObject.layer == 17)//Camera
            {
                manager.cam.playerInOtherFollowCamArea = true;
                manager.cam.followOffset = other.GetComponentsInChildren<Transform>()[1].localPosition;
                manager.cam.followRotate = other.GetComponentsInChildren<Transform>()[1].localRotation;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 7)//Camera
        {
            manager.cam.playerInOtherCamArea = false;
            manager.cam.target = manager.player.gameObject.transform;
        }
        if (other.gameObject.layer == 17)//Camera
        {
            manager.cam.playerInOtherFollowCamArea = false;
            manager.cam.followOffset = manager.cam.offset;
            manager.cam.followRotate = manager.cam.rotation;
        }
        if (other.gameObject.tag == "Cloud")
        {
            gameObject.transform.parent = null;
        }
    }

    void CollectColliders()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        foreach(Collider c in colliders)
        {
            if(c.gameObject.tag == "Ragdoll")
            {
                c.isTrigger = true;
                c.GetComponent<Rigidbody>().useGravity = false;
                ragdoll.Add(c);
            }
        }
    }

    public void SetRagdoll(bool t)
    {
        manager.player.rigid.useGravity = !t;
        Collider[] colli = this.gameObject.GetComponentsInChildren<Collider>();
        foreach (Collider co in colli)
        {
            if(co.gameObject.tag != "Ragdoll")
            {
                co.enabled = !t;
            }
        }
        foreach(Collider c in ragdoll) {
            c.isTrigger = !t;
            c.GetComponent<Rigidbody>().useGravity = t;
            c.attachedRigidbody.velocity = Vector3.zero;
        }
        manager.player.anim.anim.enabled = !t;
        manager.player.move.enabled = !t;
        manager.player.use.enabled = !t;
    }
}
