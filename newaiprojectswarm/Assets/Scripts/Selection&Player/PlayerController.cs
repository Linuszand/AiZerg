using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private RectTransform SelectionBox;
    [SerializeField] private LayerMask _unitLayers;
    [SerializeField] private LayerMask _floorLayers;
    [SerializeField] private GameObject clickEffectPrefab;
    [SerializeField] private TextMeshProUGUI _statusText;
    
    
    private Vector2 StartMousePosition;
    private bool _isDrawingSelection;
    private bool _isWaitingForPatrolClick = false;
    
    private void Update()
    {
        HandleSelectionInputs();
        HandleMovementInputs();
    }
    
    public void SetPatrolMode()
    {
        _isWaitingForPatrolClick = !_isWaitingForPatrolClick;

        if (_isWaitingForPatrolClick)
        {
            UpdateStatusText("Ready to patrol");
        }
        else
        {
            UpdateStatusText("Not ready for patrol");
        }
    }
    
    private void UpdateStatusText(string message)
    {
        if (_statusText != null)
        {
            _statusText.text = message;
        }
    }

    private void HandleMovementInputs()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (Physics.Raycast(
                    _camera.ScreenPointToRay((Mouse.current.position.ReadValue())),
                    out RaycastHit hit, Mathf.Infinity, _floorLayers))
            {
                if (SelectionManager.Instance.SelectedUnits.Count > 0)
                {
                    Vector3 effectPos = hit.point + new Vector3(0, 0.1f, 0);
                    Instantiate(clickEffectPrefab, effectPos, Quaternion.Euler(90, 0, 0));
                }
                
                if (_isWaitingForPatrolClick)
                {
                    foreach (SelectableUnit unit in SelectionManager.Instance.SelectedUnits)
                    {
                        unit.StartPatrol(hit.point);
                    }
                    _isWaitingForPatrolClick = false;
                    UpdateStatusText("Patrol started!");
                }
                else
                {
                    foreach (SelectableUnit unit in SelectionManager.Instance.SelectedUnits)
                    {
                        unit.MoveTo(hit.point);
                    }
                }
                
            }
        }
    }

    private void HandleSelectionInputs()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                _isDrawingSelection = false; 
                return; 
            }
            
            _isDrawingSelection = true;
            SelectionBox.sizeDelta = Vector2.zero;
            SelectionBox.gameObject.SetActive(true);
            StartMousePosition = Mouse.current.position.ReadValue();
        } 
        
        else if (Mouse.current.leftButton.isPressed && _isDrawingSelection)
        {
            ResizeSelectionBox();
        } 
    
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            _isDrawingSelection = false;
            SelectionBox.sizeDelta = Vector2.zero;
            SelectionBox.gameObject.SetActive(false);
        }
    }

    private void ResizeSelectionBox()
    {
        float width = Mouse.current.position.ReadValue().x - StartMousePosition.x;
        float height = Mouse.current.position.ReadValue().y - StartMousePosition.y;
        
        SelectionBox.anchoredPosition = StartMousePosition + new Vector2(width / 2, height / 2);
        SelectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        
        Bounds bounds = new Bounds(SelectionBox.anchoredPosition, SelectionBox.sizeDelta);
        for (int i = 0; i < SelectionManager.Instance.AvailableUnits.Count; i++)
        {
            if (UnitIsInSelectionBox(
                    _camera.WorldToScreenPoint(SelectionManager.Instance.AvailableUnits[i].transform.position),
                    bounds))
            {
                SelectionManager.Instance.Select(SelectionManager.Instance.AvailableUnits[i]);
            }
            else
            {
                SelectionManager.Instance.Deselect(SelectionManager.Instance.AvailableUnits[i]);
            }
        }
    }

    private bool UnitIsInSelectionBox(Vector2 position, Bounds bounds)
    {
        return position.x > bounds.min.x && position.x < bounds.max.x && position.y > bounds.min.y && position.y < bounds.max.y;
    }
}
