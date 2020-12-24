﻿using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections;
using System.Collections.Generic;

public class Rocket2 : Agent
{
    private Transform tr;
    private Rigidbody rb;
    public Transform targetTr;
    public Transform deadZone;
    public Renderer floorRd;
    public Collider launchPad;

    private Material originMt;
    public Material badMt;
    public Material goodMt;

    private float timer = 0.0f;
    private float waitTime = 10.0f;

    private float distanceLimit = 1000.0f;

    private float velX = 0.0f;
    private float velZ = 0.0f;

    private float upwardsThrust = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void Initialize()
    {
        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        originMt = floorRd.material;
    }

    public override void OnEpisodeBegin()
    {
        // the beginning setting of each episodes

        timer = 0;
        tr.localPosition = new Vector3(418.5815f, -1.4f, -58.23349f);
        tr.localRotation = Quaternion.Euler(0,-45,0);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        velX = 0.0f;
        velZ = 0.0f;
        upwardsThrust = 0.0f;

        Debug.Log($"velocity and angular velocity = {rb.velocity} {rb.angularVelocity}");
        Debug.Log($"Initial y position tr={tr.localPosition.y}");
        

        float targetX = 418.5815f + Random.Range(-200.0f, 200.0f);
        float targetZ = -58.23349f + Random.Range(-200.0f, 200.0f);
        float targetY = 500f + Random.Range(0.0f, 20.0f);

        targetTr.localPosition = new Vector3(targetX,
                                             targetY,
                                             targetZ);

    }

    public override void CollectObservations(Unity.MLAgents.Sensors.VectorSensor sensor)
    {

        // observation settings

        sensor.AddObservation(targetTr.localPosition); // 3
        sensor.AddObservation(tr.localPosition); // 3
        sensor.AddObservation(rb.velocity.y); // 1
        sensor.AddObservation(rb.angularVelocity.x); // 1
        sensor.AddObservation(rb.angularVelocity.z); // 1
        sensor.AddObservation(timer); // 1

        // total = 9
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Debug.Log($"Initial y position tr={tr.localPosition.y}");
        var continuousActions = actionBuffers.ContinuousActions;

        timer += Time.deltaTime;
        if(timer > 2.0f){
            launchPad.isTrigger = true;
        }
        if(timer > waitTime){
            Debug.Log($"Time={timer}");
            Debug.Log($"End Episode");
            timer = 0;
            EndEpisode();
        }

        var distance = Vector3.Distance(deadZone.localPosition, tr.localPosition);
        
        if(distance > distanceLimit){
            Debug.Log($"Rocket Out of Distance Limit. End Episode");
            AddReward(-1.0f);
            EndEpisode();
        }

        if(tr.localPosition.y < -1.5f){
            Debug.Log($"Position below initial Y");
            AddReward(-5.0f);
            EndEpisode();   
        }
        
        upwardsThrust += continuousActions[2] * 1;
        // upwardsThrust -= continuousActions[3] * 1;

        velX = Mathf.Clamp(continuousActions[0], -1.0f, 1.0f);
        velZ = Mathf.Clamp(continuousActions[1], -1.0f, 1.0f);

        if(continuousActions[0] > 0.0f){
            Debug.Log($"Vector Action 0 = {continuousActions[0]}");
            Debug.Log($"Vector Action 1 = {continuousActions[1]}");
            Debug.Log($"Vector Action 2 = {continuousActions[2]}");
        }

        rb.AddRelativeTorque(velX*0.1f,
                             0.0f,
                             velZ*0.1f);

        rb.AddRelativeForce(0.0f, 
                            upwardsThrust * 11, 
                            0.0f);

        AddReward(-0.001f);

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal"); // 상하 키
        continuousActionsOut[1] = Input.GetAxis("Vertical"); // 좌우 키
        continuousActionsOut[2] = Input.GetAxis("Fire1");  // left ctrl
        // continuousActionsOut[3] = Input.GetAxis("Fire2");  // left alt

        // Debug.Log($"[0]={continuousActionsOut[0]} [1]={continuousActionsOut[1]} [2]={continuousActionsOut[2]} [3]={continuousActionsOut[3]}");
    }


    void OnCollisionEnter(Collision coll)
    {

        if(coll.collider.CompareTag("Target"))
        {

            AddReward(+5.0f);

            EndEpisode();

        }

        if(coll.collider.CompareTag("LaunchPosition"))
        {

            AddReward(-1.0f);

            EndEpisode();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag=="LaunchPosition"){
            AddReward(-1.0f);
            EndEpisode();
        }

        if(other.tag=="Target"){
            AddReward(-1.0f);
            EndEpisode();
        }
    }

    // IEnumerator TimeChecker()
    // {
    //     yield return new WaitForSeconds(1.0f);
    //     AddReward(-0.001f);
    //     Debug.Log($"Time={timer}");
    //     if (timer > waitTime){
    //         EndEpisode();
    //     }
    // }

    private void FixedUpdate(){

    }

    // Update is called once per frame
    void Update()
    {
        // this is not used in the training environment

        // float turnX = Input.GetAxis("Horizontal"); // 상하 키
        // float turnZ = Input.GetAxis("Vertical"); // 좌우 키
        // float turnY = Input.GetAxis("Fire1"); // left ctrl

        // velX += turnX * 1;
        // velZ += turnZ * 1;
        // velY += turnY * 1;

        // float currentAngularVelX = rb.angularVelocity.x;
        // float currentAngularVelZ = rb.angularVelocity.z;
        // float currentVelY = rb.velocity.y;
        // timer += Time.deltaTime;
        // Debug.Log($"Time={timer}");

        // Debug.Log($"Thrust = {currentVelY}");
        // Debug.Log($"[velX] = {velX} [velZ] = {velZ} [velY]= {velY}");

        // rb.AddRelativeTorque(velX*0.1f,
                    //          0.0f,
                    //          velZ*0.1f);
        
        // rb.AddRelativeForce(0.0f, 
        //                     velY * 11, 
        //                     0.0f);

        // rb.AddForce(0.0f,
        //             currentVelY + velY * 11,
        //             0.0f);
        // if(Time.deltaTime % 20 == 1){

        //     Debug.Log($"Time={Time.deltaTime}");
        //     velX = 0;
        //     velY = 0;
        //     velZ = 0;
        // }
    }
}
