using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FormationButtons : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private Button _createUnit;
    
    [Header("Slider References")]
    [SerializeField] private Slider _radiusSlider;
    [SerializeField] private Slider _forceSlider;
    [SerializeField] private Slider _arrivalThresholdSlider;
    
    [Header("Prefab References")]
    [SerializeField] private GameObject _unitPrefab;
    
    void Start()
    {
        _createUnit.onClick.AddListener(InstantiateUnit);
        _radiusSlider.onValueChanged.AddListener(ChangeSeparationRadius);
        _forceSlider.onValueChanged.AddListener(ChangeSeparationForce);
        _arrivalThresholdSlider.onValueChanged.AddListener(ChangeArrivalThreshold);
        SelectionManager.OnUnitSelected += UpdateSliders;
    }
    
    void OnDestroy()
    {
        SelectionManager.OnUnitSelected -= UpdateSliders;
    }

    void InstantiateUnit()
    {
        Vector3 spawnPos = new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));
        Instantiate(_unitPrefab, spawnPos, Quaternion.identity);
    }

    void ChangeSeparationForce(float newForce)
    {
        HashSet<SelectableUnit> targets = SelectionManager.Instance.SelectedUnits;

        if (targets.Count > 0)
        {
            foreach (SelectableUnit unit in targets)
            {
                unit.SeparationForce = newForce;
            }
            SelectionManager.NotifyFormationChanged();
            Debug.Log($"amount of selected units: {targets.Count}");
        }
    }
    
    void ChangeArrivalThreshold(float newThreshold)
    {
        HashSet<SelectableUnit> targets = SelectionManager.Instance.SelectedUnits;

        if (targets.Count > 0)
        {
            foreach (SelectableUnit unit in targets)
            {
                unit.ArrivalThreshold = newThreshold;
            }
            
            SelectionManager.NotifyFormationChanged();
            
            Debug.Log($"amount of selected units: {targets.Count}");
        }
    }
    
    
    void ChangeSeparationRadius(float newRadius)
    {
        HashSet<SelectableUnit> targets = SelectionManager.Instance.SelectedUnits;

        if (targets.Count > 0)
        {
            foreach (SelectableUnit unit in targets)
            {
                unit.SeparationRadius = newRadius;
            }
            
            SelectionManager.NotifyFormationChanged();
            
            Debug.Log($"amount of selected units: {targets.Count}");
        }
    }
    
    public void UpdateSliders(float currentRadius, float currentForce, float currentArrivalThreshold)
    {
        _radiusSlider.SetValueWithoutNotify(currentRadius);
        _forceSlider.SetValueWithoutNotify(currentForce);
        _arrivalThresholdSlider.SetValueWithoutNotify(currentArrivalThreshold);
    }
}
