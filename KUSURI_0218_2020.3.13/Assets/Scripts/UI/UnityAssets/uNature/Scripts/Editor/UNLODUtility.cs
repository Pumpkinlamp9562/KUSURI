using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using uNature.Core.FoliageClasses;

namespace uNature.Core.Utility
{
    public static class UNLODUtility
    {
        private const float LOD_GUI_HEIGHT = 40;

        private static FoliageLODLevel currentlyEditedLOD;
        private static LODEditMode currentlyEditedLODMode = LODEditMode.None;

        private static FoliagePrototype currentPrototype;

        private enum LODEditMode
        {
            None,
            Horizontal,
            Vertical
        }

        public static List<FoliageLODLevel> DrawLODs(FoliagePrototype prototype, List<FoliageLODLevel> lods)
        {
            currentPrototype = prototype;

            int lastCoverage = 0;
            float lastLODValue = 1;

            float size = Screen.width - (Screen.width / 30f);

            GUILayout.Space(20);

            Rect rect = GUILayoutUtility.GetLastRect();
            Rect currentRect;
            bool lastLOD;

            for (int i = 0; i < lods.Count; i++)
            {
                lastLOD = i == lods.Count - 1;
                currentRect = new Rect(rect.x + (3 + lastCoverage / 100f * size), rect.y + 20, size * ((lods[i].LOD_Coverage_Percentage - lastCoverage) / 100f), LOD_GUI_HEIGHT);

                DrawLOD(currentRect, lods[i], i, lastCoverage, lastLODValue, lastLOD, lastLOD ? null : lods[i + 1]);

                //GUI.Button(new Rect(rect.x + (3 + lastCoverage / 100f * size), rect.y + 20, size * ((lods[i].coveragePercentage - lastCoverage) / 100f), 40), lods[i].coveragePercentage.ToString());

                lastCoverage = lods[i].LOD_Coverage_Percentage;
                lastLODValue = lods[i].LOD_Value_Multiplier;
            }

            GUILayout.Space(50);

            if (currentPrototype.FoliageType == FoliageType.Prefab)
            {
                GUILayout.Space(30); // make sure the layout works accordingly. [After making renderers lods]
            }

            return lods;
        }

