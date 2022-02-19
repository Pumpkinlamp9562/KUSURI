using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class OilBug : MonoBehaviour
{
    public GameObject startPoint;
    public GameObject target;

    public NavMeshAgent agent;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        target = startPoint;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (target != null)
        {
            if (target.transform.IsChildOf(other.gameObject.transform))
            {
                target.gameObject.SetActive(false);
            }
        }

        if (other.tag == "DeadArea")
            Dead();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<ItemPotionUse>() == null)
            return;

        if (collision.gameObject.transform.GetComponentInChildren<WhiteBloodCore>() != null)
        {
            if(collision.gameObject.GetComponent<Renderer>() != null)
            {
                Renderer mesh = collision.gameObject.GetComponent<Renderer>();
                if (mesh.materials[1].GetInt("_On") == 1)
                    return;
                mesh.materials[1].SetInt("_On", 1);
                collision.gameObject.GetComponent<ItemPotionUse>().canBurn = true;
                Dead();
            }
        }
        else if (collision.gameObject.GetComponentsInChildren<Renderer>() != null)
        {
            Renderer[] mesh = collision.gameObject.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < mesh.Length; i++)
            {
                if (mesh[i].materials[1].GetInt("_On") == 1)
                    break;
                mesh[i].materials[1].SetInt("_On", 1);
            }
            collision.gameObject.GetComponent<ItemPotionUse>().canBurn = true;
            Dead();
        }
    }

    public void Dead()
    {//Dead Effect
        gameObject.transform.position = startPoint.transform.position;
    }
}
