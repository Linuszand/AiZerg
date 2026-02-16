using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager
{
    private static SelectionManager _instance;
    
    public static Action<float, float, float> OnUnitSelected;
    
    public static SelectionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SelectionManager();
            }
            return _instance;
        }
        set
        {
            _instance = value;
        }
    }
    
    public HashSet<SelectableUnit> SelectedUnits = new HashSet<SelectableUnit>();
    public List<SelectableUnit> AvailableUnits = new List<SelectableUnit>();
    
    public static Action OnFormationChanged;

    private SelectionManager() {}
    
    public void Select(SelectableUnit unit)
    {
        SelectedUnits.Add(unit);
        unit.OnSelected();
        
        OnUnitSelected?.Invoke(unit.SeparationRadius, unit.SeparationForce, unit.ArrivalThreshold);
        
        NotifyFormationChanged();
    }

    public void Deselect(SelectableUnit unit)
    {
        unit.OnDeselected();
        SelectedUnits.Remove(unit);
    }

    public void DeselectAllUnits()
    {
        foreach (SelectableUnit unit in SelectedUnits)
        {
            unit.OnDeselected();
        }
        SelectedUnits.Clear();
    }

    public bool IsSelected(SelectableUnit unit)
    {
        return SelectedUnits.Contains(unit);
    }
    
    public static void NotifyFormationChanged()
    {
        OnFormationChanged?.Invoke();
    }
    
}
