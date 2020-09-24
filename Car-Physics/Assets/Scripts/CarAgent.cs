using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class CarAgent : Agent
{

    DriftController car;

    // LayerMask is a bitwise bools: 01010100, each represent a layer.
    public LayerMask raycastLayers;     // Layers to be included in raycast
    //public LayerMask outLayers;       // Off-track layers

    public int decisionInt = 5;         // Request a decision every X steps.

    public float debugRaycastTime = 3f;
    public float raycastDistance = 20;
    public float hitDist = 1f;          // Raycast distance for hit
    public Transform[] raycasts;

    float throttle;
    float turn;

    public float rewardOnCheckpoint = 1;

    Vector3 startingPos;
    Quaternion startingRot;

    void Awake() {
        car = GetComponent<DriftController>();
        startingPos = this.transform.position;
        startingRot = this.transform.rotation;
    }

    private void FixedUpdate() {
        // Request a decision every X steps. RequestDecision() automatically calls RequestAction(),
        // but for the steps in between, we need to call it explicitly to take action using the results
        // of the previous decision
        if (StepCount % decisionInt == 0) {
            RequestDecision();
        } else {
            RequestAction();
        }
    }

    public override void OnEpisodeBegin() {
        base.OnEpisodeBegin();
        car.FullReset();
        //trackManager.RestartRace();
    }

    //public void OnReachCheckpoint(Checkpoint checkpoint) {
    //    this.AddReward(rewardOnCheckpoint);
    //}

    public override void OnActionReceived(float[] vectorAction) {
        base.OnActionReceived(vectorAction);
        throttle = vectorAction[0];
        if (throttle > 0) throttle = 1;
        turn = vectorAction[1];

        car.inThrottle = throttle;
        car.inTurn = turn;

        AddReward(car.speed * .01f);
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(car.speed);

        // Cast multiple rays around the car
        for (int i = 0; i < raycasts.Length; i++) {
            AddRaycastVectorObs(sensor, raycasts[i]);
        }
    }

    void AddRaycastVectorObs(VectorSensor sensor, Transform ray) {
        // Cast a ray around the car
        RaycastHit hitInfo = new RaycastHit();
        var hit = Physics.Raycast(ray.position, ray.forward, out hitInfo, raycastDistance, raycastLayers.value, QueryTriggerInteraction.Ignore);

        //int layer = 0;
        //if (hit)
        //    layer = hitInfo.transform.gameObject.layer;

        // This is valid only if it directly hit off-track object
        var distance = hitInfo.distance;

        //// If it hit layer other than the off-track layermask list
        //if (!(outLayers == (outLayers | (1 << layer)))) {
        //    // As if it doesn't hit anything
        //    distance = raycastDistance;
        //}

        if (!hit) distance = raycastDistance;
        var obs = distance / raycastDistance;
        sensor.AddObservation(obs);

        // Example, hit barrier that's included in the off-track list
        // Or fall sideways into the off-side track
        if (distance < hitDist) {
            this.EndEpisode();
            this.OnEpisodeBegin();
        }
        Debug.DrawRay(ray.position, ray.forward * distance, Color.Lerp(Color.red, Color.green, obs), Time.deltaTime * debugRaycastTime);
    }


}
