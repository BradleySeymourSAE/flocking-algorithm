using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EmergentBehaviour
{
    public class Flock : MonoBehaviour 
    { 
        public BoidSettings settings;
        public bool AllowTargetFollowing; 
        private BoidEntity[] boids;
        [SerializeField] private Transform m_TargetObject;
    
        private bool CanFollowTarget => m_TargetObject != null;  

        private void Start () {
            boids = FindObjectsOfType<BoidEntity> ();
            foreach (BoidEntity b in boids) {
                b.Initialize (settings, null);
            }
        }

        private void Update () {
            if (boids != null)
            {
                int length = boids.Length;
                var storedBoidData = new BoidData[length];

                for (int i = 0; i < boids.Length; i++) 
                {
                    boids[i].AverageHeading = storedBoidData[i].FlockHeading;
                    boids[i].CenterOfFlock = storedBoidData[i].CohesionWeight;
                    boids[i].AverageAvoidanceHeading = storedBoidData[i].AvoidanceWeight;
                    boids[i].TotalNeighbouringBoidEntities = storedBoidData[i].NeighbouringBoidEntities;
                    if (CanFollowTarget && AllowTargetFollowing)
                    {
                        boids[i].SetFollowTarget(m_TargetObject);
                    }
                    boids[i].UpdateBoid(settings);
                }
            }
        }
        
        public struct BoidData {
            public Vector3 FlockHeading;
            public Vector3 CohesionWeight;
            public Vector3 AvoidanceWeight;
            public int NeighbouringBoidEntities;
        }
    }
}