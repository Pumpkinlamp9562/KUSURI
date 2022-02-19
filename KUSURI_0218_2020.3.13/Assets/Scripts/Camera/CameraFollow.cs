using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Follow Setting")]
    public Transform player;
    public Vector3 offset;
    public Quaternion rotation;
    public float moveSpeed = 0.3f;
    public float rotateSpeed = 1;


    [Header("Player Potion Camera Setting")]
    public Vector3 turnBigCameraOffset = new Vector3(6.34f, 3.2f, 0);
    public Vector3 turnSmallCameraOffset = new Vector3(1.91f, 0.86f, 0);

    [Header("Camera Collision Setting")]
    public LayerMask collisionLayer;
    public float maxDistance = 4f;
    public float minDintance = 1f;
    [Range(0,1)]
    public float cameraOffset = 0.8f;
    Vector3 _dir;
    float dis;

    [Header("DisappearWall Detect Setting")]
    public LayerMask layerMask;
    public float smoothness = 3f;
    public GameObject[] walls;
    public float detectHeight;
    public float detectWidth;
    public Collider[] hitInfo;

    [HideInInspector]
    public Transform target;
    [HideInInspector]
    public bool playerInOtherCamArea;

    [HideInInspector]
    public bool playerInOtherFollowCamArea;
    [HideInInspector]
    public Quaternion followRotate;
    [HideInInspector]
    public Vector3 followOffset;

    [HideInInspector]
    public Transform whiteBloodCam;
    [HideInInspector]
    public bool wallDestoryed;

    Vector3 velocity = Vector3.zero;
    Vector3 velocity2 = Vector3.zero;
    public bool restart;
    private void Awake()
    {
        walls = GameObject.FindGameObjectsWithTag("DisappearWall");
        _dir = transform.localPosition.normalized;
        dis = transform.localPosition.magnitude;
    }

    void LateUpdate()
    {
        ChangeCamera();
        DetectDisappearWall();
    }

    void ChangeCamera()
    {//rotate
        if (playerInOtherCamArea || playerInOtherFollowCamArea || wallDestoryed)
        {
            if (playerInOtherCamArea)
                gameObject.transform.parent.transform.rotation = Quaternion.Slerp(gameObject.transform.parent.transform.rotation, target.rotation, rotateSpeed * Time.fixedDeltaTime);
            if (playerInOtherFollowCamArea)
                gameObject.transform.parent.transform.rotation = Quaternion.Slerp(gameObject.transform.parent.transform.rotation, followRotate, rotateSpeed * Time.fixedDeltaTime);
            if(wallDestoryed)
                gameObject.transform.parent.transform.rotation = Quaternion.Slerp(gameObject.transform.parent.transform.rotation, whiteBloodCam.rotation, rotateSpeed * Time.fixedDeltaTime);
        }
        else
            gameObject.transform.parent.transform.rotation = Quaternion.Slerp(gameObject.transform.parent.transform.rotation, rotation, rotateSpeed * Time.fixedDeltaTime); //Look At Player

        //position
        if (playerInOtherCamArea || playerInOtherFollowCamArea || wallDestoryed)
        {
            if (playerInOtherCamArea)
                Detect(target.position);
            if (playerInOtherFollowCamArea)
                Detect(player.position + followOffset);
            if (wallDestoryed)
            {
                Detect(whiteBloodCam.position);
            }
        }
        else
            Detect(player.position + offset);
    }

    public void PotionCamera(int target)
    {
        UnityEngine.Rendering.Universal.UniversalAdditionalCameraData additionalCameraData = transform.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        additionalCameraData.SetRenderer(target);
    }

    void Detect(Vector3 target)
    {
        /*RaycastHit hitInfo;
        _dir = gameObject.transform.parent.position - player.position;
        if (Physics.Linecast(gameObject.transform.localPosition + (-gameObject.transform.forward), gameObject.transform.position + (gameObject.transform.forward * 4), out hitInfo, collisionLayer) && !restart)
        {
            dis = Mathf.Clamp((hitInfo.distance * cameraOffset), minDintance, maxDistance);
            gameObject.transform.localPosition = Vector3.SmoothDamp(gameObject.transform.localPosition, gameObject.transform.parent.position + _dir.normalized * dis, ref velocity2, 1f);
        }
        else
            gameObject.transform.localPosition = Vector3.SmoothDamp(gameObject.transform.localPosition, Vector3.zero, ref velocity2, 1f);*/
        gameObject.transform.parent.position = Vector3.SmoothDamp(gameObject.transform.parent.position, target, ref velocity, moveSpeed);
    }
    void DetectDisappearWall()
    {
        Vector3 center = (gameObject.transform.position + player.position) / 2;
        Vector3 size = new Vector3(detectHeight, detectWidth, Vector3.Distance(gameObject.transform.position, player.position) - 0.5f);
        hitInfo = Physics.OverlapBox(center, size / 2, gameObject.transform.rotation, layerMask);

        foreach (Collider c in hitInfo)
        {
            if (c != null)
            {
                foreach (GameObject g in walls)
                {
                    if (c.gameObject == g)
                    {
                        g.GetComponent<Renderer>().material.SetFloat("_Alphaclip", Mathf.Lerp(g.GetComponent<Renderer>().material.GetFloat("_Alphaclip"), 2, smoothness * Time.fixedDeltaTime));
                    }
                }
            }
        }
        if (hitInfo.Length <= 0)
        {
            if (walls != null)
            {
                foreach (GameObject g in walls)
                {
                    if (g.GetComponent<Renderer>().material.GetFloat("_Alphaclip") != -1)
                        g.GetComponent<Renderer>().material.SetFloat("_Alphaclip", Mathf.Lerp(g.GetComponent<Renderer>().material.GetFloat("_Alphaclip"), 0.8f, smoothness * Time.fixedDeltaTime));
                }//try to change boxcollider other find a way to regonize which wall is not inneed
            }
        }
    }


}
