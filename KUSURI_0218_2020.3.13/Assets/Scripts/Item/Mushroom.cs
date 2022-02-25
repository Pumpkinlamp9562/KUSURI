using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : MonoBehaviour
{
    public float power = 100f;
    public float bounceSize = 1.5f;

    Vector3 scale;
    GameObject mushroom;
    AudioSource source;
    GameManager manager;
    private void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        //Add AudioSource
        if (gameObject.GetComponent<AudioSource>() == null)
            source = gameObject.AddComponent<AudioSource>();
        else
            source = gameObject.GetComponent<AudioSource>();
        source.outputAudioMixerGroup = manager.audios.vfxAudio.outputAudioMixerGroup;
        source.spatialBlend = manager.audios.vfxAudio.spatialBlend;
        AnimationCurve curve = manager.audios.vfxAudio.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
        source.rolloffMode = AudioRolloffMode.Custom;
        source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);
        source.maxDistance = manager.audios.vfxAudio.maxDistance;

        mushroom = gameObject.transform.parent.gameObject;
        scale = mushroom.transform.localScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Rigidbody>() != null)
        {
            source.pitch = Random.Range(1, manager.audios.pitchRandom);
            source.PlayOneShot(manager.audios.mushroom, manager.audios.mushroom_v);
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
