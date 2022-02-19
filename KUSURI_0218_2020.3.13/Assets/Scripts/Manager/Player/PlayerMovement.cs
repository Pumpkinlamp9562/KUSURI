using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    public float walkSpeed;
    [HideInInspector]
    public float runSpeed;
    public float speedSmoothTime = 1f;
    public float turnSmoothTime = 0.1f;
    public float speed;
    public float jumpForce = 4f;
    //public Transform stairCheck;
    //public float stepOffset;
    //public float stepSmooth = 0.75f;

    [Header("Stair")]
    public GameObject groundChecker;
    public float stairMaxHeight;
    public LayerMask ground;
    public float maxGroundAngle = 120;
    public bool debug;

    [Header("Height")]
    public float fallHeight = -15f;

    float SmoothVelocity;
    Vector3 groundAngle;
    PlayerManager player;
    GameManager manager;

    public float height;

    [Header("Ground & Stair Check")] 
    public bool haveStair;
    public bool grounded;

    Vector3 direction;

    [Header("Debug")]
    public GameObject upper;
    public GameObject lower;

    [SerializeField]
    bool readyJump;
    float jumpSpeed;

    float groundCheckRadius;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        player = GetComponent<PlayerManager>();
        speed = walkSpeed * transform.localScale.x;
        height = GetComponent<CapsuleCollider>().height;
        groundCheckRadius = GetComponent<CapsuleCollider>().radius;
    }

    void FixedUpdate()
    {
        CalculateDirection();
        CheckGround();

        PlayerFallDead();
        DrawDebugLines();
        //ClimbStep();
    }
    private void Update()
    {
        PushPosYLock();
    }

    void CalculateDirection()
    {
        float horizontal = manager.input.move.x;
        float vertical = manager.input.move.y;
        direction = new Vector3(-(vertical), 0, horizontal).normalized;

        //speed not able to lower than 0
        if (speed < 0)
            speed = 0;
        //Move&Turn!
        if (direction.magnitude > 0.1f)
        {
            CheckStair();
            CalculateGroundAngle();
            CalculateSpeed();
            Rotate();
            Move();
        }
        else
            speed = 0;
    }
    void CalculateSpeed()
    {
        runSpeed = walkSpeed * 2;
        if (readyJump)
        {
            if (speed > 0)//Is not run (run->walk)
                speed -= 4f * Time.deltaTime;
        }
        else
        {
            if (manager.input.run && speed < runSpeed || speed < 0.1f) //Is run (0->run/walk->run)
                speed += speedSmoothTime * Time.deltaTime;

            if (!manager.input.run && speed > walkSpeed)//Is not run (run->walk)
                speed -= speedSmoothTime * Time.deltaTime;

            if (!manager.input.run && speed < walkSpeed)//Is not run (0->walk)
                speed += speedSmoothTime * Time.deltaTime;
        }
    }
    void Rotate()
    {
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref SmoothVelocity, turnSmoothTime);

        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }
    void Move()
    {
        //transform.Translate(direction * speed * Time.deltaTime, Space.World);
        //charaControl.Move(moveDir.normalized * speed * Time.deltaTime);
        player.rigid.AddForce(direction * speed * 80,ForceMode.Acceleration);
    }

    public void ReadyToJump()
    {
        jumpSpeed = speed;
        readyJump = true;
    }
    public void ReadyJumpFalse()
    {
        speed = jumpSpeed;
        readyJump = false;
    }

    public void Jump()
    {
        player.rigid.constraints = RigidbodyConstraints.None;
        player.rigid.freezeRotation = true;
        player.rigid.velocity = transform.up * jumpForce;
    }

    void PushPosYLock() {
        if (haveStair || !grounded || 
            ((player.anim.anim.GetCurrentAnimatorStateInfo(0).IsName("jump_up")|| player.anim.anim.GetCurrentAnimatorStateInfo(0).IsName("stay") 
            || player.anim.anim.GetCurrentAnimatorStateInfo(0).IsName("jump_down"))))
        {
            player.rigid.constraints = RigidbodyConstraints.None;
            player.rigid.freezeRotation = true;
        }
        if (!haveStair && !player.anim.anim.GetCurrentAnimatorStateInfo(0).IsName("jump_up") && !player.anim.anim.GetCurrentAnimatorStateInfo(0).IsName("stay")
            && !player.anim.anim.GetCurrentAnimatorStateInfo(0).IsName("jump_down") && grounded && player.rigid.velocity.y <= 0)
        {
            player.rigid.constraints = RigidbodyConstraints.FreezePositionY;
            player.rigid.freezeRotation = true;
        }
    }
    void CheckStair()
    {
        Vector3 boxCheckPos = groundChecker.transform.position + groundChecker.transform.forward/2 + new Vector3(0, stairMaxHeight * transform.localScale.x / 2 + 0.01f, 0);
        Vector3 boxCheckScale = new Vector3(0.25f, stairMaxHeight * transform.localScale.x / 2, 0.5f / 2);

        Vector3 upperBoxPos = groundChecker.transform.position + groundChecker.transform.forward/2 + new Vector3(0, stairMaxHeight * transform.localScale.x + 0.01f + 0.025f, 0);
        Vector3 upperBoxScale = new Vector3(0.25f, 0.025f, 0.5f / 2);
        float distance = 0.5f * transform.localScale.x;

        upper.transform.position = upperBoxPos;
        upper.transform.localScale = upperBoxScale * 2;
        lower.transform.position = boxCheckPos;
        lower.transform.localScale = boxCheckScale * 2;


        if ((Physics.CheckBox(boxCheckPos, 
            Vector3.Scale(boxCheckScale, transform.localScale),new Quaternion(0,0,0,0), ground))
            && !(Physics.CheckBox(upperBoxPos,
            Vector3.Scale(upperBoxScale, transform.localScale), new Quaternion(0, 0, 0, 0), ground)))
        {
            haveStair = true;
        }
        else
        {
            haveStair = false;
        }
    }
    void CalculateGroundAngle()
    {
        if (!grounded && !haveStair)
        {
            groundAngle = Vector3.zero;
        }
        if (haveStair && !player.anim.anim.GetCurrentAnimatorStateInfo(0).IsTag("Jump"))
        {
            RaycastHit hit;
            RaycastHit hit2;
            Physics.Raycast(groundChecker.transform.position, -transform.up, out hit, 1f, ground);
            Physics.Raycast(groundChecker.transform.position, transform.forward, out hit2, 1f, ground);
            if (hit.normal != hit2.normal)
            {
                if(hit.collider != null && hit2.collider != null)
                {
                    if (hit.collider.GetComponent<WhiteBloodCell>() == null && hit2.collider.GetComponent<WhiteBloodCell>() == null)
                    {
                        groundAngle = Vector3.Normalize(groundChecker.transform.up + new Vector3(0, 1, 0));
                        player.rigid.velocity += (groundAngle * 0.4f);
                    }
                }
            }
        }
    }

    void CheckGround()
    {
        if ((Physics.CheckBox(groundChecker.transform.position,new Vector3(groundCheckRadius, groundCheckRadius, 0.05f),Quaternion.identity, ground,QueryTriggerInteraction.Ignore)))
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }
    }

    void PlayerFallDead()
    {
        float fall = fallHeight;
        foreach(PlayerManager.State s in player.potionState)
        {
            if (s == PlayerManager.State.timeBig) fall = fallHeight * 2;
            else fall = fallHeight;
        }
        if (player.rigid.velocity.y < fall)
        {
            if ((Physics.CheckSphere(groundChecker.transform.position, 0.4f, ground)))
            {
                player.Dead();
                Debug.Log(player.rigid.velocity.y);
            }
        }
    }

    void DrawDebugLines()
    {
        upper.SetActive(debug);
        lower.SetActive(debug);
        if (!debug) return;

        Debug.DrawRay(groundChecker.transform.position, -groundChecker.transform.up * 2, Color.green);

        Debug.DrawRay(groundChecker.transform.position, Vector3.Normalize(groundChecker.transform.forward + groundAngle) * 2, Color.yellow);
    }
}
