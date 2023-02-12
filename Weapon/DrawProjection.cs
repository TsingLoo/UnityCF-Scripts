using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// draw a predicted trajectory line
/// https://www.youtube.com/watch?v=RnEO3MRPr5Y
/// </summary>
public class DrawProjection : MonoBehaviour
{
    WeaponController weaponController;
    LineRenderer lineRenderer;

    // Number of points on the line
    public int startPointNum = 0;
    public int numPoints = 50;

    // distance between those points on the line
    public float timeBetweenPoints = 0.1f;
    // 1000 means world is 1000 times smaller
    public float worldScaleFactor = 1.0f;

    // The physics layers that will cause the line to stop being drawn
    public LayerMask CollidableLayers;
    void Start()
    {
        weaponController = GetComponent<WeaponController>();
        lineRenderer = GetComponent<LineRenderer>();
    }


    void Update()
    {
        // todo performance issue due to too small world
        UpdateProjectionLine();

    }

    private void UpdateProjectionLine()
    {
        if(weaponController.CurrentState != EWeaponState.Idle)
        {
            lineRenderer.positionCount = 0;
        }
        else
        {
            lineRenderer.positionCount = (int)numPoints;
            List<Vector3> points = new List<Vector3>();
            Vector3 startingPosition = weaponController._muzzlePosition.position;
            Vector3 startingVelocity = weaponController._muzzlePosition.forward
                * weaponController.projectileForce;

            for (int i = startPointNum; i < numPoints; i++)
            {
                float t = i * timeBetweenPoints;

                Vector3 newPoint = startingPosition + t * startingVelocity;
                newPoint.y = startingPosition.y
                    + startingVelocity.y * t
                    + worldScaleFactor * Physics.gravity.y / 2f * t * t;

                points.Add(newPoint);
                if (Physics.OverlapSphere(newPoint, 1, CollidableLayers).Length > 0)
                {
                    lineRenderer.positionCount = points.Count;
                    break;
                }
            }

            lineRenderer.SetPositions(points.ToArray());
        }

    }
}