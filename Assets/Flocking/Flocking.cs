using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flocking
{
    public class Flocking : MonoBehaviour
    {
        [SerializeField] protected FlockUnit m_FlockUnitPrefab;
        [SerializeField] protected int m_FlockSize;
        [SerializeField] protected Vector3 m_SpawnArea;
        [SerializeField] protected BoxCollider m_SpawnAreaBounds;

        [Range(0, 10), SerializeField] protected float m_MinimumMovementSpeed = 5.0f;
        [Range(0, 10), SerializeField] protected float m_MaximumMovementSpeed = 5.0f;
        [Range(0, 10), SerializeField] protected float m_CohesionFalloff = 5.0f;
        [Range(0, 10), SerializeField] protected float m_SeparationFalloff = 2.0f;
        [Range(0, 10), SerializeField] protected float m_AlignmentFalloff = 2.0f;
        [Range(0, 10), SerializeField] protected float m_ObstacleAvoidanceFalloff = 10.0f;
        [Range(0, 100), SerializeField] protected float m_BoundsFalloff = 20.0f;
        [Range(0, 10), SerializeField] protected float m_CohesionWeight = 7.0f;
        [Range(0, 10), SerializeField] protected float m_AvoidanceWeight = 10.0f;
        [Range(0, 10), SerializeField] protected float m_AlignmentWeight = 0.0f;
        [Range(0, 10), SerializeField] protected float m_BoundsAvoidanceWeight = 10.0f;
        [Range(0, 100), SerializeField] protected float m_ObstacleAvoidanceWeight = 0.0f;
        public float MinimumSpeed => m_MinimumMovementSpeed;
        public float MaximumSpeed => m_MaximumMovementSpeed;
        public float CohesionFalloffRange => m_CohesionFalloff;
        public float AvoidanceFalloffRange => m_SeparationFalloff;
        public float AlignmentFalloffRange => m_AlignmentFalloff;
        public float ObstacleAvoidanceFalloff => m_ObstacleAvoidanceFalloff;
        public float BoundsDistanceThreshold => m_BoundsFalloff;
        public float CohesionWeight => m_CohesionWeight;
        public float AvoidanceWeight => m_AvoidanceWeight;
        public float AlignmentWeight => m_AlignmentWeight;
        public float BoundsAvoidanceWeight => m_BoundsAvoidanceWeight;
        public float ObstacleAvoidanceWeight => m_ObstacleAvoidanceWeight;

        public FlockUnit[] Units { get; protected set; }

        protected void Awake()
        {
            this.m_SpawnAreaBounds = GetComponent<BoxCollider>();
        }

        protected virtual void Start() => Initialize();
        
        protected virtual void Update()
        {
            for (int i = 0; i < Units.Length; i++)
            {
                Units[i].UpdateBehaviour();
            }
        }

        protected virtual void Initialize()
        {
            Units = new FlockUnit[m_FlockSize];
            for (int i = 0; i < m_FlockSize; i++)
            {
                var randomVector = UnityEngine.Random.insideUnitSphere;
                randomVector = new Vector3(randomVector.x * m_SpawnArea.x, randomVector.y * m_SpawnArea.y, randomVector.z * m_SpawnArea.z);
                var spawnPosition = transform.position + randomVector;
                var rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
                Units[i] = Instantiate(m_FlockUnitPrefab, spawnPosition, rotation);
                Units[i].AssignFlock(this);
                Units[i].InitializeSpeed(UnityEngine.Random.Range(MinimumSpeed, MaximumSpeed));
            }
        }


        protected void OnDrawGizmos()
        {
            if (m_SpawnAreaBounds != null && m_SpawnAreaBounds.enabled && !Application.isPlaying)
            {
                Gizmos.color = new Color(0, 1, 0, 0.2f); 
                // draw the collider bounds 
                Gizmos.DrawCube(m_SpawnAreaBounds.bounds.center, m_SpawnAreaBounds.bounds.size);
            }
        }
    }
}