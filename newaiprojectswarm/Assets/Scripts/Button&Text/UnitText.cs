using TMPro;
using UnityEngine;

public class UnitText : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private TextMeshProUGUI _modeText;
    private SelectableUnit _unit;

    void Start()
    {
        _text = transform.Find("Settings").GetComponent<TextMeshProUGUI>();
        _unit = GetComponentInParent<SelectableUnit>();
        _modeText = transform.Find("TravelMode").GetComponent<TextMeshProUGUI>();
        
        SelectionManager.OnFormationChanged += RefreshText;
        
        RefreshText();
    }
    
    void OnDestroy()
    {
        SelectionManager.OnFormationChanged -= RefreshText;
    }

    void Update()
    {
        if (_unit != null)
        {
            UpdateModeDisplay();
        }
    }
    
    void UpdateModeDisplay()
    {
        string modeLabel = $"{_unit._currentMode}";
        
        _modeText.text = $"{modeLabel}";

        switch (_unit._currentMode)
        {
            case TravelMode.Chasing:
                _modeText.color = Color.red;
                break;
            case TravelMode.Traveling:
                _modeText.color = Color.blue;
                break;
            case TravelMode.Parking:
                _modeText.color = Color.green;
                break;
            case TravelMode.Patrol:
                _modeText.color = Color.yellow;
                break;
            case TravelMode.ReturnToPatrol:
                _modeText.color = Color.magenta;
                break;
        }
    }
    
    void RefreshText()
    {
        if (_unit != null && _text != null)
        {
            _text.text = $"SRadius {_unit.SeparationRadius:F2} SForce: {_unit.SeparationForce:F2} Threshold: {_unit.ArrivalThreshold} Damping: {_unit.Damping}";
        }
    }
}
