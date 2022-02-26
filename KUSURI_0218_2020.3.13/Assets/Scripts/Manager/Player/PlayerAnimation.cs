using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//GUA is veryvery good

public class PlayerAnimation : MonoBehaviour
{
    [Header("Walk Animation Setting")]
    public float acceleration;
    public float deceleration;
    public float maximumWalkVelocity = 0.5f;
    public float maximumRunVelocity = 2.0f;

    [HideInInspector]
    public Animator anim;

    GameManager manager;

    [Header("Walk Animation Debug")]
    float velocityX;
    float velocityZ;
    int VelocityZHash;
    int VelocityXHash;

    public bool frontAnim;
    public bool backAnim;
    public bool leftAnim;
    public bool rightAnim;

    [Header("IK Debug")]
    [Range(0, 1)]
    public float PushIKHandWeight = 0f;
    [Range(0, 1)]
    public float PickIKHandWeight = 0f;
    public float LookiKWeight;
    [SerializeField]
    float LookiKWeightlerp = 0;
    public GameObject target;
    public GameObject pushTarget;
    [HideInInspector]
    public float distance;

    [Header("Fall Animation")]
    [Range(-8, -1)]
    public float fallVelocity;

    

    //Ik, IK Target, PickUp Animation Layer

    void Awake()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        anim = GetComponent<Animator>();

