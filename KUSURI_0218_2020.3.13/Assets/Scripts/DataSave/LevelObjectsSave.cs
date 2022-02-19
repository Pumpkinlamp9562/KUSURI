using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObjectsSave : MonoBehaviour
{
    public GameObject[] objectsSave;
    public List<GameObject> ObjectChild = new List<GameObject>();
    [SerializeField]
    SaveSetting save;

    //LevelObjects Save while player OnTrigger SaveArea
    public void UpdateLevelObjectsChild(GameObject collider)
    {
        Transform[] transforms;
        save.lastSavePoint = collider;
        for (int i = 0; i < save.savePoints.Length; i++)
        {
            if (save.savePoints[i] == collider)
            {
                transforms = save.levelObjects[i].transform.GetComponentsInChildren<Transform>();
                ObjectChild.Clear();
                foreach (Transform t in transforms)
                {
                    ObjectChild.Add(t.gameObject);
                }
            }
        }
    }

    //LevelObjects Reload
    public void ReloadLevelObjects()
    {
        for (int i = 0; i < save.savePoints.Length; i++)
        {
            if (save.savePoints[i] == save.lastSavePoint)
            {
                if (objectsSave.Length < i)
                    return;
                GameObject clone = Instantiate(objectsSave[i], null);
                foreach (GameObject g in ObjectChild)
                {
                    Destroy(g);
                }
                ObjectChild.Clear();
                save.levelObjects[i] = clone;
                Resources.UnloadUnusedAssets();
            }
        }
        UpdateLevelObjectsChild(save.lastSavePoint);
    }

}
