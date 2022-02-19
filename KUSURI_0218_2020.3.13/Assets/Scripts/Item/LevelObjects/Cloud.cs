using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    [SerializeField]
    float force = 15;
    Rigidbody rigid;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }


    void FixedUpdate()
    {
        rigid.AddForce(0, force, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9 && other.gameObject.GetComponent<Rigidbody>() != null)
            other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.transform.gameObject.tag == "Player" || collision.gameObject.layer == 9) && collision.gameObject.GetComponent<Rigidbody>() != null)
        {
            Debug.Log(collision.gameObject.GetComponent<Rigidbody>());
            collision.transform.parent = gameObject.transform;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if ((collision.transform.gameObject.tag == "Player" || collision.gameObject.layer == 9) && collision.gameObject.GetComponent<Rigidbody>() != null)
            collision.transform.parent = null;
    }
}
