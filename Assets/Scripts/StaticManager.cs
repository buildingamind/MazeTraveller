using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticManager : MonoBehaviour
{
    public static List<GameObject> agents;
    public static List<AgentBehavior> agentBehaviors;

    static void Start()
    {
        agents = new();
        agentBehaviors = new();
    }

}
