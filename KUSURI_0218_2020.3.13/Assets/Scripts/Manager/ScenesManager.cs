using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    public Vector3 endingSpawnPoint;
    public string activeScene;
    GameManager manager;
    UIManager ui;

    // Start is called before the first frame update
    void Awake()
    {
        //except Start UI Scene!!!!!!!!!!!!!!!!!!!!!!!!!
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        activeScene = SceneManager.GetActiveScene().name;
        ui = GetComponent<UIManager>();
    }

    public void ChangeScene(string nextSceneName)
    {
        StopAllCoroutines();
        ui.FadeInOut(1);
        SceneSpawnPoint(nextSceneName);
        StartCoroutine(LoadScene(nextSceneName));
    }

    public void SceneSpawnPoint(string nextSceneName)
    {
        Scene now = SceneManager.GetSceneByName(activeScene);
        Scene next = SceneManager.GetSceneByName(nextSceneName);
        Scene previous = SceneManager.GetSceneByName(manager.save.data.scenePlayed);

        if (now.buildIndex == 0 || previous.buildIndex == 0)
            return;

        for(int i = 0; i < manager.save.levelSpawnPoint.Length; i++)
        {
            if((now.buildIndex == i + 1 || next.buildIndex == i + 1) && (previous.buildIndex != now.buildIndex))
            {
                Debug.Log("Spawn Point Update" + manager.save.levelSpawnPoint[i]);
                manager.save.Save(manager.save.levelSpawnPoint[i]);
                manager.save.LoadSave();
            }
        }

        if((now.name == "Home" || next.name == "Home") && Home_Tent.ending)
        {
            manager.save.Save(endingSpawnPoint);
            manager.save.LoadSave();
        }
    }

    IEnumerator LoadScene(string nextSceneName)
    {
        Time.timeScale = 1;
        yield return new WaitForSeconds(2);
        ui.loadingUi.SetActive(true);
        AsyncOperation operation = SceneManager.LoadSceneAsync(nextSceneName);

        while (!operation.isDone)
        {
            ui.LoadingUI(operation.progress);
            yield return null;
        }
    }
}
