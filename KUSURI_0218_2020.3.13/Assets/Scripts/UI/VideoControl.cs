using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoControl : MonoBehaviour
{
    [SerializeField] RawImage image;
    [SerializeField] VideoPlayer video;
    [SerializeField] GameObject[] stopObjects;
    [SerializeField] GameObject videoCamera;
    [SerializeField] bool finish = false;
    [SerializeField] GameManager manager;
    public event EventHandler videoFinish;
    MyEventArgs e;

    // Start is called before the first frame update
    void Awake()
    {
        video.loopPointReached += OnMovieFinished;
        videoCamera.SetActive(true);
        image.enabled = true;
        video.Play();
        for(int i = 0; i < stopObjects.Length; i++)
        {
            stopObjects[i].SetActive(false);
        }
    }


    //the action on finish
    void OnMovieFinished(VideoPlayer vp)
    {
        vp.playbackSpeed = vp.playbackSpeed / 10.0F;
        image.enabled = false;
        video.Stop();
        for (int i = 0; i < stopObjects.Length; i++)
        {
            stopObjects[i].SetActive(true);
        }
        videoCamera.SetActive(false);
        videoFinish(this, new MyEventArgs(manager));
    }


}
public class MyEventArgs : EventArgs
{
    GameManager gm;
    public GameManager _GM { get { return gm; } private set { gm = value; } }
    public MyEventArgs(GameManager GM)
    {
        _GM = GM;
    }
}
