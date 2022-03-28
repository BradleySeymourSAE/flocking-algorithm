using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Random = UnityEngine.Random;

public class BoidManager : MonoBehaviour
{
    public enum EdgeBehavior
    {
        Wrap,
        Collide
    }

    [Header("Manager")]
    public static BoidManager instance;

    [Header("World")]
    public Vector2      worldSize = new Vector2(100, 100);
    public EdgeBehavior edge = EdgeBehavior.Wrap;
    public int         boidSpawnAmount = 10;

    [Header("Boids")]
    public GameObject boidPrefab;
    public List<Boid> boids;
    public float      boidSpeed = 20.0f;
    public float      boidSightRange = 10.0f;
    public int maximumAuthoritativeBoids = 15;
    public int maximumOutcastBoids = 15; 
    

    public bool  EnableCohesion = false;
    public bool EnableSmoothCohesionFalloff = false;
    
    public bool  EnableSeperation = false;
    public bool  EnableAlignment = false;

    public bool EnableAuthoritativeBoids = false; 
    public bool EnableOutcastBoids = false;
    // Smooth falloff of the boid's influence
    
    public float BoidCohesionStrength = 1.0f;
    public float BoidCohesionFalloffStrength = 0.5f; 
    public float BoidSeperationStrength = 1.0f;
    public float BoidAlignmentStrength = 1.0f;
    
    public float BoidAuthoritativeBoidsStrength = 1.0f;
    public float BoidOutcastBoidsStrength = 1.0f;

    [Header("Debug")]
    public bool debugRanges = false;
    public bool debugNearby = false;
    public bool debugSelected = false;

    public int TotalAuthoritativeBoids 
    {
        get
        {
            if (boids == null || boids.Count == 0) return-1;
            return boids.Count(boid => boid.IsAuthoritativeBoid); 
        }
    }

    public int TotalOutcastBoids 
    {
        get
        {
            if (boids == null || boids.Count == 0) return-1;
            return boids.Count(boid => boid.IsOutcastBoid);
        }
    }
    
    private void Awake()
    {
        // Store the singleton reference
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Draw the world boundaries
        Debug.DrawLine(new Vector3(0, 0, 0), new Vector3(worldSize.x, 0, 0), Color.red);
        Debug.DrawLine(new Vector3(0, worldSize.y, 0), new Vector3(worldSize.x, worldSize.y, 0), Color.red);
        Debug.DrawLine(new Vector3(0, 0, 0), new Vector3(0, worldSize.y, 0), Color.red);
        Debug.DrawLine(new Vector3(worldSize.x, 0, 0), new Vector3(worldSize.x, worldSize.y, 0), Color.red);

        long diff = boidSpawnAmount - boids.Count; // The signed difference between how many boids we want and have.
        
        // Not enough boids, spawn more
        if (diff > 0)
        {
            for (int i = 0; i < diff; ++i)
            {
                Vector2 spawnPosition = new Vector2(Random.Range(1.0f, worldSize.x - 1.0f), Random.Range(1.0f, worldSize.x - 1.0f));
                Boid spawnedBoid = Instantiate(boidPrefab, spawnPosition, Quaternion.identity).GetComponent<Boid>();
                float dirInfluence = Random.Range(0, 2.0f * Mathf.PI);
                if (EnableAuthoritativeBoids)
                {
                    SetAuthoritativeBoid(spawnedBoid);
                }
                else
                {
                    spawnedBoid.IsAuthoritativeBoid = false; 
                }
                if (EnableOutcastBoids)
                {
                    SetOutcastBoid(spawnedBoid);
                }
                else
                {
                    spawnedBoid.IsOutcastBoid = false; 
                }
                spawnedBoid.position = spawnPosition;
                spawnedBoid.velocity.x = Mathf.Cos(dirInfluence) * boidSpeed;
                spawnedBoid.velocity.y = Mathf.Sin(dirInfluence) * boidSpeed;
                boids.Add(spawnedBoid);
            }
        }
        // Too many boids, remove some
        else if(diff < 0)
        {
            for (int i = 0; i < -diff; ++i)
            {
                Destroy(boids[boids.Count - 1].gameObject);
                boids.RemoveAt(boids.Count - 1);
            }
        }
        ProcessBoids(boids);
    }

    private void SetAuthoritativeBoid(Boid boid)
    {
        if (TotalAuthoritativeBoids < maximumAuthoritativeBoids)
        {
            boid.IsAuthoritativeBoid = Random.Range(0.0f, 1.0f) < 0.5f;
            if (boid.IsAuthoritativeBoid)
            {
                boid.IsOutcastBoid = false;
            }
        }
    }

