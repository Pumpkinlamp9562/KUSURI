using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemInfo))]
public class ItemListEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ItemInfo list = (ItemInfo)target;

        EditorGUI.BeginDisabledGroup(list.type != ItemInfo.ItemCategory.Herb);
        list.herb = (ItemInfo.HerbType)EditorGUILayout.EnumPopup("Herb Type", list.herb);
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(list.type != ItemInfo.ItemCategory.Mine);
        list.mine = (ItemInfo.MineType)EditorGUILayout.EnumPopup("Mineral Type", list.mine);
        EditorGUI.EndDisabledGroup();
    }
}
