using UnityEngine;
namespace EmergentBehaviour
{
    public class BoidEntitySpawner : MonoBehaviour 
    {
        public enum GizmoType { Never, SelectedOnly, Always }

            public BoidEntity prefab;
            public float spawnRadius = 10;
            public int spawnCount = 10;
            public Color colour;
            public GizmoType showSpawnRegion;

            private void Awake ()
            {
                for (int i = 0; i < spawnCount; i++) {
                    Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
                    BoidEntity boid = Instantiate(prefab);
                    boid.transform.position = pos;
                    boid.transform.forward = Random.insideUnitSphere;
                    boid.SetMaterialColor(colour);
                }
            }

            private void OnDrawGizmos() 
            {
                if (showSpawnRegion == GizmoType.Always) {
                    DrawGizmos ();
                }
            }

            private void OnDrawGizmosSelected () 
            {
                if (showSpawnRegion == GizmoType.SelectedOnly) 
                {
                    DrawGizmos ();
                }
            }

            private void DrawGizmos () 
            {
                Gizmos.color = new Color (colour.r, colour.g, colour.b, 0.3f);
                Gizmos.DrawSphere (transform.position, spawnRadius);
            }
        }
}