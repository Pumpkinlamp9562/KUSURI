using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuccessPanel : MonoBehaviour
{
    public GameObject Text_clicked;
    // Start is called before the first frame update
    public void CloseSuccess()
    {
        Text_clicked.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
