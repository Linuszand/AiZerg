using UnityEngine;


public class UnitAI : MonoBehaviour
{
    [Header("Settings")]
    public float chaseRange = 10f;
    public Transform target;
    
    private SelectableUnit _unit;
    private UnitPatrol _patrol;

    void Awake()
    {
        _unit = GetComponent<SelectableUnit>();
        _patrol = GetComponent<UnitPatrol>();
        
        if (target == null)
        {
            GameObject potentialTarget = GameObject.FindWithTag("Target"); 
            if (potentialTarget != null)
            {
                target = potentialTarget.transform;
            }
        }
    }

    void SwitchToChasing(TravelMode mode, float distanceToTarget)
    {
        if (target != null && distanceToTarget <= chaseRange)
        {
            if (mode != TravelMode.Chasing)
            {
                _unit.SwitchState(TravelMode.Chasing);
            }
        }
    }

    public void AIStates(TravelMode mode, float distanceToTarget)
    {
        if (mode != TravelMode.Parking)
        {
            _unit.TravelingSeparation();
        }
        
        SwitchToChasing(mode, distanceToTarget);
        
        switch (mode)
        {
            case TravelMode.Patrol:
                UpdatePatrolLogic(distanceToTarget);
                break;
            case TravelMode.Chasing:
                UpdateChasingLogic(distanceToTarget);
                break;
            case TravelMode.ReturnToPatrol:
                UpdateReturningLogic(distanceToTarget);
                break;
        }
    }
    
    private void UpdatePatrolLogic(float distanceToTarget)
    {
        // makes sure the unit patrols back and forth between pointA and pointB
        _patrol.BackAndForthPatrol();
        
    }
    
    private void UpdateChasingLogic(float distanceToTarget)
    {
        if (target != null)
        {
            _unit._agent.SetDestination(target.position);
            
            if (distanceToTarget > chaseRange)
            {
                _unit.SwitchState(TravelMode.ReturnToPatrol);
            }
        }
        else
        {
            _unit.SwitchState(TravelMode.ReturnToPatrol);
        }
    }

    private void UpdateReturningLogic(float distanceToTarget)
    {
        bool hasArrived = !_unit._agent.pathPending &&
                          _unit._agent.remainingDistance <= _unit._agent.stoppingDistance + _unit.ArrivalThreshold;
        
        if (hasArrived || _unit._agent.velocity.sqrMagnitude < 0.01f)
        {
            if (_unit.isPatrolling)
                _unit.SwitchState(TravelMode.Patrol);
            else
                _unit.SwitchState(TravelMode.Parking);
        }
    }
}
