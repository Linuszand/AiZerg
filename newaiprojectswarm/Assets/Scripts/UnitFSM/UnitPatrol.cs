using UnityEngine;
using UnityEngine.AI;

public class UnitPatrol : MonoBehaviour
{
    [Header("Patrol settings")]
    [SerializeField] private float _waypointTolerance = 1.5f;
    
    private Vector3 _pointA;
    private Vector3 _pointB;
    
    private bool _isGoingToB = true;
    
    private NavMeshAgent _agent;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }
    
    public void SetPatrolPoints(Vector3 startPoint, Vector3 targetPoint)
    {
        _pointA = startPoint;
        _pointB = targetPoint;
        _isGoingToB = true;
        _agent.SetDestination(_pointB);
    }

    public void BackAndForthPatrol()
    {
        if (!_agent.pathPending && _agent.remainingDistance <= _waypointTolerance)
        {
            _isGoingToB = !_isGoingToB;
            Vector3 nextDestination = _isGoingToB ? _pointB : _pointA;
            _agent.SetDestination(nextDestination);
        }
    }
    
    public Vector3 GetActiveWaypoint()
    {
        return _isGoingToB ? _pointB : _pointA;
    }
}