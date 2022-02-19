using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteBloodCore : MonoBehaviour
{
    public GameObject whiteBloodCell;
    public GameObject whiteBloodWall;
    Vector3 startPos;

    private void Start()
    {
        whiteBloodWall = whiteBloodCell.GetComponent<WhiteBloodCell>().whiteBloodWall;
        startPos = transform.position;
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.GetComponent<PoisonWater>() != null)
        {
            if (other.GetComponent<PoisonWater>().IsPoison)
            {//Add Disappear Shader IEnumerator
                SetActiveCustom(gameObject, false);
                whiteBloodWall.GetComponent<WhiteWallDisappear>().WallsDisappear(false, true);
                Resources.UnloadUnusedAssets();
            }
        }
        if(other.gameObject.tag == "DeadArea")
        {
            SetActiveCustom(whiteBloodCell, true);

            Destroy(gameObject.GetComponent<Rigidbody>());
            Destroy(gameObject.GetComponent<MeshCollider>());
            gameObject.transform.position = startPos;
        }
    }

    void SetActiveCustom(GameObject target,bool tf)
    {
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
