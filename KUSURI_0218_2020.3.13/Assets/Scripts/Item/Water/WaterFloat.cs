using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterFloat : MonoBehaviour
{
    public float bounceDamp = 0.05f;
    public Vector3 buoyancyCentreOffset;

    public float waterLevel;
    float forceFactor;
    Vector3 actionPoint;
    Vector3 upLift;
    public List<Rigidbody> rigid = new List<Rigidbody>();
    public List<float> floatHeight = new List<float>();
    bool In;
    GameManager manager;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        waterLevel = gameObject.GetComponent<Collider>().bounds.size.y / 2 + gameObject.transform.position.y;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null && other.gameObject.layer != 10)
        {
            if (other.GetComponent<Rigidbody>().useGravity)
            {

                In = true;
                rigid.Add(other.GetComponent<Rigidbody>());
                PlayAudio(other);
                if (other.GetComponent<Rigidbody>().mass > 0.1f)
                    floatHeight.Add(other.GetComponent<Rigidbody>().mass);
                else
                    floatHeight.Add(0.1f);
            }
        }
    }

    void PlayAudio(Collider other)
    {
        AudioSource source;
        if (other.gameObject.GetComponent<AudioSource>() == null)
            source = other.gameObject.AddComponent<AudioSource>();
        else
            source = other.gameObject.GetComponent<AudioSource>();
        source.outputAudioMixerGroup = manager.audios.vfxAudio.outputAudioMixerGroup;
        source.spatialBlend = manager.audios.vfxAudio.spatialBlend;
        AnimationCurve curve = manager.audios.vfxAudio.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
        source.rolloffMode = AudioRolloffMode.Custom;
        source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);
        source.maxDistance = manager.audios.vfxAudio.maxDistance;
        source.pitch = Random.Range(1, manager.audios.pitchRandom);
        source.PlayOneShot(manager.audios.water, manager.audios.water_v);
        Destroy(other.gameObject.GetComponent<AudioSource>(), 2f);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null && other.gameObject.layer != 10)
        {
            if (other.GetComponent<Rigidbody>().useGravity)
            {
                for (int i = 0; i < rigid.Count; i++)
                {
                    if(rigid[i] != null)
                    {
                        if (rigid[i].gameObject.name == other.gameObject.name)
                        {
                            rigid.Remove(rigid[i]);
                            floatHeight.Remove(floatHeight[i]);
                        }
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (In) ObjectFloat();
        if(rigid.Count == 0) In = false;
    }

    void ObjectFloat()
    {
        for(int i = 0; i < rigid.Count; i++)
        {
            if (rigid[i] != null)
            {
                actionPoint = rigid[i].transform.position + rigid[i].transform.TransformDirection(buoyancyCentreOffset);
                forceFactor = 1f - ((actionPoint.y - waterLevel) / floatHeight[i]);

                if (forceFactor > 0f)
                {
                    upLift = -Physics.gravity * (forceFactor - rigid[i].velocity.y * bounceDamp);
                    rigid[i].AddForceAtPosition(upLift, actionPoint);
                }
            }
        }
    }
}
