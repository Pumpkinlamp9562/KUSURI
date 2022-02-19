using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using uNature.Core.Terrains;
using uNature.Core.Sectors;
using uNature.Core.Pooling;
using uNature.Core.Seekers;

using System;
using System.Linq;

namespace uNature.Core.ClassExtensions
{
    /// <summary>
    /// Some extensions that helps achieving things that aren't built in with unity.
    /// </summary>
    public static class ClassExtensions
    {
        /// <summary>
        /// Conver a terrain position to the world position
        /// </summary>
        /// <param name="pos">current position in local space</param>
        /// <param name="terrainScale">the size of the terrain this point is on</param>
        /// <param name="terrainPosition">the position of the terrain this point is on</param>
        /// <returns>the new position in world space</returns>
        public static Vector3 LocalToWorld(this Vector3 pos, Vector3 terrainScale, Vector3 terrainPosition)
        {
            return Vector3.Scale(pos, terrainScale) + terrainPosition; 
        }

        /// <summary>
        /// Convert a world space position to terrain position
        /// </summary>
        /// <param name="pos">the world space position</param>
        /// <param name="terrain">the terrain you want to get this position on</param>
        /// <returns>the new converted local spaced position.</returns>
        public static Vector3 WorldToLocal(this Vector3 pos, Terrain terrain)
        {
            return new Vector3(
                    pos.x / terrain.terrainData.size.x,
                    pos.y / terrain.terrainData.size.y,
                    pos.z / terrain.terrainData.size.z) - terrain.transform.position;
        }

        /// <summary>
        /// Get only the used prototypes from a terrain (the ones who are drawn on the terrain).
        /// </summary>
        /// <param name="terrainData">the terrain data.</param>
        /// <returns>Only the used prototypes on that terrain.</returns>
        public static List<TreePrototype> GetUsedPrototypes(this TerrainData terrainData)
        {
            //List<TreePrototype> prototypes;
            List<TreePrototype> prototypesDict = new List<TreePrototype>();

            TreePrototype prototype;
            TreeInstance tree;

            if (terrainData != null && terrainData.treePrototypes != null)
            {
                for (int i = 0; i < terrainData.treePrototypes.Length; i++)
                {
                    prototype = terrainData.treePrototypes[i];

                    for (int b = 0; b < terrainData.treeInstanceCount; b++)
                    {
                        tree = terrainData.GetTreeInstance(b);

                        if (tree.prototypeIndex == i)
                        {
                            prototypesDict.Add(prototype);
                            break;
                        }
                    }
                }
            }

            return prototypesDict;
        }

