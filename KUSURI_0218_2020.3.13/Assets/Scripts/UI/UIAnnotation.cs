using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIAnnotation : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IDeselectHandler, IPointerExitHandler
{
    public GameObject annotationUI;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(gameObject.GetComponent<Button>().interactable)
            annotationUI.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        annotationUI.SetActive(false);
    }
    public void OnSelect(BaseEventData eventData)
    {
        if (gameObject.GetComponent<Button>().interactable)
            annotationUI.SetActive(true);
    }
    public void OnDeselect(BaseEventData data)
    {
        annotationUI.SetActive(false);
    }
}
