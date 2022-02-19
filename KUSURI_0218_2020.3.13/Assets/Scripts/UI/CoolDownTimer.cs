using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoolDownTimer : MonoBehaviour
{
    public bool count;

    Image coolDownImage;
    float timer;
    PlayerManager player;
    public int potionTime;

    // Start is called before the first frame update
    void Start()
    {
        coolDownImage = GetComponent<Image>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
        timer = 0;
        coolDownImage.fillAmount = 0;
    }

    private void Update()
    {
        if (count)
        {
            if (timer < potionTime || coolDownImage.fillAmount < 1)
            {
                timer += Time.deltaTime;
                coolDownImage.fillAmount = (timer / potionTime);
            }
            if(coolDownImage.fillAmount >= 1) count = false;
        }
        else
        {
            timer = 0;
            coolDownImage.fillAmount = 0;
            count = false;
        }
    }
}
