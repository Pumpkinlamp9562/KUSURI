using System.Reflection;
using UnityEngine;
using System.Collections.Generic;

using uNature.Core.FoliageClasses;

namespace uNature.Core.Utility
{
    public static class UNStandaloneUtility
    {
        static GUIStyle _boldLabel;
        public static GUIStyle boldLabel
        {
            get
            {
                if (_boldLabel == null)
                {
                    _boldLabel = new GUIStyle("Label");
                    _boldLabel.fontStyle = FontStyle.Bold;
                }

                return _boldLabel;
            }
        }

#if UN_WorldStreamer
        static bool _checkedForWStremaerInstance = false;

        static WorldMover worldStreamer_Mover;
        public static WorldMover WorldStreamer_Mover
        {
            get
            {
                if(worldStreamer_Mover == null && !_checkedForWStremaerInstance)
                {
                    worldStreamer_Mover = GameObject.FindObjectOfType<WorldMover>();
                    _checkedForWStremaerInstance = true;
                }

                return worldStreamer_Mover;
            }
        }
#endif

        static System.Action<Plane[], Matrix4x4> _ExtractPlanes = null;
        private static System.Action<Plane[], Matrix4x4> ExtractPlanes
        {
            get
            {
                if (_ExtractPlanes == null)
                {
                    MethodInfo info = typeof(GeometryUtility).GetMethod("Internal_ExtractPlanes", BindingFlags.Static | BindingFlags.NonPublic, null, new System.Type[] { typeof(Plane[]), typeof(Matrix4x4) }, null);
                    _ExtractPlanes = System.Delegate.CreateDelegate(typeof(System.Action<Plane[], Matrix4x4>), info) as System.Action<Plane[], Matrix4x4>;
                }

                return _ExtractPlanes;
            }
        }

        /// <summary>
        /// Begin horizontal offset
        /// </summary>
        /// <param name="offset"></param>
        public static void BeginHorizontalOffset(float offset)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Space(offset);

            GUILayout.BeginVertical();
        }

        /// <summary>
        /// End horizontal offset
        /// </summary>
        /// <param name="offset"></param>
        public static void EndHorizontalOffset()
        {
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Get an UI Icon - used in the core system of uNature.
        /// </summary>
        /// <param name="iconName"></param>
        /// <returns></returns>
        public static Texture2D GetUIIcon(string iconName)
        {
            string skin = "DarkTheme";

#if UNITY_EDITOR
            skin = Settings.UNSettings.EditorSkin == Settings.UNEditorSkin.ProSkin ? "DarkTheme" : "LightTheme";
#endif

            return Resources.Load<Texture2D>(string.Format("Images/UI/{0}/{1}_Icon", skin, iconName));
        }

        /// <summary>
        /// This returns the streaming value from the streaming solutions.
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetStreamingAdjuster()
        {
            Vector3 result = Vector3.zero;

#if UN_WorldStreamer
            if (WorldStreamer_Mover != null)
            {
                result = WorldStreamer_Mover.currentMove * -1; // invert current move.
            }
#endif

            return result;
        }

        public static Plane[] CalculateFrustumPlanes(Camera camera, Plane[] planeArray)
        {
            ExtractPlanes(planeArray, camera.projectionMatrix * camera.worldToCameraMatrix);

            return planeArray;
        }

        #region Foliage Utilities
        /// <summary>
        /// Try to get the id of an certain detail prototype from the db.
        /// </summary>
        /// <param name="prototype"></param>
        /// <returns></returns>
        public static int TryGetPrototypeIndex(DetailPrototype prototype)
        {
            for (int i = 0; i < FoliageDB.unSortedPrototypes.Count; i++)
            {
                if (FoliageDB.unSortedPrototypes[i].EqualsToPrototype(prototype))
                {
                    return FoliageDB.unSortedPrototypes[i].id;
                }
            }

            return -1;
        }

        /// <summary>
        /// Get the terrain details on the terrain
        /// </summary>
        /// <returns></returns>
        public static List<int[,]> GetTerrainDetails(TerrainData terrainData)
        {
            List<int[,]> detailPrototypes = new List<int[,]>();

            for (int i = 0; i < terrainData.detailPrototypes.Length; i++)
            {
                detailPrototypes.Add(terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, i));
            }

            return detailPrototypes;
        }

