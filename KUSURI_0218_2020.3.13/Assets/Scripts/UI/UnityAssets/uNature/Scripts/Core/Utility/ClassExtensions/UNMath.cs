using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace uNature.Core.Math
{
    public static class UNMath
    {
        /// <summary>
        /// Possibly faster distance calculation than using Vector2.Distance
        /// </summary>
        /// <param name="a">point A</param>
        /// <param name="b">point B</param>
        /// <returns>the distance between the 2 points</returns>
        public static float Distance(Vector2 a, Vector2 b)
        {
            return (a - b).sqrMagnitude;
        }

        /// <summary>
        /// Possibly faster distance calculation than using Vector3.Distance
        /// </summary>
        /// <param name="a">point A</param>
        /// <param name="b">point B</param>
        /// <returns>the distance between the 2 points</returns>
        public static float Distance(Vector3 a, Vector3 b)
        {
            return (a - b).sqrMagnitude;
        }
       
        /// <summary>
        /// Check if a certain Vector2 is between bounds.
        /// </summary>
        /// <param name="checkVector"></param>
        /// <param name="boundsA"></param>
        /// <param name="boundsB"></param>
        /// <returns></returns>
        public static bool CheckIfBetween(Vector3 checkVector, int boundsA, int boundsB)
        {
            return checkVector.x > boundsA && checkVector.x < boundsB &&
                checkVector.y > boundsA && checkVector.y < boundsB;
        }
    }
}

