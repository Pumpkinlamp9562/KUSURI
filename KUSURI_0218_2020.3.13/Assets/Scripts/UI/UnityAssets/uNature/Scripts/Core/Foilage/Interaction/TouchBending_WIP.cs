using UnityEngine;
using System.Collections;

namespace uNature.Core.FoliageClasses.Interactions
{
    public class TouchBending_WIP : BaseInteraction
    {
        #region Variables
        [SerializeField]
        private float _radius = 1f;
        public float radius
        {
            get
            {
                return _radius;
            }
            set
            {
                if (_radius != value)
                {
                    _radius = value;
                }
            }
        }
        #endregion

        #region Methods
        protected override Color32[] CalculateInteraction(Vector3 normalizedCoords, RenderingQueue_InteractionReceiver receiver, Color32[] mapPixels)
        {
            int mapResolution = (int)receiver.interactionMapResolution;

            #region Local Variables
            float transformedRadius = TransformCoord(radius, receiver);
            float transformedX = TransformCoord(normalizedCoords.x, receiver);
            float transformedZ = TransformCoord(normalizedCoords.z, receiver);

            float startX = transformedX - transformedRadius;
            float startZ = transformedZ - transformedRadius;

            float endX = transformedX + transformedRadius;
            float endZ = transformedZ + transformedRadius;

            Vector3 center = new Vector3(transformedX, 0, transformedZ);
            #endregion

            #region Per Coord variables
            float normalizedDistance;

            Vector3 coordVector = Vector3.zero;
            Vector3 normalizedBladeCoord;

            Color currentColor;
            int index;
            #endregion

            for (float x = startX; x < endX; x++)
            {
                for (float z = startZ; z < endZ; z++)
                {
                    if (x < 0 || z < 0 || x > mapResolution || z > mapResolution) continue; // if out of bounds continue to next coordinate

                    coordVector.x = x;
                    coordVector.z = z;

                    normalizedDistance = Vector3.Distance(center, coordVector) / radius;
                    normalizedDistance = Mathf.Clamp(1 - normalizedDistance, 0, 1);

                    normalizedBladeCoord = (coordVector - center).normalized;

                    normalizedBladeCoord *= normalizedDistance;

                    index = (int)x + (int)z * mapResolution;
                    currentColor = mapPixels[index];

                    currentColor.r = 1;
                    currentColor.g = 1;

                    mapPixels[index] = currentColor;
                }
            }

            return mapPixels;
        }
        #endregion
    }
}