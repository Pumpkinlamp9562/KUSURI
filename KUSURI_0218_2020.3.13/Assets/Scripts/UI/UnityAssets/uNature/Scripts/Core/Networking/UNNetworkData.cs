using UnityEngine;
using System.Collections.Generic;

using System.Runtime.Serialization.Formatters.Binary;

using System.IO;

using uNature.Core.Terrains;
using uNature.Core.Pooling;

namespace uNature.Core.Networking
{
    /// <summary>
    /// A class which can be used for an abstract networking data.
    /// <typeparam name="T">The network connection type which the networking library uses.</typeparam>
    /// </summary>
    [System.Serializable]
    public class UNNetworkData<T> : BaseUNNetworkData
    {
        /// <summary>
        /// Pack the data and create a data instance
        /// </summary>
        public static T2 Pack<T1, T2>(Terrain terrain, int treeInstanceID, int health, PacketType type) where T2 : UNNetworkData<T1>
        {
            T2 instance = System.Activator.CreateInstance<T2>();
            instance.terrainID = terrain.name;
            instance.treeInstanceID = treeInstanceID;
            instance.eventType = type;

            UNTerrain UNTerrain = terrain.GetComponent<UNTerrain>();

            if (instance.minHealth == -1 && instance.maxHealth == -1 && UNTerrain != null)
            {
                TreeInstance treeInstance = UNTerrain.terrain.terrainData.GetTreeInstance(treeInstanceID);
                HarvestableTIPoolItem harvestableComponent = UNTerrain.terrain.terrainData.treePrototypes[treeInstance.prototypeIndex].prefab.GetComponent<HarvestableTIPoolItem>();

                if (harvestableComponent != null)
                {
                    instance.minHealth = harvestableComponent.minHealth;
                    instance.maxHealth = harvestableComponent.maxHealth;
                }
                else
                {
                    harvestableComponent = UNTerrain.Pool.TryGetType<HarvestableTIPoolItem>();

                    if (harvestableComponent != null)
                    {
                        instance.minHealth = harvestableComponent.minHealth;
                        instance.maxHealth = harvestableComponent.maxHealth;
                    }
                }
            }

            instance.health = health;

            return instance;
        }

        /// <summary>
        /// Unpack the data
        /// </summary>
        public override void UnPack()
        {
            base.UnPack();

            var terrains = UNTerrain.terrains;
            Terrain terrain;
            UNTerrain unTerrainComponent;

            for (int i = 0; i < terrains.Count; i++)
            {
                unTerrainComponent = terrains[i];
                terrain = unTerrainComponent.terrain;

                if (terrain.name == terrainID)
                {
                    if (eventType == PacketType.HealthUpdate)
                    {
                        BaseUNNetworkData data = null;

                        if (!bufferedData.Contains(this))
                        {
                            bufferedData.Add(this);
                            data = this;
                        }
                        else
                        {
                            for (int b = 0; b < bufferedData.Count; b++)
                            {
                                if (this.Equals(bufferedData[b]))
                                {
                                    data = bufferedData[b];
                                    break;
                                }
                            }
                        }

                        if (data != null)
                        {
                            if (data.minHealth == -1 && data.maxHealth == -1)
                            {
                                TreeInstance treeInstance = terrain.terrainData.GetTreeInstance(data.treeInstanceID);
                                HarvestableTIPoolItem harvestableComponent = terrain.terrainData.treePrototypes[treeInstance.prototypeIndex].prefab.GetComponent<HarvestableTIPoolItem>();

                                if (harvestableComponent != null)
                                {
                                    data.minHealth = harvestableComponent.minHealth;
                                    data.maxHealth = harvestableComponent.maxHealth;
                                }
                            }

                            data.health = health;
                        }
                    }

                    break;
                }
                else if (i == terrains.Count - 1)
                {
                    BaseUNNetworkData.WaitingForStreamData.Add(this);
                }
            }
        }

        /// <summary>
        /// Send data to server
        /// </summary>
        public virtual void SendToServer()
        {

        }

        /// <summary>
        /// Send data to connection
        /// <param name="connection">the targeted connection</param>
        /// </summary>
        public virtual void SendToConnection(T connection)
        {

        }

        /// <summary>
        /// Send data to clients
        /// </summary>
        public virtual void SendToClients()
        {

        }

        /// <summary>
        /// Send data to other connections
        /// </summary>
        public virtual void SendToOthers()
        {
        }

        /// <summary>
        /// Create equal state which checks whether those 2 instances of NetworkData are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            UNNetworkData<T> instance = obj as UNNetworkData<T>;

            if (obj == null) return false;

            bool result = instance.terrainID == this.terrainID && instance.treeInstanceID == this.treeInstanceID;

            return result;
        }

        /// <summary>
        /// Overrided this method only to get rid of a warning.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>serialized bytes</returns>
        public virtual byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();

                bf.Serialize(ms, this);

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Deserialize the data.
        /// </summary>
        /// <param name="bytes">the data.</param>
        /// <returns>the deserialized object.</returns>
        public static UNNetworkData<T> Deserialize(byte[] bytes)
        {
            using(MemoryStream ms = new MemoryStream(bytes))
            {
                BinaryFormatter bf = new BinaryFormatter();
                return bf.Deserialize(ms) as UNNetworkData<T>;
            }
        }
    }

    public enum PacketType
    {
        HealthUpdate
    }
}
