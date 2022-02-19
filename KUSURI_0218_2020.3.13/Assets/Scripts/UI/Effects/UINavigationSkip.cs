using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UINavigationSkip : MonoBehaviour
{
    GameManager manager;
    GameObject lastSelcetedButton;
    GameObject modifySelect;

    void Awake()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            NavigationSkip();
            ShowSelectedImage();
        }
    }

    public void NavigationSkip()
    {
        if (EventSystem.current.currentSelectedGameObject != null && (manager.uiSetting.backpackON || manager.uiSetting.craftON || manager.uiSetting.settingON || manager.scenes.activeScene == "Start_UI"))
        {
            if (EventSystem.current.currentSelectedGameObject.GetComponent<Button>() != null)
            {
                while (!EventSystem.current.currentSelectedGameObject.GetComponent<Button>().interactable)
                {
                    if (manager.input.uiMove.x < 0)
                    {
                        if (EventSystem.current.currentSelectedGameObject.GetComponent<Button>().navigation.selectOnLeft != null)
                        {
                            UISelectedUpdate(EventSystem.current.currentSelectedGameObject.GetComponent<Button>().navigation.selectOnLeft.gameObject);
                            break;
                        }
                        else { UISelectedUpdate(lastSelcetedButton); break; }
                    }
                    if (manager.input.uiMove.x > 0)
                    {
                        if (EventSystem.current.currentSelectedGameObject.GetComponent<Button>().navigation.selectOnRight != null)
                        {
                            UISelectedUpdate(EventSystem.current.currentSelectedGameObject.GetComponent<Button>().navigation.selectOnRight.gameObject);
                            break;
                        }
                        else { UISelectedUpdate(lastSelcetedButton); break; }
                    }
                    if (manager.input.uiMove.y < 0)
                    {
                        if (EventSystem.current.currentSelectedGameObject.GetComponent<Button>().navigation.selectOnDown != null)
                        {
                            UISelectedUpdate(EventSystem.current.currentSelectedGameObject.GetComponent<Button>().navigation.selectOnDown.gameObject);
                            break;
                        }
                        else { UISelectedUpdate(lastSelcetedButton); break; }
                    }
                    if (manager.input.uiMove.y > 0)
                    {
                        if (EventSystem.current.currentSelectedGameObject.GetComponent<Button>().navigation.selectOnUp != null)
                        {
                            UISelectedUpdate(EventSystem.current.currentSelectedGameObject.GetComponent<Button>().navigation.selectOnUp.gameObject);
                            break;
                        }
                        else { UISelectedUpdate(lastSelcetedButton); break; }
                    }
                    if (manager.input.uiMove.y == 0 && manager.input.uiMove.x == 0)
                        break;
                }
                if (EventSystem.current.currentSelectedGameObject.GetComponent<Button>().interactable)
                    lastSelcetedButton = EventSystem.current.currentSelectedGameObject;
            }
        }
    }

    public void UISelectedUpdate(GameObject button){ if (manager.input.previousControlScheme == "Gamepad") EventSystem.current.SetSelectedGameObject(button);}
    void ShowSelectedImage()
    {
        if (manager.uiSetting.settingON)
        {
            if (EventSystem.current.currentSelectedGameObject.name == "Music_Slider") manager.uiSetting.musicText.enabled = true;
            else manager.uiSetting.musicText.enabled = false;

            if (EventSystem.current.currentSelectedGameObject.name == "Sound_Slider") manager.uiSetting.soundText.enabled = true;
            else manager.uiSetting.soundText.enabled = false;
        }
    }

}
