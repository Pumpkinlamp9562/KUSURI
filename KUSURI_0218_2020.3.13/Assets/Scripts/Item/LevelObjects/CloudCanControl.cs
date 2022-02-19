using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudCanControl : MonoBehaviour
{
    [SerializeField]
    bool playerIn;
    [SerializeField]
    bool move;
    [SerializeField]
    GameObject target;
    [SerializeField]
    int objectCount;
    [SerializeField]
    float speed = 0.01f;
    [SerializeField]
    MeshRenderer cloudMesh;

    Vector3 targetPos;
    Vector3 originPos;

    private void Start()
    {
        originPos = gameObject.transform.position;
        targetPos = target.transform.position;
    }

    private void Update()
    {
        if (move && objectCount <= 0 && playerIn)
        {
            if (Vector3.Distance(gameObject.transform.position, targetPos) > 0.01f)
                gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, targetPos, speed);
        }
        else if(Vector3.Distance(gameObject.transform.position, originPos) > 0.01f)
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, originPos, speed);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 10)
        {
            playerIn = true;

            int state = 0;
            if (other.GetComponent<PlayerManager>() != null)
            {
                if (other.GetComponent<PlayerManager>().potionState.Count != 0)
                    for (int i = 0; i < other.GetComponent<PlayerManager>().potionState.Count; i++)
                    {
                        if (other.GetComponent<PlayerManager>().potionState[i] == PlayerManager.State.scaleBig)
                            state++;
                        if (i >= other.GetComponent<PlayerManager>().potionState.Count && state == 0)
                            StartCoroutine(WaitForMove(true));
                    }
                else
                    StartCoroutine(WaitForMove(true));
            }
        }
        else
            if (other.GetComponent<MeshRenderer>() != null)
            if (!other.GetComponent<MeshRenderer>().enabled)
                objectCount = 0;
        if (other.transform.gameObject.layer != 10)
        {
            targetPos = other.transform.position;
        }
        else
            targetPos = target.transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Rigidbody>()!= null && other.gameObject.layer != 10 && other.gameObject.layer == 9)
        {
            objectCount++;
            StopAllCoroutines();
            StartCoroutine(WaitForMove(false));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer == 10)
        {
            playerIn = false;
            StopAllCoroutines();
            StartCoroutine(WaitForMove(false));
        }
        if (other.GetComponent<Rigidbody>() != null && other.gameObject.layer == 9)
            objectCount--;
    }
    IEnumerator WaitForMove(bool m)
    {
        cloudMesh.material.SetInt("_CloudFloat", m ? 1 : 0);
        yield return new WaitForSeconds(1);
        move = m;
    }
}
