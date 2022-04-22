using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionMouseOn : MonoBehaviour
{
    [SerializeField]
    bool debug;
    [Header("RayCast Setting")]
    [SerializeField]
    GameObject rightHand;
    public bool mouseOn;
    public LayerMask rayLayer;
    [Header("Parabola")]
    [SerializeField]
    float force;
    [SerializeField]
    int resolution = 50;
    [Header("Select Outline")]
    [SerializeField]
    [ColorUsage(true, true)]
    private Color outlineColor = Color.yellow;
    [SerializeField]
    private float outlineWidth = 0.5f;

    bool canHit;
    bool 抛射 = false;   //抛射：仰角 > 45°，否：仰角 < 45°
    bool fixedUpdate = false;
    bool fixedUpdate1 = false;
    GameManager manager;
    GameObject[] potions;
    GameObject thrown;
    LineRenderer line;
    RaycastHit hitData;

    List<GameObject> hitObjects = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        line = manager.GetComponent<LineRenderer>();
        potions = Resources.LoadAll<GameObject>("Models/Other_Potions");
    }

    // Update is called once per frame
    void Update()
    {
        if (mouseOn)
        {
            fixedUpdate = true;

            Ray ray = Camera.main.ScreenPointToRay(manager.input.pointOnScreen);
            if (Physics.Raycast(ray, out hitData, Mathf.Infinity, rayLayer))
            {
                if (debug)
                    Debug.Log(hitData.transform.gameObject.name);
                SelectOutline(hitData.collider.gameObject);

                //distance = Vector3.Distance(rightHand.transform.position, hitData.point);
                //angle = Mathf.Asin((distance * Physics2D.gravity.y) / (force * force)) / 2 * Mathf.Rad2Deg;
                fixedUpdate1 = true;
                line.enabled = true;
                if (manager.input.comfirm)
                {
                    line.enabled = false;
                    if(thrown != null)
                        thrown.transform.parent = null;
                    if (thrown != null)
                        thrown.AddComponent<SphereCollider>().isTrigger = true;
                    if (thrown != null)
                        thrown.AddComponent<Rigidbody>().velocity = thrown.transform.forward * force;
                    manager.cam.PotionCamera(0);
                    fixedUpdate = false;
                    fixedUpdate1 = false;
                    CancelOutline();
                    mouseOn = false;
                }
                if (manager.input.cancel)
                {
                    CancelOutline();
                    Cancelthrown();
                }
            }
            else
            {
                if(hitData.collider != null)
                if(hitData.collider.gameObject.GetComponent<Outline>() != null)
                    Destroy(hitData.collider.gameObject.GetComponent<Outline>());
            }
        }
    }

    [System.Obsolete]
    private void FixedUpdate()
    {
        if (fixedUpdate &&  thrown != null)
        {
            thrown.transform.position = rightHand.transform.position;
        }
        if (fixedUpdate1)
        {
            RotateToMouseDirection(thrown, hitData.point);
            GenerateLine(hitData.point);
        }
    }

    void SelectOutline(GameObject hit)
    {
        if (hit.layer == 9 && canHit)
            hitObjects.Add(hit);
        
        for (int i = 0; i < hitObjects.Count; i++)
        {
            if (hit == hitObjects[i])
            {
                if (hit.layer == 9 && canHit)
                {
                    if (hitObjects[i].GetComponent<Outline>() == null)
                        hitObjects[i].AddComponent<Outline>();
                    else
                    {
                        hitObjects[i].GetComponent<Outline>().OutlineMode = Outline.Mode.OutlineVisible;
                        hitObjects[i].GetComponent<Outline>().OutlineColor = outlineColor;
                        hitObjects[i].GetComponent<Outline>().OutlineWidth = outlineWidth;
                    }
                }
            }
            else
            {
                if(hitObjects[i] != null)
                if (hitObjects[i].GetComponent<Outline>() == null)
                    hitObjects.Remove(hitObjects[i]);
                else
                {
                    Destroy(hitObjects[i].GetComponent<Outline>());
                    hitObjects.Remove(hitObjects[i]);
                }
            }
        }
    }
    void CancelOutline()
    {
        for (int i = 0; i < hitObjects.Count; i++)
        {
            if (hitObjects[i] != null)
            {
                if ((hitObjects[i].GetComponent<Outline>()) != null)
                    Destroy(hitObjects[i].GetComponent<Outline>());
                hitObjects.Remove(hitObjects[i]);
            }
        }
    }
    public void Cancelthrown()
    {
        if(thrown != null)
            thrown.GetComponent<PotionHit>().Cancel();
        line.enabled = false;
        manager.cam.PotionCamera(0);
        mouseOn = false;
        fixedUpdate = false;
        fixedUpdate1 = false;
        manager.ui.UI_Update();
    }

    //Throw
    void RotateToMouseDirection(GameObject obj, Vector3 destination)
    {
        Vector3 direction = destination - rightHand.transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);

        Vector3 finalAngle = rotation.eulerAngles;
        float targetAng = Angle(destination);
        finalAngle = new Vector3(-targetAng, finalAngle.y, finalAngle.z);//注意正负

        obj.transform.localRotation = Quaternion.Euler(finalAngle);
    }

    float Angle(Vector3 target)
    {
        float angleX;
        float distX = Vector2.Distance(new Vector2(target.x, target.z), new Vector2(thrown.transform.position.x, thrown.transform.position.z));
        float distY = target.y - thrown.transform.position.y;
        float posBase = (Physics.gravity.y * Mathf.Pow(distX, 2.0f)) / (2.0f * Mathf.Pow(force, 2.0f));
        float posX = distX / posBase;
        float posY = (Mathf.Pow(posX, 2.0f) / 4.0f) - ((posBase - distY) / posBase);
        if (posY >= 0.0f)
        {
            if (抛射)
                angleX = Mathf.Rad2Deg * Mathf.Atan(-posX / 2.0f + Mathf.Pow(posY, 0.5f));
            else
                angleX = Mathf.Rad2Deg * Mathf.Atan(-posX / 2.0f - Mathf.Pow(posY, 0.5f));
        }
        else
        {
            angleX = 45.0f;
        }
        return angleX;
    }

    [System.Obsolete]
    void GenerateLine(Vector3 target)
    {
        line.SetVertexCount(resolution + 1);

        Vector3 velocity = thrown.transform.forward * force;
        Vector3 position = rightHand.transform.position;

        for (int i = 0; i <= resolution; i++)
        {
            if (Vector3.Distance(position, hitData.point) > 1)
            {
                velocity += Physics.gravity * Time.fixedDeltaTime;
                position += velocity * Time.fixedDeltaTime;
                line.SetPosition(i, position);
                canHit = false;
            }
            else
            {
                canHit = true;
                line.SetPosition(i, position);
            }
        }
    }

    public void GetPotion(int i)
    {
        thrown = Instantiate(potions[i], rightHand.transform);//then mouseOn
        thrown.transform.parent = null;
    }
}
