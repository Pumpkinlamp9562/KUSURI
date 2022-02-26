using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class JsonHelper
{
    public static T[] getJsonArray<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    public static string arrayToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T> { array = array };
        string json = JsonUtility.ToJson(wrapper);
        var pos = json.IndexOf(":");
        json = json.Substring(pos + 1); // cut away "{ \"array\":"
        pos = json.LastIndexOf('}');
        json = json.Substring(0, pos); // cut away "}" at the end
        return json;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}
public class SaveSetting : MonoBehaviour 
{
    public Vector3[] levelSpawnPoint;
    [Header("Save Setting")]
    public GameObject[] levelObjects;
    public GameObject[] savePoints;
    public Database data;
    public BackPackItem backpack;
    public BloodCellDatabase bloodData;

    [Header("White Blood Objects")]
    [SerializeField]
    WhiteBloodSave whiteSave;

    [Header("Level Objects")]
    public LevelObjectsSave levelSave;
    [Header("Other")]
    public GameObject startUI;
    public GameObject lastSavePoint;
    GameManager manager;

    private void Awake()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    private void Start()
    {
        if (savePoints.Length != 0)
        {
            lastSavePoint = savePoints[0];
            levelSave.UpdateLevelObjectsChild(lastSavePoint);
        }
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Start_UI")
            levelSave.objectsSave = Resources.LoadAll<GameObject>("LevelObjects/" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        whiteSave.WhiteBloodPrefabsUpdate();

        LoadSave();
        manager.scenes.SceneSpawnPoint("none");
        manager.ui.UI_Update();

    }

    //LoadSave while GameStart
    public void LoadSave()
    {
        Time.timeScale = 1;
        StreamReader file;
        if (!System.IO.File.Exists(Application.dataPath + "/Data"))
            System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath + "/Data"),JsonUtility.ToJson(data));
        file = new StreamReader(System.IO.Path.Combine(Application.dataPath + "/Data"));
        string loadJson = file.ReadToEnd();
        file.Close();
        data = JsonUtility.FromJson<Database>(loadJson);

