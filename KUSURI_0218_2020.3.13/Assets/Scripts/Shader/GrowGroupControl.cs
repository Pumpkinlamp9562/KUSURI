using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowGroupControl : MonoBehaviour
{
    [SerializeField] bool bigPlant;
    [SerializeField] float smooth = 0.1f;
/*    public Vector3 colliBigCenter;
    public Vector3 colliSizeBig;

    Vector3 ColliderCenter;
    Vector3 ColliderNormal;
*/
    bool colliderGrow;
    MeshFilter[] filter;
    List<MeshRenderer> growMeshes = new List<MeshRenderer>();
    [HideInInspector]
    public enum Grow {grow, normal, minify}
    public Grow grow;

    SkinnedMeshRenderer[] skin;
    List<PoisonItem> poison = new List<PoisonItem>();
    GameManager manager;
    AudioSource source;

    void Start()
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

        skin = GetComponentsInChildren<SkinnedMeshRenderer>();

        filter = GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < filter.Length; i++)
            if (filter[i].GetComponent<MeshRenderer>().material.name == "Grow Shader (Instance)")
                growMeshes.Add(filter[i].GetComponent<MeshRenderer>());

        /*collider = GetComponent<BoxCollider>();
        ColliderNormal = collider.size;
        ColliderCenter = collider.center;*/

        poison.AddRange(GetComponentsInChildren<PoisonItem>());
        grow = Grow.normal;
        UsedGrow();
    }

    public void UsedGrow()
    {
        switch (grow)
        {
            case Grow.grow:
                StopAllCoroutines();
                if (bigPlant)//Play Audio
                    source.PlayOneShot(manager.audios.grow_big, manager.audios.grow_big_v);
                else
                    source.PlayOneShot(manager.audios.grow_small, manager.audios.grow_small_v);
                if (skin == null) //UV
                    for (int i = 0; i < growMeshes.Count; i++)
                        StartCoroutine(Growing(1, i));
                else //ShapeKey
                {
                    for (int i = 0; i < skin.Length; i++)
                    {
                        StopAllCoroutines();
                        StartCoroutine(BlendShapeGrow(0, 0, i));
                    }
                }

                //StartCoroutine(ColliderGrowing(colliSizeBig, colliBigCenter)); //collider change
                StartCoroutine(Poison(true));
                if (poison.Count != 0 && poison[0] != null) //if have poison fruit 
                    for (int i = 0; i < poison.Count; i++)
                        poison[i].IsGrowed();
                
                break;
            case Grow.normal:
                StopAllCoroutines();

                if (skin == null) //UV
                    for (int i = 0; i < growMeshes.Count; i++)
                        StartCoroutine(Growing(0.5f, i));
                else //ShapeKey
                {
                    //Grow
                    for (int i = 0; i < skin.Length; i++)
                    {
                        StopAllCoroutines();
                        StartCoroutine(BlendShapeGrow(100, 0, i));
                    }
                    if (skin.Length != 1)
                        return;
                    if (skin[0].GetBlendShapeWeight(0) < 50)//Play Audio
                    {
                        if (bigPlant)
                            source.PlayOneShot(manager.audios.grow_big, manager.audios.grow_big_v);
                        else
                            source.PlayOneShot(manager.audios.grow_small, manager.audios.grow_small_v);
                    }
                    else
                    {
                        if (bigPlant)
                            source.PlayOneShot(manager.audios.growBack_big, manager.audios.growBack_big_v);
                        else
                            source.PlayOneShot(manager.audios.growBack_small, manager.audios.growBack_small_v);
                    }
                }
                StartCoroutine(Poison(true));

                //StartCoroutine(ColliderGrowing(ColliderNormal, ColliderCenter));//collider change
                break;
            case Grow.minify:
                StopAllCoroutines();
                if (bigPlant)//Play Audio
                    source.PlayOneShot(manager.audios.growBack_big, manager.audios.growBack_big_v);
                else
                    source.PlayOneShot(manager.audios.growBack_small, manager.audios.growBack_small_v);
                if (skin == null) //UV
                    for (int i = 0; i < growMeshes.Count; i++)
                        StartCoroutine(Growing(0, i));
                else //ShapeKey
                {
                    for (int i = 0; i < skin.Length; i++)
                    {
                        StopAllCoroutines();
                        StartCoroutine(BlendShapeGrow(100, 100, i));
                    }
                }

                //collider.enabled = false; //collider change

                StartCoroutine(Poison(false));
                break;
            default:
                break;
        }
    }

    void SkinMeshColliderCalculate(int i)
    {
        Mesh bakeMesh = new Mesh();
        skin[i].BakeMesh(bakeMesh);
        var collider = GetComponentsInChildren<MeshCollider>();
        collider[i].sharedMesh = bakeMesh;
    }

    IEnumerator Growing(float target, int i)
    {
        while (Mathf.Abs(growMeshes[i].material.GetFloat("_Grow")-target) > 0.01f)
        {
            growMeshes[i].material.SetFloat("_Grow", Mathf.Lerp(growMeshes[i].material.GetFloat("_Grow"), target, smooth));
            yield return new WaitForSeconds(0);
        }
    }

    IEnumerator Poison(bool tf)
    {
        yield return new WaitForSeconds(0.5f);
        //poison.Clear();
        //poison.AddRange(GetComponentsInChildren<PoisonItem>());
        if (poison.Count > 0)
            for (int i = 0; i < poison.Count; i++)
                    poison[i].gameObject.SetActive(tf);
    }

    //BlendShape Grow
    IEnumerator BlendShapeGrow(float target, float target2, int i)
    {
        while (Mathf.Abs(skin[i].GetBlendShapeWeight(0) - target) > 0.01f)
        {
            skin[i].SetBlendShapeWeight(0, Mathf.Lerp(skin[i].GetBlendShapeWeight(0), target, 0.01f));
            SkinMeshColliderCalculate(i);

            if(GetComponentInChildren<BlendPlatformPosition>() != null)
                GetComponentInChildren<BlendPlatformPosition>().Move();//move movePlatform with blendshape
            yield return new WaitForSeconds(0);
        }
        while (Mathf.Abs(skin[i].GetBlendShapeWeight(1) - target2) > 0.01f)
        {
            skin[i].SetBlendShapeWeight(1, Mathf.Lerp(skin[i].GetBlendShapeWeight(1), target2, 0.01f));
            SkinMeshColliderCalculate(i);

            if (GetComponentInChildren<BlendPlatformPosition>() != null)
                GetComponentInChildren<BlendPlatformPosition>().Move();//move movePlatform with blendshape
            yield return new WaitForSeconds(0);
        }
    }
}
