using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteWallDisappear : MonoBehaviour
{
    [SerializeField]
    Transform wallCamera;
    public GameObject[] objectWalls;
    [SerializeField]
    float smooth = 0.005f;
    public List<Material> materials = new List<Material>();

    SkinnedMeshRenderer skinMesh;
    MeshRenderer mesh;
    GameManager manager;
    // Start is called before the first frame update
    void Awake()
    {
        if(wallCamera != null)
            manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        if (GetComponent<SkinnedMeshRenderer>() != null)
        {
            skinMesh = GetComponent<SkinnedMeshRenderer>();
            materials.Add(skinMesh.material);
            if (!skinMesh.enabled)
            {
                for (int i = 0; i < objectWalls.Length; i++)
                {
                    SetActiveCustom(objectWalls[i], false);
                    SetActiveCollider(objectWalls[i], false);
                }
            }
        }
        if (GetComponent<MeshRenderer>() != null)
        {
            mesh = GetComponent<MeshRenderer>();
            materials.Add(mesh.material);
            if (!mesh.enabled)
            {
                for (int i = 0; i < objectWalls.Length; i++)
                {
                    SetActiveCustom(objectWalls[i], false);
                    SetActiveCollider(objectWalls[i], false);
                }
            }
        }
        for (int i = 0; i < objectWalls.Length; i++)
        {
            if (objectWalls[i].GetComponent<MeshRenderer>() != null)
                materials.Add(objectWalls[i].GetComponent<MeshRenderer>().material);
            if (objectWalls[i].GetComponent<SkinnedMeshRenderer>() != null)
                materials.Add(objectWalls[i].GetComponent<SkinnedMeshRenderer>().material);
        }
    }

    public void WallsDisappear(bool active, bool camera)
    {
        for (int i = 0; i < materials.Count; i++)
        {
            StartCoroutine(Alpha(materials[i], active, camera));
        }
    }

    void SetActiveCustom(GameObject target, bool tf)
    {
        if (target.GetComponent<MeshRenderer>() != null)
            target.GetComponent<MeshRenderer>().enabled = tf;
        if (target.GetComponent<SkinnedMeshRenderer>() != null)
            target.GetComponent<SkinnedMeshRenderer>().enabled = tf;
    }
    void SetActiveCollider(GameObject target, bool tf)
    {
        if (target.GetComponent<CapsuleCollider>() != null)
            target.GetComponent<CapsuleCollider>().enabled = tf;
        if (target.GetComponent<BoxCollider>() != null)
            target.GetComponent<BoxCollider>().enabled = tf;
        if (target.GetComponent<MeshCollider>() != null)
            target.GetComponent<MeshCollider>().enabled = tf;
    }

    IEnumerator Alpha(Material m, bool active,bool camera)
    {
        if (!active)
        {
            if (camera)
            {
                manager.cam.whiteBloodCam = wallCamera;
                manager.cam.wallDestoryed = true;
            }
            while (m.GetFloat("_Disappear1") > 0)
            {
                m.SetFloat("_Disappear1", Mathf.Lerp(m.GetFloat("_Disappear1"), 0, smooth));
                SetActiveCollider(gameObject, false);
                for (int i = 0; i < objectWalls.Length; i++)
                {
                    SetActiveCollider(objectWalls[i], false);
                }
                if (m.GetFloat("_Disappear1") < 0.01f)
                {
                    SetActiveCustom(gameObject, false);
                    for (int i = 0; i < objectWalls.Length; i++)
                    {
                        SetActiveCustom(objectWalls[i], false);
                        m.SetFloat("_Disappear1", 0);
                    }
                }
                if(m.GetFloat("_Disappear1") < 0.3f)
                {
                    if (camera)
                    {
                        manager.cam.wallDestoryed = false;
                        manager.cam.whiteBloodCam = null;
                    }
                }
                yield return null;
            }
        }
        else
        {
            m.SetFloat("_Disappear1", 1);
            SetActiveCollider(gameObject, true);
            SetActiveCustom(gameObject, true);
            for (int i = 0; i < objectWalls.Length; i++)
            {
                SetActiveCollider(objectWalls[i], true);
            }
            for (int i = 0; i < objectWalls.Length; i++)
            {
                SetActiveCustom(objectWalls[i], true);
                m.SetFloat("_Disappear1", 1);
            }

            yield return null;
        }
    }
}
