using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SelectableUnit : MonoBehaviour
{
   private NavMeshAgent _agent;
   [SerializeField] private SpriteRenderer _spriteSelection;


   
   private void Awake()
   {
      SelectionManager.Instance.AvailableUnits.Add(this);
      _agent = GetComponent<NavMeshAgent>();
   }
   

   public void MoveTo(Vector3 position)
   {
      _agent.SetDestination(position);
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
}
