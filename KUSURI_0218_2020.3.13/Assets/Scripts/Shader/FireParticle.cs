using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireParticle : MonoBehaviour
{
    [SerializeField] float size = 0.8f;
    AudioSource source;
    // Start is called before the first frame update
    void OnEnable()
    {
        AudioClipManager audios = GameObject.FindGameObjectWithTag("GameManager").GetComponent<AudioClipManager>();
        if (gameObject.GetComponent<AudioSource>() == null)
            source = gameObject.AddComponent<AudioSource>();
        else
            source = gameObject.GetComponent<AudioSource>();
        source.outputAudioMixerGroup = audios.vfxAudio.outputAudioMixerGroup;
        source.spatialBlend = audios.vfxAudio.spatialBlend;
        AnimationCurve curve = audios.vfxAudio.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
        source.rolloffMode = AudioRolloffMode.Custom;
        source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);
        source.maxDistance = audios.vfxAudio.maxDistance;
        source.PlayOneShot(audios.fire, audios.fire_v);
        if(gameObject.transform.parent.gameObject != null)
        {
            if (gameObject.transform.parent.GetComponent<MeshFilter>() != null)
            {
                GetComponent<MeshFilter>().mesh = gameObject.transform.parent.GetComponent<MeshFilter>().mesh;
                ParticleSystem[] particle = GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i < particle.Length; i++)
                {
                    particle[i].transform.localScale = gameObject.transform.parent.GetComponent<MeshFilter>().mesh.bounds.size * size;
                }
            }

        }
    }
}
