using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using uNature.Core.Pooling;
using uNature.Core.Collections;

namespace uNature.Core.Networking
{

    /// <summary>
    /// A template for networking, which can be used by networking extensions to easily get the networking actions done.
    /// <typeparam name="T1">the targeted networking connection</typeparam>
    /// <typeparam name="T2">the type of the data</typeparam>
    /// </summary>
    public class UNNetworkManager<T1, T2> where T2 : UNNetworkData<T1>
    {
        public const float STREAM_UPDATE_CHECK_INTERVAL_SECONDS = 2;

        /// <summary>
        /// a static instance of this object.
        /// </summary>
        public static UNNetworkManager<T1, T2> manager;

        /// <summary>
        /// The buffered data which will be sent to all of the connecting connections.
        /// </summary>
        public static UNList<BaseUNNetworkData> bufferedData
        {
            get { return BaseUNNetworkData.bufferedData; }
        }
             
        /// <summary>
        /// Are we the server?
        /// </summary>
        public virtual bool isServer
        {
            get { return true; }
        }

        /// <summary>
        /// is the server architecture is authoritative?
        /// </summary>
        public virtual bool isAuth
        {
            get { return uNature.Core.Settings.UNSettings.instance.UN_Networking_Auth; }
        }

        /// <summary>
        /// The constructor of this class, initiate basic events.
        /// </summary>
        public UNNetworkManager(MonoBehaviour managerInstance)
        {
            manager = this;

            TerrainPoolItem.OnTreeInstanceRestored += (Terrain terrain, int id) =>
                {
                    UNNetworkData<T1> instance;
                    instance = UNNetworkData<T1>.Pack<T1, T2>(terrain, id, 0, PacketType.HealthUpdate);

                    var similarItem = bufferedData.TryGet(instance as T2);
                    bufferedData.Remove(similarItem);

                    //SendEvent(instance);
                };

            HarvestableTIPoolItem.OnItemDamagedEvent += OnItemDamaged;
            HarvestableTIPoolItem.OnItemPooledEvent += OnHarvestableTreeInstancePooled;

            managerInstance.StartCoroutine(CheckForStreamingBufferedUpdates());

            Awake();
        }

        /// <summary>
        /// This method is checking every certain amount of seconds for new loaded streamed areas to update data that is waiting for a streamed terrain to be loaded.
        /// </summary>
        /// <returns></returns>
        protected IEnumerator CheckForStreamingBufferedUpdates()
        {
            while(true)
            {
                BaseUNNetworkData.CheckForStreamedData();

                yield return new WaitForSeconds(STREAM_UPDATE_CHECK_INTERVAL_SECONDS);
            }
        }

        /// <summary>
        /// Called when an harvestable item instance has been created
        /// </summary>
        /// <param name="instance">the created instance</param>
        protected void OnHarvestableTreeInstancePooled(HarvestableTIPoolItem instance)
        {
            BaseUNNetworkData data;

            for (int i = 0; i < bufferedData.Count; i++)
            {
                data = bufferedData[i];

                if (instance.uid == data.treeInstanceID && instance.terrain.name == data.terrainID && instance.isCollider)
                {
                    instance.health = data.health;
                }
            }
        }

        /// <summary>
        /// Called when the network manager is initialized.
        /// </summary>
        public virtual void Awake()
        {
            UpdatePermissions();
        }

        /// <summary>
        /// Reupdate the permissions for the Pool items. (needs to be called when ever there's a networking change for the owner/controller)
        /// </summary>
        public virtual void UpdatePermissions()
        {
            TerrainPoolItem.canModify = isServer || !isAuth;
            //TerrainPoolItem.canRestore = isServer;

            HarvestableTIPoolItem.canHarvestCollider = false;
        }

        /// <summary>
        /// Update item damage and handle synchorization.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="health"></param>
        protected void OnItemDamaged(HarvestableTIPoolItem item, int damage)
        {
            if (!item.isCollider) return;

            BaseUNNetworkData data = null;
            BaseUNNetworkData current = null;

            for(int i = 0; i < bufferedData.Count; i++)
            {
                current = bufferedData[i];

                if (current.treeInstanceID == item.uid && current.terrainID == item.terrain.name)
                {
                    current.health -= damage;
                    data = current;

                    break;
                }
            }

            if(data == null)
            {
                data = UNNetworkData<T1>.Pack<T1, T2>(item.terrain, item.uid, item.health, PacketType.HealthUpdate);
                bufferedData.Add(data as T2);
            }

            SendEvent(data as T2);
        }

        /// <summary>
        /// Send event to the correct location
        /// </summary>
        /// <param name="instance">the data instance</param>
        public virtual void SendEvent(UNNetworkData<T1> instance)
        {
            if (isServer || !isAuth)
            {
                SendToOthers(instance);
            }
        }

        /// <summary>
        /// Send to certain connection the data
        /// </summary>
        /// <param name="connection">the connection</param>
        /// <param name="terrainName">the terrain name (terrain.name)</param>
        /// <param name="instanceID">the tree instance</param>
        /// <param name="destroy">you want to destroy/ restore the tree?</param>
        public virtual void SendToConnection(T1 connection, UNNetworkData<T1> instance)
        {
            instance.SendToConnection(connection);
        }

        /// <summary>
        /// Send to all of the clients.
        /// </summary>
        /// <param name="terrainName">the terrain name (terrain.name)</param>
        /// <param name="instanceID">the tree instance</param>
        /// <param name="destroy">you want to destroy/ restore the tree?</param>
        public virtual void SendToClients(UNNetworkData<T1> instance)
        {
            instance.SendToClients();
        }

        /// <summary>
        /// Send to all other connections.
        /// </summary>
        /// <param name="terrainName">the terrain name (terrain.name)</param>
        /// <param name="instanceID">the tree instance</param>
        /// <param name="destroy">you want to destroy/ restore the tree?</param>
        public virtual void SendToOthers(UNNetworkData<T1> instance)
        {
            instance.SendToOthers();
        }

        /// <summary>
        /// Send to the server the data.
        /// </summary>
        /// <param name="terrainName">the terrain name (terrain.name)</param>
        /// <param name="instanceID">the tree instance</param>
        /// <param name="destroy">you want to destroy/ restore the tree?</param>
        public virtual void SendToServer(UNNetworkData<T1> instance)
        {
            instance.SendToServer();
        }

        /// <summary>
        /// Called when the client connects, send all data.
        /// </summary>
        /// <param name="conn">the connection</param>
        public void OnClientConnected(T1 conn)
        {
            uNature.Core.Threading.UNThreadManager.instance.DelayActionSeconds(new Threading.ThreadTask<T1>((T1 connection) =>
                {
                    UNNetworkData<T1> current;

                    for (int i = 0; i < bufferedData.Count; i++)
                    {
                        current = bufferedData[i] as T2;

                        SendToConnection(connection, current);
                    }
                }, conn), 2);
        }
    }
}
