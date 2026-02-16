using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum TravelMode { Traveling, Parking, Patrol, Chasing, ReturnToPatrol}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class SelectableUnit : MonoBehaviour
{
   [SerializeField] private SpriteRenderer _spriteSelection;

   [Header("Flocking Settings")]
   public float SeparationRadius = 3f;
   public float SeparationForce = 0.5f;
   public float ArrivalThreshold = 1.0f;
   [SerializeField] private float _dampening = 5f;
   [SerializeField] private float _nextScanTime;
   [SerializeField] private float _scanRate = 0.2f;
   
   
   private Vector3 _currentPushVelocity;
   private Rigidbody _rb;
   public NavMeshAgent _agent;
   private UnitAI _unitAI;
   private UnitPatrol _patrol;
   
   private Collider[] neighborColliders = new Collider[30];
   public TravelMode _currentMode = TravelMode.Traveling;
   private float _pathResetTimer = 0.2f;
   public Vector3 LastDestination;
   public bool isPatrolling = false;
   
   private void Awake()
   {
      SelectionManager.Instance.AvailableUnits.Add(this);
      _rb = GetComponent<Rigidbody>();
      _agent = GetComponent<NavMeshAgent>();
      _unitAI = GetComponent<UnitAI>();
      _patrol = GetComponent<UnitPatrol>();
   }
   
   void Update()
   {
      // int currentCount = Physics.OverlapSphereNonAlloc(transform.position, SeparationRadius, neighborColliders);
      // Debug.Log($"{gameObject.name} sees {currentCount} colliders");
      
      float distanceToTarget = float.MaxValue; 
    
      if (_unitAI != null && _unitAI.target != null)
      {
         distanceToTarget = Vector3.Distance(transform.position, _unitAI.target.position);
      }
      
      switch (_currentMode)
      {
         case TravelMode.Traveling:
            HandleTravelLogic();
            _unitAI.AIStates(_currentMode, distanceToTarget);
            DrawDebugLines(Color.red);
            break;
         
         case TravelMode.Parking:
            HandleParkingLogic();
            _unitAI.AIStates(_currentMode, distanceToTarget);
            DrawDebugLines(Color.blue);
            break;
         
         case TravelMode.Patrol:
         case TravelMode.Chasing:
         case TravelMode.ReturnToPatrol:
            _unitAI.AIStates(_currentMode, distanceToTarget);
            break;
      }
      ApplyVelocity();
      Debug.Log($"Current mode: {_currentMode}");
   }

   public Vector3 GetLastDestination()
   {
      return LastDestination;
   }
   
   public void DrawDebugLines(Color color)
   {
      int count = Physics.OverlapSphereNonAlloc(transform.position, SeparationRadius, neighborColliders);
      for (int i = 0; i < count; i++)
      {
         if (neighborColliders[i].gameObject != gameObject && neighborColliders[i].CompareTag("Unit"))
         {
            Debug.DrawLine(transform.position, neighborColliders[i].transform.position, color);
         }
      }
   }

   public void ParkingSeparation()
   {
      if (_rb.IsSleeping()) _rb.WakeUp();
      
      System.Array.Clear(neighborColliders, 0, neighborColliders.Length);
      int count = Physics.OverlapSphereNonAlloc(transform.position, SeparationRadius, neighborColliders);
      
      Vector3 nudgeOffset = Vector3.zero;
      int neighborsFound = 0;

      for (int i = 0; i < count; i++)
      {
         Collider other = neighborColliders[i];
         // if the other gameobject is ourselves or not gameobject with "Unit" tag
         if (other.gameObject == gameObject || !other.CompareTag("Unit")) continue;
         
         // get the separation vector
         Vector3 diff = transform.position - other.transform.position;
         
         // make sure we don't move on the y axis
         diff.y = 0;
         float distance = diff.magnitude;

         if (distance <= 0.01f || distance >= SeparationRadius) continue;
         
         // stronger force away from unit the closer we are
         float strength = (SeparationRadius - distance) / SeparationRadius;
         nudgeOffset += diff.normalized * strength;
         neighborsFound++;
      }

      if (neighborsFound > 0)
      {
         if (nudgeOffset.magnitude > 0.05f)
         {
            Vector3 nudgePoint = transform.position + (nudgeOffset * SeparationForce);
            
            _agent.SetDestination(nudgePoint);
         }
      }
      else if (_agent.hasPath)
      {
         _agent.ResetPath();
      }
   }

   void CheckForArrival()
   {
         if (_pathResetTimer > 0) _pathResetTimer -= Time.deltaTime;

         if (_pathResetTimer <= 0 && !_agent.pathPending && _agent.hasPath)
         {
            if (_agent.remainingDistance <= _agent.stoppingDistance + ArrivalThreshold)
            {
               SwitchState(TravelMode.Parking);
            }
         }
   }

   void ApplyTravelingVelocity()
   {
      Vector3 finalVelocity = _agent.desiredVelocity + _currentPushVelocity;
      _agent.velocity = Vector3.ClampMagnitude(finalVelocity, _agent.speed);
   }
   
   void HandleTravelLogic()
   {
      
      CheckForArrival();
      
      TravelingSeparation();
   }

   void HandleParkingLogic()
   {
      if (Time.time >= _nextScanTime)
      {
         ParkingSeparation();
         _nextScanTime = Time.time + _scanRate;
      }
   }

   public void TravelingSeparation()
   {
      Vector3 separationForce = Vector3.zero;
      
      System.Array.Clear(neighborColliders, 0, neighborColliders.Length);
      int count = Physics.OverlapSphereNonAlloc(transform.position, SeparationRadius, neighborColliders);

      for (int i = 0; i < count; i++)
      {
         Collider other = neighborColliders[i];
         if (other.gameObject == gameObject || !other.CompareTag("Unit")) continue;
         
         Vector3 pushDirection = transform.position - other.transform.position;
         pushDirection.y = 0;

         float distance = pushDirection.magnitude;
         if (distance <= 0.01f || distance >= SeparationRadius) continue;
         
         float strength = (SeparationRadius - distance) / SeparationRadius;
         
         separationForce += pushDirection.normalized * strength * SeparationForce;
      }
      
      _currentPushVelocity = Vector3.Lerp(_currentPushVelocity, separationForce, Time.deltaTime * _dampening);
   }
   
   public void SwitchState(TravelMode newMode)
   {
      if (_currentMode == newMode) return;
      _currentMode = newMode;

      if (_currentMode == TravelMode.Parking)
      {
         _agent.ResetPath();
         _agent.velocity = Vector3.zero;
         _currentPushVelocity = Vector3.zero;
         
         _agent.acceleration = 100f; 
         _agent.speed = 1.5f; 
      } else if (_currentMode == TravelMode.ReturnToPatrol)
      {
         _agent.isStopped = false;
         _agent.acceleration = 30f;
         _agent.speed = 3.5f;
         
         Vector3 returnPos = isPatrolling ? _patrol.GetActiveWaypoint() : LastDestination;
         _agent.SetDestination(returnPos);
      }
      else // traveling
      {
         _agent.isStopped = false;
         _agent.acceleration = 30f;
         _agent.speed = 3.5f;
      }
   }
   
   private void ApplyVelocity()
   {
      Vector3 desiredMove = _agent.desiredVelocity;
      
      Vector3 finalVelocity = desiredMove + _currentPushVelocity;
      
      _agent.velocity = finalVelocity;
      
      if (finalVelocity.sqrMagnitude > 0.1f)
      {
         Quaternion targetRot = Quaternion.LookRotation(finalVelocity.normalized);
         transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
      }
   }
   
   public void MoveTo(Vector3 position)
   {
      isPatrolling = false;
      LastDestination = position;
      _pathResetTimer = 0.2f;
      SwitchState(TravelMode.Traveling);
      _agent.isStopped = false;
      _agent.SetDestination(position);
   }

   public void StartPatrol(Vector3 targetPoint)
   {
      isPatrolling = true;
      _pathResetTimer = 0.2f;

      if (_patrol != null)
      {
         _patrol.SetPatrolPoints(transform.position, targetPoint);
         SwitchState(TravelMode.Patrol);
      }
   }

   public void OnSelected()
   {
      _spriteSelection.gameObject.SetActive(true);
   }

   public void OnDeselected()
   {
      _spriteSelection.gameObject.SetActive(false);
      _spriteSelection.transform.localEulerAngles = new Vector3(90, 0, 0);
   }
   
   private void OnDrawGizmos()
   {
      if (_agent != null)
      {
         Gizmos.color = Color.green;
         Gizmos.DrawWireSphere(transform.position, SeparationRadius);
         if (_currentPushVelocity.magnitude > 0.1f)
         {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, _currentPushVelocity);
         }
      }
   }
}


