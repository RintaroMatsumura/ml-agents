using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class TemplateAgent : Agent
{
    public override void CollectObservations(VectorSensor sensor)
    {
    }

    public override void OnActionReceived(ActionSegment<float> continuousActions, ActionSegment<int> discreteActions)
    {
    }

    public override void OnEpisodeBegin()
    {
    }
}
