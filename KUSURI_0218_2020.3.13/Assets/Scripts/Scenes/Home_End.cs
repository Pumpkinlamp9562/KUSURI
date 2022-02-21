using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Home_End : MonoBehaviour
{
    [SerializeField] GameObject[] activeObjects;
    [SerializeField] GameObject[] inactiveObjects;
    [SerializeField] GameManager manager;
    // Start is called before the first frame update
    void Start()
    {
        if (Home_Tent.ending)
        {
            SetActive(true);
        }
        else
        {
            SetActive(false);
        }
    }

    void SetActive(bool tf)
    {
        for (int i = 0; i < activeObjects.Length; i++)
        {
            activeObjects[i].SetActive(tf);
        }
        for (int i = 0; i < inactiveObjects.Length; i++)
        {
            inactiveObjects[i].SetActive(!tf);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
