using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Home_Tent : MonoBehaviour
{
    public static bool ending = false;
    [SerializeField] VideoControl video;
    [SerializeField] WhiteWallDisappear disappear;
    [SerializeField] GameObject goodParticle;
    [SerializeField] GameObject badParticle;
    [SerializeField] float endSeconds = 5f;
    [SerializeField] float particleSeconds = 3f;

    GameManager manager;

    // Start is called before the first frame update
    void Start()
    {
        video.videoFinish += AfterVideo;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PotionHit>() == null)
            return;
        if (other.gameObject.name == "timebig(Clone)")
        {
            goodParticle.GetComponent<ParticleSystem>().Play();
            disappear.WallsDisappear(false, false);
            gameObject.GetComponent<Collider>().enabled = false;
            goodParticle.GetComponentInChildren<UIFade>().FadeInOut(1);
        }

        if (other.gameObject.name == "timesmall(Clone)")
        {
            badParticle.GetComponent<ParticleSystem>().Play();
            gameObject.GetComponent<Collider>().enabled = false;
            badParticle.GetComponentInChildren<UIFade>().FadeInOut(1);
        }
    }
    public void End()
    {
        StartCoroutine(Ending());
        Destroy(badParticle.GetComponentInChildren<UIFade>());
        Destroy(goodParticle.GetComponentInChildren<UIFade>());
    }
    private void AfterVideo(object sender, EventArgs e)
    {
        MyEventArgs m = (MyEventArgs)e;
        manager = m._GM;
        StartCoroutine(NeedWait());
    }

    IEnumerator NeedWait()
    {
        yield return new WaitUntil(() => manager.save.levelSave.objectsSave.Length > 0);
        manager.save.lastSavePoint = manager.save.savePoints[0];
        manager.save.DeveloperResetData();
    }

    IEnumerator Ending()
    {
        yield return new WaitForSeconds(endSeconds);
        PlayerPrefs.SetInt("ending", 1);
        ending = true;
        manager.scenes.ChangeScene("Home");
    }

}
