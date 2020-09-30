using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class CarAgent : Agent
{

    public CarChecks carChecks;
    DriftController car;

    // LayerMask is a bitwise bools: 01010100, each represent a layer.
    public LayerMask raycastLayers;     // Layers to be included in raycast

    public int decisionInt = 5;         // Request a decision every X steps.

    public float debugRaycastTime = 3f;
    public float raycastDistance = 20;
    public float hitDist = 1f;          // Raycast distance for hit
    public Transform[] raycasts;

    float throttle;
    float turn;

    public bool singleLap = false;
    public float checkpointReward = 1;
    public float speedReward = .001f;
    public float angleReward = .1f;

    public float reward;

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
        carChecks.Restart();
    }

    public void OnReachCheckpoint() {
        this.AddReward(checkpointReward);
    }

    public void OnLap() {
        if (singleLap) {
            this.EndEpisode();
            this.OnEpisodeBegin(); ;
        }
    }

    public override void OnActionReceived(float[] vectorAction) {
        base.OnActionReceived(vectorAction);
        float angle = Mathf.Abs(car.angle);

        throttle = vectorAction[0];
        if (throttle > 0) throttle = 1;
        if (throttle < 0) throttle = -1;
        turn = vectorAction[1];

        car.inThrottle = throttle;
        car.inTurn = turn;

        AddReward(car.speed * speedReward);

        // Reward drifting, but punish reversing. Minimum speed to ignore small movements.
        if (car.speed > 3f) {
            if (angle > 10 && angle < 120) AddReward(angle * angleReward);
            else if (angle >= 120) AddReward((120 - angle) * angleReward);
        }

        // Check reward
        reward = GetCumulativeReward();
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(car.speed);

        // Cast multiple rays around the car
        for (int i = 0; i < raycasts.Length; i++) {
            AddRaycastVectorObs(sensor, raycasts[i]);
        }

        sensor.AddObservation(car.angle);
        sensor.AddObservation(car.slips);
    }

    void AddRaycastVectorObs(VectorSensor sensor, Transform ray) {
        // Cast a ray around the car
        RaycastHit hitInfo = new RaycastHit();
        var hit = Physics.Raycast(ray.position, ray.forward, out hitInfo, raycastDistance, raycastLayers.value, QueryTriggerInteraction.Ignore);

        // This is valid only if it directly hit off-track object
        var distance = hitInfo.distance;

        if (!hit) distance = raycastDistance;
        var obs = distance / raycastDistance;
        sensor.AddObservation(obs);

        // If hit wall
        if (distance < hitDist) {
            // Discourage high speed near the wall
            // Facilitate finding new path
            AddReward(-10 * car.speed * speedReward);

            this.EndEpisode();
            this.OnEpisodeBegin();
        }
        Debug.DrawRay(ray.position, ray.forward * distance, Color.Lerp(Color.red, Color.green, obs), Time.deltaTime * debugRaycastTime);
    }


}
