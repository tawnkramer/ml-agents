using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class AVCAgent : Agent
{
    public GameObject carObj;
    public ICar car;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        car = carObj.GetComponent<ICar>();
    }

    public override void CollectObservations()
    {
        //uses camera
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        float steering = vectorAction[0];
        float throttle = vectorAction[1];
        car.RequestSteering(steering);
        car.RequestThrottle(throttle);

        float reward = 0.001f * Vector3.Dot(carObj.transform.forward, car.GetVelocity());
        AddReward(reward);
    }

    void OnCollisionEnter(Collision col)
    {
        SetReward(-1f);
        Done();
    }

    public override void AgentReset()
    {
        car.RestorePosRot();
    }
    
}
