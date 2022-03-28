using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmergentBehaviour
{
    public static class raycast
    {

        private const int k_RAYCAST_VIEW_DIRECTIONS = 300;
        public static readonly Vector3[] directions;

        static raycast () {
            directions = new Vector3[raycast.k_RAYCAST_VIEW_DIRECTIONS];

            float ratio = (1 + Mathf.Sqrt (5)) / 2;
            float angle = Mathf.PI * 2 * ratio;
            for (int i = 0; i < k_RAYCAST_VIEW_DIRECTIONS; i++) 
            {
                float increment = (float) i / k_RAYCAST_VIEW_DIRECTIONS;
                float inclination = Mathf.Acos (1 - 2 * increment);
                float angleIndex = angle * i;
                float x = Mathf.Sin (inclination) * Mathf.Cos (angleIndex);
                float y = Mathf.Sin (inclination) * Mathf.Sin (angleIndex);
                float z = Mathf.Cos (inclination);
                directions[i] = new Vector3 (x, y, z);
            }
        }
    }
}