        VelocityZHash = Animator.StringToHash("VelocityZ");
        VelocityXHash = Animator.StringToHash("VelocityX");
    }

    void Update()
    {
        AnimationDirection();
        float currentMaxVelocity = manager.input.run ? maximumRunVelocity : maximumWalkVelocity;//if run is true will set to the first option, otherwise with set to the second option

        //handle change in velocity
        ChangeVelocity(currentMaxVelocity);
        ResetOrLockVelocity(currentMaxVelocity);

        //all move animation On Off
        if (velocityX != 0 || velocityZ != 0)
            anim.SetBool("Move", true);
        else
            anim.SetBool("Move", false);

        //set the velocity to the animation float
        anim.SetFloat(VelocityXHash, velocityX);
        anim.SetFloat(VelocityZHash, velocityZ);

        FallDownAnim();

        if((anim.GetFloat("VelocityY") == 0)|| manager.player.move.grounded)
            anim.SetBool("Grouded", true);
        else
            anim.SetBool("Grouded", false);
    }

    public void Jump()
    {
        //Jump & Fall SetUp
        if ((manager.player.move.grounded || manager.input.jump))
        {
            anim.SetBool("jump",true);
        }
        if(!manager.input.jump)
        {
            anim.SetBool("jump", false);
        }
    }

    void AnimationDirection()
    {
        //leftAnim = input.left;
        //rightAnim = input.right;
/*        if (input.back && !(input.right && input.back) && !(input.left && input.back))
            backAnim = true;
        else
            backAnim = false;*/
        if (manager.input.front || manager.input.left || manager.input.right || manager.input.back)
            frontAnim = true;
        else
            frontAnim = false;
    }

    void ChangeVelocity(float currentMaxVelocity)
    {
        //increase velocity when input and run
        if (frontAnim && velocityZ < currentMaxVelocity)
            velocityZ += Time.deltaTime * acceleration;
        if (backAnim && velocityZ > -currentMaxVelocity)
            velocityZ -= Time.deltaTime * acceleration;
        if (leftAnim && velocityX < currentMaxVelocity)
            velocityX += Time.deltaTime * acceleration;
        if (rightAnim && velocityX > -currentMaxVelocity)
            velocityX -= Time.deltaTime * acceleration;

        //deceleration when input is not pressing and velocity is not equal to zero
        if (!frontAnim && velocityZ > 0.0f)
            velocityZ -= Time.deltaTime * deceleration;
        if (!backAnim && velocityZ < 0.0f)
            velocityZ += Time.deltaTime * deceleration;
        if (!leftAnim && velocityX > 0.0f)
            velocityX -= Time.deltaTime * deceleration;
        if (!rightAnim && velocityX < 0.0f)
            velocityX += Time.deltaTime * deceleration;
    }

    void ResetOrLockVelocity(float currentMaxVelocity)
    {
        //reset velocity to 0
        if (!rightAnim && !leftAnim && (velocityX > -0.05 && velocityX < 0.05f))
            velocityX = 0;
        if (!frontAnim && !backAnim && (velocityZ > -0.05 && velocityZ < 0.05f))
            velocityZ = 0;

        //set velocity to currentMaxVelocity
        if (frontAnim && manager.input.run && velocityZ > currentMaxVelocity)
            velocityZ = currentMaxVelocity;
        else if (frontAnim && velocityZ > currentMaxVelocity)
        {
            velocityZ -= Time.deltaTime * deceleration;
            if (velocityZ > currentMaxVelocity && velocityZ < currentMaxVelocity + 0.05)
                velocityZ = currentMaxVelocity;
        }
        else if (frontAnim && velocityZ < currentMaxVelocity && velocityZ > (currentMaxVelocity - 0.05f))
            velocityZ = currentMaxVelocity;

        if (backAnim && manager.input.run && velocityZ < -currentMaxVelocity)
            velocityZ = -currentMaxVelocity;
        else if (backAnim && velocityZ < -currentMaxVelocity)
        {
            velocityZ += Time.deltaTime * deceleration;
            if (velocityZ < currentMaxVelocity && velocityZ > -currentMaxVelocity - 0.05)
                velocityZ = -currentMaxVelocity;
        }
        else if (backAnim && velocityZ > -currentMaxVelocity && velocityZ < (-currentMaxVelocity + 0.05f))
            velocityZ = -currentMaxVelocity;

        if (leftAnim && manager.input.run && velocityX > currentMaxVelocity)
            velocityX = currentMaxVelocity;
        else if (leftAnim && velocityX > currentMaxVelocity)
        {
            velocityX -= Time.deltaTime * deceleration;
            if (velocityX > currentMaxVelocity && velocityX < currentMaxVelocity + 0.05)
                velocityX = currentMaxVelocity;
        }
        else if (leftAnim && velocityX < currentMaxVelocity && velocityX > (currentMaxVelocity - 0.05f))
            velocityX = currentMaxVelocity;

        if (rightAnim && manager.input.run && velocityX < -currentMaxVelocity)
            velocityX = -currentMaxVelocity;
        else if (rightAnim && velocityX < -currentMaxVelocity)
        {
            velocityX += Time.deltaTime * deceleration;
            if (velocityX < currentMaxVelocity && velocityX > -currentMaxVelocity - 0.05)
                velocityX = -currentMaxVelocity;
        }
        else if (rightAnim && velocityX > -currentMaxVelocity && velocityX < (-currentMaxVelocity + 0.05f))
            velocityX = -currentMaxVelocity;
    }

    public void ChangeIdlePose()
    {
        anim.SetFloat("random", Random.Range(1, 5));
    }

    void FallDownAnim()
    {
        float maxVelocity = -8;
        float value = (anim.GetFloat("VelocityY") / maxVelocity);
        anim.SetLayerWeight(2, value);
        if (manager.player.move.haveStair)
            anim.SetFloat("VelocityY", 0);
        else
            anim.SetFloat("VelocityY", manager.player.rigid.velocity.y);
    }

    public void PickUp()
    {
        anim.SetBool ("PickUp",false);
        anim.SetBool("PickUp", true);
    }

    public void PickUpEnd()
    {
        anim.SetBool("PickUp", false);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if(manager.player.pickUp.newItemObject != null)
            anim.SetLookAtPosition(manager.player.pickUp.newItemObject.transform.position);
        if (target != null)
        {
            LookiKWeightlerp = Mathf.Lerp(LookiKWeightlerp, LookiKWeight, 0.01f);
            anim.SetLookAtWeight(LookiKWeightlerp);//distance and lerp
        }
        else
        {
            LookiKWeightlerp = Mathf.Lerp(LookiKWeightlerp, 0, 0.01f);
            anim.SetLookAtWeight(LookiKWeightlerp);//distance and lerp
        }
        if (anim.GetBool("PickUp"))
        {
            if(target != null)
            {
                PickIKHandWeight = Mathf.Lerp(PickIKHandWeight, 1, 0.001f);
                anim.SetLookAtWeight(0.3f, 0.4f, 0.4f, 0.7f, 0.2f);//distance and lerp

                anim.SetIKPositionWeight(AvatarIKGoal.RightHand, PickIKHandWeight);//distance and lerp
                anim.SetIKPosition(AvatarIKGoal.RightHand, target.transform.position);
            }
        }
        else
        {
            PickIKHandWeight = 0;
            if (pushTarget != null)
            {
                PushIKHandWeight = Mathf.Lerp(PushIKHandWeight, 1, 0.01f);

                anim.SetIKPositionWeight(AvatarIKGoal.RightHand, PushIKHandWeight);//distance and lerp
                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, PushIKHandWeight);//distance and lerp

                anim.SetIKPosition(AvatarIKGoal.RightHand, pushTarget.transform.position);
                anim.SetIKPosition(AvatarIKGoal.LeftHand, pushTarget.transform.position);
            }
            else
            {
                if (anim.GetIKPositionWeight(AvatarIKGoal.LeftHand) > 0 || anim.GetIKPositionWeight(AvatarIKGoal.RightHand) > 0 || PushIKHandWeight >0)
                {
                    PushIKHandWeight = Mathf.Lerp(PushIKHandWeight, 0, 0.01f);
                    anim.SetIKPositionWeight(AvatarIKGoal.RightHand, PushIKHandWeight);
                    anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, PushIKHandWeight);//distance and lerp
                }
            }
        }
    }
}
