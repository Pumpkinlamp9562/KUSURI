using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : MonoBehaviour
{
    public float power = 100f;
    public float bounceSize = 1.5f;

    Vector3 scale;
    GameObject mushroom;

    private void Start()
    {
        mushroom = gameObject.transform.parent.gameObject;
        scale = mushroom.transform.localScale;
    }

    private void OnTriggerEnter(Collider other)
    {

        if(other.GetComponent<Rigidbody>() != null)
        {


            StartCoroutine(BounceAnim(scale*bounceSize));
            Rigidbody Collrigid;
            Collrigid = other.GetComponent<Rigidbody>();
            Collrigid.AddForce(mushroom.transform.up * -Collrigid.velocity.y * Collrigid.mass * power, ForceMode.Force);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null)
        {
            StopAllCoroutines();
            StartCoroutine(BounceAnim(scale));
        }
    }

    IEnumerator BounceAnim(Vector3 target)
    {
        while (Mathf.Abs(mushroom.transform.localScale.x - target.x) > 0.1f)
        {
            mushroom.transform.localScale = Vector3.Lerp(mushroom.transform.localScale, target, 0.1f);
            yield return null;
        }
    }
}
