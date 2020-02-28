using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPSController : MonoBehaviour
{
    public Vector2 currentPosition;
    public Vector2 lastPosition;

    private bool active = false;
    private const double EARTH = 6371e3;

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        if(!active)
            return;

        currentPosition.x = Input.location.lastData.latitude;
        currentPosition.y = Input.location.lastData.longitude;
    }

    private void FixedUpdate()
    {
        double distance = CalculateDistance(lastPosition, currentPosition);
        float direction = CalculateDirection(lastPosition, currentPosition) + 180;
        Debug.LogFormat("Distance: {0}km", (distance / 1000f).ToString("0.00"));
        Debug.LogFormat("Degree: {0}°", direction.ToString("0.00"));

        Debug.DrawRay(transform.position, transform.forward, Color.red, 1f);
        Debug.DrawRay(transform.position, Quaternion.AngleAxis(direction, Vector3.up) * Vector3.forward, Color.green, 1f);
    }

    private double CalculateDistance(Vector2 pos1, Vector2 pos2)
    {
        // --- Pos1&2 lat in rad ---
        float φ1 = pos1.x * Mathf.Deg2Rad;
        float φ2 = pos2.x * Mathf.Deg2Rad;

        // --- Delta Position ---
        float Δφ = (pos2.x - pos1.x) * Mathf.Deg2Rad;
        float Δλ = (pos2.y - pos1.y) * Mathf.Deg2Rad;

        float a = Mathf.Sin(Δφ / 2) * Mathf.Sin(Δφ / 2) +
                Mathf.Cos(φ1) * Mathf.Cos(φ2) *
                Mathf.Sin(Δλ / 2) * Mathf.Sin(Δλ / 2);
        float curious = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));

        return EARTH* curious;
    }

    private float CalculateDirection(Vector2 pos1, Vector2 pos2)
    {
        float y = Mathf.Sin(pos2.x - pos1.x) * Mathf.Cos(pos2.y);
        float x = Mathf.Cos(pos1.y) * Mathf.Sin(pos2.y) -
                Mathf.Sin(pos1.y) * Mathf.Cos(pos2.y) * Mathf.Cos(pos2.x - pos1.x);

        return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
    }
}
