using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EmergentBehaviour
{
    
    public class BoidEntity : MonoBehaviour
    {
        public BoidSettings settings;

            // State
        [HideInInspector]
        public Vector3 Position;
        [HideInInspector]
        public Vector3 ForwardDirection;

        private Vector3 m_CurrentVelocity;

        // To update:
        private Vector3 Acceleration;
        [HideInInspector] public Vector3 AverageHeading;
        [HideInInspector] public Vector3 AverageAvoidanceHeading;
        [HideInInspector] public Vector3 CenterOfFlock;
        [HideInInspector] public int TotalNeighbouringBoidEntities;

        // Cached
        private Material m_CachedMaterial;
        private Transform m_CachedTransform;
        private Transform m_CachedTarget;

        private void Awake () 
        {
            m_CachedMaterial = transform.GetComponent<MeshRenderer>().material;
            m_CachedTransform = transform;
        }

        public void SetFollowTarget(Transform target)
        {
            this.m_CachedTarget = target; 
        }

        public void Initialize (BoidSettings settings, Transform target) 
        {
            this.m_CachedTarget = target;
            this.settings = settings;
            float initialVelocity = (settings.MinimumSpeed + settings.MaximumSpeed) / 2;
            
            Position = m_CachedTransform.position;
            ForwardDirection = m_CachedTransform.forward;
            m_CurrentVelocity = transform.forward * initialVelocity;
        }

        public void SetMaterialColor (Color col) 
        {
            if (m_CachedMaterial != null)
            {
                m_CachedMaterial.color = col;
            }
        }

        public void UpdateBoid (BoidSettings UpdatedBoidConfig)
        {
            this.settings = UpdatedBoidConfig; 
            Vector3 acceleration = Vector3.zero;
            if (m_CachedTarget != null) 
            {
                Vector3 offsetToTarget = (m_CachedTarget.position - Position);
                acceleration = SteerTowards (offsetToTarget) * settings.AuthoritativeWeight;
            }
            if (TotalNeighbouringBoidEntities != 0) 
            {
                CenterOfFlock /= TotalNeighbouringBoidEntities;
                Vector3 centerOffset = (CenterOfFlock - Position);
                Vector3 alignmentForce = SteerTowards(AverageHeading) * settings.AlignmentWeight;
                Vector3 cohesionForce = SteerTowards(centerOffset) * settings.CohesionWeight;
                Vector3 seperationForce = SteerTowards (AverageAvoidanceHeading) * settings.SeperationWeight;
                acceleration += alignmentForce;
                acceleration += cohesionForce;
                acceleration += seperationForce;
            }

            if (IsHeadingForCollision()) 
            {
                Vector3 collisionAvoidDir = ObstacleRays ();
                Vector3 collisionAvoidForce = SteerTowards (collisionAvoidDir) * settings.CollisionAvoidanceWeight;
                acceleration += collisionAvoidForce;
            }
            m_CurrentVelocity += acceleration * Time.deltaTime;
            float magnitude = m_CurrentVelocity.magnitude;
           
            Vector3 direction = m_CurrentVelocity / magnitude;
            magnitude = Mathf.Clamp (magnitude, settings.MinimumSpeed, settings.MaximumSpeed);
            m_CurrentVelocity = direction * magnitude;
            m_CachedTransform.position += m_CurrentVelocity * Time.deltaTime;
            m_CachedTransform.forward = direction;
            Position = m_CachedTransform.position;
            ForwardDirection = direction;
        }

        private bool IsHeadingForCollision() 
        {
            RaycastHit hit;
            if (Physics.SphereCast (Position, settings.BoundsRadius, ForwardDirection, out hit, settings.CollisionAvoidanceDistance, settings.CollisionAvoidanceLayerMask)) 
            {
                return true;
            }
            else
            {
                
            }
            return false;
        }

        private Vector3 ObstacleRays()
        {
            Vector3[] rayDirections = raycast.directions;

            for (int i = 0; i < rayDirections.Length; i++) 
            {
                Vector3 dir = m_CachedTransform.TransformDirection(rayDirections[i]);
                Ray ray = new Ray (Position, dir);
                if (!Physics.SphereCast (ray, settings.BoundsRadius, settings.CollisionAvoidanceDistance, settings.CollisionAvoidanceLayerMask)) 
                {
                    // Debug.DrawRay(Position, dir * settings.CollisionAvoidanceDistance, Color.green);
                    return dir;
                }
                // else
                // {
                //     Debug.DrawRay(Position, dir * settings.CollisionAvoidanceDistance, Color.red);
                // }
            }
            // Debug.DrawRay(Position, ForwardDirection * 1.0f, Color.blue);
            return ForwardDirection;
        }

        private Vector3 SteerTowards (Vector3 vector) 
        {
            Vector3 v = vector.normalized * settings.MaximumSpeed - m_CurrentVelocity;
            return Vector3.ClampMagnitude (v, settings.MaximumAllowedSteeringForce);
        }
    }
}