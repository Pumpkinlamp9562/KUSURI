using UnityEngine;
using System.Collections;
using System.Linq;

using uNature.Core.FoliageClasses;

namespace uNature.Core
{
    /// <summary>
    /// This is a base class for a UCPhysicsObject.
    /// Every class that inherites this class will be counted in the physics system.
    /// </summary>
    public struct UNPhysicsObject
    {
        static UNPhysicsHit_Grass hit = new UNPhysicsHit_Grass();

        [SerializeField]
        private bool _enabled;
        public bool enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if(_enabled != value)
                {
                    _enabled = value;
                }
            }
        }

        private Matrix4x4 transform;

        [SerializeField]
        private Bounds Bounds;

        /// <summary>
        /// Create Physics Object.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="spread"></param>
        /// <param name="densityIndex"></param>
        /// <param name="prototype"></param>
        internal UNPhysicsObject(UNPhysicsTemplate template, FoliageMeshInstance meshInstance)
        {
            Bounds = new Bounds();
            _enabled = true;
            transform = Matrix4x4.identity;

            CalculateTransform(0, 0, 0, template, meshInstance);
        }

        /// <summary>
        /// Destroy Physics Object.
        /// </summary>
        internal void Destroy()
        {
            System.GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Calculate the transform matrix.
        /// </summary>
        internal void CalculateTransform(float x, float y, float z, UNPhysicsTemplate template, FoliageMeshInstance meshInstance)
        {
            Vector3 position;

            // position
            position.x = template.position.x + (template.spread.x * template.prototype.spread) + x + meshInstance.position.x;
            position.y = template.position.y + y;
            position.z = template.position.z + (template.spread.y * template.prototype.spread) + z + meshInstance.position.z;

            // rotation
            Quaternion randomRotation = Quaternion.Euler(0, template.spread.x * 360, 0);

            // scale
            float widthSizeDifference = template.prototype.maximumWidth - template.prototype.minimumWidth;
            widthSizeDifference *= template.spread.x;

            float widthSize = template.prototype.minimumWidth + widthSizeDifference;

            float heightSizeDifference = template.prototype.maximumHeight - template.prototype.minimumHeight;
            widthSizeDifference *= template.spread.x;

            float heightSize = template.prototype.minimumHeight + heightSizeDifference;

            Vector3 width_height_scale = new Vector3(widthSize, heightSize, widthSize);
            Vector3 worldScale = template.prototype.FoliageInstancedMeshData.worldScale;

            worldScale.Scale(width_height_scale);

            transform = Matrix4x4.TRS(position, randomRotation, worldScale);

            UpdateBounds();
        }

        /// <summary>
        /// Update object's bounds
        /// </summary>
        /// <param name="center">The center of the bounds, worldspace</param>
        /// <param name="size">The size of the bounds, worldspace</param>
        void UpdateBounds()
        {
            Vector3 center = Vector3.zero;
            Vector3 size = Vector3.one;

            Vector3 min = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            Vector3 max = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);

            Vector3[] points = new Vector3[8];
            Vector3 extents = size / 2;

            points[0] = transform.MultiplyPoint3x4(new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z));
            points[1] = transform.MultiplyPoint3x4(new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z));
            points[2] = transform.MultiplyPoint3x4(new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z));
            points[3] = transform.MultiplyPoint3x4(new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z));
            points[4] = transform.MultiplyPoint3x4(new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z));
            points[5] = transform.MultiplyPoint3x4(new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z));
            points[6] = transform.MultiplyPoint3x4(new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z));
            points[7] = transform.MultiplyPoint3x4(new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z));

            Vector3 point;
            for(int i = 0; i < points.Length; i++)
            {
                point = points[i];

                if (point.x < min.x) min.x = point.x;
                if (point.x > max.x) max.x = point.x;
                if (point.y < min.y) min.y = point.y;
                if (point.y > max.y) max.y = point.y;
                if (point.z < min.z) min.z = point.z;
                if (point.z > max.z) max.z = point.z;
            }

            Vector3 boundsSize = new Vector3(max.x - min.x, max.y - min.y, max.z - min.z);
            Bounds = new Bounds(new Vector3(min.x + boundsSize.x / 2f, min.y + boundsSize.y / 2f, min.z + boundsSize.z / 2f), new Vector3(max.x - min.x, max.y - min.y, max.z - min.z));
        }

        /// <summary>
        /// Draw gizmos
        /// </summary>
        public void OnDrawGizmos()
        {
            if (!enabled) return;

            #if UNITY_EDITOR
            DrawShape(transform);
            #endif
        }

        /// <summary>
        /// Draw the shape of the bounds
        /// </summary>
        /// <param name="matrix">the matrix of the bounds</param>
        /// <param name="selected">is the shape selected in heirachy</param>
        void DrawShape(Matrix4x4 matrix)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = matrix;

            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        /// <summary>
        /// Raycast the physics object
        /// </summary>
        /// <param name="origin">ray origin</param>
        /// <param name="direction">ray direction</param>
        /// <param name="_hit">hit data</param>
        /// <param name="distance">max distance</param>
        /// <param name="mask">layerMask</param>
        /// <returns>Did we hit something?</returns>
        public bool Raycast(Ray ray, out UNPhysicsHit_Grass _hit, LayerMask mask)
        {
            _hit = hit;

            if (!enabled) return false;

            if (Bounds.IntersectRay(ray, out _hit.distance))
            {
                _hit.point          = ray.GetPoint(_hit.distance);

                return true;
                //return VerifyUnityCollisions(_hit, ray);
            }

            return false;
        }

        /*
        bool VerifyUnityCollisions(UNPhysicsHit_Grass hit, Ray ray)
        {
            Transform obj = hit.transform;
            IUTCPhysicsIgnored ignoreInterface;

            RaycastHit[] hits = UnityEngine.Physics.RaycastAll(ray.origin, (obj.position - ray.origin), hit.distance).OrderBy(x => x.distance).ToArray();
            RaycastHit _hit;

            for (int i = 0; i < hits.Length; i++)
            {
                _hit = hits[i];
                ignoreInterface = _hit.transform.GetComponent<IUTCPhysicsIgnored>();

                if (ignoreInterface != null && ignoreInterface.ignore) continue;

                if (_hit.transform == obj)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
        */
    }

}