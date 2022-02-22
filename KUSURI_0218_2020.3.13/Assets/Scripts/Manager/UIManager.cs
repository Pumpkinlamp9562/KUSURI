using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [Header("Save&Scene")]
    [SerializeField]
    Image fadeImage;
    [SerializeField]
    GameObject saveImage;
    [SerializeField]
    Text saveText;
    public GameObject loadingUi;

    [Header("Setting")]
    public bool settingON;
    [SerializeField]
    Image settingFade;
    [SerializeField]
    GameObject settingUI;
    [SerializeField]
    GameObject settingFirstSelect;
    public Image musicText;
    public Image soundText;
    [SerializeField]
    GameObject keyboard;
    [SerializeField]
    GameObject gamePad;

    [Header("Backpack")]
    public bool backpackON;
    public Image backpackRedDot;
    [SerializeField]
    GameObject backpackUI;

    [Header("CraftUI")]
    public bool craftON;
    [SerializeField]
    GameObject craftUI;
    public CraftLineControl[] lines;
    public Image craftBubble;
    public Image craftRedDot;
    public Animator craftAnim;
    public Image potion;
    List<GameObject> craftButton = new List<GameObject>();

    [Header("PlayerState")]
    [SerializeField]
    Sprite[] state;
    [SerializeField]
    Image[] playerStateUI;
    [SerializeField]
    Image deadFade;

    [Header("PlayerState")]
    [SerializeField]
    GameObject techUIGamePad;
    [SerializeField]
    GameObject techUIMouse;

    Slider loading;
    GameManager manager;

    private void Awake()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }
    private void Start()
    {
        Button[] crafts;
        crafts = craftUI.GetComponentsInChildren<Button>();
        for (int i = 0; i < crafts.Length; i++)
            craftButton.Add(crafts[i].gameObject);

        lines = craftUI.GetComponentsInChildren<CraftLineControl>();

        if (saveImage != null)
            saveImage.SetActive(false);
        loadingUi.SetActive(false);
        loading = loadingUi.GetComponentInChildren<Slider>();
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1);
        FadeInOut(0);
        OpenBackpackUI(false);
        OpenSettingUI(false);
        OpenCraftUI(false);
        craftRedDot.enabled = false;
        backpackRedDot.enabled = false;
        if (manager.scenes.activeScene == "Start_UI")
        {
            if (manager.input.previousControlScheme == "Gamepad")
                manager.uiNav.UISelectedUpdate(GameObject.FindObjectOfType<EventSystem>().firstSelectedGameObject);
            if (manager.input.previousControlScheme == "Keyboard")
                manager.uiNav.UISelectedUpdate(null);
        }
    }

    public void FadeInOut(float target)
    {
        fadeImage.GetComponent<UIFade>().FadeInOut(target);
    }

    public void FadeInOutUIBackground(float target)
    {
        settingFade.GetComponent<UIFade>().FadeInOut(target);
    }
    public void FadeInOutUIDead(float target)
    {
        deadFade.GetComponent<UIFade>().FadeInOut(target);
    }

    public void LoadingUI(float progress)
    {
        loading.value = progress;
        if (saveImage != null)
            SaveUIRun();
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void SaveUIRun()
    {
        StopCoroutine(SaveUIActive());
        StartCoroutine(SaveUIActive());
        StartCoroutine(SaveAnim(true));
    }

    public void SettingKeyboardButtonOnClick()
    {
        if (manager.input.previousControlScheme == "Keyboard")
        {
            keyboard.SetActive(!keyboard.activeInHierarchy);
            gamePad.SetActive(false);
            manager.uiNav.UISelectedUpdate(keyboard.GetComponentInChildren<Button>().gameObject);
        }
        if (manager.input.previousControlScheme == "Gamepad")
        {
            gamePad.SetActive(!gamePad.activeInHierarchy);
            keyboard.SetActive(false);
            manager.uiNav.UISelectedUpdate(gamePad.GetComponentInChildren<Button>().gameObject);
        }
    }
    public void CraftUI() { OpenCraftUI(!craftON); }
    public void SettingUI() { OpenSettingUI(!settingON); }
    public void BackpackUI() { OpenBackpackUI(!backpackON); }
    public void OpenCraftUI(bool open)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Start_UI")
            return;
        if (PlayerPrefs.GetInt("ending", 0) == 1 && open)
        {
            open = false;
            return;
        }

        craftON = open;
        foreach (GameObject g in craftButton)
        {
            if (open)
            {
                if (backpackON)
                    OpenBackpackUI(false);
                if (settingON)
                    OpenSettingUI(false);
                g.GetComponent<UIFade>().fadeSpeed = 0.1f;
                g.GetComponent<UIFade>().FadeInOut(1);
                g.GetComponentInChildren<Text>().gameObject.GetComponent<UIFade>().fadeSpeed = 0.1f;
                g.GetComponentInChildren<Text>().gameObject.GetComponent<UIFade>().FadeInOutText(1);
                if (manager.input.previousControlScheme == "Gamepad")
                {
                    techUIMouse.SetActive(false);
                    techUIGamePad.SetActive(true);
                    if (manager.ui.itemButtons[2].interactable)
                        manager.uiNav.UISelectedUpdate(manager.ui.itemButtons[2].gameObject);
                    if (!manager.ui.itemButtons[2].interactable)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (manager.ui.itemButtons[i].interactable)
                                manager.uiNav.UISelectedUpdate(manager.ui.itemButtons[i].gameObject);
                        }
                    }
                }
                else
                {
                    techUIGamePad.SetActive(false);
                    techUIMouse.SetActive(true);
                }
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    manager.ui.itemButtons[i].gameObject.GetComponent<UIAnnotation>().annotationUI.SetActive(false);
                }
                g.GetComponent<UIFade>().fadeSpeed = 0.1f;
                g.GetComponent<UIFade>().FadeInOut(0);
                g.GetComponentInChildren<Text>().gameObject.GetComponent<UIFade>().fadeSpeed = 0.1f;
                g.GetComponentInChildren<Text>().gameObject.GetComponent<UIFade>().FadeInOutText(0);
                UICancelSeletion();
            }
        }

        if (manager == null)
        {
            return;
        }

        craftRedDot.enabled = false;
        craftAnim.SetBool("Play", false);
        manager.uiNav.UISelectedUpdate(null);
        manager.craft.Clear();

    }

    void OpenSettingUI(bool open)
    {
        manager.uiNav.UISelectedUpdate(null);
        settingON = open;
        if (open)
        {
            techUIGamePad.SetActive(false);
            techUIMouse.SetActive(false);
            Time.timeScale = 0;
            if (craftON)
                OpenCraftUI(false);
            if (backpackON)
                OpenBackpackUI(false);
            if (manager.input.previousControlScheme == "Gamepad")
            {
                manager.uiNav.UISelectedUpdate(settingFirstSelect.gameObject);
                techUIMouse.SetActive(false);
                techUIGamePad.SetActive(true);
            }
            else
            {
                techUIMouse.SetActive(true);
                techUIGamePad.SetActive(false);
            }

            FadeInOutUIBackground(0.7f);
        }
        else
        {
            keyboard.SetActive(false);
            FadeInOutUIBackground(0f);
            UICancelSeletion();
            Time.timeScale = 1;
        }
        settingUI.SetActive(open);
    }

    void OpenBackpackUI(bool open)
    {
        if (Home_Tent.ending && open)
        {
            open = false;
            return;
        }
        backpackRedDot.enabled = false;
        if (manager.scenes.activeScene != "Start_UI")
        {
            if (manager.input.previousControlScheme == "Keyboard")
            {
                manager.uiNav.UISelectedUpdate(null);
                techUIGamePad.SetActive(false);
                techUIMouse.SetActive(true);
            }
            else
            {
                if (manager.input.previousControlScheme == "Gamepad")
                {
                    techUIMouse.SetActive(false);
                    techUIGamePad.SetActive(true);
                }
            }
            backpackON = open;
            if (!open)
            {
                if (!settingON && !craftON)
                    manager.uiNav.UISelectedUpdate(null);
                backpackUI.GetComponent<CraftLineControl>().IsCrafting(new Vector2(-648, 0));
                UICancelSeletion();
            }
            else
            {
                if (settingON)
                    OpenSettingUI(false);
                if (craftON)
                    OpenCraftUI(false);
                backpackUI.GetComponent<CraftLineControl>().IsCrafting(new Vector2(0, 0));
                if (manager.input.previousControlScheme == "Gamepad")
                    manager.ui.BackPackCurrentSelectedUpdate(manager.ui.itemButtons[8].gameObject);
            }
        }
    }

    void UICancelSeletion() 
    { 
        if (!backpackON && !settingON && !craftON && manager.scenes.activeScene != "Start_UI") 
        { 
            manager.uiNav.UISelectedUpdate(null);
            techUIMouse.SetActive(false);
            techUIGamePad.SetActive(false);
        } 
    }

    public void PlayerStateUIUpdate()
    {
        for (int i = 0; i < playerStateUI.Length; i++)
            playerStateUI[i].color = new Color(1, 1, 1, 0);
        for (int i = 0; i < manager.player.potionState.Count; i++)
        {
            playerStateUI[i].color = Color.white;
            switch (manager.player.potionState[i])
            {
                case PlayerManager.State.scaleBig:
                    playerStateUI[i].sprite = state[0];
                    break;
                case PlayerManager.State.scaleSmall:
                    playerStateUI[i].sprite = state[1];
                    break;
                case PlayerManager.State.timeBig:
                    playerStateUI[i].sprite = state[2];
                    break;
                case PlayerManager.State.timeSmall:
                    playerStateUI[i].sprite = state[3];
                    break;
                case PlayerManager.State.lightBig:
                    playerStateUI[i].sprite = state[4];
                    break;
                case PlayerManager.State.lightSmall:
                    playerStateUI[i].sprite = state[5];
                    break;
            }
        }
    }

    IEnumerator SaveUIActive()
    {
        saveImage.SetActive(true);
        saveText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        saveImage.SetActive(false);
        saveText.gameObject.SetActive(false);
        StartCoroutine(SaveAnim(false));
    }

    IEnumerator SaveAnim(bool play)
    {
        while (play)
        {
            saveImage.transform.rotation = Quaternion.Euler(Vector3.Lerp(saveImage.transform.rotation.eulerAngles, new Vector3(saveImage.transform.rotation.x, saveImage.transform.rotation.y, saveImage.transform.rotation.z + 360), 0.01f));
            saveImage.GetComponent<Image>().color = new Color(saveImage.GetComponent<Image>().color.r, saveImage.GetComponent<Image>().color.g, saveImage.GetComponent<Image>().color.b, Mathf.PingPong(Time.time, 1));
            saveText.color = new Color(saveText.color.r, saveText.color.g, saveText.color.b, Mathf.PingPong(Time.time, 1));
            yield return null;
        }
    }
}
