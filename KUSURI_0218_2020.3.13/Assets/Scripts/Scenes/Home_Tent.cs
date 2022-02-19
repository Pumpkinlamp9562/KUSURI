using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Home_Tent : MonoBehaviour
{
    [SerializeField] VideoControl video;
    [SerializeField] WhiteWallDisappear disappear;
    [SerializeField] GameObject goodParticle;
    [SerializeField] GameObject badParticle;
    [SerializeField] float endSeconds = 5f;

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
            gameObject.GetComponent<Collider>().enabled = false;
            StartCoroutine(Ending());
        }

        if (other.gameObject.name == "timesmall(Clone)")
        {
            badParticle.GetComponent<ParticleSystem>().Play();
            disappear.WallsDisappear(false, false);
            gameObject.GetComponent<Collider>().enabled = false;
            StartCoroutine(Ending());
        }
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
        SaveSetting.ending = true;
        manager.scenes.ChangeScene("Home");
    }
}
