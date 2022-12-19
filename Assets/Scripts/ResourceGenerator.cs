using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceGenerator : MonoBehaviour
{
    public float foodPerSecond = 10f;
    public float waterPerSecond = 10f;
    public float heatPerSecond = 10f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Agent"))
        {
            AgentBehavior collidingAgent = other.gameObject.GetComponent<AgentBehavior>();
            collidingAgent.hungerValue += foodPerSecond * Time.deltaTime;
            collidingAgent.waterValue += waterPerSecond * Time.deltaTime;
            collidingAgent.heatValue += heatPerSecond * Time.deltaTime;
        }
    }
}
