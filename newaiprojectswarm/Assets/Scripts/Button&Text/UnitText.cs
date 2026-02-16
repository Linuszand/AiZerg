using TMPro;
using UnityEngine;

public class UnitText : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private SelectableUnit _unit;

    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _unit = GetComponentInParent<SelectableUnit>();
        
        SelectionManager.OnFormationChanged += RefreshText;
        
        RefreshText();
    }
    
    private void OnDestroy()
    {
        SelectionManager.OnFormationChanged -= RefreshText;
    }
    
    private void RefreshText()
    {
        if (_unit != null && _text != null)
        {
            _text.text = $"SRadius {_unit.SeparationRadius:F2} SForce: {_unit.SeparationForce:F2} Threshold: {_unit.ArrivalThreshold}";
        }
    }
}
