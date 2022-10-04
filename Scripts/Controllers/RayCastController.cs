using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RayCastController : MonoBehaviour
{
    public LayerMask collisionMask;

    public const float skinwidth = .015f;
    const float dstBetweenRays = .1f;
    [HideInInspector]
    public int horizontalRayCount ;
    [HideInInspector]
    public int verticalRayCount ;

    [HideInInspector]
    public float horizontalRaySpacing;
    [HideInInspector]
    public float verticalRaySpacing;

    [HideInInspector]
    public BoxCollider2D colliders;
    [HideInInspector]
    public RayCastOrigins raycastOrigins;

    public virtual void Awake()
    {
        colliders = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    public virtual void Start()
    {
        CalculateRaySpacing();
    }

    public void UpdateRaycastOrigins()
    {
        Bounds bounds = colliders.bounds;
        bounds.Expand(skinwidth * -2);

        raycastOrigins.bottomleft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomright = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topleft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topright = new Vector2(bounds.max.x, bounds.max.y);

    }

    public void CalculateRaySpacing()
    {
        Bounds bounds = colliders.bounds;
        bounds.Expand(skinwidth * -2);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        horizontalRayCount = Mathf.RoundToInt(boundsHeight / dstBetweenRays);
        verticalRayCount = Mathf.RoundToInt(boundsWidth/ dstBetweenRays);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    public struct RayCastOrigins
    {
        public Vector2 topleft, topright;
        public Vector2 bottomleft, bottomright;
    }
}
