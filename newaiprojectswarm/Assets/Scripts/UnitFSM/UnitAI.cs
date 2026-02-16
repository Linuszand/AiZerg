using UnityEngine;


public class UnitAI : MonoBehaviour
{
    private SelectableUnit _unit;
    private UnitPatrol _patrol;
    
    [Header("Settings")]
    public float chaseRange = 10f;
    public Transform target;

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

    public void AIStates(TravelMode mode, float distanceToTarget)
    {
        if (target != null && distanceToTarget <= chaseRange)
        {
            if (mode != TravelMode.Chasing)
            {
                _unit.SwitchState(TravelMode.Chasing);
                return;
            }
        }
        
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
        _unit.TravelingSeparation();
        _patrol.BackAndForthPatrol();

        if (target != null && distanceToTarget <= chaseRange)
        {
            _unit.SwitchState(TravelMode.Chasing);
        }
    }

    private void UpdateChasingLogic(float distanceToTarget)
    {
        _unit.TravelingSeparation();
        
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
        _unit.TravelingSeparation();
        
        if (!_unit._agent.pathPending && _unit._agent.remainingDistance <= _unit._agent.stoppingDistance + 0.1f)
        {
            if (_unit.isPatrolling)
                _unit.SwitchState(TravelMode.Patrol);
            else
                _unit.SwitchState(TravelMode.Parking);
        }

        if (target != null && distanceToTarget <= chaseRange)
        {
            _unit.SwitchState(TravelMode.Chasing);
        }
    }
}
