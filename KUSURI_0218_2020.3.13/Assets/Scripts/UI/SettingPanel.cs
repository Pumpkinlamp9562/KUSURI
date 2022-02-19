using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


public class SettingPanel : MonoBehaviour
{
    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider slider;
    [SerializeField] string volume = "BG_Volume";

    public void Awake()
    {
        slider.onValueChanged.AddListener(HandleSlideedrValueChanged);
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat(volume, slider.value);
    }

    private void Start()
    {
        slider.value = PlayerPrefs.GetFloat(volume, slider.value);
    }

    private void HandleSlideedrValueChanged(float value)
    {
        mixer.SetFloat(volume, value);
    }
}
