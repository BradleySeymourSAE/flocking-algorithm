using UnityEngine;

namespace EmergentBehaviour
{
    [CreateAssetMenu(fileName = "Behaviour", menuName = "EmergentBehaviour/Behaviour", order = 1)]
    public class BoidSettings : ScriptableObject
    {
        public float MinimumSpeed = 5;
        public float MaximumSpeed = 8;
        public float MaximumAllowedSteeringForce = 8;
        
        public float AlignmentWeight = 2;
        public float CohesionWeight = 1;
        public float SeperationWeight = 2.5f;
        public float AuthoritativeWeight = 2.0f;
       
        public LayerMask CollisionAvoidanceLayerMask;
        public float BoundsRadius = 0.27f;
        public float CollisionAvoidanceWeight = 20;
        public float CollisionAvoidanceDistance = 5;
    }
}