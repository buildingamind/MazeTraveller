using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicDoorManager : MonoBehaviour
{
    public MagicDoorManager partnerDoor;
    public GameObject observer;
    public GameObject viewHolder;

    private bool readyToTeleport = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        viewHolder.transform.position = new Vector3(this.transform.position.x - (partnerDoor.transform.position.x - observer.transform.position.x),
            observer.transform.position.y,
            this.transform.position.z - (partnerDoor.transform.position.z - observer.transform.position.z));
    }

    void OnTriggerEnter(Collider other)
    {
        if (!readyToTeleport) { return; }

        if (other.gameObject.CompareTag("Agent"))
        {
            AgentBehavior agentBehavior = other.GetComponent<AgentBehavior>();
            other.transform.position = partnerDoor.viewHolder.transform.position + agentBehavior.motionVector * 5f;
            partnerDoor.readyToTeleport = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        readyToTeleport = true;
    }
}
