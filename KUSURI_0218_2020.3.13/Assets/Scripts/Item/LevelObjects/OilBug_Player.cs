using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilBug_Player : MonoBehaviour
{
    public OilBug_Light light;
    GameObject target;
    private void Start()
    {
        light.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<OilBug>() != null) 
        {
            light.oilBugs.Add(other.gameObject);
            light.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<OilBug>() != null)
        {
            light.oilBugs.Clear();
            light.enabled = false;
        }
    }
}