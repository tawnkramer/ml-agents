using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class TWalkerAgent : Agent
{
    [Header("Specific to Walker")] [Header("Target To Walk Towards")] [Space(10)]
    public Transform target;
    public bool UseAllObservations = true;

    Vector3 dirToTarget;

    public Transform[] joints; //all joints
    public Transform[] x_y_z_rot_joints; //subset which rotate in x, y, and z
    public Transform[] x_y_rot_joints; //subset which rotate in x and y
    public Transform[] x_rot_joints; //subset which rotate in x only
    public Transform[] y_rot_joints; //subset which rotate in x only

    JointDriveController jdController;
    bool isNewDecisionStep;
    int currentDecisionStep;
    Transform hips;
    

    public override void InitializeAgent()
    {
        jdController = GetComponent<JointDriveController>();
        foreach(Transform joint in joints)
        {
            jdController.SetupBodyPart(joint);

            if (joint.gameObject.name == "hips")
            {
                hips = joint;
            }
        }        
    }

    /// <summary>
    /// Add relevant information on each body part to observations.
    /// </summary>
    public void CollectObservationBodyPart(BodyPart bp)
    {
        var rb = bp.rb;
        AddVectorObs(bp.groundContact.touchingGround ? 1 : 0); // Is this bp touching the ground

        if (UseAllObservations)
        {
            AddVectorObs(rb.position.y); // height above ground
            AddVectorObs(rb.velocity);
            AddVectorObs(rb.angularVelocity);
            Vector3 localPosRelToHips = hips.InverseTransformPoint(rb.position);
            AddVectorObs(localPosRelToHips);
        }
       
        AddVectorObs(bp.currentXNormalizedRot);
        AddVectorObs(bp.currentYNormalizedRot);
        AddVectorObs(bp.currentZNormalizedRot);
        AddVectorObs(bp.currentStrength / jdController.maxJointForceLimit);
    }

    /// <summary>
    /// Loop over body parts to add them to observation.
    /// </summary>
    public override void CollectObservations()
    {
        jdController.GetCurrentJointForces();

        AddVectorObs(dirToTarget.normalized);
        AddVectorObs(jdController.bodyPartsDict[hips].rb.position);
        AddVectorObs(hips.forward);
        AddVectorObs(hips.up);

        foreach (var bodyPart in jdController.bodyPartsDict.Values)
        {
            CollectObservationBodyPart(bodyPart);
        }
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        dirToTarget = target.position - jdController.bodyPartsDict[hips].rb.position;

        float joint_strength_total = 0.0f;

        // Apply action to all relevant body parts. 
        if (isNewDecisionStep)
        {
            var bpDict = jdController.bodyPartsDict;
            int i = -1;

            foreach(Transform joint in x_y_rot_joints)
            {
                bpDict[joint].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
            }

            foreach (Transform joint in x_rot_joints)
            {
                bpDict[joint].SetJointTargetRotation(vectorAction[++i], 0, 0);
            }

            foreach (Transform joint in y_rot_joints)
            {
                bpDict[joint].SetJointTargetRotation(0, vectorAction[++i], 0);
            }

            foreach (Transform joint in x_y_z_rot_joints)
            {
                bpDict[joint].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);
            }

            foreach (Transform joint in joints)
            {
                if (joint == hips)
                    continue;

                //update joint strength settings
                float jointStrength = vectorAction[++i];
                joint_strength_total += jointStrength;
                bpDict[joint].SetJointStrength(jointStrength);
            }            
        }

        IncrementDecisionTimer();

        // reward term trail 1
        // This resulted in successful gait. Was very energetic galloping hop
          AddReward(
            +0.03f * Vector3.Dot(dirToTarget.normalized, jdController.bodyPartsDict[hips].rb.velocity)
            + 0.01f * Vector3.Dot(dirToTarget.normalized, hips.forward)
            + 0.02f * (hips.position.y)            
        );
        
        /* reward term trail 2
         * This resulted in more uniform steps
          AddReward(
            +0.03f * Vector3.Dot(dirToTarget.normalized, jdController.bodyPartsDict[hips].rb.velocity)
            + 0.01f * Vector3.Dot(dirToTarget.normalized, hips.forward)
            + 0.02f * (hips.position.y)
            - 0.01f * jdController.bodyPartsDict[hips].rb.velocity.magnitude
            - 0.01f * jdController.bodyPartsDict[hips].rb.angularVelocity.magnitude
            - 0.0001f * joint_strength_total
        );
         */ 
       
        /*

        // Set reward for this step according to mixture of the following elements.
        // a. Velocity alignment with goal direction.
        // b. Rotation alignment with goal direction.
        // c. Encourage hips height.
        // d. Discourage hips movement.
        // e. Discourage hips rotation
        // f. Discourage lots of energy use
        AddReward(
            +0.03f * Vector3.Dot(dirToTarget.normalized, jdController.bodyPartsDict[hips].rb.velocity)
            + 0.01f * Vector3.Dot(dirToTarget.normalized, hips.forward)
            - 0.01f * jdController.bodyPartsDict[hips].rb.velocity.magnitude
            - 0.01f * jdController.bodyPartsDict[hips].rb.angularVelocity.magnitude
            - 0.01f * joint_strength_total
        );
        */
    }

    /// <summary>
    /// Only change the joint settings based on decision frequency.
    /// </summary>
    public void IncrementDecisionTimer()
    {
        if (currentDecisionStep == agentParameters.numberOfActionsBetweenDecisions ||
            agentParameters.numberOfActionsBetweenDecisions == 1)
        {
            currentDecisionStep = 1;
            isNewDecisionStep = true;
        }
        else
        {
            currentDecisionStep++;
            isNewDecisionStep = false;
        }
    }

    /// <summary>
    /// Loop over body parts and reset them to initial conditions.
    /// </summary>
    public override void AgentReset()
    {
        if (dirToTarget != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dirToTarget);
        }

        foreach (var bodyPart in jdController.bodyPartsDict.Values)
        {
            bodyPart.Reset(bodyPart);
        }

        isNewDecisionStep = true;
        currentDecisionStep = 1;
    }
}
