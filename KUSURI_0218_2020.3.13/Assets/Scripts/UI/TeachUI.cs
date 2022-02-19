using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeachUI : MonoBehaviour
{
    [SerializeField]
    float smoothness;
    [SerializeField]
    string gamePadText;
    string keyBoardText;

    [SerializeField]
    GameObject[] gamePadImages;

    List<GameObject> Object = new List<GameObject>();
    Text text;
    Image image;
    GameManager manager;

    private void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        if (GetComponentInChildren<Text>() != null)
        {
            text = GetComponentInChildren<Text>();
            Object.Add(text.gameObject);
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
            keyBoardText = text.text;
        }
        if (GetComponentInChildren<Image>() != null)
        {
            image = GetComponentInChildren<Image>();
            Object.Add(image.gameObject);
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        }
        foreach (GameObject g in Object)
            g.SetActive(false);
        if (gamePadImages != null)
        {
            foreach (GameObject g in gamePadImages)
            {
                if (g != null)
                    g.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            StopAllCoroutines();
            foreach (GameObject g in Object)
            {
                StartCoroutine(AlphaLerp(1, g));
                g.SetActive(true);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if (manager.input.previousControlScheme == "Keyboard" && text != null)
            {
                text.text = keyBoardText;
                if (gamePadImages != null)
                    foreach (GameObject g in gamePadImages)
                    {
                        if (g != null)
                        {
                            g.SetActive(false);
                        }
                    }
            }
            if (manager.input.previousControlScheme == "Gamepad" && text != null)
            {
                text.text = gamePadText;
                if (gamePadImages != null)
                {

                    foreach (GameObject g in gamePadImages)
                    {
                        if (g != null)
                        {
                            StartCoroutine(AlphaImageLerp(1, g));
                            g.SetActive(true);
                        }
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            StopAllCoroutines();
            if (gamePadImages != null)
                foreach (GameObject g in gamePadImages)
                {
                    if (g != null)
                        StartCoroutine(AlphaLerp(0, g));
                }
            foreach (GameObject g in Object)
                StartCoroutine(AlphaLerp(0, g));
        }
    }

    IEnumerator AlphaLerp(float target, GameObject targetObject)
    {
        if (targetObject != null)
        {
            if (targetObject.GetComponent<Text>() != null)
            {
                while (Mathf.Abs(text.color.a - target) > 0.001f)
                {
                    text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Lerp(text.color.a, target, smoothness));
                    if (text.color.a < 0.1f) text.gameObject.SetActive(false);
                    yield return null;
                }
            }
            if (targetObject.GetComponent<Image>() != null)
            {
                while (Mathf.Abs(targetObject.GetComponent<Image>().color.a - target) > 0.001f)
                {
                    targetObject.GetComponent<Image>().color = new Color(targetObject.GetComponent<Image>().color.r, targetObject.GetComponent<Image>().color.g,
                        targetObject.GetComponent<Image>().color.b, Mathf.Lerp(targetObject.GetComponent<Image>().color.a, target, smoothness));
                    if (targetObject.GetComponent<Image>().color.a < 0.1f) targetObject.GetComponent<Image>().gameObject.SetActive(false);
                    yield return null;
                }
            }
        }
    }
    IEnumerator AlphaImageLerp(float target, GameObject targetObject)
    {
        if (targetObject != null)
        {
            if (targetObject.GetComponent<Text>() != null)
            {
                while (Mathf.Abs(text.color.a - target) > 0.001f)
                {
                    text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Lerp(text.color.a, target, smoothness));
                    if (text.color.a < 0.1f) text.gameObject.SetActive(false);
                    yield return null;
                }
            }
            if (targetObject.GetComponent<Image>() != null)
            {
                while (Mathf.Abs(targetObject.GetComponent<Image>().color.a - target) > 0.001f)
                {
                    targetObject.GetComponent<Image>().color = new Color(targetObject.GetComponent<Image>().color.r, targetObject.GetComponent<Image>().color.g,
                        targetObject.GetComponent<Image>().color.b, Mathf.Lerp(targetObject.GetComponent<Image>().color.a, target, smoothness));
                    if (targetObject.GetComponent<Image>().color.a < 0.1f) targetObject.GetComponent<Image>().gameObject.SetActive(false);
                    yield return null;
                }
            }
        }
    }
}
