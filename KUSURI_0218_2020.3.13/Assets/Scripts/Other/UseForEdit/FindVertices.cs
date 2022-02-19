using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindVertices : MonoBehaviour
{
    public List<int> platformPoint = new List<int>();
    RaycastHit ray;

    void Update()
    {
        Mesh mesh1 = new Mesh();
        Mesh mesh = mesh1;
        gameObject.GetComponent<SkinnedMeshRenderer>().BakeMesh(mesh);
        Vector3[] verticecs = mesh.vertices;
        for(int i = 0; i < verticecs.Length; i++)
        {
            Debug.DrawRay(verticecs[i] + transform.position, transform.up, Color.green, 1);
            if (Physics.Raycast(verticecs[i] + transform.position, transform.up, out ray, 1))
            {
                platformPoint.Add(i);
            }
        }
    }
}
