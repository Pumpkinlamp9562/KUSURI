using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilBug_Light : MonoBehaviour
{
    public List<GameObject> oilBugs = new List<GameObject>();

    private void OnEnable()
    {
        Debug.Log(gameObject);
        for(int i = 0; i< oilBugs.Count; i++)
        {
            oilBugs[i].GetComponent<OilBug>().target = gameObject;
            oilBugs[i].GetComponent<OilBug>().agent.enabled = true;
            oilBugs[i].GetComponent<OilBug>().agent.SetDestination(oilBugs[i].GetComponent<OilBug>().target.transform.position);
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < oilBugs.Count; i++)
        {
            oilBugs[i].GetComponent<OilBug>().target = null;
            oilBugs[i].GetComponent<OilBug>().agent.enabled = false;
        }
    }
}
