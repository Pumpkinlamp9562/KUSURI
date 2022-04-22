using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using uNature.Core.Targets;
using uNature.Core.Sectors;
using uNature.Core.Utility;
using uNature.Core.Pooling;
using uNature.Core.FoliageClasses;

namespace uNature.Core.Terrains
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UNTerrain))]
    public class UNTerrainEditor : UNTargetEditor
    {
        [SerializeField]
        UNTerrain _terrain;
        UNTerrain terrain
        {
            get
            {
                if(_terrain == null)
                {
                    _terrain = (UNTerrain)target;
                }

                return _terrain;
            }
        }
        bool m_showSector;

        GUIStyle invisibleButtonStyle;
        GUIStyle boxStyle;
        GUIStyle tabsKeysStyle;

        MonoScript PoolItemScript;

        Color chosenColor = new Color(0.75f, 0.75f, 0.75f, 1);

        List<SelectionBoxItems<System.Type>> _PoolCache;
        public List<SelectionBoxItems<System.Type>> PoolCache
        {
            get
            {
                if(_PoolCache == null)
                {
                    GetPoolCache();
                }

                return _PoolCache;
            }
        }

        public List<UNTreePrototype> selectedPrototypes = new List<UNTreePrototype>();
        Vector2 prototypesScrollbarPos;

        public TerrainTabs currentTab = TerrainTabs.Grids;

        Vector3 lastScenePosition;
        bool ctrlPressed;
        
        void Update()
        {
            Repaint();
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(terrain, "UNTerrain changes");

            ctrlPressed = Event.current == null ? false : Event.current.control;

            if (invisibleButtonStyle == null)
            {
                invisibleButtonStyle = new GUIStyle("Box");

                invisibleButtonStyle.normal.background = null;
                invisibleButtonStyle.focused.background = null;
                invisibleButtonStyle.hover.background = null;
                invisibleButtonStyle.active.background = null;
            }
            if (boxStyle == null)
            {
                boxStyle = new GUIStyle("Box");

                boxStyle.normal.textColor = invisibleButtonStyle.normal.textColor;
                boxStyle.focused.textColor = invisibleButtonStyle.focused.textColor;
                boxStyle.hover.textColor = invisibleButtonStyle.hover.textColor;
                boxStyle.active.textColor = invisibleButtonStyle.active.textColor;
            }
            if(tabsKeysStyle == null)
            {
                tabsKeysStyle = new GUIStyle("Button");

                invisibleButtonStyle.focused.background = null;
                invisibleButtonStyle.hover.background = null;
            }

            EmptyVoidToUpdateTerrainData(terrain.terrainData.treePrototypes);

            GUILayout.Space(5);

            DrawTabs();

            GUILayout.Space(10);

            switch(currentTab)
            {
                case TerrainTabs.Grids:
                    GridsCategory();
                    break;

                case TerrainTabs.Pool:
                    PoolCategory();
                    break;

                case TerrainTabs.Vegetation:
                    VegetationCategory();
                    break;

                case TerrainTabs.Trees:
                    TreesCategory();
                    break;
            }

            if(GUI.changed)
            {
                EditorUtility.SetDirty(target);
                Undo.FlushUndoRecordObjects();
            }
        }

        void EmptyVoidToUpdateTerrainData(List<UNTreePrototype> prototypes)
        {
        }

        void DrawTabs()
        {
            string[] tabNames = System.Enum.GetNames(typeof(TerrainTabs));
            Texture2D tabImage;
            TerrainTabs currentTabIndex;

            GUILayout.BeginHorizontal();
            GUILayout.Space(Screen.width / 2.5f);

            bool buttonPressed = false;

            for (int i = 0; i < tabNames.Length; i++)
            {
                currentTabIndex = (System.Enum.GetValues(typeof(TerrainTabs)) as TerrainTabs[])[i];

                GUI.color = currentTab == currentTabIndex ? chosenColor : Color.white;

                if(currentTabIndex == TerrainTabs.Vegetation)
                {
                    tabImage = UNStandaloneUtility.GetUIIcon(tabNames[i] + (terrain.manageGrass ? "_On" : "_Off"));
                }
                else if(currentTabIndex == TerrainTabs.Trees)
                {
                    tabImage = UNStandaloneUtility.GetUIIcon(tabNames[i] + (terrain.manageTrees ? "_On" : "_Off"));
                }
                else
                {
                    tabImage = UNStandaloneUtility.GetUIIcon(tabNames[i]);
                }

                buttonPressed = GUILayout.Button(tabImage, tabsKeysStyle, GUILayout.MaxWidth(30), GUILayout.MaxHeight(30));

                if (buttonPressed)
                {
                    currentTab = currentTabIndex;
                }

                GUI.color = Color.white;
            }
            GUILayout.EndHorizontal();
        }

        void GridsCategory()
        {
            UNEditorUtility.DrawHelpBox("Grids Management", "Here you can manage your grids.", false, false);

            GUILayout.BeginHorizontal();
            GUILayout.Space(Screen.width / 5);

            int resolution = terrain.sectorResolution == 1 ? 300 : (300 / terrain.sectorResolution);

            UNEditorUtility.DrawGridPreview(terrain.sectorResolution, UNStandaloneUtility.GetUIIcon("Square"), resolution, resolution);

            GUILayout.EndHorizontal();

            GUILayout.Space(15);

            terrain.sectorResolution = EditorGUILayout.IntSlider(new GUIContent("Sector Resolution", "The resolution of the sector of the terrain, grids count = (resolution)^2"), terrain.sectorResolution, 1, Sector.resolutionLimit);

            if(GUILayout.Button("Recalculate Trees", GUILayout.Height(30)))
            {
                terrain.sector.FetchTreeInstances(true, null);
            }
        }

        void PoolCategory()
        {
            UNEditorUtility.DrawHelpBox("Pool Management", "Here you can manage your items Pool.", false, false);

            terrain.PoolItemType = UNEditorUtility.DrawSelectionBox<System.Type>(new GUIContent("Pool Item Type : ", "What would be the type of the Pool ?"), PoolCache, PoolCache.Find(x => x.item == terrain.PoolItemType));
            terrain.PoolAmount = EditorGUILayout.IntSlider("Pool Amount :", terrain.PoolAmount, 1, 100);

            GUILayout.Space(10);

            if(GUILayout.Button("Update Pool"))
            {
                terrain.CreatePool(terrain.PoolItemType);
            }
        }

        void VegetationCategory()
        {
            terrain.manageGrass = UNEditorUtility.DrawHelpBox("Vegetation Management", "Here you can manage your vegetation.", true, terrain.manageGrass);

            GUILayout.Space(5);

            terrain.updateGrassOnHeightsChange = EditorGUILayout.Toggle("Update Foliage On Height Change: ", terrain.updateGrassOnHeightsChange);

            GUILayout.Space(5);

            var style = GUI.skin.GetStyle("Box");

            GUILayout.BeginVertical(style, GUILayout.MaxWidth(200));

            GUILayout.Label(terrain.terrainData.backedUpTerrainData == null ? "No Backup Found." : "You have an backup available. \nUse it to revert your changes.", EditorStyles.boldLabel);

            GUILayout.Space(15);

            GUILayout.BeginHorizontal();

            if (terrain.terrainData.backedUpTerrainData == null)
            {
                if (GUILayout.Button("Create Backup", GUILayout.MaxWidth(100), GUILayout.MaxHeight(25)))
                {
                    terrain.terrainData.Backup();
                }
            }
            else
            {
                if (GUILayout.Button("Apply Backup", GUILayout.MaxWidth(100), GUILayout.MaxHeight(25)))
                {
                    terrain.terrainData.ApplyBackup(terrain.terrain);
                }

                if (GUILayout.Button("Delete Backup", GUILayout.MaxWidth(100), GUILayout.MaxHeight(25)))
                {
                    if (EditorUtility.DisplayDialog("uNature Backup", "Do you wish to delete the current backup?\nThat cannot be undone!", "Yes", "No"))
                    {
                        terrain.terrainData.DeleteBackup();
                    }
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.Space(15);

            if (GUILayout.Button("Copy Foliage"))
            {
                if (FoliageCore_MainManager.instance != null)
                {
                    if (terrain.terrainData.backedUpTerrainData == null)
                    {
                        if (EditorUtility.DisplayDialog("uNature Backup", "It seems you have no backup created for the terrain data\nWould you want to create one?", "Yes", "No"))
                        {
                            terrain.terrainData.Backup();
                        }
                    }

                    var removeUnityGrass = EditorUtility.DisplayDialog("uNature Copy",
                        "Would you like to remove the current unity grass?",
                        "Yes", "No");

                    FoliageCore_MainManager.instance.InsertFoliageFromTerrain(terrain.terrain, removeUnityGrass);
                }
                else
                {
                    const string ERROR_MESSAGE = "Cannot copy Foliage. FoliageManager can't be found.";
                    EditorUtility.DisplayDialog("uNature Error", ERROR_MESSAGE, "Ok");

                    return;
                }
            }
            if(GUILayout.Button("Remove Foliage From Terrain"))
            {
                terrain.terrainData.ClearDetails();
            }
        }

        void TreesCategory()
        {
            terrain.manageTrees = UNEditorUtility.DrawHelpBox("TreeInstances Management", "Here you can manage your trees.", true, terrain.manageTrees);

            float selectionBoxHeight = terrain.terrainData.treePrototypes.Count > 0 ? 80 : 25;
            selectedPrototypes = UNEditorUtility.DrawPrototypesSelector(terrain.terrainData.treePrototypes, selectedPrototypes, ctrlPressed, ref prototypesScrollbarPos, selectionBoxHeight, false, false, null);

            GUILayout.Space(5);

            GUILayout.BeginVertical("Box");

            if (selectedPrototypes.Count == 0) // no items selected
            {
                GUILayout.Label("No items selected", EditorStyles.centeredGreyMiniLabel);
            }
            else if (selectedPrototypes.Count > 1) // more than 1 object selected.
            {
                GUILayout.Label("Multi-Edit is not currently supported.", EditorStyles.centeredGreyMiniLabel);
            }
            else // draw the prototype edit ui.
            {
                GUILayout.Label("Pool Settings :", EditorStyles.boldLabel);

                selectedPrototypes[0].forcePoolCreation = EditorGUILayout.Toggle(new GUIContent("Force creation of this item: ", "Will this item be generated in the Pool even if its not being used on the terrain"), selectedPrototypes[0].forcePoolCreation);
                selectedPrototypes[0].ignoreTriggetColliders = EditorGUILayout.Toggle("Ignore Creation Trigger Collisions: ", selectedPrototypes[0].ignoreTriggetColliders);
            }

            GUILayout.EndVertical();
        }

        void GetPoolCache()
        {
            _PoolCache = new List<SelectionBoxItems<System.Type>>();
            _PoolCache.Add(new SelectionBoxItems<System.Type>("None", null));

            System.Type type;

            for(int i = 0; i < PoolItem.PoolTypes.Length; i++)
            {
                type = PoolItem.PoolTypes[i];

                _PoolCache.Add(new SelectionBoxItems<System.Type>(type.Name, type));
            }
        }
    }

    public enum TerrainTabs
    {
        Grids,
        Pool,
        Vegetation,
        Trees
    }
}