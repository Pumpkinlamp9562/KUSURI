using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public GameObject fire;
    public GameObject whiteBlood;
    // Start is called before the first frame update
    void Start()
    {
        fire = Resources.Load<GameObject>("Effect/Fire");
        whiteBlood = Resources.Load<GameObject>("Effect/Leukocyte_Boom");
    }
}
