using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Flocking
{
	public class FlockUnit : MonoBehaviour
{
	[SerializeField] protected float m_FieldOfViewAngle;
	[SerializeField] protected float m_SmoothDampening;
	[SerializeField] protected LayerMask m_ObstacleLayerMask;
	[SerializeField] protected Vector3[] m_ObstacleAvoidanceDirections =
	{
		new Vector3(1.0f, 0.0f, 0.0f),
		new Vector3(-1.0f, 0.0f, 0.0f),
		new Vector3(0.0f, 1.0f, 0.0f),
		new Vector3(0.0f, -1.0f, 0.0f),
	};
	protected List<FlockUnit> m_CohesionNeighbours = new List<FlockUnit>();
	protected List<FlockUnit> m_SeparationNeighbours = new List<FlockUnit>();
	protected List<FlockUnit> m_AlignmentNeighbours = new List<FlockUnit>();
	
	private Flocking m_AssignedFlock;
	private Vector3 m_CurrentVelocity;
	private Vector3 m_CurrentObstacleAvoidanceVector;
	private float m_CurrentSpeed;
	public Transform CachedTransform { get; protected set; }
	public Flocking AssignedFlock { get; protected set; }
	
	protected virtual void Awake() => CachedTransform = transform;

	public virtual void AssignFlock(Flocking flock) => this.m_AssignedFlock = flock;

	public virtual void InitializeSpeed(float speed) => this.m_CurrentSpeed = speed;
	public virtual void UpdateBehaviour()
	{
		FindNeighbours();
		CalculateSpeed();
		Vector3 cohesion = CalculateCohesion() * m_AssignedFlock.CohesionWeight;
		Vector3 separation = CalculateSeparation() * m_AssignedFlock.AvoidanceWeight;
		Vector3 alignment = CalculateAlignment() * m_AssignedFlock.AlignmentWeight;
		Vector3 bounds = CalculateBounds() * m_AssignedFlock.BoundsAvoidanceWeight;
		Vector3 obstacle = CalculateObstacleAvoidance() * m_AssignedFlock.ObstacleAvoidanceWeight;
		
		Vector3 direction = cohesion + separation + alignment + bounds + obstacle;
		 direction = Vector3.SmoothDamp(CachedTransform.forward, direction, ref m_CurrentVelocity, m_SmoothDampening);
		 direction = direction.normalized * m_CurrentSpeed;
		 if (direction.Equals(Vector3.zero))
		 {
		 	direction = transform.forward;
		 }
		CachedTransform.forward = direction;
		CachedTransform.position += direction * Time.deltaTime;
	}

	

	protected virtual void FindNeighbours()
	{
		m_CohesionNeighbours.Clear();
		m_SeparationNeighbours.Clear();
		m_AlignmentNeighbours.Clear();
		FlockUnit[] assignedFlockUnits = m_AssignedFlock.Units;
		for (int i = 0; i < assignedFlockUnits.Length; i++)
		{
			FlockUnit otherUnit = assignedFlockUnits[i];
			if (otherUnit != this)
			{
				float sqrMagnitude = Vector3.SqrMagnitude(otherUnit.CachedTransform.position - CachedTransform.position);
				if(sqrMagnitude <= m_AssignedFlock.CohesionFalloffRange * m_AssignedFlock.CohesionFalloffRange)
				{
					m_CohesionNeighbours.Add(otherUnit);
				}
				if (sqrMagnitude <= m_AssignedFlock.AvoidanceFalloffRange * m_AssignedFlock.AvoidanceFalloffRange)
				{
					m_SeparationNeighbours.Add(otherUnit);
				}
				if (sqrMagnitude <= m_AssignedFlock.AlignmentFalloffRange * m_AssignedFlock.AlignmentFalloffRange)
				{
					m_AlignmentNeighbours.Add(otherUnit);
				}
			}
		}
	}

	protected virtual void CalculateSpeed()
	{
		if (m_CohesionNeighbours.Count == 0)
		{
			return;
		}
		m_CurrentSpeed = 0;
		for (int i = 0; i < m_CohesionNeighbours.Count; i++)
		{
			m_CurrentSpeed += m_CohesionNeighbours[i].m_CurrentSpeed;
		}
		m_CurrentSpeed /= m_CohesionNeighbours.Count;
		m_CurrentSpeed = Mathf.Clamp(m_CurrentSpeed, m_AssignedFlock.MinimumSpeed, m_AssignedFlock.MaximumSpeed);
	}

	protected virtual Vector3 CalculateCohesion()
	{
		Vector3 cohesionVector = Vector3.zero;
		if (m_CohesionNeighbours.Count == 0)
		{
			return Vector3.zero;
		}
		int neighbourIndices = 0;
		for (int i = 0; i < m_CohesionNeighbours.Count; i++)
		{
			if (IsWithinFieldOfView(m_CohesionNeighbours[i].CachedTransform.position))
			{
				neighbourIndices++;
				cohesionVector += m_CohesionNeighbours[i].CachedTransform.position;
			}
		}
		cohesionVector /= neighbourIndices;
		cohesionVector -= CachedTransform.position;
		cohesionVector = cohesionVector.normalized;
		return cohesionVector;
	}

	protected virtual Vector3 CalculateAlignment()
	{
		Vector3 aligementVector = CachedTransform.forward;
		if (m_AlignmentNeighbours.Count == 0)
		{
			return CachedTransform.forward;
		}
		int detectedWithinFov = 0;
		for (int i = 0; i < m_AlignmentNeighbours.Count; i++)
		{
			if (IsWithinFieldOfView(m_AlignmentNeighbours[i].CachedTransform.position))
			{
				detectedWithinFov++;
				aligementVector += m_AlignmentNeighbours[i].CachedTransform.forward;
			}
		}
		aligementVector /= detectedWithinFov;
		aligementVector = aligementVector.normalized;
		return aligementVector;
	}

	protected virtual Vector3 CalculateSeparation()
	{
		Vector3 separation = Vector3.zero;
		if (m_AlignmentNeighbours.Count == 0)
		{
			return Vector3.zero;
		}
		int detectedWithinFov = 0;
		for (int i = 0; i < m_SeparationNeighbours.Count; i++)
		{
			if (IsWithinFieldOfView(m_SeparationNeighbours[i].CachedTransform.position))
			{
				detectedWithinFov++;
				separation += (CachedTransform.position - m_SeparationNeighbours[i].CachedTransform.position);
			}
		}
		separation /= detectedWithinFov;
		separation = separation.normalized;
		return separation;
	}

	protected virtual Vector3 CalculateBounds()
	{
		Vector3 offsetToCenter = m_AssignedFlock.transform.position - CachedTransform.position;
		bool isNearCenter = (offsetToCenter.magnitude >= m_AssignedFlock.BoundsDistanceThreshold * 0.9f);
		return isNearCenter 
			? offsetToCenter.normalized 
			: Vector3.zero;
	}

	protected virtual Vector3 CalculateObstacleAvoidance()
	{
		Vector3 obstacleVector = Vector3.zero;
		RaycastHit hit;
		if (Physics.Raycast(CachedTransform.position, CachedTransform.forward, out hit, m_AssignedFlock.ObstacleAvoidanceFalloff, m_ObstacleLayerMask))
		{
			obstacleVector = CalculateDirectionToAvoidObstacle();
		}
		else
		{
			m_CurrentObstacleAvoidanceVector = Vector3.zero;
		}
		return obstacleVector;
	}

	protected virtual Vector3 CalculateDirectionToAvoidObstacle()
	{
		if(!m_CurrentObstacleAvoidanceVector.Equals(Vector3.zero))
		{
			RaycastHit hit;
			if(!Physics.Raycast(CachedTransform.position, CachedTransform.forward, out hit, m_AssignedFlock.ObstacleAvoidanceFalloff, m_ObstacleLayerMask))
			{
				return m_CurrentObstacleAvoidanceVector;
			}
		}
		float maxDistance = int.MinValue;
		Vector3 selectedDirection = Vector3.zero;
		for (int i = 0; i < m_ObstacleAvoidanceDirections.Length; i++)
		{
			RaycastHit hit;
			Vector3 curDirection = CachedTransform.TransformDirection(m_ObstacleAvoidanceDirections[i].normalized);
			if(Physics.Raycast(CachedTransform.position, curDirection, out hit, m_AssignedFlock.ObstacleAvoidanceFalloff, m_ObstacleLayerMask))
			{
				float dist = (hit.point - CachedTransform.position).sqrMagnitude;
				if(dist > maxDistance)
				{
					maxDistance = dist;
					selectedDirection = curDirection;
				}
			}
			else
			{
				selectedDirection = curDirection;
				m_CurrentObstacleAvoidanceVector = curDirection.normalized;
				return selectedDirection.normalized;
			}
		}
		return selectedDirection.normalized;
	}

	protected virtual bool IsWithinFieldOfView(Vector3 position)
	{
		return Vector3.Angle(CachedTransform.forward, position - CachedTransform.position) <= m_FieldOfViewAngle;
	}
}
}