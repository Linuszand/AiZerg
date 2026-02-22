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
    [SerializeField] private Slider _dampeningSlider;
    
    [Header("Prefab References")]
    [SerializeField] private GameObject _unitPrefab;
    
    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI _fpsText;
    
    private float _timer;
    private float avgFrameRate;

    void Awake()
    {
        Application.targetFrameRate = 60;
    }
    
    void Start()
    {
        _createUnit.onClick.AddListener(InstantiateUnit);
        _radiusSlider.onValueChanged.AddListener(ChangeSeparationRadius);
        _forceSlider.onValueChanged.AddListener(ChangeSeparationForce);
        _arrivalThresholdSlider.onValueChanged.AddListener(ChangeArrivalThreshold);
        _dampeningSlider.onValueChanged.AddListener(ChangeDampening);
        SelectionManager.OnUnitSelected += UpdateSliders;
    }
    

    public void Update()
    {
        // Update the counter every 0.5 seconds to make it readable
        _timer += Time.unscaledDeltaTime;
        if (_timer > 0.5f)
        {
            // This calculates the FPS based on the last frame's duration
            avgFrameRate = 1f / Time.unscaledDeltaTime;
            _fpsText.text = ((int)avgFrameRate).ToString();
            _timer = 0;
        }
    }
    
    void OnDestroy()
    {
        SelectionManager.OnUnitSelected -= UpdateSliders;
    }

    void InstantiateUnit()
    {
        Vector3 spawnPos = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
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
    
    void ChangeDampening(float newDamp)
    {
        HashSet<SelectableUnit> targets = SelectionManager.Instance.SelectedUnits;

        if (targets.Count > 0)
        {
            foreach (SelectableUnit unit in targets)
            {
                unit.Damping = newDamp;
            }
            
            SelectionManager.NotifyFormationChanged();
            
            Debug.Log($"amount of selected units: {targets.Count}");
        }
    }
    
    public void UpdateSliders(float currentRadius, float currentForce, float currentArrivalThreshold, float currentDampening)
    {
        _radiusSlider.SetValueWithoutNotify(currentRadius);
        _forceSlider.SetValueWithoutNotify(currentForce);
        _arrivalThresholdSlider.SetValueWithoutNotify(currentArrivalThreshold);
        _dampeningSlider.SetValueWithoutNotify(currentDampening);
    }
}