        StreamReader file1;
        if (!System.IO.File.Exists(Application.dataPath + "/BPData"))
            System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath + "/BPData"), JsonUtility.ToJson(backpack));
        file1 = new StreamReader(System.IO.Path.Combine(Application.dataPath + "/BPData"));
        string loadJson1 = file1.ReadToEnd();
        file1.Close();
        backpack = JsonUtility.FromJson<BackPackItem>(loadJson1);

        if (!System.IO.File.Exists(Application.dataPath + "/Data2"))
            System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath + "/Data2"), JsonHelper.arrayToJson<bool>(bloodData.bloodObjectsActive));
        string loadJson2 = File.ReadAllText(Application.dataPath + "/Data2");
        bloodData.bloodObjectsActive = JsonHelper.getJsonArray<bool>(loadJson2);

        if (!System.IO.File.Exists(Application.dataPath + "/Data3"))
            System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath + "/Data3"), JsonHelper.arrayToJson<string>(bloodData.bloodObjectsName));
        string loadJson3 = File.ReadAllText(Application.dataPath + "/Data3");
        bloodData.bloodObjectsName = JsonHelper.getJsonArray<string>(loadJson3);

        string whiteBloodCheck = manager.scenes.activeScene + "_Leukocyte";
        if(bloodData.bloodObjectsName.Length != 0)
        {
            if (bloodData.bloodObjectsName[0] != whiteBloodCheck)
            {
                //WhiteBloodObjectsUpdateToNewLevel
                whiteSave.WhiteBloodObjectsFindAtRuntime();
                whiteSave.WhiteBloodObjectsSave(whiteSave.whiteBloodObjectsFindAtRuntime);
                for (int i = 0; i < bloodData.bloodObjectsActive.Length; i++)
                    bloodData.bloodObjectsActive[i] = true;
            }
        }
        else
        {
            //WhiteBloodObjectsUpdateToNewLevel
            whiteSave.WhiteBloodObjectsFindAtRuntime();
            whiteSave.WhiteBloodObjectsSave(whiteSave.whiteBloodObjectsFindAtRuntime);
            for (int i = 0; i < bloodData.bloodObjectsActive.Length; i++)
                bloodData.bloodObjectsActive[i] = true;
        }
        manager.ui.UI_Update();
        levelSave.ReloadLevelObjects();
        whiteSave.WhiteBloodObjectsFindAtRuntime();
        whiteSave.WhiteBloodObjectsLoad(whiteSave.whiteBloodObjectsFindAtRuntime);
        Debug.Log("Data Loaded");
        if (manager.scenes.activeScene == "Start_UI")
        {
            startUI.GetComponent<StartUI>().loadButtonSet();
            manager.player.transform.position = Vector3.zero;
        }
        else
        {
            manager.player.transform.position = new Vector3(manager.save.data.x, manager.save.data.y, manager.save.data.z);
        }
    }

    //Save while player OnTrigger SaveArea
    public void Save(Vector3 position)
    {
        Time.timeScale = 1;
        Debug.Log("Save");
        manager.uiSetting.SaveUIRun();
        //WhiteBloodObjectsSave
        whiteSave.WhiteBloodObjectsFindAtRuntime();
        whiteSave.WhiteBloodObjectsSave(whiteSave.whiteBloodObjectsFindAtRuntime);
        //SceneNameSave
        data.scenePlayed = manager.scenes.activeScene;
        //SpawnPointPositionSave
        data.x = position.x;
        data.y = position.y;
        data.z = position.z;
        //Save
        string savedata = JsonUtility.ToJson(data);
        string jsonInfo = JsonHelper.arrayToJson<bool>(bloodData.bloodObjectsActive);
        string jsonInfo2 = JsonHelper.arrayToJson<string>(bloodData.bloodObjectsName);

        StreamWriter file = new StreamWriter(System.IO.Path.Combine(Application.dataPath + "/Data"));
        file.Write(savedata);
        file.Close();

        StreamWriter file2 = new StreamWriter(System.IO.Path.Combine(Application.dataPath + "/Data2"));
        file2.Write(jsonInfo);
        file2.Close();

        StreamWriter file3 = new StreamWriter(System.IO.Path.Combine(Application.dataPath + "/Data3"));
        file3.Write(jsonInfo2);
        file3.Close();

        BackPackSave();
    }

    public void BackPackSave()
    {
        string backpackData = JsonUtility.ToJson(backpack);

        StreamWriter BPfile = new StreamWriter(System.IO.Path.Combine(Application.dataPath + "/BPData"));
        BPfile.Write(backpackData);
        BPfile.Close();
    }

    public void DeveloperResetData()
    {
        PlayerPrefs.SetInt("ending", 0);
        Home_Tent.ending = false;
        data.x = -7f;
        data.y = -0.6f;
        data.z = -37f;

        //material
        backpack.lightHerb = 0;
        backpack.timeHerb = 0;
        backpack.scaleHerb = 0;
        backpack.fruit = 0;
        backpack.bigMine = 0;
        backpack.smallMine = 0;

        //potion
        backpack.o_lightBig = 0;
        backpack.o_lightSmall = 0;
        backpack.o_timeBig = 0;
        backpack.o_timeSmall = 0;
        backpack.o_scaleBig = 0;
        backpack.o_scaleSmall = 0;
        backpack.p_lightBig = 0;
        backpack.p_lightSmall = 0;
        backpack.p_timeBig = 0;
        backpack.p_timeSmall = 0;
        backpack.p_scaleBig = 0;
        backpack.p_scaleSmall = 0;

        //LevelObjectsReload
        data.scenePlayed = "";

        //WhiteBloodCellReload
        whiteSave.WhiteBloodPrefabsUpdate();
        whiteSave.WhiteBloodObjectsSave(whiteSave.whiteBloodObjectsChild);

        //Save
        BackPackSave();
        string savedata = JsonUtility.ToJson(data);
        string jsonInfo = JsonHelper.arrayToJson<bool>(bloodData.bloodObjectsActive);
        string jsonInfo2 = JsonHelper.arrayToJson<string>(bloodData.bloodObjectsName);

        StreamWriter file = new StreamWriter(System.IO.Path.Combine(Application.dataPath + "/Data"));
        file.Write(savedata);

        file.Close();

        StreamWriter file2 = new StreamWriter(System.IO.Path.Combine(Application.dataPath + "/Data2"));
        file2.Write(jsonInfo);

        file2.Close();

        StreamWriter file3 = new StreamWriter(System.IO.Path.Combine(Application.dataPath + "/Data3"));
        file3.Write(jsonInfo2);
        file3.Close();

        Debug.Log("Data Reset & Update");
        LoadSave();
    }
}
