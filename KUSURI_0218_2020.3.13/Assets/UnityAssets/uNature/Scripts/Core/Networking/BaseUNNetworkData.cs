using UnityEngine;

using uNature.Core.ClassExtensions;
using uNature.Core.Collections;
using uNature.Core.Terrains;

namespace uNature.Core.Networking
{
    [System.Serializable]
    public class BaseUNNetworkData
    #if UN_UNet
        : UnityEngine.Networking.MessageBase
    #endif
    {
        /// <summary>
        /// static instance of the buffered data list.
        /// </summary>
        internal static readonly UNList<BaseUNNetworkData> bufferedData = new UNList<BaseUNNetworkData>();
        /// <summary>
        /// Data which isn't yet streamed (the terrain isnt loaded yet, waiting for the terrain to be loaded first)
        /// </summary>
        internal static readonly UNList<BaseUNNetworkData> WaitingForStreamData = new UNList<BaseUNNetworkData>();

        #region Serialize-Variables
        public int treeInstanceID;
        public string terrainID;

        private int _minHealth = -1;
        private int _maxHealth = -1;

        public int minHealth
        {
            get
            {
                return _minHealth;
            }
            set
            {
                _minHealth = value;
            }
        }
        public int maxHealth
        {
            get
            {
                return _maxHealth;
            }
            set
            {
                _maxHealth = value;
            }
        }

        [SerializeField]
        protected int _health;
        public int health
        {
            get { return _health; }
            set
            {
                value = Mathf.Clamp(value, minHealth, maxHealth);

                _health = value;

                if (value == minHealth)
                {
                    Terrain[] terrains = GameObject.FindObjectsOfType<Terrain>();
                    Terrain terrain;

                    for (int i = 0; i < terrains.Length; i++)
                    {
                        terrain = terrains[i];

                        if (terrain.name == terrainID)
                        {
                            terrain.ConvertTreeInstance(treeInstanceID, terrain.GetComponent<Terrains.UNTerrain>());
                        }
                    }
                }
            }
        }

        public PacketType eventType = PacketType.HealthUpdate;
        #endregion

        /// <summary>
        /// Unpack the data
        /// </summary>
        public virtual void UnPack()
        {
        }

        internal static void CheckForStreamedData()
        {
            Terrain terrain;
            BaseUNNetworkData data;

            for (int i = 0; i < UNTerrain.terrains.Count; i++)
            {
                terrain = UNTerrain.terrains[i].terrain;

                for (int b = 0; b < WaitingForStreamData.Count; b++)
                {
                    data = WaitingForStreamData[b];

                    if (terrain.name == data.terrainID)
                    {
                        data.UnPack();
                        WaitingForStreamData.Remove(data);
                    }
                }
            }
        }
    }
}