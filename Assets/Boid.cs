using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public Vector3 velocity;
    public Vector3 position;
    public Vector3 force;
    public float maxVelocity = 100.0f;

    private Vector3 m_StartingVelocity; 
    private Vector3 m_StartingPosition;
    private Vector3 m_StartingForce;
    
    /// <summary>
    ///     Whether or not the boid is a leader of other boids or not 
    /// </summary>
    public bool IsAuthoritativeBoid { get; set; }
    
    /// <summary>
    ///     Whether or not the boid is an outcast of other boids or not
    /// </summary>
    public bool IsOutcastBoid { get; set; }

    /// <summary>
    ///     If the boid <see cref="IsAuthoritativeBoid"/> is true, this is whether or not the leader boid has reached
    ///     the target destination or not. 
    /// </summary>
    public bool HasReachedDestination { get; private set; } = false; 

    public Vector3 Destination { get; set; } = Vector3.zero; 
    
    
    public void AddForce(Vector3 Value)
    {
        force += Value; 
    }
    
    private void Start()
    {
        position = transform.position;
    }

    private void Update()
    {
        List<Boid> boidsWithinRange = BoidManager.instance.FindBoidsInRange(this, position, BoidManager.instance.boidSightRange);
        Vector3 average = Vector3.zero;
        int found = 0;

        // If there are nearby Boids
        if (boidsWithinRange != null && boidsWithinRange.Count > 0)
        {
            // If cohesion is enabled
            if (BoidManager.instance.EnableCohesion)
            {
                // Find the average position of nearby Boids
                foreach (Boid boid in boidsWithinRange)
                {
                    if (BoidManager.instance.EnableAuthoritativeBoids && boid.IsAuthoritativeBoid)
                    {
                        average += boid.position * BoidManager.instance.BoidAuthoritativeBoidsStrength;
                    }
                    average += boid.position;
                    found++;
                }
                average /= found;
                // Calculate the direction to the average position
                Vector3 direction = average - position;

                // if the boid is a leader, add BoidAuthoritativeBoidStrength force to the boid
                if (IsAuthoritativeBoid)
                {
                    AddForce(direction.normalized * BoidManager.instance.BoidAuthoritativeBoidsStrength);
                }
                // if the boid manager instance EnableSmoothCohesionFallout is true, 
                float strength = BoidManager.instance.EnableSmoothCohesionFalloff
                    ? BoidManager.instance.BoidCohesionFalloffStrength * (1 - (found / boidsWithinRange.Count))
                    : BoidManager.instance.BoidCohesionStrength; 
                
                AddForce(direction * strength);
            }
            // If alignment is enabled
            if (BoidManager.instance.EnableAlignment)
            {
                // Find the average velocity of nearby Boids
                foreach (Boid boid in boidsWithinRange)
                {
                    // if enable authoritative boids is true, and the boid is a leader, add BoidAuthoritativeBoidStrength force to the velocity of the boid  
                    if (BoidManager.instance.EnableAuthoritativeBoids && boid.IsAuthoritativeBoid)
                    {
                        average += boid.velocity * BoidManager.instance.BoidAuthoritativeBoidsStrength;
                    }
                    average += boid.velocity;
                    found++;
                }
                average /= found;
                // Calculate the direction to the average velocity
                Vector3 direction = average - velocity;
                // Add the force to the Boid
                AddForce(direction * BoidManager.instance.BoidAlignmentStrength);
            }
            // If separation is enabled
            if (BoidManager.instance.EnableSeperation)
            {
                // Find the average position of nearby Boids
                foreach (Boid boid in boidsWithinRange)
                {
                    // Calculate the distance between the Boid and the other Boid
                    float distance = Vector3.Distance(position, boid.position);

                    // If the distance is less than the Boid's sight range
                    if (distance < BoidManager.instance.boidSightRange)
                    {
                        // Calculate the direction to the other Boid
                        Vector3 direction = position - boid.position;
                        // If the boid is an outcast 
                        if (boid.IsOutcastBoid)
                        {
                            AddForce(direction.normalized * BoidManager.instance.BoidOutcastBoidsStrength);
                        }
                        // Add the force to the Boid
                        AddForce(direction * BoidManager.instance.BoidSeperationStrength);
                    }
                }
            }
        }
    }
}