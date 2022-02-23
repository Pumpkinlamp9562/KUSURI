using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PagesFlip : MonoBehaviour
{
    [SerializeField] bool dontFlip;
    [SerializeField] GameObject nextPage;
    [SerializeField] float smooth = 0.01f;
    [SerializeField] float fadeSmooth = 0.1f;
    Button button;
    Material flipShader;
    GameManager manager;
    
    // Start is called before the first frame update
    void Start()
    {
        if (dontFlip)
            gameObject.AddComponent<UIFade>();
        if (nextPage != null)
            nextPage.GetComponent<Button>().interactable = false;
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        button = gameObject.GetComponent<Button>();
        flipShader = Instantiate<Material>(gameObject.GetComponent<Image>().material);
        flipShader.SetFloat("_Flip", 0);
        gameObject.GetComponent<Image>().material = flipShader;
        button.onClick.AddListener(ButtonOnClick);
    }

    void ButtonOnClick()
    {
        if(nextPage == null)
        {
            manager.scenes.ChangeScene("LV1");
            return;
        }

        if (dontFlip)
        {
            button.interactable = false;
            nextPage.GetComponent<Button>().interactable = true;
            manager.uiNav.UISelectedUpdate(nextPage);
            gameObject.GetComponent<UIFade>().fadeSpeed = fadeSmooth;
            gameObject.GetComponent<UIFade>().FadeInOut(0);
            return;
        }
        StartCoroutine(Lerp());
    }

    IEnumerator Lerp()
    {
        while (Mathf.Abs(flipShader.GetFloat("_Flip") - -0.5f) > smooth)
        {
            if(Mathf.Abs(flipShader.GetFloat("_Flip") - -0.5f) < (smooth + 0.01f))
            {
                manager.uiNav.UISelectedUpdate(nextPage);
                button.interactable = false;
                gameObject.GetComponent<Image>().enabled = false;
                nextPage.GetComponent<Button>().interactable = true;
                StopAllCoroutines();
            }
            flipShader.SetFloat("_Flip", Mathf.Lerp(flipShader.GetFloat("_Flip"), -0.5f, smooth));
            yield return null;
        }
    }
}