    private void SetOutcastBoid(Boid boid)
    {
        if (TotalOutcastBoids < maximumOutcastBoids)
        {
            boid.IsOutcastBoid = Random.Range(0.0f, 1.0f) < 0.5f;
            if (boid.IsOutcastBoid)
            {
                boid.IsAuthoritativeBoid = false;
            }
        }
    }

    private void ProcessBoids(List<Boid> boids)
    {
        // Process each boid
        foreach (var b in boids)
        {
            // EUler integration
            b.velocity += b.force * Time.deltaTime;
            b.position += b.velocity * Time.deltaTime;
            b.force = Vector2.zero;
            
            // clamp the velocity to the maximumSpeed 
            if (b.velocity.magnitude > b.maxVelocity)
            {
                b.velocity = b.velocity.normalized * b.maxVelocity;
            }

            switch(edge)
            {
                // Apply edge wrapping
                case EdgeBehavior.Wrap:
                {
                    if (b.position.x < 0)
                        b.position.x += worldSize.x;
                    if (b.position.y < 0)
                        b.position.y += worldSize.y;
                    if (b.position.x >= worldSize.x)
                        b.position.x -= worldSize.x;
                    if (b.position.y >= worldSize.y)
                        b.position.y -= worldSize.y;
                    break;
                }

                // Apply edge collision
                case EdgeBehavior.Collide:
                {
                    if (b.position.x < 0)
                        b.position.x = 0;
                    if (b.position.y < 0)
                        b.position.y = 0;
                    if (b.position.x >= worldSize.x)
                        b.position.x = worldSize.x;
                    if (b.position.y >= worldSize.y)
                        b.position.y = worldSize.y;
                    break;
                }
            }
            // Apply the Boid's pos to the transform
            b.transform.position = b.position;

            // Rotate the boid's sprite to face its velocity
            if (!b.velocity.Equals(Vector2.zero))
            {
                var angle = Mathf.Atan2(-b.velocity.x, b.velocity.y) * Mathf.Rad2Deg;
                b.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    // Draw a debug circle
    public static void DrawCircle(Vector2 Point, float Radius, Color color)
    {
        Vector3 p3 = new Vector3(Point.x, Point.y, 0);
        Vector3 old = new Vector3(Radius, 0, 0) + p3;
        for (int i = 0; i < 32; ++i)
        {
            float a = (i / 31.0f) * 2.0f * Mathf.PI;
            Vector3 current = new Vector3(Mathf.Cos(a) * Radius, Mathf.Sin(a) * Radius, 0) + p3;
            Debug.DrawLine(old, current, color);
            old = current;
        }
    }

    public static void DrawLine(Vector2 p1, Vector2 p2, Color c)
    {
        Debug.DrawLine(new Vector3(p1.x, p1.y, 0), new Vector3(p2.x, p2.y, 0), c);
    }

    public static void DrawArrow(Vector2 p1, Vector2 p2, Color c, float headSize = 0.3f)
    {
        Vector3 a = new Vector3(p1.x, p1.y, 0);
        Vector3 b = new Vector3(p2.x, p2.y, 0);
        Vector3 dir = (b - a).normalized;
        Vector3 perp = new Vector3(-dir.y, dir.x, 0);
        Debug.DrawLine(a, b, c);
        Debug.DrawLine(b, b - dir * headSize + perp * headSize, c);
        Debug.DrawLine(b, b - dir * headSize - perp * headSize, c);
    }

    // Find all boids in range, skipping the self boid
    public List<Boid> FindBoidsInRange(Boid self, Vector2 p, float range)
    {
        if(debugRanges)
            DrawCircle(p, range, Color.white);
        List<Boid> found = new List<Boid>();
        foreach(var b in boids)
        {
            if(b!=self && Vector2.Distance(p,b.position)<=range)
            {
                found.Add(b);
                if(debugNearby)
                    DrawArrow(p, b.position, Color.green);
            }
        }
        return found;
    }

    protected void OnDestroy()
    {
        boids.Clear();
    }
    
    #region Debug

    protected void OnDrawGizmos()
    {
        if (boids == null || boids.Count == 0) return; 
        if (EnableAuthoritativeBoids)
        {
            
            var authoritativeBoids = from boid in boids where boid.IsAuthoritativeBoid select boid;
            foreach (var authoritativeBoid in authoritativeBoids)
            {
                DrawCircle(authoritativeBoid.position, 5.0f, Color.red);
            }
        }
        if (EnableOutcastBoids)
        {
            var outcastBoids = from boid in boids where boid.IsOutcastBoid select boid;
            foreach (var outcastBoid in outcastBoids)
            {
                DrawCircle(outcastBoid.position, 5.0f, Color.magenta);
            }
        }
    }

    #endregion
}
