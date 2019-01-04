using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class AVCAgent : Agent
{
    public GameObject carObj;
    public ICar car;

    float steering_scale = 16.0f;
    float prev_steering = 0.0f;
    float prev_throttle = 0.0f;
    Vector3 prev_pos;
    float time_prev_pos = 0.0f;
    float going_backwards_time = 0.0f;
    float going_in_a_circle_time = 0.0f;

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
        float steering = vectorAction[0] * steering_scale;
        float throttle = vectorAction[1] + 0.5f; //map [-.5,.5] to [0.0, 1.0];
        car.RequestSteering(steering);
        car.RequestThrottle(throttle);

        float reward = 0.001f * Vector3.Dot(carObj.transform.forward, car.GetVelocity())
                        - 0.00001f * Mathf.Abs(prev_steering - steering)
                        - 0.00001f * Mathf.Abs(prev_throttle - throttle);

        //measure time where all steering is positive or negative
        if(prev_steering > 0.0 && steering > 0.0f)
            going_in_a_circle_time += Time.deltaTime;
        if (prev_steering < 0.0 && steering < 0.0f)
            going_in_a_circle_time += Time.deltaTime;
        else
            going_in_a_circle_time = 0.0f;

        prev_steering = steering;
        prev_throttle = throttle;

        AddReward(reward);

        CheckStuck();
    }

    void CheckStuck()
    {
        //every 5 seconds
        if(Time.timeSinceLevelLoad - time_prev_pos > 5.0)
        {
            //Make sure we've traveled at least 2 units. about 2 feet
            if ((prev_pos - carObj.transform.position).magnitude < 2.0)
            {
                SetReward(-0.5f);
                Done();
            }
            else
            {
                prev_pos = carObj.transform.position;
                time_prev_pos = Time.timeSinceLevelLoad;
            }
        }

        if(prev_throttle < 0.0f)
        {
            going_backwards_time += Time.deltaTime;
        }
        else
        {
            going_backwards_time = 0.0f;
        }

        //We were going around backwards in circles for long periods of time...
        if(going_backwards_time > 5.0)
        {
            SetReward(-1.0f);
            Done();
        }

        if(going_in_a_circle_time > 20.0f)
        {
            SetReward(-10.0f);
            Done();
        }


    }

    void OnCollisionEnter(Collision col)
    {
        SetReward(-1f);
        Done();
    }

    public override void AgentReset()
    {
        car.RestorePosRot();
        prev_steering = 0.0f;
        prev_throttle = 0.0f;
        prev_pos = carObj.transform.position;
        time_prev_pos = Time.timeSinceLevelLoad;
        going_backwards_time = 0.0f;
        going_in_a_circle_time = 0.0f;
    }
    
}
