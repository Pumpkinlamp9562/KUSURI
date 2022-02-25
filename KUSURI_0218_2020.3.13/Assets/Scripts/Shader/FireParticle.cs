using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireParticle : MonoBehaviour
{
    MeshFilter mesh;
    AudioSource source;
    // Start is called before the first frame update
    void Start()
    {
        source = gameObject.GetComponent<AudioSource>();
        AudioClipManager audios = GameObject.FindGameObjectWithTag("GameManager").GetComponent<AudioClipManager>();
        source = audios.vfxAudio;
        source.PlayOneShot(audios.fire, audios.fire_v);
        mesh = GetComponent<MeshFilter>();
        if(gameObject.transform.parent != null)
        {
            if (gameObject.transform.parent.GetComponent<MeshFilter>() != null)
            {
                mesh.mesh = gameObject.transform.parent.GetComponent<MeshFilter>().mesh;
                for(int i = 0; i < gameObject.transform.childCount; i++)
                {
                    gameObject.transform.GetChild(i).transform.localScale = gameObject.transform.parent.GetComponent<MeshFilter>().mesh.bounds.size/2 * 
                        ((gameObject.transform.localScale.x + gameObject.transform.localScale.y + gameObject.transform.localScale.z)/2);
                }
            }

        }
    }
}
