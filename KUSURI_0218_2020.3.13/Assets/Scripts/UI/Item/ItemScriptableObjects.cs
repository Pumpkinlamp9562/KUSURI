using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "UI_Item",menuName = "Create UI_Item")]
public class ItemScriptableObjects : ScriptableObject
{
    public string Info;
    public Text count;
}