        /// <summary>
        /// Run this safe method if you want to edit anything on the specific tree instance.
        /// This will cause the changes to be saved over the chunks and it will be "safe".
        /// </summary>
        /// <param name="treeInstance">the tree instance</param>
        /// <param name="treeInstanceID">The id of the tree instance</param>
        /// <param name="action">the action</param>
        public static void RunActionOnTreeInstance(this Terrain terrain, TreeInstance treeInstance, int treeInstanceID, System.Func<TreeInstance, TreeInstance> action, System.Action<Sectors.ChunkObject> runOnChunk)
        {
            UNTerrain baseTerrain = terrain.GetComponent<UNTerrain>();

            if (baseTerrain != null)
            {
                TIChunk chunk = baseTerrain.sector.getChunk(treeInstance.position.LocalToWorld(terrain.terrainData.size, Vector3.zero), 0) as TIChunk;

                if (chunk != null)
                {
                    if (action != null)
                    {
                        TreeInstance result = action(treeInstance);

                        if (chunk != null)
                        {
                            for (int i = 0; i < chunk.objects.Count; i++)
                            {
                                if (chunk.objects[i].instanceID == treeInstanceID)
                                {
                                    chunk.objects[i].treeInstance = result; // update tree instance

                                    if (runOnChunk != null)
                                    {
                                        runOnChunk(chunk.objects[i]);
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Remove a certain tree instance from the terrain.
        /// </summary>
        /// <param name="treeInstanceID">the tree instance id of the tree you wish to remove.</param>
        public static void RemoveTreeInstance(this TerrainData terrainData, int treeInstanceID, UNTerrain UNTerrain)
        {
            TreeInstance _treeInstance = terrainData.GetTreeInstance(treeInstanceID);

            UNTerrain.terrain.RunActionOnTreeInstance(_treeInstance, treeInstanceID, (TreeInstance treeInstance) =>
                {
                    treeInstance.heightScale = UNTerrain.removedTreeInstanceHeight;

                    terrainData.SetTreeInstance(treeInstanceID, treeInstance);

                    return treeInstance;
                },
                (Sectors.ChunkObject chunkObject) =>
                {
                    chunkObject.Remove();
                });
        }

        /// <summary>
        /// Restore a certain tree instance from the terrain.
        /// </summary>
        /// <param name="treeInstanceID">the tree instance id of the tree you wish to restore.</param>
        /// <param name="originalHeight">the original height of the targeted tree.</param>
        public static void RestoreTreeInstance(this TerrainData terrainData, int treeInstanceID, UNTerrain UNTerrain)
        {
            TreeInstance treeInstance = terrainData.GetTreeInstance(treeInstanceID);
            Sectors.ChunkObject chunkObject;

            // Update the sector chunk...
            TIChunk chunk = UNTerrain.sector.getChunk(treeInstance.position.LocalToWorld(UNTerrain.terrainData.multiThreaded_terrainDataSize, Vector3.zero), 0) as TIChunk;

            if (chunk != null)
            {
                for (int i = 0; i < chunk.objects.Count; i++)
                {
                    chunkObject = chunk.objects[i];

                    if (chunkObject.instanceID == treeInstanceID)
                    {
                        UNTerrain.Pool.TryResetOnUID(treeInstanceID, true);

                        treeInstance.heightScale = chunkObject.originalHeight;

                        terrainData.SetTreeInstance(treeInstanceID, treeInstance);

                        chunkObject.treeInstance = treeInstance;

                        UNSeeker seeker;
                        for (int c = 0; c < UNSeeker.FReceivers.Count; c++)
                        {
                            seeker = UNSeeker.FReceivers[c] as UNSeeker;

                            Targets.UNTarget.CheckTargets(seeker, seeker.seekingDistance); // check targets to verify colliders assigning
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Convert a tree and assign a Pool item to it.
        /// </summary>
        /// <param name="terrain">the terrain.</param>
        /// <param name="treeInstanceID">the tree instance id</param>
        /// <param name="UNTerrain">the UN terrain instance</param>
        public static void ConvertTreeInstance(this Terrain terrain, int treeInstanceID, UNTerrain UNTerrain)
        {
            TreeInstance tree = terrain.terrainData.GetTreeInstance(treeInstanceID);

            Vector3 treeScale = tree.GetWorldScale();
            Quaternion treeRotation = tree.GetWorldRotation();

            terrain.terrainData.RemoveTreeInstance(treeInstanceID, UNTerrain);

            Vector3 pos = tree.position.LocalToWorld(terrain.terrainData.size, terrain.transform.position);

            UNTerrain.Pool.TryResetOnUID(treeInstanceID, true);
            var item = UNTerrain.Pool.TryPool<HarvestableTIPoolItem>(tree.prototypeIndex, 0, treeInstanceID, true, false);

            if (item != null)
            {
                item.transform.rotation = treeRotation;
                item.transform.localScale = treeScale;

                item.MoveItem(pos);

                item.OnPool();
            }
        }

        /// <summary>
        /// Return a scale of a tree instance
        /// </summary>
        /// <param name="treeInstance">the tree instance</param>
        /// <returns>A world space scale from the tree instance</returns>
        public static Vector3 GetWorldScale(this TreeInstance treeInstance)
        {
            return new Vector3(treeInstance.widthScale, treeInstance.heightScale, treeInstance.widthScale);
        }

        /// <summary>
        /// Get relative to terrain scale. (in order to use terrain's properties)
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="terrain"></param>
        /// <returns></returns>
        public static Vector2 GetRelativeScale(this Vector2 scale, UNTerrain terrain)
        {
            Vector2 relatedScale = new Vector2(scale.x / (terrain.terrainData.multiThreaded_terrainDataSize.x / terrain.terrainData.multiThreaded_detailWidth), scale.y / (terrain.terrainData.multiThreaded_terrainDataSize.z / terrain.terrainData.multiThreaded_detailHeight));
            return relatedScale;
        }

        /// <summary>
        /// Return a rotation of a tree instance
        /// </summary>
        /// <param name="treeInstance">the tree instance</param>
        /// <returns>A world space rotation from the tree instance</returns>
        public static Quaternion GetWorldRotation(this TreeInstance treeInstance)
        {
            return Quaternion.AngleAxis(treeInstance.rotation * Mathf.Rad2Deg, Vector3.up);
        }

        /// <summary>
        /// Checks if the item is contained inside a list.
        /// Checks for an object instead of a type.
        /// </summary>
        /// <returns></returns>
        public static bool Contains<T>(this List<T> list, object obj)
        {
            for(int i = 0; i < list.Count; i++)
            {
                if(list[i].Equals(obj))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Copy a component from 1 object to another.
        /// 
        /// Credits:
        /// Script shared by "vexe", created by "Jamora" on unity forums.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comp"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static T GetCopyOf<T>(this System.Object comp, T other)
        {
            System.Type type = comp.GetType();
            if (type != other.GetType()) return default(T); // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite && pinfo.Name != "name")
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
            return (T)comp;
        }

        /// <summary>
        /// Add an existing component (duplicate).
        /// 
        /// Credits:
        /// Script shared by "vexe", created by "Jamora" on unity forums.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <param name="toAdd"></param>
        /// <returns></returns>
        public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent<T>().GetCopyOf(toAdd) as T;
        }

        /// <summary>
        /// Check if a type inherites a certain type.
        /// 
        /// Credit to Sergii Bogomolov at StackOverflow.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static bool InheritsFrom(this Type type, Type baseType)
        {
            // null does not have base type
            if (type == null)
            {
                return false;
            }

            // only interface can have null base type
            if (baseType == null)
            {
                return type.IsInterface;
            }

            // check implemented interfaces
            if (baseType.IsInterface)
            {
                return type.GetInterfaces().Contains(baseType);
            }

            // check all base types
            var currentType = type;
            while (currentType != null)
            {
                if (currentType.BaseType == baseType)
                {
                    return true;
                }

                currentType = currentType.BaseType;
            }

            return false;
        }
    }
}
