using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{ 
    // Start is called before the first frame update
    public float speed;
    public bool chase = false;
    bool isOnTrack = true;
    public Transform startingPoint;
    public float searchRadius;
    public float shootingRange;
    public LayerMask chaseTargetMask;

    public Vector3[] localWayPoints;
    Vector3[] globalWayPoints;
    public bool cyclic;
    public float waitTime;
    [Range(0, 3)]
    public float easeAmount;

    int fromWayPointIndex;
    float percentBetweenwayPoints;
    float nextMoveTime;
    public Collider2D player;
    public GameObject bullet;
    public GameObject bulletParent;
    public float fireRate = 1f;
    private float nextFireTime;
    private Transform player_1;
    Controller2D controller;
    public bool shootingenemy;

    private void Start()
    {
        controller = GetComponent<Controller2D>();
        globalWayPoints = new Vector3[localWayPoints.Length];
        for (int i = 0; i < localWayPoints.Length; i++)
        {
            globalWayPoints[i] = localWayPoints[i] + transform.position;
            player_1= GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        float distanceFromPlayer = Vector2.Distance(player_1.position,transform.position);
        player = Physics2D.OverlapCircle(transform.position, searchRadius, chaseTargetMask);

        if (player == null && isOnTrack==true)
        {
            Vector3 velocity = CalculateMovement();
            transform.Translate(velocity);
        }
        else if (player != null && distanceFromPlayer > shootingRange)
        {
            Chase();
            Flip();

        }
        else if(distanceFromPlayer <= shootingRange && nextFireTime <Time.time && shootingenemy)
        {
            
            Instantiate(bullet,bulletParent.transform.position,Quaternion.identity);
            nextFireTime = (Time.time+ fireRate);
        }
        else if(player==null && isOnTrack==false)
        {
            ReturnStartPoint();//go to starting position
        }
            

    }

    private void Chase()
    {
        transform.position=Vector2.MoveTowards(transform.position,player.transform.position,speed * Time.deltaTime);
        isOnTrack = false;
    }

    private void ReturnStartPoint()
    {
        transform.position=Vector2.MoveTowards(transform.position,globalWayPoints[fromWayPointIndex],speed*Time.deltaTime);
        if(transform.position== globalWayPoints[fromWayPointIndex])
        {
            isOnTrack = true;
        }
    }

    private void Flip()
    {
        if(transform.position.x>player.transform.position.x)
            transform.rotation=Quaternion.Euler(0,0,0);
        else
            transform.rotation=Quaternion.Euler(0,180,0);
    }

    Vector3 CalculateMovement()
    {
        if (Time.time < nextMoveTime)
        {
            return Vector3.zero;
        }
        if (isOnTrack)
        {
            fromWayPointIndex %= globalWayPoints.Length;
            int toWayPointIndex = (fromWayPointIndex + 1) % globalWayPoints.Length;
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
        else 
        {
            fromWayPointIndex = 0;
            percentBetweenwayPoints = 0;
            return transform.position;
        }
        
        
        
    }

    float Ease(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    } 

    void OnDrawGizmos()
    {
        Color chaseRadiuscolor;
        chaseRadiuscolor = new Color(0, 1, 0, .25f);
        Gizmos.color = chaseRadiuscolor;
        Gizmos.DrawSphere(transform.position, searchRadius);
        Gizmos.color= Color.green;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        if (localWayPoints != null)
        {
            Gizmos.color = Color.red;
            float size = .3f;
            for (int i = 0; i < localWayPoints.Length; i++)
            {
                Vector3 globalWayPointPos = (Application.isPlaying) ? globalWayPoints[i] : localWayPoints[i] + transform.position;
                Gizmos.DrawLine(globalWayPointPos - Vector3.up * size, globalWayPointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWayPointPos - Vector3.right * size, globalWayPointPos + Vector3.right * size);
            }
        }
        if (player != null)
        {
            Gizmos.DrawLine(transform.position, player.transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color= Color.green;
        Gizmos.DrawWireSphere(transform.position, shootingRange);
    }

}
