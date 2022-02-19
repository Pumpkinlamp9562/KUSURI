using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPotionUse : MonoBehaviour
{
    public bool cantDestory;
    public bool canBurn;
    public int potionTime = 5;
    public Color emisstion = new Color(255, 255, 255, 0.7f);

    GameManager manager;
    GrowGroupControl control;

    GameObject clone;
    GameObject lightGameObject;

    Collider colli;
    Renderer[] mesh;
    Rigidbody rigid;
    public Material[] render;

    Color[] defaultEmissColor;
    Vector3 defaultScale;
    float defaultMass;
    bool falldownOpen;
    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        mesh = GetComponentsInChildren<Renderer>(); //more than one

        colli = GetComponent<Collider>();
        render = new Material[mesh.Length];
        for (int i = 0; i < mesh.Length; i++)
            render[i] = mesh[i].material; //more than one

        if (GetComponent<Rigidbody>() != null)
        {
            rigid = GetComponent<Rigidbody>();
            defaultMass = gameObject.GetComponent<Rigidbody>().mass;
        }


        if (GetComponent<GrowGroupControl>() != null)
            control = GetComponent<GrowGroupControl>();


        defaultEmissColor = new Color[render.Length];
        for (int i = 0; i < render.Length; i++)
        {
            if(render[i].HasProperty("_EmissionColor"))
                defaultEmissColor[i] = render[i].GetColor("_EmissionColor");
        }

        defaultScale = gameObject.transform.localScale;
        falldownOpen = false;
        if (GetComponentInChildren<Light>() != null)
        {
            lightGameObject = GetComponentInChildren<Light>().gameObject;
            lightGameObject.SetActive(false);
        }

    }

    private void FixedUpdate()
    {
        if (!(Physics.CheckSphere(gameObject.transform.position, 0.4f, LayerMask.GetMask("Ground"))) && falldownOpen)
            rigid.AddForce(new Vector3(0, -20, 0));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "DeadArea" && other.GetComponent<Rigidbody>() != null)
        {
            rigid.useGravity = false;
            rigid.freezeRotation = true;
            rigid.velocity = Vector3.zero;
        }
    }

    //potionUse
    public void o_lightBig()
    {
        manager.ui.o_lightBig();
        clone = Instantiate(manager.effect.fire, gameObject.transform);
        StartCoroutine(Fire());
        StartCoroutine(O_lightWaitForReturn(clone));
    }
    public void o_lightSmall()
    {
        manager.ui.o_lightSmall();
        for (int i = 0; i < render.Length; i++)
            render[i].SetColor("_EmissionColor", emisstion);

        if (lightGameObject != null)
            lightGameObject.SetActive(true);
        StartCoroutine(O_lightSmallWaitForReturn());

    }
    public void o_scaleBig()
    {
        manager.ui.o_scaleBig();
        if (gameObject.GetComponent<Rigidbody>() != null)
            gameObject.GetComponent<Rigidbody>().mass += 5; //kg
        gameObject.transform.localScale = gameObject.transform.localScale * 2; //scale
        StartCoroutine(O_scaleWaitForReturn());

    }
    public void o_scaleSmall()
    {
        manager.ui.o_scaleSmall();
        if (gameObject.GetComponent<Rigidbody>() != null)
            gameObject.GetComponent<Rigidbody>().mass -= 5; //kg
        gameObject.transform.localScale = gameObject.transform.localScale / 2; //scale
        StartCoroutine(O_scaleWaitForReturn());

    }
    public void o_timeBig()
    {
        manager.ui.o_timeBig();
        //shader GrowUp
        if (control != null)
        {
            control.grow = GrowGroupControl.Grow.grow;
            control.UsedGrow();
        }

        //rigidbody
        if (rigid != null)
            falldownOpen = true;

        StartCoroutine(O_timeWaitForReturn());

    }
    public void o_timeSmall()
    {
        manager.ui.o_timeSmall();
        //shader GrowBack
        if (control != null)
        {
            control.grow = GrowGroupControl.Grow.minify;
            control.UsedGrow();
        }

        //rigidbody
        if (rigid != null)
            rigid.drag = 20f;

        StartCoroutine(O_timeWaitForReturn());

    }

    IEnumerator Fire()
    {
        yield return new WaitForSeconds(3);

        if (canBurn)
        {
            if (GetComponent<WhiteBloodCell>() != null)
                GetComponent<WhiteBloodCell>().DestoryAndHaveChlid();
            else
                for (int i = 0; i < mesh.Length; i++)
                    mesh[i].enabled = false;
            colli.isTrigger = true;
            gameObject.layer = 2;
            if (rigid != null)
                rigid.useGravity = false;
            if (lightGameObject != null)
                lightGameObject.SetActive(false);
        }
        Destroy(clone);
    }

    //Return
    IEnumerator O_scaleWaitForReturn()
    {
        manager.ui.CoolDown(8, 9, false, gameObject);
        yield return new WaitForSeconds(potionTime);
        gameObject.transform.localScale = defaultScale;
        if (rigid != null)
            rigid.mass = defaultMass;
        manager.ui.CoolDown(8, 9, true, gameObject);
    }
    IEnumerator O_timeWaitForReturn()
    {
        manager.ui.CoolDown(10, 11, false, gameObject);
        yield return new WaitForSeconds(potionTime);
        falldownOpen = false;
        //rigidbody
        if (rigid != null)
            rigid.drag = 0f;
        //shader Return
        if (control != null)
        {
            control.grow = GrowGroupControl.Grow.normal;
            control.UsedGrow();
        }
        manager.ui.CoolDown(10, 11, true, gameObject);
    }
    IEnumerator O_lightWaitForReturn(GameObject clone)
    {
        manager.ui.CoolDown(6, 7, false, gameObject);
        yield return new WaitForSeconds(potionTime);

        if (cantDestory && canBurn)
        {
            colli.isTrigger = false;
            gameObject.layer = 9;
            for (int i = 0; i < mesh.Length - 1; i++)
                mesh[i].enabled = true;

            if (rigid != null)
                rigid.useGravity = true;
        }

        manager.ui.CoolDown(6, 7, true, gameObject);
    }
    IEnumerator O_lightSmallWaitForReturn()
    {
        manager.ui.CoolDown(6, 7, false, gameObject);
        yield return new WaitForSeconds(potionTime);

        for (int i = 0; i < render.Length; i++)
            render[i].SetColor("_EmissionColor", defaultEmissColor[i]);
        if (lightGameObject != null && lightGameObject.gameObject.tag != "Light")
            lightGameObject.SetActive(false);
        manager.ui.CoolDown(6, 7, true, gameObject);
    }
}
