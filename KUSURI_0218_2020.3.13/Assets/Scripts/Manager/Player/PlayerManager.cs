using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameManager manager;
    public PlayerAnimation anim;
    public PlayerMovement move;
    public PlayerCollision collision;
    public PlayerPickUp pickUp;
    public PotionUse use;
    public Rigidbody rigid;
    public Renderer render;
    public enum State { lightBig, lightSmall, timeBig, timeSmall, scaleBig, scaleSmall };
    public List<State> potionState = new List<State>();



    public void Dead()
    {
        StopAllCoroutines();
        manager.player.transform.parent = null;
        manager.mouse.Cancelthrown();
        collision.SetRagdoll(true);
        manager.uiSetting.FadeInOutUIDead(1);
        manager.cam.restart = true;
        StartCoroutine(DeadCountDown());
    }

    IEnumerator DeadCountDown()
    {
        yield return new WaitForSeconds(3);
        //Respawn
        //Animation
        manager.player.anim.anim.SetBool("PickUp", false);
        manager.player.anim.target = null;
        manager.player.anim.pushTarget = null;
        manager.player.anim.anim.SetFloat("LookAtWeight", 0);
        manager.player.anim.anim.SetFloat("HandWeight", 0);
        manager.player.anim.anim.SetFloat("PushIKWeight", 0);
        //Movement
        move.speed = 0;
        //Physic
        rigid.velocity = Vector3.zero;
        collision.SetRagdoll(false);
        //Craft
        manager.craft.Clear();
        use.StopAllCoroutines();
        use.ClearAll();
        //
        manager.cam.playerInOtherCamArea = false;
        manager.cam.playerInOtherFollowCamArea = false;
        manager.save.LoadSave();
        manager.uiSetting.FadeInOutUIDead(0);
        manager.cam.restart = false;
    }
}
