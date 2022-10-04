using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RayCastController
{

	public LayerMask passengerMask;


	public Vector3[] localWayPoints;
	Vector3[] globalWayPoints;

	public float speed;
	public bool cyclic;
	public float waitTime;
	[Range(0,3)]
	public float easeAmount;

	int fromWayPointIndex;
	float percentBetweenwayPoints;
	float nextMoveTime;

	List<PassengerMovement> passengerMovement;
	Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

	public override void Start()
	{
		base.Start();

		globalWayPoints = new Vector3[localWayPoints.Length];
		for (int i= 0;i < localWayPoints.Length; i++){
			globalWayPoints[i] = localWayPoints[i] + transform.position;
        }
	}

	void Update()
	{

		UpdateRaycastOrigins();

		Vector3 velocity = CalculatePlatformMovement();
		CalculatePassengerMovement(velocity);
		MovePassengers(true);
		transform.Translate(velocity);
		MovePassengers(false);
	}

	float Ease(float x)
    {
		float a = easeAmount+1;
		return Mathf.Pow(x, a) / (Mathf.Pow(x,a)+ Mathf.Pow(1-x, a));
    }

	Vector3 CalculatePlatformMovement()
	{
        if (Time.time < nextMoveTime)
        {
			return Vector3.zero;
        }
		fromWayPointIndex %= globalWayPoints.Length;
		int toWayPointIndex = (fromWayPointIndex + 1)%globalWayPoints.Length;
		float distanceBetweenWaypoints = Vector3.Distance(globalWayPoints[fromWayPointIndex], globalWayPoints[toWayPointIndex]);
		percentBetweenwayPoints += Time.deltaTime * speed / distanceBetweenWaypoints;
		percentBetweenwayPoints = Mathf.Clamp01(percentBetweenwayPoints);
		float easePercentBetweenWayPoints = Ease(percentBetweenwayPoints);
		
		Vector3 newPos = Vector3.Lerp(globalWayPoints[fromWayPointIndex], globalWayPoints[toWayPointIndex], easePercentBetweenWayPoints);

        if (percentBetweenwayPoints >= 1)
        {
			percentBetweenwayPoints = 0;
			fromWayPointIndex++;
            if (!cyclic)
            {
				if (fromWayPointIndex >= globalWayPoints.Length - 1)
				{
					fromWayPointIndex = 0;
					System.Array.Reverse(globalWayPoints);
				}
			}
			nextMoveTime = Time.time + waitTime;
        }

		return newPos - transform.position;
    }

	void MovePassengers(bool beforeMovePlateform)
    {
		foreach(PassengerMovement passenger in passengerMovement)
        {
            if (!passengerDictionary.ContainsKey(passenger.transform))
            {
				passengerDictionary.Add(passenger.transform,passenger.transform.GetComponent<Controller2D>());
            }
			if (passenger.moveBeforePlatform == beforeMovePlateform)
            {
				passengerDictionary[passenger.transform].Move(passenger.velocity,passenger.standingOnPlateform);
            }
        }
    }

	void CalculatePassengerMovement(Vector3 velocity)
	{
		HashSet<Transform> movedPassengers = new HashSet<Transform>();
		passengerMovement = new List<PassengerMovement>();

		float directionX = Mathf.Sign(velocity.x);
		float directionY = Mathf.Sign(velocity.y);

		// Vertically moving platform
		if (velocity.y != 0)
		{
			float rayLength = Mathf.Abs(velocity.y) + skinwidth;

			for (int i = 0; i < verticalRayCount; i++)
			{
				Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomleft : raycastOrigins.topleft;
				rayOrigin += Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

				if (hit)
				{
					if (!movedPassengers.Contains(hit.transform))
					{
						movedPassengers.Add(hit.transform);
						float pushX = (directionY == 1) ? velocity.x : 0;
						float pushY = velocity.y - (hit.distance - skinwidth) * directionY;

						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
					}
				}
			}
		}

		// Horizontally moving platform
		if (velocity.x != 0)
		{
			float rayLength = Mathf.Abs(velocity.x) + skinwidth;

			for (int i = 0; i < horizontalRayCount; i++)
			{
				Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomleft : raycastOrigins.bottomright;
				rayOrigin += Vector2.up * (horizontalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

				if (hit)
				{
					if (!movedPassengers.Contains(hit.transform))
					{
						movedPassengers.Add(hit.transform);
						float pushX = velocity.x - (hit.distance - skinwidth) * directionX;
						float pushY = -skinwidth;

						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
					}
				}
			}
		}

		// Passenger on top of a horizontally or downward moving platform
		if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
		{
			float rayLength = skinwidth * 2;

			for (int i = 0; i < verticalRayCount; i++)
			{
				Vector2 rayOrigin = raycastOrigins.topleft + Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

				if (hit)
				{
					if (!movedPassengers.Contains(hit.transform))
					{
						movedPassengers.Add(hit.transform);
						float pushX = velocity.x;
						float pushY = velocity.y;

						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
					}
				}
			}
		}
	}

	struct PassengerMovement
    {
		public Transform transform;
		public Vector3 velocity;
		public bool standingOnPlateform;
		public bool moveBeforePlatform;

		public PassengerMovement(Transform _transform,Vector3 _velocity, bool _standingOnPlateform, bool _moveBeforePlatform)
        {
			transform = _transform;
			velocity = _velocity;
			standingOnPlateform = _standingOnPlateform;
			moveBeforePlatform = _moveBeforePlatform;
		}

    }

	void OnDrawGizmos()
	{
		
		if (localWayPoints != null)
		{
			Gizmos.color=Color.red;
			float size = .3f;
			for (int i = 0; i < localWayPoints.Length; i++)
            {
				Vector3 globalWayPointPos = (Application.isPlaying)?globalWayPoints[i]:localWayPoints[i] + transform.position;
				Gizmos.DrawLine(globalWayPointPos - Vector3.up * size, globalWayPointPos + Vector3.up * size);
				Gizmos.DrawLine(globalWayPointPos - Vector3.right * size, globalWayPointPos + Vector3.right * size);
			}
		}
	}
}