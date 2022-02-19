using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteBloodCell : MonoBehaviour
{
    public GameObject whiteBloodWall;
    public float pressureWeight;
    public float speedInNeed = 5;
    public float coreRigidbodyMass = 0.5f;
    public Collider[] bone;

    float totalWeight;

    private void Start()
    {
        bone = gameObject.GetComponentsInChildren<CapsuleCollider>();
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (collision.gameObject.transform.localScale.x > 1.1f || collision.gameObject.GetComponent<PlayerMovement>().speed > speedInNeed)
            {
                DestoryAndHaveChlid();
            }
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("PotionItem") && collision.gameObject.GetComponent<Rigidbody>() != null)
        {
            if (Mathf.Abs(collision.gameObject.GetComponent<Rigidbody>().velocity.x) > 0.0000004768372f ||
            Mathf.Abs(collision.gameObject.GetComponent<Rigidbody>().velocity.y) > 0.0000004768372f ||
            Mathf.Abs(collision.gameObject.GetComponent<Rigidbody>().velocity.z) > 0.0000004768372f)
            {
                DestoryAndHaveChlid();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PotionItem") && other.GetComponent<Rigidbody>() != null)
        {
            totalWeight += other.GetComponent<Rigidbody>().mass;
            if (totalWeight >= pressureWeight)
                DestoryAndHaveChlid();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PotionItem") && other.GetComponent<Rigidbody>() != null)
        {
            totalWeight -= other.GetComponent<Rigidbody>().mass;
        }
    }

    public void DestoryAndHaveChlid()
    {
        if (GetComponentInChildren<WhiteBloodCore>() != null)
        {//Add Disappear Shader IEnumerator
            foreach (CapsuleCollider c in bone)
                c.enabled = false;
            
            GameObject core = GetComponentInChildren<WhiteBloodCore>().gameObject;
            if (core.GetComponent<Rigidbody>() == null)
                core.AddComponent<Rigidbody>();
            core.GetComponent<Rigidbody>().mass = coreRigidbodyMass;
            if (core.GetComponent<MeshCollider>() == null)
            {
                core.AddComponent<MeshCollider>();
                core.GetComponent<MeshCollider>().convex = true;
            }
            if (gameObject.GetComponent<MeshRenderer>() != null)
                gameObject.GetComponent<MeshRenderer>().enabled = false;
            if (gameObject.GetComponent<SkinnedMeshRenderer>() != null)
                gameObject.GetComponent<SkinnedMeshRenderer>().enabled = false;
            if (gameObject.GetComponent<CapsuleCollider>() != null)
                gameObject.GetComponent<CapsuleCollider>().enabled = false;
            if (gameObject.GetComponent<BoxCollider>() != null)
                gameObject.GetComponent<BoxCollider>().enabled = false;
        }else
        {//Add Disappear Shader IEnumerator
            Debug.Log(GetComponentInChildren<WhiteBloodCore>());
            SetActiveCustom(gameObject, false);
            whiteBloodWall.GetComponent<WhiteWallDisappear>().WallsDisappear(false,true);
            Resources.UnloadUnusedAssets();
        }
    }

    void SetActiveCustom(GameObject target, bool tf)
    {
        foreach (CapsuleCollider c in bone)
            c.enabled = tf;
        if (target.GetComponent<MeshRenderer>() != null)
            target.GetComponent<MeshRenderer>().enabled = tf;
        if (target.GetComponent<SkinnedMeshRenderer>() != null)
            target.GetComponent<SkinnedMeshRenderer>().enabled = tf;
        if (target.GetComponent<CapsuleCollider>() != null)
            target.GetComponent<CapsuleCollider>().enabled = tf;
        if (target.GetComponent<BoxCollider>() != null)
            target.GetComponent<BoxCollider>().enabled = tf;
        if (target.GetComponent<MeshCollider>() != null)
            target.GetComponent<MeshCollider>().enabled = tf;
    }
}
