using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf : MonoBehaviour
{
    MeshCollider mesh;
    public SkinnedMeshRenderer skin;

    private void Start()
    {
        mesh = gameObject.GetComponent<MeshCollider>();
        //skin = GetComponent<SkinnedMeshRenderer>();
    }

    private void FixedUpdate()
    { 
        Mesh bakemesh = new Mesh();
        skin.BakeMesh(bakemesh);
        mesh.sharedMesh = bakemesh;
    }
}
