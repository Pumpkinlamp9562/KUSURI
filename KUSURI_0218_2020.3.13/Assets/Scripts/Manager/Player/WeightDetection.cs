using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightDetection : MonoBehaviour
{
    public float objectsVelocity = 2;
    public float pressureWeight;
    float totalWeight;
    GameManager manager;
    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PotionItem") && other.GetComponent<Rigidbody>() != null)
        {
            totalWeight += other.GetComponent<Rigidbody>().mass;
            if (totalWeight >= pressureWeight && (Mathf.Abs(other.GetComponent<Rigidbody>().velocity.x) > objectsVelocity || 
                Mathf.Abs(other.GetComponent<Rigidbody>().velocity.y) > objectsVelocity || Mathf.Abs(other.GetComponent<Rigidbody>().velocity.z) > objectsVelocity))
            {
                manager.player.Dead();
                Debug.Log("¯{¦º");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PotionItem") && other.GetComponent<Rigidbody>() != null)
        {
            totalWeight -= other.GetComponent<Rigidbody>().mass;
        }
    }
}
