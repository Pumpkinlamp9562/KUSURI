using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftLineControl : MonoBehaviour
{
    [SerializeField]
    bool playOnAwake = false;
    [SerializeField]
    float smooth;
    public float posY;
    RectTransform rect;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        if (playOnAwake)
            IsCrafting(new Vector2(0, posY));
    }

    //For ItemCraft & UIManager
    public void IsCrafting(Vector2 target)
    {
        StopAllCoroutines();
        StartCoroutine(Lerp(target));
    }

    IEnumerator Lerp(Vector2 target)
    {
        while (Vector2.Distance(rect.anchoredPosition, target) > 0.1f)
        {
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition,
                target, smooth);
            yield return null;
        }
    }
}
