using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonItem : MonoBehaviour
{
    GrowGroupControl parent;
    Vector3 pos;

    private void Start()
    {
        parent = gameObject.transform.parent.gameObject.GetComponent<GrowGroupControl>();
        pos = gameObject.transform.position;
    }

    public void IsGrowed()
    {
        //parent.gameObject.transform.DetachChildren();
        if(gameObject.GetComponent<Rigidbody>() == null)
            gameObject.AddComponent<Rigidbody>();
        if (gameObject.GetComponent<SphereCollider>() == null)
            gameObject.AddComponent<SphereCollider>();
        StartCoroutine(wait());
    }

    IEnumerator wait()
    {
        yield return new WaitForSeconds(parent.gameObject.GetComponent<ItemPotionUse>().potionTime);
        if(gameObject != null)
        {
            gameObject.transform.position = pos;
            Destroy(gameObject.GetComponent<Rigidbody>());
            Destroy(gameObject.GetComponent<SphereCollider>());
        }
    }
}