        private static FoliageLODLevel DrawLOD(Rect rect, FoliageLODLevel lod, int id, int lastCoverage, float lastLODValue, bool lastLOD, FoliageLODLevel nextLOD)
        {
            Rect horizontalRect = new Rect(rect.x + rect.width - 10, rect.y, 20, rect.height);
            Rect verticalRect = new Rect(rect.x + 30, rect.y + (1 - lod.LOD_Value_Multiplier) * LOD_GUI_HEIGHT, rect.width - 30, lod.LOD_Value_Multiplier * LOD_GUI_HEIGHT + 10);

            Event evnt = Event.current;

            if(evnt != null)
            {
                bool inHorizontal = horizontalRect.Contains(evnt.mousePosition) && !lastLOD;
                bool inVertical = verticalRect.Contains(evnt.mousePosition);

                if (inHorizontal != false || inVertical != false || currentlyEditedLOD != null)
                {
                    EditorGUIUtility.AddCursorRect(new Rect(evnt.mousePosition.x, evnt.mousePosition.y, 20, 20), 
                        currentlyEditedLODMode == LODEditMode.Horizontal ? MouseCursor.ResizeHorizontal :
                        currentlyEditedLODMode == LODEditMode.Vertical ? MouseCursor.ResizeVertical : (inHorizontal ? MouseCursor.ResizeHorizontal : MouseCursor.ResizeVertical ));
                }

                switch(evnt.type)
                {
                    case EventType.MouseDown:
                        if (evnt.button == 0)
                        {
                            if (inHorizontal && !lastLOD) // horizontal [affect percentage -> distance]
                            {
                                currentlyEditedLOD = lod;
                                currentlyEditedLODMode = LODEditMode.Horizontal;
                            }
                            else if (inVertical) // vertical [density multiplier -> lodLevel]
                            {
                                currentlyEditedLOD = lod;
                                currentlyEditedLODMode = LODEditMode.Vertical;
                            }
                        }
                        else if(evnt.button == 1)
                        {
                            if (rect.Contains(evnt.mousePosition))
                            {
                                GenericMenu menu = new GenericMenu();

                                if (id == 0)
                                {
                                    menu.AddDisabledItem(new GUIContent("Add Before Item"));
                                }
                                else
                                {
                                    menu.AddItem(new GUIContent("Add Before Item"), false, OnAdd, id);
                                }

                                if (id == 0 || lastLOD)
                                {
                                    menu.AddDisabledItem(new GUIContent("Remove Current"));
                                }
                                else
                                {
                                    menu.AddItem(new GUIContent("Remove Current"), false, OnRemove, lod);
                                }

                                menu.ShowAsContext();
                            }
                        }
                        break;
                    case EventType.KeyUp:
                    case EventType.MouseUp:
                        currentlyEditedLOD = null;
                        currentlyEditedLODMode = LODEditMode.None;
                        break;
                }
            }

            if(currentlyEditedLOD == lod && evnt.type != EventType.Layout)
            {
                if (currentlyEditedLODMode == LODEditMode.Horizontal) // handle horizontal
                {
                    lod.LOD_Coverage_Percentage = (int)(evnt.mousePosition.x / Screen.width * 100);
                    lod.LOD_Coverage_Percentage = Mathf.Clamp(lod.LOD_Coverage_Percentage, 2, nextLOD.LOD_Coverage_Percentage - 2);
                }
                else if(currentlyEditedLODMode == LODEditMode.Vertical) // handle vertical
                {
                    lod.LOD_Value_Multiplier = 1 - ((evnt.mousePosition.y - rect.y) / LOD_GUI_HEIGHT);
                    lod.LOD_Value_Multiplier = Mathf.Clamp(lod.LOD_Value_Multiplier, nextLOD == null ? 0 : nextLOD.LOD_Value_Multiplier, lastLODValue); // clamp between former lod and current lod levels 
                }
            }

            GUI.color = FoliageLODLevel.lodGUIColors[id];
            GUI.Label(rect, (100 - lastCoverage).ToString() + "%" + "\n" + lod.LOD_Value_Multiplier + "%", "Box");

            GUI.color = FoliageLODLevel.lodGUIColors_overlay[id];

            verticalRect.x = rect.x; // reset x so it renders properly
            verticalRect.width = rect.width; // reset width so it renders properly
            verticalRect.height -= 10; //reduce 10 from collision adjuster

            GUI.Label(verticalRect, "", "Box");

            GUI.color = Color.white;

            rect.y += 50; // add some space
            rect.height = 17; // change height

            bool enabled = GUI.enabled;

            GUI.enabled = id != 0; // disable changing the first lod

            if (currentPrototype.FoliageType == FoliageType.Prefab)
            {
                lod.SetMeshLOD((Mesh)EditorGUI.ObjectField(rect, "", lod.GetMeshLOD(id, currentPrototype), typeof(Mesh), false), currentPrototype.lods[0]);
            }

            GUI.enabled = enabled;

            return lod;
        }

        private static void OnAdd(object obj)
        {
            int addBeforeID = (int)obj;

            if (currentPrototype != null)
            {
                if (currentPrototype.lods.Count >= FoliageLODLevel.LOD_MAX_AMOUNT) return; // cant have more than X lods.

                FoliageLODLevel addBefore = currentPrototype.lods[addBeforeID];
                FoliageLODLevel addAfter = currentPrototype.lods[addBeforeID - 1]; // will always be true as you can't remove the first lod.

                int averegePercentage = (int)((addBefore.LOD_Coverage_Percentage + addAfter.LOD_Coverage_Percentage) / 2f);
                float averegeValue = (addBefore.LOD_Value_Multiplier + addAfter.LOD_Value_Multiplier) / 2f;

                currentPrototype.lods.Insert(addBeforeID, new FoliageLODLevel(averegePercentage, averegeValue));

                currentPrototype.UpdateLODs();
            }
        }

        private static void OnRemove(object obj)
        {
            FoliageLODLevel lod = (FoliageLODLevel)obj;
            
            if(currentPrototype != null && lod != null)
            {
                currentPrototype.lods.Remove(lod);

                currentPrototype.UpdateLODs();
            }
        }
    }
}
