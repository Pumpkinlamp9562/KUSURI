using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIFade : MonoBehaviour
{
    [SerializeField]
    GameObject Object;
    public float fadeSpeed = 0.01f;
    Image image;
    Button button;
    Text text;
    public UnityEvent fadeInOut;

    private void Awake()
    {
        if (GetComponent<Image>() != null)
            image = GetComponent<Image>();
        if (GetComponent<Text>() != null)
            text = GetComponent<Text>();
        if (GetComponent<Button>() != null)
            button = GetComponent<Button>();
    }
    public void FadeInOut(float target)
    {
        StopAllCoroutines();
        if (target != 0) image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        if(image == null) image = GetComponent<Image>();
        image.enabled = true;
        if (button != null)
            button.enabled = true;
        if (Object != null) Object.SetActive(true);
        StartCoroutine(FadeLerp(target, image, fadeSpeed));
    }

    public void FadeInOutText(float target)
    {
        StopAllCoroutines();
        if (target != 0) text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        text.enabled = true;
        if (button != null) button.enabled = true;
        if (Object != null) Object.SetActive(true);
        StartCoroutine(FadeLerpText(target, text, fadeSpeed));
    }

    IEnumerator FadeLerp(float target, Image fade, float fadeSpeed)
    {
        while (fade.color.a != target)
        {
            fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, Mathf.Lerp(fade.color.a, target, fadeSpeed));
            if (fade.color.a < 0.05f)
            {
                fade.enabled = false;
                if (button != null) button.enabled = false;
                if (Object != null) Object.SetActive(false);
            }
            else
            {
                fade.enabled = true;
                if (button != null) button.enabled = true;
                if (Object != null) Object.SetActive(true);
            }
            if(Mathf.Abs(fade.color.a - target) < 0.01f && fadeInOut != null)
            {
                fadeInOut.Invoke();
            }
            yield return null;
        }
    }
    IEnumerator FadeLerpText(float target, Text fade, float fadeSpeed)
    {
        while (fade.color.a != target)
        {
            fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, Mathf.Lerp(fade.color.a, target, fadeSpeed));
            if (fade.color.a < 0.01f)
            {
                fade.enabled = false;
                if (button != null) button.enabled = false;
                if (Object != null) Object.SetActive(false);
            }
            yield return null;
        }
    }
}
