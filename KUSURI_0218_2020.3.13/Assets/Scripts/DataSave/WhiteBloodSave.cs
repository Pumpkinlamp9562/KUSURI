using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteBloodSave : MonoBehaviour
{
    public List<GameObject> whiteBloodObjectsChild = new List<GameObject>();
    public List<GameObject> whiteBloodObjectsFindAtRuntime;
    public List<GameObject> whiteBloodCore = new List<GameObject>();
    public List<Transform> coreTrans = new List<Transform>();
    GameObject[] whiteBloodObjectsPrefab;
    [SerializeField]
    SaveSetting save;


    //Get All The WhiteBloodObjects and child In Start, In Order To Recognize Their Name
    public void WhiteBloodPrefabsUpdate()
    {
        whiteBloodObjectsChild.Clear();
        whiteBloodCore.Clear();
        coreTrans.Clear();
        whiteBloodObjectsPrefab = Resources.LoadAll<GameObject>("WhiteBloodObjects");

        //Get All the GameObjects in Prefab and save it in whiteBloodObjectsChild
        for (int i = 0; i < whiteBloodObjectsPrefab.Length; i++)
        {
            Transform[] child = whiteBloodObjectsPrefab[i].GetComponentsInChildren<Transform>();
            for (int f = 0; f < child.Length; f++)
            {
                whiteBloodObjectsChild.Add(child[f].gameObject);
                if (child[f].gameObject.GetComponent<WhiteBloodCore>() != null)
                {//Get All the whiteBloodCore Data, in order to respawn
                    whiteBloodCore.Add(child[f].gameObject);
                    coreTrans.Add(child[f]);
                }
            }
        }
    }

    //Save Target WhiteBloodObjects State In Data
    public void WhiteBloodObjectsSave(List<GameObject> target)
    {
        List<string> name = new List<string>();
        List<bool> active = new List<bool>();
        for (int i = 0; i < target.Count; i++)
        {
            name.Add(target[i].name); //Add target objects name in data
            if(target[i].GetComponent<Collider>() != null)
                active.Add(target[i].GetComponent<Collider>().enabled); //Add target objects active bool in data
            else if (target[i].GetComponent<SkinnedMeshRenderer>() != null)
                active.Add(target[i].GetComponent<SkinnedMeshRenderer>().enabled);
            else if (target[i].GetComponent<MeshRenderer>() != null)
                active.Add(target[i].GetComponent<MeshRenderer>().enabled);
        }
        save.bloodData.bloodObjectsName = name.ToArray();
        save.bloodData.bloodObjectsActive = active.ToArray();
    }

    public void WhiteBloodObjectsLoad(List<GameObject> target)
    {
        foreach (GameObject g in target)
        {
            for (int i = 0; i < save.bloodData.bloodObjectsName.Length; i++)
            {
                if (save.bloodData.bloodObjectsName[i] == g.name)
                {
                    if (g.GetComponent<MeshCollider>() != null)
                        g.GetComponent<MeshCollider>().enabled = save.bloodData.bloodObjectsActive[i];
                    if (g.GetComponent<Collider>() != null)
                        g.GetComponent<Collider>().enabled = save.bloodData.bloodObjectsActive[i]; //Set GameObjects active bool same to data bool
                    if (g.GetComponent<CapsuleCollider>() != null)
                        g.GetComponent<CapsuleCollider>().enabled = save.bloodData.bloodObjectsActive[i];
                    if (g.GetComponent<MeshRenderer>() != null)
                        g.GetComponent<MeshRenderer>().enabled = save.bloodData.bloodObjectsActive[i];
                    if (g.GetComponent<SkinnedMeshRenderer>() != null)
                        g.GetComponent<SkinnedMeshRenderer>().enabled = save.bloodData.bloodObjectsActive[i];
                    if (g.GetComponentInChildren<WhiteBloodCore>() == null && g.GetComponent<WhiteBloodCell>() != null) //if it is a WhiteBloodCell and dont have core
                        WhiteBloodWallSetActive(g.GetComponent<WhiteBloodCell>().whiteBloodWall.gameObject, save.bloodData.bloodObjectsActive[i]);//Setting the wall
                    if(g.GetComponent<WhiteBloodCore>() != null) //if it is a WhiteBloodCell and have core
                        WhiteBloodWallSetActive(g.GetComponent<WhiteBloodCore>().whiteBloodWall.gameObject, save.bloodData.bloodObjectsActive[i]);//Setting the wall
                }
                for (int f = 0; f < whiteBloodCore.Count; f++)
                {
                    if (whiteBloodCore[f].name == g.name) //Respawn White Blood Core
                    {
                        g.transform.position = coreTrans[f].position;
                        g.transform.rotation = coreTrans[f].rotation;
                        g.transform.localScale = coreTrans[f].localScale;
                        WhiteBloodCoreUnparent(g);
                    }
                }
            }
        }
    }

    //Get All The WhiteBloodObjects at RunTime, base on the whiteBloodObjectsChild found at Start()
    public void WhiteBloodObjectsFindAtRuntime()
    {
        whiteBloodObjectsFindAtRuntime.Clear();
        for (int i = 0; i < whiteBloodObjectsChild.Count; i++)
        {
            if (GameObject.Find(whiteBloodObjectsChild[i].name) != null)
            {
                whiteBloodObjectsFindAtRuntime.Add(GameObject.Find(whiteBloodObjectsChild[i].name));
            }
        }
    }
    //Check parent
    public void WhiteBloodCoreUnparent(GameObject core)
    {
        GameObject cell = core.GetComponent<WhiteBloodCore>().whiteBloodCell;
        if (cell.GetComponent<WhiteBloodCell>() != null)
        {
            if (cell.GetComponent<MeshCollider>() != null)
            {
                if (cell.GetComponent<MeshCollider>().enabled == false)
                {
                    cell.GetComponent<WhiteBloodCell>().DestoryAndHaveChlid();
                }
            }
            else
            {
                if (cell.GetComponent<SkinnedMeshRenderer>() != null)
                    if (cell.GetComponent<SkinnedMeshRenderer>().enabled == false)
                        cell.GetComponent<WhiteBloodCell>().DestoryAndHaveChlid();
            }
        }
    }

    void WhiteBloodWallSetActive(GameObject g, bool saveData)
    {
        if (g.GetComponent<WhiteWallDisappear>() != null)
        {
            g.GetComponent<WhiteWallDisappear>().WallsDisappear(saveData,false);
        }
    }

}
