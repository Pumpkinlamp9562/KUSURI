using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using uNature.Core.FoliageClasses;

namespace uNature.Core
{

    /// <summary>
    /// This class handles all custom physics.
    /// </summary>
    public static class UNPhysics
    {
        static UNPhysicsObject currentPObject;
        static RaycastHit rayHit;
        static Ray ray;

        /// <summary>
        ///  Create a raycast
        /// </summary>
        /// <param name="origin">The origin of the raycast</param>
        /// <param name="direction">The direction of the raycast</param>
        /// <param name="distance">max distance</param>
        /// <param name="mask">mask</param>
        /// <param name="offset">raycast offset</param>
        /// <returns>The hits value</returns>
        public static UNPhysicsHitsArray RaycastAll(Vector3 origin, Vector3 direction, float distance, int mask, float offset)
        {
            /*
            UNPhysicsHit_Grass hit;
            hits = new UNPhysicsHitsArray();
            ray = new Ray(origin, direction);

            FoliagePrototype prototype;
            FoliageChunk lastRenderedChunk = FoliageManager.instance.middleChunk;
            byte maxDensity;
            GPUMesh meshData;
            GPUMeshLOD meshLOD;

            for (int prototypeIndex = 0; prototypeIndex < FoliageDB.unSortedPrototypes.Count; prototypeIndex++)
            {
                prototype = FoliageDB.unSortedPrototypes[prototypeIndex];

                if (lastRenderedChunk != null)
                {
                    maxDensity = lastRenderedChunk.GetMaxDensityOnArea(prototype.id);
                    meshData = FoliageManager.instance.prototypeMeshInstances[prototype.id];
                }
            }

            return hits;
            */

            return null;
        }
        /// <summary>
        /// Creates a raycast
        /// </summary>
        /// <param name="ray">The ray of the raycast</param>
        /// <param name="distance">Max distance</param>
        /// <param name="mask">LayerMask</param>
        /// <returns>Returns the hits array</returns>
        public static UNPhysicsHitsArray RaycastAll(Ray ray, float distance, int mask)
        {
            return RaycastAll(ray.origin, ray.direction, distance, mask, 0);
        }
        /// <summary>
        /// Creates a raycast
        /// </summary>
        /// <param name="ray">The ray of the raycast</param>
        /// <param name="distance">Max distance</param>
        /// <param name="mask">LayerMask</param>
        /// <param name="offset">raycast offset</param>
        /// <returns>Returns the hits array</returns>
        public static UNPhysicsHitsArray RaycastAll(Ray ray, float distance, int mask, float offset)
        {
            return RaycastAll(ray.origin, ray.direction, distance, mask, offset);
        }
        /// <summary>
        /// Creates a raycast
        /// </summary>
        /// <param name="origin">the origin of the raycast</param>
        /// <param name="direction">the direction of the raycast</param>
        /// <param name="hit">returns the hit data</param>
        /// <param name="distance">max distance</param>
        /// <param name="mask">layerMask</param>
        /// <param name="offset">raycast offset</param>
        /// <returns>did we hit something?</returns>
        public static bool Raycast(Vector3 origin, Vector3 direction, out UNPhysicsHit_Grass hit, float distance, int mask, float offset)
        {
            UNPhysicsHitsArray hits = RaycastAll(origin, direction, distance, mask, offset);
            hit = new UNPhysicsHit_Grass();

            if (hits.Count <= 0) return false;

            hits.Sort();

            hit = hits[0];
            return true;
        }
        /// <summary>
        /// Creates a raycast
        /// </summary>
        /// <param name="origin">the origin of the raycast</param>
        /// <param name="direction">the direction of the raycast</param>
        /// <param name="hit">returns the hit data</param>
        /// <param name="distance">max distance</param>
        /// <param name="mask">layerMask</param>
        /// <returns>did we hit something?</returns>
        public static bool Raycast(Vector3 origin, Vector3 direction, out UNPhysicsHit_Grass hit, float distance, int mask)
        {
            UNPhysicsHitsArray hits = RaycastAll(origin, direction, distance, mask, 0);
            hit = new UNPhysicsHit_Grass();

            if (hits.Count <= 0) return false;

            hits.Sort();

            hit = hits[0];
            return true;
        }
        /// <summary>
        /// Creates a raycast
        /// </summary>
        /// <param name="origin">The origin of the ray</param>
        /// <param name="direction">The direction of the ray</param>
        /// <param name="hit">The hit data of the ray</param>
        /// <param name="distance">max distance</param>
        /// <returns>did we hit something?</returns>
        public static bool Raycast(Vector3 origin, Vector3 direction, out UNPhysicsHit_Grass hit, float distance)
        {
            return Raycast(origin, direction, out hit, distance, -1);
        }
        /// <summary>
        /// Creates a raycast
        /// </summary>
        /// <param name="ray">the ray of the raycast</param>
        /// <param name="hit">the hit data</param>
        /// <param name="distance">max distance</param>
        /// <returns></returns>
        public static bool Raycast(Ray ray, out UNPhysicsHit_Grass hit, float distance, bool TakeUnityPhysicsIntoAccount, Transform target)
        {
            return Raycast(ray.origin, ray.direction, out hit, distance, -1);
        }
        /// <summary>
        /// Creates a raycast
        /// </summary>
        /// <param name="ray">the ray of the raycast</param>
        /// <param name="hit">returns the hit data</param>
        /// <param name="distance">max distance</param>
        /// <param name="mask">layerMask</param>
        /// <returns>did we hit something?</returns>
        public static bool Raycast(Ray ray, out UNPhysicsHit_Grass hit, float distance, int mask)
        {
            return Raycast(ray.origin, ray.direction, out hit, distance, mask);
        }

    }

    /// <summary>
    /// An custom array that holds all ray results in an array
    /// </summary>
    public class UNPhysicsHitsArray
    {
        private List<UNPhysicsHit_Grass> _data;

        public UNPhysicsHitsArray()
        {
            _data = new List<UNPhysicsHit_Grass>();
        }

        public UNPhysicsHit_Grass this[int index]
        {
            get { return _data[index]; }
        }
        public void AddToList(UNPhysicsHit_Grass hit)
        {
            _data.Add(hit);
        }
        public void Sort()
        {
            _data.Sort(delegate(UNPhysicsHit_Grass a, UNPhysicsHit_Grass b)
            {
                return a.distance.CompareTo(b.distance);
            });
        }
        public int Count
        {
            get { return _data.Count; }
        }
    }

    /// <summary>
    /// A class that holds the data for the hit data
    /// </summary>
    public struct UNPhysicsHit_Grass
    {
        public Vector3 point;

        public float distance;
    }
    /// <summary>
    /// Ignore all physics on this script.
    /// </summary>
    interface IUTCPhysicsIgnored
    {
        bool ignore { get; }
    }
}