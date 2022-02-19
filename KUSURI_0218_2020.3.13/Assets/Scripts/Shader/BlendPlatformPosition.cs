using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendPlatformPosition : MonoBehaviour
{
    public int targetVertex = 1894;
    // Start is called before the first frame update
    void Start()
    {
        Move();
    }

    public void Move()
    {
        gameObject.transform.position = transform.parent.transform.TransformPoint(transform.parent.transform.gameObject.GetComponent<MeshCollider>().sharedMesh.vertices[targetVertex]);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.gameObject.tag == "Player" || other.gameObject.layer == 9)
            other.transform.parent = gameObject.transform;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.gameObject.tag == "Player" || other.gameObject.layer == 9)
            other.transform.parent = null;
    }
}
