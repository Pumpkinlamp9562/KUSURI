using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlaceInChild : MonoBehaviour
{
    public GameObject place;
    public bool spawn;
    public bool unParents;
    public bool destoryGameObjects;
    void Update()
    {
        if(spawn)
            Instantiate(place, transform);
        if (unParents)
            transform.DetachChildren();
        if (destoryGameObjects)
        {
            DestroyImmediate(gameObject);
            Resources.UnloadUnusedAssets();
        }
    }
}
