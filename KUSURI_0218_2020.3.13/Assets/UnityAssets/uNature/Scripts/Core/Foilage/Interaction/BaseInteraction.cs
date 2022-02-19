using UnityEngine;
using System.Collections.Generic;

using uNature.Core.Threading;
using uNature.Core.Utility;
using uNature.Wrappers.Linq;

namespace uNature.Core.FoliageClasses.Interactions
{
    public abstract class BaseInteraction : ThreadItem
    {
        private static List<BaseInteraction> _interactions;
        public static List<BaseInteraction> interactions
        {
            get
            {
                if(_interactions == null)
                {
                    _interactions = FindObjectsOfType<BaseInteraction>().ToList();
                }

                return _interactions;
            }
        }

        #region Constructors
        protected override void OnEnable()
        {
            base.OnEnable();

            interactions.Add(this);
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            interactions.Remove(this);
        }
        #endregion

        #region Interaction_Calculation
        /// <summary>
        /// Recalculat the map.
        /// </summary>
        /// <param name="receiver"></param>
        public static void RecalculateMap(RenderingQueue_InteractionReceiver receiver)
        {
            var interactionMap = receiver.interactionMap;
            Color32[] currentMapPixels = UNMapGenerators.PoolColors((int)receiver.interactionMapResolution);

            CallMapsUpdate(receiver, currentMapPixels);

            interactionMap.SetPixels32(currentMapPixels);
        }

        /// <summary>
        /// Private method for calling the maps update.
        /// </summary>
        /// <param name="receiver"></param>
        private static void CallMapsUpdate(RenderingQueue_InteractionReceiver receiver, Color32[] mapColors)
        {
            BaseInteraction interaction;

            Vector3 interaction_normalizedPosition;

            for (int i = 0; i < interactions.Count; i++)
            {
                interaction = interactions[i];

                if (interaction == null) continue;

                interaction_normalizedPosition = NormalizePosition(interaction, receiver);

                if (IsInMapBounds(interaction_normalizedPosition, receiver))
                {
                    mapColors = interaction.CalculateInteraction(interaction_normalizedPosition, receiver, mapColors);
                }
            }
        }

        /// <summary>
        /// Check if in bounds.
        /// </summary>
        /// <param name="normalizedPosition"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        protected static bool IsInMapBounds(Vector3 normalizedPosition, RenderingQueue_InteractionReceiver receiver)
        {
            int mapSize = (int)receiver.interactionMapSize;

            bool isXValid = normalizedPosition.x >= 0 && normalizedPosition.x <= mapSize;
            bool isZValid = normalizedPosition.z >= 0 && normalizedPosition.z <= mapSize;

            return isXValid && isZValid;
        }

        /// <summary>
        /// Normalize the position to the map position.
        /// </summary>
        /// <param name="interaction"></param>
        public static Vector3 NormalizePosition(BaseInteraction interaction, RenderingQueue_InteractionReceiver receiver)
        {
            Vector3 normalizedPosition = interaction.threadPosition;
            Vector3 receiverCenter = receiver.interactionCenter;

            normalizedPosition.x = Mathf.Abs(normalizedPosition.x - receiverCenter.x);
            normalizedPosition.x = Mathf.Abs(normalizedPosition.z - receiverCenter.z);

            return normalizedPosition;
        }

        /// <summary>
        /// Transform coords.
        /// [From normalized to transformed]
        /// </summary>
        /// <param name="normalizedPositionX"></param>
        /// <returns></returns>
        public static float TransformCoord(float normalizedPositionX, RenderingQueue_InteractionReceiver receiver)
        {
            return normalizedPositionX / receiver.interactionMapMultiplier;
        }

        /// <summary>
        /// Invert coords.
        /// [From transformed to normalized]
        /// </summary>
        /// <param name="normalizedPositionX"></param>
        /// <returns></returns>
        public static float InvertCoord(float normalizedPositionX, RenderingQueue_InteractionReceiver receiver)
        {
            return normalizedPositionX * receiver.interactionMapMultiplier;
        }

        /// <summary>
        /// Calculate interactions and insert it into the map pixels.
        /// </summary>
        /// <param name="_mapPixels"></param>
        /// <returns>is successful?</returns>
        protected virtual Color32[] CalculateInteraction(Vector3 normalizedCoords, RenderingQueue_InteractionReceiver receiver, Color32[] mapPixels)
        {
            return mapPixels;
        }
        #endregion
    }

    /// <summary>
    /// An interaction map for interactions (Includes data for the interactions)
    /// </summary>
    public sealed class InteractionMap : UNMap
    {
        private InteractionMap(Texture2D tex) : base(tex, tex.GetPixels32(), null)
        {

        }

        /// <summary>
        /// Create a map.
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns></returns>
        internal static InteractionMap CreateMap(RenderingQueue_InteractionReceiver receiver)
        {
            int size = (int)receiver.interactionMapResolution;

            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true);

            tex.SetPixels32(UNMapGenerators.PoolColors(size));

            return new InteractionMap(tex);
        }
    }
}
