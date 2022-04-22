using UnityEngine;
using System.Collections;

namespace uNature.Core
{
    /// <summary>
    /// An custom math class.
    /// </summary>
    public static class UNMath
    {
        public static System.Random RANDOM = new System.Random();

        /// <summary>
        /// Get an random point between two values.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float GetRndRange(this System.Random rnd, float min, float max)
        {
            if (rnd == null) rnd = RANDOM;

            return (float)(min + rnd.NextDouble() * (max - min)) ;
        }

        /// <summary>
        /// Get the terrain height at a point (thread-safe)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="heights"></param>
        /// <returns></returns>
        public static float GetHeightAtWorldPoint(float x, float z, Vector3 terrainSize, float[,] heights)
        {
            x = Mathf.Clamp(x, 0, terrainSize.x);
            z = Mathf.Clamp(z, 0, terrainSize.z);

            return heights[(int)x, (int)z];

            //return heights[Mathf.CeilToInt((z / terrainSize.z) * mapHeight), Mathf.CeilToInt((x / terrainSize.x) * mapWidth)] * (terrainSize.y);
        }

        /// <summary>
        /// Get the terrain height at a point (thread-safe)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="heights"></param>
        /// <returns></returns>
        public static Vector3 GetNormalAtWorldPoint(float x, float z, Vector3 terrainSize, Vector3[,] normals)
        {
            x = Mathf.Clamp(x, 0, terrainSize.x);
            z = Mathf.Clamp(z, 0, terrainSize.z);

            return normals[(int)x, (int)z];
        }

        /// <summary>
        /// Check if a bit is contained in a bit mask
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static bool isBitMasked(this int mask, int bit)
        {
            return (mask & (1 << bit)) > 0;
        }

        /// <summary>
        /// Check if a position is inside an offseted bounds.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="offset"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static bool CheckMinMaxBounds(Vector2 min, Vector2 max, float offset, Vector2 position)
        {
            Vector2 offset2D = new Vector2(offset, offset);

            // adjust the min&max points to have an offset.
            min -= offset2D;
            max += offset2D;

            return position.x > min.x && position.y > min.y // check pos is bigger than minimum value
                && position.x < max.x && position.y < max.y; // check pos is tinier than max value.
        }

        public static int iClamp(int x, int from, int to)
        {
            if (x < from)
                return from;
            if (x > to)
                return to;

            return x;
        }

        public static int iClampMin(int x, int from)
        {
            if (x < from)
                return from;

            return x;
        }

        public static int iClampMax(int x, int to)
        {
            if (x > to)
                return to;

            return x;
        }
    }

    public struct Vector2i
    {
        public int x;
        public int y;

        public Vector2i(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2i operator +(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.x + b.x, a.y + b.y);
        }

        public static Vector2i operator -(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.x - b.x, a.y - b.y);
        }

        public static Vector2i operator *(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.x * b.x, a.y * b.y);
        }

        public static Vector2i operator /(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.x / b.x, a.y / b.y);
        }

        public static bool operator ==(Vector2i a, Vector2i b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(Vector2i a, Vector2i b)
        {
            return a.x != b.x || a.y != b.y;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Vector2i)) return false;

            Vector2i instance = (Vector2i)obj;

            return instance == this;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("uNature Vector2i : \nx: {0} \ny : {1}", x, y);
        }
    }

    public struct Vector2b
    {
        public byte x;
        public byte y;

        public Vector2b(byte x, byte y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
