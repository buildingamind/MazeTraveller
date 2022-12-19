using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;

public class AgentBehavior : Agent
{
    public bool AgentIsClone;

    public static int agentID;

    [ReadOnly]
    public float lifetime;
    [ReadOnly]
    public float hungerValue = 100f;
    public float hungerDrainPerSecond = 0.1f;
    [ReadOnly]
    public float waterValue = 100f;
    public float waterDrainPerSecond = 0.1f;
    [ReadOnly]
    public float heatValue = 100f;
    public float heatDrainPerSecond = 0.1f;
    [ReadOnly]
    public bool IsColliding = false;

    // Reflector Info
    public Collider agentCollider;
    public Camera SpectatorCamera; // Has a larger FoV to simulate both the eyes it has.

    private BehaviorParameters behaviorParameters;
    private DecisionRequester decisionRequester;

    [ReadOnly]
    public int step;
    [ReadOnly]
    public float currentReward;
    [ReadOnly]
    public int trials;

    public float MoveSpeed;
    public float RotateSpeed;

    private Vector3 lastMotionVector;
    [ReadOnly]
    public Vector3 motionVector;


    private void Awake()
    {
        agentCollider = GetComponent<Collider>();
        decisionRequester = GetComponent<DecisionRequester>();
        behaviorParameters = GetComponent<BehaviorParameters>();

        if (!AgentIsClone)
        {
            this.gameObject.name = "Agent" + string.Format("{0:D2}", agentID);
            agentID++;
            behaviorParameters.BehaviorName = gameObject.name;
        }
        else
        {
            this.gameObject.name = "AgentClone";
            behaviorParameters.BehaviorName = gameObject.name;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        motionVector = transform.position - lastMotionVector;
        lastMotionVector = transform.position;
        // Let's try holding their hands, metaphorically. Though if it helps, I'll hold a newborn AI's hand.
        currentReward = GetCumulativeReward();
        lifetime += Time.deltaTime;

        /*if (StaticManager.currentBallBehavior != null && StaticManager.currentBallBehavior.currentTarget != null)
        {
            SpectatorCamera.enabled = StaticManager.currentBallBehavior.currentTarget == this.gameObject;
        }*/

        hungerValue -= Time.deltaTime * hungerDrainPerSecond;
        waterValue -= Time.deltaTime * waterDrainPerSecond;
        heatValue -= Time.deltaTime * heatDrainPerSecond;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(hungerValue); // This is where you could add other Oracle Information
        sensor.AddObservation(waterValue); // This is where you could add other Oracle Information
        sensor.AddObservation(heatValue); // This is where you could add other Oracle Information
    }

    // ML AGENTS CODE //////////////////////////////////////////////////

    public override void OnActionReceived(ActionBuffers actions)
    {
        step++;
        int rotate = 0; // Direction of rotation
        int moveDir = 0; // Moving the agent forward and backward
        int strafeDir = 0; // Moving the Agent to the left and right.

        if (actions.DiscreteActions.Length > 0)
        {
            moveDir = Mathf.FloorToInt(actions.DiscreteActions[0]);
        }
        if (actions.DiscreteActions.Length > 1)
        {
            rotate = Mathf.FloorToInt(actions.DiscreteActions[1]);
        }
        if (actions.DiscreteActions.Length > 2)
        {
            strafeDir = Mathf.FloorToInt(actions.DiscreteActions[2]);
        }

        switch (moveDir)
        {
            case 0: // Do noting
                break;
            case 1: // W: Move forward
                transform.position += MoveSpeed * Time.deltaTime * Vector3.Scale(transform.forward, new Vector3(1f, 0f, 1f)).normalized;
                break;
            case 2: // S: Move backward (Leave this out of random if you don't want it to move randomly)
                transform.position -= MoveSpeed * Time.deltaTime * Vector3.Scale(transform.forward, new Vector3(1f, 0f, 1f)).normalized;
                break;
            default:
                break;
        }

        switch (strafeDir)
        {
            case 0: // Do noting
                break;
            case 1: // W: Move forward
                transform.position += MoveSpeed * Time.deltaTime * transform.right;
                break;
            case 2: // S: Move backward (Leave this out of random if you don't want it to move randomly)
                transform.position -= MoveSpeed * Time.deltaTime * transform.right;
                break;
            default:
                break;
        }

        switch (rotate)
        {
            case 0:
                break;
            case 1: // D: Turn right
                transform.RotateAround(transform.position, Vector3.up, RotateSpeed * Time.deltaTime);
                break;
            case 2: // A: Turn left
                transform.RotateAround(transform.position, Vector3.up, -RotateSpeed * Time.deltaTime);
                break;
            default:
                break;
        }

        SetReward(hungerValue + waterValue + heatValue);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActionsOut = actionsOut.DiscreteActions;

        if (actionsOut.DiscreteActions.Length > 0)
        {
            // MOVE /////////////////////
            if (Input.GetKey(KeyCode.W))
            {
                discreteActionsOut[0] = 1;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                discreteActionsOut[0] = 2;
            }
            else
            {
                discreteActionsOut[0] = 0;
            }
        }

        if (actionsOut.DiscreteActions.Length > 1)
        {
            // ROTATE ///////////////////
            if (Input.GetKey(KeyCode.RightArrow))
            {
                discreteActionsOut[1] = 1;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                discreteActionsOut[1] = 2;
            }
            else
            {
                discreteActionsOut[1] = 0;
            }
        }


        if (actionsOut.DiscreteActions.Length > 2)
        {
            // ROTATE ///////////////////
            if (Input.GetKey(KeyCode.D))
            {
                discreteActionsOut[2] = 1;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                discreteActionsOut[2] = 2;
            }
            else
            {
                discreteActionsOut[2] = 0;
            }
        }
    }

    private void OnDestroy()
    {
        // PASS
    }

    private void OnDrawGizmos()
    {
        GameObject currentTarget;
        RaycastHit hit;
        Vector3 agentPos = transform.position;
        for (int i = 1; i < 100; i++)
        {
            if (Physics.SphereCast(agentPos + (transform.forward * i), i, transform.forward, out hit, 1000, layerMask: LayerMask.GetMask("Targetable")))
            {
                currentTarget = hit.transform.gameObject;
                Debug.DrawLine(transform.position, currentTarget.transform.position, Color.green);
                break;
            }
        }

        Debug.DrawLine(SpectatorCamera.transform.position, SpectatorCamera.transform.position + SpectatorCamera.transform.forward, Color.blue);
    }

    private void OnCollisionStay(Collision collision)
    {
        IsColliding = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        IsColliding = false;
    }
}
