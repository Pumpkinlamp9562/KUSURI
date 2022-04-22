using UnityEngine;
using System.Collections;
using UnityEditor;

using uNature.Core.Terrains;
using uNature.Core.FoliageClasses;

namespace uNature.Core.Editor.Helpers
{
    public class UNEditorHelpers
    {
        [MenuItem("Window/uNature/Helpers/Setup_SceneTerrains")]
        public static void SetupSceneTerrains()
        {
            Terrain terrain;
            Terrain[] terrains = GameObject.FindObjectsOfType<Terrain>();
            UNTerrain terrainComponent;

            for (int i = 0; i < terrains.Length; i++)
            {
                terrain = terrains[i];

                terrainComponent = terrain.GetComponent<UNTerrain>();

                if (terrainComponent == null)
                {
                    terrainComponent = terrain.gameObject.AddComponent<UNTerrain>();
                }

                if (terrainComponent.sector == null)
                {
                    terrainComponent.GenerateSector(terrainComponent.sectorResolution);
                }

                if (terrainComponent.Pool == null)
                {
                    terrainComponent.CreatePool(terrainComponent.PoolItemType);
                }
            }
        }

        [MenuItem("Window/uNature/Helpers/Fix_TreeInstances")]
        public static void FixCorruptedTreeInstanceOnSceneTerrains()
        {
            UNTerrain[] terrains = GameObject.FindObjectsOfType<UNTerrain>();
            UNTerrain terrain;
            TreeInstance[] instances;
            TreeInstance instance;
            int count = 0;

            for (int i = 0; i < terrains.Length; i++)
            {
                terrain = terrains[i];
                instances = terrain.terrain.terrainData.treeInstances;

                if (terrain.sector != null)
                {
                    foreach (var chunk in terrain.sector.treeInstancesChunks)
                    {
                        if (chunk.objects == null)
                        {
                            chunk.GenerateTreeInstances(instances, terrain.terrain.terrainData.size, terrain.terrain.terrainData, terrain.terrain.transform.position);
                        }

                        foreach (var obj in chunk.objects)
                        {
                            if (obj.isRemoved)
                            {
                                count++;

                                instance = terrain.terrain.terrainData.GetTreeInstance(obj.instanceID);
                                instance.heightScale = 1f;
                                obj.treeInstance = instance;

                                terrain.terrain.terrainData.SetTreeInstance(obj.instanceID, instance);
                            }
                        }
                    }
                }
            }
        }

        [MenuItem("Window/uNature/Helpers/Destroy_FoliageManager")]
        public static void DestroyFoliageManager()
        {
            if(FoliageClasses.FoliageCore_MainManager.instance != null)
            {
                FoliageClasses.FoliageCore_MainManager.DestroyManager();
            }
        }

        [MenuItem("Window/uNature/Helpers/Copy Foliage From Selected Terrains")]
        public static void CopySelectedTerrains()
        {
            if (FoliageClasses.FoliageCore_MainManager.instance != null)
            {
                Transform[] selectedTransforms = Selection.transforms;
                Terrain terrain;

                for(int i = 0; i < selectedTransforms.Length; i++)
                {
                    terrain = selectedTransforms[i].GetComponent<Terrain>();

                    if(terrain != null)
                    {
                        FoliageClasses.FoliageCore_MainManager.instance.InsertFoliageFromTerrain(terrain);
                    }

                    Debug.Log("Copying Foliage From Terrain : " + terrain.name + " Complete!");
                }
            }
        }

        [MenuItem("Window/uNature/Helpers/Update Heights On Selected Terrains")]
        public static void UpdateSelectedTerrains()
        {
            if (FoliageClasses.FoliageCore_MainManager.instance != null)
            {
                Transform[] selectedTransforms = Selection.transforms;
                Terrain terrain;

                for (int i = 0; i < selectedTransforms.Length; i++)
                {
                    terrain = selectedTransforms[i].GetComponent<Terrain>();

                    if (terrain != null)
                    {
                        FoliageClasses.FoliageCore_MainManager.instance.UpdateHeightsOnTerrain(terrain);
                    }

                    Debug.Log("Copying Foliage From Terrain : " + terrain.name + " Complete!");
                }
            }
        }

        [MenuItem("Window/uNature/Helpers/Remove projector")]
        public static void RemoveProjector()
        {
            GameObject obj = GameObject.Find("BrushProjector(Clone)");

            if(obj != null)
            {
                Debug.Log("Cleaned projector!");
                GameObject.DestroyImmediate(obj);

                obj = GameObject.Find("BrushProjector(Clone)");

                if (obj != null) RemoveProjector();
            }
        }

        [MenuItem("Window/uNature/DEBUG/DebugWindow")]
        public static void ShowDebugWindow()
        {
            if (!Settings.UNSettings.instance.UN_Console_Debugging_Enabled)
            {
                Debug.Log("Debugging disabled through the uNature Settings. Enable it first to show the debug window.");
                return;
            }

            if (FoliageClasses.FoliageCore_MainManager.instance != null)
            {
                FoliageClasses.FoliageCore_MainManager.instance.DEBUG_Window_Open = true;
            }
        }

        [MenuItem("Window/uNature/QuickSetup_ApplyUNature")]
        public static void ApplyuNatureInScene()
        {
            FoliageCore_MainManager.InitializeAndCreateIfNotFound();

            FoliageCore_MainManager manager = FoliageCore_MainManager.instance;

            Terrain[] terrains = Terrain.activeTerrains;
            Terrain terrain;
            UNTerrain unTerrain;

            for (int i = 0; i < terrains.Length; i++)
            {
                terrain = terrains[i];

                unTerrain = terrain.GetComponent<UNTerrain>();

                if(unTerrain == null)
                {
                    unTerrain = terrain.gameObject.AddComponent<UNTerrain>();
                }

                manager.InsertFoliageFromTerrain(terrain);
            }
        }
    }
}