        /// <summary>
        /// This will add prototypes to the prototypes cache if they dont already exist.
        /// </summary>
        /// <param name="terrain"></param>
        public static FoliagePrototype[] AddPrototypesIfDontExist(DetailPrototype[] prototypes)
        {
            FoliagePrototype[] foliagePrototypes = new FoliagePrototype[prototypes.Length];

            if (FoliageDB.unSortedPrototypes.Count == 0) // if there are 0 prototypes => lets create them instantly because they cant be duplicated!.
            {
                for (int i = 0; i < prototypes.Length; i++)
                {
                    foliagePrototypes[i] = FoliageDB.instance.AddPrototype(prototypes[i]);
                }

                return foliagePrototypes;
            }

            for (int i = 0; i < prototypes.Length; i++)
            {
                for (int b = 0; b < FoliageDB.unSortedPrototypes.Count; b++)
                {
                    if (FoliageDB.unSortedPrototypes[b].EqualsToPrototype(prototypes[i]))
                    {
                        foliagePrototypes[i] = FoliageDB.unSortedPrototypes[b];

                        break;
                    }
                    else if (b == FoliageDB.unSortedPrototypes.Count - 1) // if we didnt find any match and this is the last index
                    {
                        foliagePrototypes[i] = FoliageDB.instance.AddPrototype(prototypes[i]);
                    }
                }
            }

            return foliagePrototypes;
        }

        /// <summary>
        /// Get foliage chunks in range [9]
        /// </summary>
        /// <param name="pos"></param>
        public static FoliageCore_Chunk[] GetFoliageChunksNeighbors(Vector3 pos, FoliageCore_Chunk[] cache)
        {
            if (cache == null)
            {
                cache = new FoliageCore_Chunk[9];
            }

            if (cache.Length != 9)
            {
                Debug.LogError("UNATURE ERROR! CACHE SIZE IS BIGGER THAN NINE!");
                return null;
            }

            FoliageCore_MainManager mManager = FoliageCore_MainManager.instance;

            if (mManager == null)
            {
                Debug.LogError("UNATURE ERROR! Foliage Manager can't be found!");
                return null;
            }

            int topDownAdjuster = FoliageCore_MainManager.FOLIAGE_MAIN_AREA_RESOLUTION;
            int rightLeftAdjuster = 1;

            int centerMiddleID = FoliageCore_MainManager.instance.GetChunkID(pos.x, pos.z);
            int centerRightID = centerMiddleID - rightLeftAdjuster;
            int centerLeftID = centerMiddleID + rightLeftAdjuster;

            int topMiddleID = centerMiddleID + topDownAdjuster;
            int topRightID = topMiddleID + rightLeftAdjuster;
            int topLeftID = topMiddleID - rightLeftAdjuster;

            int bottomMiddleID = centerMiddleID - topDownAdjuster;
            int bottomRightID = bottomMiddleID + rightLeftAdjuster;
            int bottomLeftID = bottomMiddleID - rightLeftAdjuster;

            cache[0] = mManager.CheckChunkInBounds(bottomRightID) ? mManager.sector.foliageChunks[bottomRightID] : null; // bottom right
            cache[1] = mManager.CheckChunkInBounds(bottomMiddleID) ? mManager.sector.foliageChunks[bottomMiddleID] : null; // bottom center
            cache[2] = mManager.CheckChunkInBounds(bottomLeftID) ? mManager.sector.foliageChunks[bottomLeftID] : null; // bottom left
            cache[3] = mManager.CheckChunkInBounds(centerRightID) ? mManager.sector.foliageChunks[centerRightID] : null; // middle right
            cache[4] = mManager.CheckChunkInBounds(centerMiddleID) ? mManager.sector.foliageChunks[centerMiddleID] : null; // middle center
            cache[5] = mManager.CheckChunkInBounds(centerLeftID) ? mManager.sector.foliageChunks[centerLeftID] : null; // middle left
            cache[6] = mManager.CheckChunkInBounds(topRightID) ? mManager.sector.foliageChunks[topRightID] : null; // top right
            cache[7] = mManager.CheckChunkInBounds(topMiddleID) ? mManager.sector.foliageChunks[topMiddleID] : null; // top center
            cache[8] = mManager.CheckChunkInBounds(topLeftID) ? mManager.sector.foliageChunks[topLeftID] : null; // top left

            return cache;
        }
        #endregion
    }

    public struct Vector3_XZ_FAST
    {
        public float x;
        public float z;

        public Vector3_XZ_FAST(Vector3 v3)
        {
            x = v3.x;
            z = v3.z;
        }
        public Vector3_XZ_FAST(Vector2 v2)
        {
            x = v2.x;
            z = v2.y;
        }
        public Vector3_XZ_FAST(float x, float z)
        {
            this.x = x;
            this.z = z;
        }
    }
}
