using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticManager : object
{
    public static List<GameObject> agents;
    public static List<AgentBehavior> agentBehaviors;

    static void Start()
    {
        agents = new();
        agentBehaviors = new();
    }

}
