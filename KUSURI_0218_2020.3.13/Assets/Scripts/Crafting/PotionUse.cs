using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionUse : MonoBehaviour
{
    public int potionTime = 5;
    public Color playerEmissionColor;
    public GameObject playerLight;

    PlayerManager player;
    GameManager manager;
    CameraFollow cam;

    Vector3 defaultcamOffset;
    float defaultSpeed;
    float defaultSpeedSmooth;
    float defaultcamSmooth;
    Color defaultEmissColor;
    bool falldownOpenTime;

    private void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        player = GetComponent<PlayerManager>();
        cam = manager.cam;
        defaultSpeed = player.move.walkSpeed;
        defaultSpeedSmooth = player.move.speedSmoothTime;
        defaultcamOffset = manager.cam.offset;
        defaultcamSmooth = manager.cam.moveSpeed;
        defaultEmissColor = player.render.material.GetColor("_EmissionColor");
        playerLight.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (!player.move.grounded && falldownOpenTime)
        {
            player.rigid.useGravity = false;
            player.rigid.AddForce(new Vector3(0, Physics.gravity.y - 15, 0), ForceMode.Acceleration);
        }

        if (!player.move.grounded && falldownOpenTime && player.rigid.velocity.y < 0)
            player.rigid.AddForce(new Vector3(0, -15, 0), ForceMode.Acceleration);

    }

    //potionUse_Player
    public void p_lightBig()
    {
        manager.ui.p_lightBig();
        //Player Burn Animation
        //Burn Shader
        GameObject clone;
        clone = Instantiate(manager.effect.fire, gameObject.transform);
        Destroy(clone, potionTime);
        Resources.UnloadUnusedAssets();

        player.potionState.Add(PlayerManager.State.lightBig);
        manager.uiSetting.PlayerStateUIUpdate();
        StartCoroutine(P_lightWaitForReturn());
    }
    public void p_lightSmall()
    {
        manager.ui.p_lightSmall();

        player.render.material.SetColor("_EmissionColor", playerEmissionColor);
        playerLight.SetActive(true);

        player.potionState.Add(PlayerManager.State.lightSmall);
        manager.uiSetting.PlayerStateUIUpdate();
        StartCoroutine(P_lightWaitForReturn());
    }
    public void p_scaleBig()
    {
        manager.ui.p_scaleBig();

        player.rigid.mass = 5;

        cam.offset = cam.turnBigCameraOffset; //cam

        gameObject.transform.localScale = new Vector3(2, 2, 2); //scale

        player.potionState.Add(PlayerManager.State.scaleBig);
        manager.uiSetting.PlayerStateUIUpdate();
        StartCoroutine(P_scaleWaitForReturn());
    }
    public void p_scaleSmall()
    {
        manager.ui.p_scaleSmall();
        
        //float walkSpeed = 0.1f;

        player.rigid.mass = 0.15f; //kg   
        /*        player.move.walkSpeed = walkSpeed;
                if (manager.input.run)
                    player.move.speed = walkSpeed * 2;
                else
                    player.move.speed = walkSpeed;                */
        cam.offset = cam.turnSmallCameraOffset; //cam

        gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); //scale

        player.potionState.Add(PlayerManager.State.scaleSmall);
        manager.uiSetting.PlayerStateUIUpdate();
        StartCoroutine(P_scaleWaitForReturn());
    }
    public void p_timeBig()
    {
        manager.ui.p_timeBig();

        //move
        player.move.walkSpeed = player.move.walkSpeed * 3f;
        player.move.runSpeed = player.move.walkSpeed * 2f;
        player.move.speedSmoothTime = 6f;
        player.anim.anim.speed = 6f;//anim 
        falldownOpenTime = true;//gravity
        cam.moveSpeed = cam.moveSpeed / 2;//camera follow
        player.rigid.drag = 0;

        player.potionState.Add(PlayerManager.State.timeBig);
        manager.uiSetting.PlayerStateUIUpdate();
        StartCoroutine(P_timeWaitForReturn(false));
    }
    public void p_timeSmall()
    {
        manager.ui.p_timeSmall();
        //move
        player.move.walkSpeed = defaultSpeed / 2.5f;
        player.move.runSpeed = player.move.walkSpeed / 2.5f;
        player.anim.anim.speed = 0.5f; //anim
        player.rigid.drag = 20; //gravity

        player.potionState.Add(PlayerManager.State.timeSmall);
        manager.uiSetting.PlayerStateUIUpdate();
        StartCoroutine(P_timeWaitForReturn(true));
    }
    IEnumerator P_lightWaitForReturn()
    {
        manager.ui.CoolDown(12, 13, false, gameObject);
        yield return new WaitForSeconds(potionTime);
        for (int i = 0; i < player.potionState.Count; i++)
            if (player.potionState[i] == PlayerManager.State.lightBig)
                player.Dead();
        player.render.material.SetColor("_EmissionColor", defaultEmissColor);
        playerLight.SetActive(false);
        manager.ui.CoolDown(12, 13, true, gameObject);
        player.potionState.Remove(PlayerManager.State.lightBig);
        player.potionState.Remove(PlayerManager.State.lightSmall);
        manager.uiSetting.PlayerStateUIUpdate();
    }
    IEnumerator P_scaleWaitForReturn()
    {
        manager.ui.CoolDown(14, 15, false, gameObject);
        yield return new WaitForSeconds(potionTime);
/*        player.move.walkSpeed = defaultSpeed;
        if (manager.input.run)
            player.move.speed = defaultSpeed * 2;
        else
            player.move.speed = defaultSpeed;*/
        gameObject.transform.localScale = new Vector3(1, 1, 1); //Player Scale Return
        player.rigid.mass = 1; //Player Rigidbody Return
        player.rigid.useGravity = true;
        //Main Camera Return
        cam.moveSpeed = defaultcamSmooth;
        cam.offset = defaultcamOffset;

        manager.ui.CoolDown(14, 15, true, gameObject);
        player.potionState.Remove(PlayerManager.State.scaleBig);
        player.potionState.Remove(PlayerManager.State.scaleSmall);

        manager.uiSetting.PlayerStateUIUpdate();
    }
    IEnumerator P_timeWaitForReturn(bool big)
    {
        manager.ui.CoolDown(16, 17, false, gameObject);
/*        if (big)
            yield return new WaitForSeconds(potionTime * 2);
        else*/
            yield return new WaitForSeconds(potionTime);
        //Player Movement Return
        player.move.walkSpeed = defaultSpeed;
        player.move.runSpeed = player.move.walkSpeed * 2;
        player.move.speedSmoothTime = defaultSpeedSmooth;
        if (manager.input.run)
            player.move.speed = defaultSpeed * 2;
        else
            player.move.speed = defaultSpeed;
        player.anim.anim.speed = 1f; //Player Animation Return

        player.rigid.drag = 0;
        player.rigid.useGravity = true;        //Player Rigidbody Return

        falldownOpenTime = false;
        cam.moveSpeed = defaultcamSmooth; //Main Camera Return
        manager.ui.CoolDown(16, 17, true, gameObject);
        player.potionState.Remove(PlayerManager.State.timeBig);
        player.potionState.Remove(PlayerManager.State.timeSmall);
        manager.uiSetting.PlayerStateUIUpdate();
    }
    public void ClearAll()
    {
        player.potionState.Clear();
        manager.uiSetting.PlayerStateUIUpdate();
        gameObject.transform.localScale = new Vector3(1, 1, 1); //Player Scale Return
        player.rigid.mass = 1; //Player Rigidbody Return
        //Player Movement Return
        player.move.walkSpeed = defaultSpeed;
        player.move.runSpeed = player.move.walkSpeed * 2;
        player.move.speedSmoothTime = defaultSpeedSmooth;
        if (manager.input.run)
            player.move.speed = defaultSpeed * 2;
        else
            player.move.speed = defaultSpeed;
        //Player Animation Return
        player.anim.anim.speed = 1f;
        //Player Rigidbody Return
        player.rigid.drag = 0;
        falldownOpenTime = false;
        //Main Camera Return
        cam.moveSpeed = defaultcamSmooth;
        cam.offset = defaultcamOffset;
        //Light
        player.render.material.SetColor("_EmissionColor", defaultEmissColor);
        playerLight.SetActive(false);
        //ResetCoolDown
        manager.ui.CoolDown(6, 7, true, gameObject);
        manager.ui.CoolDown(8, 9, true, gameObject);
        manager.ui.CoolDown(10, 11, true, gameObject);
        manager.ui.CoolDown(12, 13, true, gameObject);
        manager.ui.CoolDown(14, 15, true, gameObject);
        manager.ui.CoolDown(16, 17, true, gameObject);
        manager.ui.UI_Update();
    }
}
