using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonWater : MonoBehaviour
{
    public bool IsPoison;
    public int poisonInNeed = 1;
    public float poisonCount;

    private void Start()
    {
        IsPoison = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PoisonItem>() != null)
        {
            other.gameObject.GetComponent<MeshRenderer>().enabled = false;
            other.gameObject.GetComponent<PoisonItem>().enabled = false;
            other.gameObject.GetComponent<SphereCollider>().enabled = false;
            Resources.UnloadUnusedAssets();
            poisonCount += 1;
            GetComponent<MeshRenderer>().material.SetColor("_DeepWater", Color.Lerp(GetComponent<MeshRenderer>().material.GetColor("_DeepWater")
                , other.GetComponent<MeshRenderer>().material.color, (poisonCount / poisonInNeed)));

            if (poisonCount == poisonInNeed)
            {//Add WaterColorChangeShader IEnumerator
                IsPoison = true;
            }
            if (GetComponentInChildren<PoisonWater>() != null)
            {
                GetComponentInChildren<PoisonWater>().IsPoison = IsPoison;
                GetComponentInChildren<PoisonWater>().poisonInNeed = poisonInNeed;
                GetComponentInChildren<PoisonWater>().poisonCount = poisonCount;
                GetComponentInChildren<PoisonWater>().gameObject.GetComponent<MeshRenderer>().material.SetColor("_DeepWater", Color.Lerp(GetComponent<MeshRenderer>().material.GetColor("_DeepWater")
                , other.GetComponent<MeshRenderer>().material.color, (poisonCount / poisonInNeed)));
            }
        }
    }
}
