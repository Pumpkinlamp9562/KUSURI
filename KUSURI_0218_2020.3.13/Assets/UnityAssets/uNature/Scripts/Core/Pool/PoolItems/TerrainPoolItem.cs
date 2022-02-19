using UnityEngine;
using System.Collections;

using uNature.Core.Threading;
using uNature.Core.ClassExtensions;
using System;

namespace uNature.Core.Pooling
{
    public delegate void OnTreeInstanceStateChanged(Terrain terrain, int instanceID);

    /// <summary>
    /// A Pool item for terrain. (Tree instances)
    /// </summary>
    public class TerrainPoolItem : PoolItem
    {
        #region Events
        public static event OnTreeInstanceStateChanged OnTreeInstanceRemoved;
        public static event OnTreeInstanceStateChanged OnTreeInstanceRestored;
        #endregion

        #region Variables
        /// <summary>
        /// Can this machine modify tree instances?
        /// </summary>
        public static bool canModify = true;

        /// <summary>
        /// Can this machine restore tree instances?
        /// </summary>
        public static bool canRestore = true;

        /// <summary>
        /// The rigidbody on this object, which is used for movement.
        /// </summary>
        Rigidbody _rigid;
        protected Rigidbody rigid
        {
            get
            {
                _rigid = GetComponent<Rigidbody>();

                if (_rigid == null)
                {
                    _rigid = gameObject.AddComponent<Rigidbody>();
                    _rigid.isKinematic = true;
                }

                return _rigid;
            }
        }

        /// <summary>
        /// is this instance a collider ? or an actual tree instance ?
        /// </summary>
        public bool isCollider;

        /// <summary>
        /// The terrain which owns this Pool item.
        /// </summary>
        [HideInInspector]
        public Terrain _terrain;
        public Terrain terrain
        {
            get
            {
                if (_terrain == null)
                {
                    _terrain = Pool.owner.GetComponent<Terrain>();
                }

                return _terrain;
            }
        }
        #endregion

        /// <summary>
        /// Move with rigidbody to avoid colliders movement.
        /// </summary>
        /// <param name="position">target position</param>
        public override void MoveItem(Vector3 position)
        {
            UNThreadManager.instance.RunOnUnityThread(new ThreadTask<Vector3>((Vector3 _pos) =>
            {
                if (isCollider)
                    rigid.MovePosition(_pos);
                else
                    this.transform.position = _pos;
            }, position));

            base.MoveItem(position);
        }

        /// <summary>
        /// Remove a tree instance from the terrain,
        /// Allowing you to replace it with anything else - for instance, the actual game object of the tree.
        /// </summary>
        public static void RemoveTreeInstanceFromTerrain(Terrain terrain, int treeInstanceUID)
        {
            if (terrain == null || treeInstanceUID == -1 || !canModify) return;

            uNature.Core.Terrains.UNTerrain UNTerrain = terrain.GetComponent<uNature.Core.Terrains.UNTerrain>();

            if (UNTerrain == null) return;

            if (OnTreeInstanceRemoved != null)
                OnTreeInstanceRemoved(terrain, treeInstanceUID);

            terrain.terrainData.RemoveTreeInstance(treeInstanceUID, UNTerrain);
        }

        /// <summary>
        /// Remove a tree instance from the terrain,
        /// And replace it with a Pool item.
        /// </summary>
        public static void ConvertTreeInstanceOnTerrain(Terrain terrain, int treeInstanceUID)
        {
            if (terrain == null || treeInstanceUID == -1 || !canModify) return;

            uNature.Core.Terrains.UNTerrain UNTerrain = terrain.GetComponent<uNature.Core.Terrains.UNTerrain>();

            if (UNTerrain == null) return;

            if (OnTreeInstanceRemoved != null)
                OnTreeInstanceRemoved(terrain, treeInstanceUID);

            terrain.ConvertTreeInstance(treeInstanceUID, UNTerrain);
        }

        /// <summary>
        /// Restore the tree instance back into the terrain.
        /// </summary>
        public static void RestoreTreeInstanceToTerrain(Terrain terrain, int treeInstanceUID)
        {
            if (terrain == null || treeInstanceUID == -1 || !canRestore) return;

            uNature.Core.Terrains.UNTerrain UNTerrain = terrain.GetComponent<uNature.Core.Terrains.UNTerrain>();

            if (UNTerrain == null) return;

            if (OnTreeInstanceRestored != null)
                OnTreeInstanceRestored(terrain, treeInstanceUID);

            terrain.terrainData.RestoreTreeInstance(treeInstanceUID, UNTerrain);
        }
    }
}
