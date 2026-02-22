using UnityEngine;
using UnityEngine.AI;

using Vector3 = UnityEngine.Vector3;

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
   public float Damping = 5f;

   [Header("Base Settings")]
   [SerializeField] private float baseSpeed = 4f;
   
   [Header("Update Settings")]
   [SerializeField] private float _scanRate = 0.2f;
   [SerializeField] private float _arrivalCheckRate = 0.2f;
   
   private float _nextScanTime;
   private Vector3 _separationVelocity = Vector3.zero;
   private Rigidbody _rb;
   private UnitAI _unitAI;
   private UnitPatrol _patrol;
   private Collider[] neighborColliders = new Collider[30];
   private float _pathResetTimer = 0.2f;
   private Vector3 lastFrameSeparationForce = Vector3.zero;
   
   [Header("Other Settings")]
   public TravelMode _currentMode;
   public NavMeshAgent _agent;
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

   void Start()
   {
      _currentMode = TravelMode.Parking;
      Damping = 10f;
      LastDestination = _agent.destination;
      ArrivalThreshold = 3f;
      SeparationForce = 7f;
      SeparationRadius = 6f;
   }
   
   void Update()
   {
      // some debug stuff I used to see how many colliders each unit sees
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
   
   public void ParkingSeparation()
   {
      // finds all the units around the unit in its radius. OverlapSphereNonAlloc
      // is good because it reuses the neighborColliders array instead of creating new arrays, and more
      // efficient than TriggerOnStay
      int count = Physics.OverlapSphereNonAlloc(transform.position, SeparationRadius, neighborColliders);
      
      
      Vector3 nudgeOffset = Vector3.zero;
      int neighborsFound = 0;
      
      for (int i = 0; i < count; i++)
      {
         // we need to get the other units in the neighborColliders array
         Collider other = neighborColliders[i];
         // if the other gameobject is ourselves or not gameobject with "Unit" tag
         if (other.gameObject == gameObject || !other.CompareTag("Unit")) continue;
         
         // get the separation vector
         Vector3 pushDirection = transform.position - other.transform.position;
         
         // make sure we don't move on the y axis
         pushDirection.y = 0;
         float distance = pushDirection.magnitude;
         
         // avoid division by zero and want to make sure distance is not higher than our SeparationRadius
         if (distance <= 0.01f || distance >= SeparationRadius) continue;
         
         // stronger force away from unit the closer we are
         float strength = (SeparationRadius - distance) / SeparationRadius;
         
         // how far the unit will be offset from set position
         // normalize the pushDirection because otherwise we would get an insane nudge
         // will essentially move until they are out of each other's radius
         Vector3 escapePoint = other.transform.position + (pushDirection.normalized * SeparationRadius);
         nudgeOffset += escapePoint - transform.position;
         
         
         // how many units are nearby
         neighborsFound++;
      }
      // if there are units nearby
      if (neighborsFound > 0)
      {
         if (nudgeOffset.magnitude > 0.05f)
         {
            // to make sure that units don't go further than separationradius
            Vector3 finalNudge = nudgeOffset / neighborsFound;
            
            // this is the final pos that the unit should walk to
            Vector3 finalPos = transform.position + (finalNudge * SeparationForce);
            
            _agent.SetDestination(finalPos);
            
            // lower speed when it goes to its new destination
            _agent.speed = baseSpeed * 0.5f;
         }
      } // we need to remove the path it had currently for safety's sake so it won't wander off to an earlier point
      else if (_agent.hasPath)
      {
         _agent.ResetPath();
      }
   }

   void CheckForArrival()
   {       
         if (Time.time >= _pathResetTimer && !_agent.pathPending && _agent.hasPath)
         {
            _pathResetTimer = Time.time + _arrivalCheckRate; 
            
            // if the remaninigDistnace is close to the final position + the ArrivalThreshold that we've set
            if (_agent.remainingDistance <= ArrivalThreshold) // example 3 <= 0 + 3 - it will switch state to parking
            {
               SwitchState(TravelMode.Parking);
            }
         }
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
      float distanceToTarget = _agent.remainingDistance;
      
      // to be able to change separationforce depending on distance to destination
      float activeSeparationForce;
      
      // I used smoothness to make sure that when the damping is high, they movements are fluid and soft
      // and when low they are more snappy and quick
      // 1 / 10 = 0.1
      float smoothness = 1.0f / Damping;

      // I wanted to somehow stop jittering when units are close to target.
      if (!_agent.pathPending && _agent.remainingDistance <= ArrivalThreshold)
      {
         _separationVelocity = Vector3.zero;
         return; 
      }
      
      // make sure arrivalthreshold and navmesh stoppdingdistance are synced
      _agent.stoppingDistance = ArrivalThreshold;
      
      // changes totalSeparationForce and _agent variables depending on distance to target
      if (!_agent.pathPending && distanceToTarget <= 4.0f)
      {
         //halves speed
         _agent.speed = baseSpeed * 0.5f;
         //halves force
         activeSeparationForce = _currentMode == TravelMode.Chasing ? SeparationForce * 0.1f : SeparationForce * 0.5f;
      }
      else
      {
         _agent.speed = baseSpeed;
         activeSeparationForce = _currentMode == TravelMode.Chasing ? SeparationForce * 0.1f : SeparationForce * 1f;
      }
      
      if (Time.frameCount % 2 == 0)
      {
         Vector3 currentFrameForce = Vector3.zero;
         int count = Physics.OverlapSphereNonAlloc(transform.position, SeparationRadius, neighborColliders);

         for (int i = 0; i < count; i++)
         {
            Collider other = neighborColliders[i];
            if (other.gameObject == gameObject || !other.CompareTag("Unit")) continue;
         
            Vector3 pushDirection = transform.position - other.transform.position;
            pushDirection.y = 0;

            float distance = pushDirection.magnitude;
            if (distance <= 0.01f || distance >= SeparationRadius) continue;
         
            // pushes harder the further into another units radius we are and softer the further out
            float strength = (SeparationRadius - distance) / SeparationRadius; // Example: ( 2 - 2) / 2 = 0/2 = 0f - zero push Example2: ( 2 - 0.1) / 2 = 1.9 / 2 = 0.95f - strong push
         
            // a force that will push the unit in the opposite direction even harder - dependent on how high the SeparationForce is
            currentFrameForce += pushDirection.normalized * (strength * activeSeparationForce); // 1 * (0.95 * 1 * 1) =  0.95 // 1 * (0.95 * 1 * 0.4) = 0.38
         }
         lastFrameSeparationForce = currentFrameForce;
      }
      
      // clamp to 0.01f to ensure the units don't start floating
      // smoothness = Mathf.Clamp(smoothness, 0.01f, 1.0f);
      
      _separationVelocity = Vector3.Lerp(_separationVelocity, lastFrameSeparationForce, Time.deltaTime * smoothness);
   }
   
   public void SwitchState(TravelMode newMode)
   {
      if (_currentMode == newMode) return;
      _currentMode = newMode;

      if (_currentMode == TravelMode.Parking)
      {
         // need to clear agents path
         _agent.velocity = Vector3.zero;
         _separationVelocity = Vector3.zero;
         
         _agent.speed = baseSpeed * 0.5f; 
      } else if (_currentMode == TravelMode.ReturnToPatrol)
      {
         _agent.speed = baseSpeed * 0.9f;
         
         Vector3 returnPos = isPatrolling ? _patrol.GetActiveWaypoint() : GetLastDestination();
         _agent.SetDestination(returnPos);
      }
   }
   
   private void ApplyVelocity()
   {
      // Get the agents desired velocity
      Vector3 desiredVelocity = _agent.desiredVelocity;
      
      // combine pathfinding with separation forces
      Vector3 finalVelocity = desiredVelocity + _separationVelocity;
      
      // override the agents velocity with our new velocity
      _agent.velocity = finalVelocity;
      
   }
   
   public void MoveTo(Vector3 position)
   {
      // for checking if a unit is patrolling
      isPatrolling = false;
      
      // to know where the unit's last clicked position was
      LastDestination = position;
      
      SwitchState(TravelMode.Traveling);
      _agent.SetDestination(position);
   }

   public void StartPatrol(Vector3 targetPoint)
   {
      isPatrolling = true;

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
   
   private void OnDrawGizmos()
   {
      if (_agent != null)
      {
         Gizmos.color = Color.green;
         Gizmos.DrawWireSphere(transform.position, SeparationRadius);
         
         if (_separationVelocity.magnitude > 0.1f)
         {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, _separationVelocity);
         }
      }
   }
}


