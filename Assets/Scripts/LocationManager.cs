using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using UnityEngine.Events;

public class LocationManager : MonoBehaviour
{
    // --- Components ---
    public Transform actor;
    public Transform[] locations;

    // --- Settings ---
    public float tweenSpeed = 1f;
    public AnimationCurve tweenCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // --- Variables ---
    private Vector3 startPosition;
    private Vector3 endPosition;
    private Timer tweenTimer = new Timer(1f);
    public bool processTween { get; private set; }
    public Transform tweenTarget { get; private set; }

    private UnityAction tweenCallback;

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    public void Loop(float dt)
    {
        if(processTween)
            ProcessTween(dt);
    }

    public void SetLocation(int index)
    {
        if(index >= locations.Length)
            return;

        SetLocation(locations[index]);
    }

    public void SetLocation(Transform marker)
    {
        actor.SetPositionAndRotation(marker.position, marker.rotation);
    }

    public void StartLocationTween(int index, UnityAction callback)
    {
        if(index >= locations.Length)
            return;

        StartLocationTween(locations[index], callback);
    }

    public void StartLocationTween(Transform marker, UnityAction callback)
    {
        startPosition = actor.transform.position;
        endPosition = marker.position;
        
        float distance = Vector3.Distance(startPosition, endPosition);

        if(distance < 0.1f)
            return;

        tweenTarget = marker;
        tweenTimer.Reset(distance / tweenSpeed);
        tweenCallback = callback;
        processTween = true;
    }

    public void Stop()
    {
        processTween = false;
    }

    private void ProcessTween(float dt)
    {
        if(tweenTimer.Update(dt))
        {
            processTween = false;

            if(tweenCallback != null)
                tweenCallback.Invoke();
        }
        
        actor.transform.position = Vector3.Lerp(startPosition, endPosition, tweenCurve.Evaluate(tweenTimer.percentage));
    }
}
