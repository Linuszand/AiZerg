using UnityEngine;

public class FixText : MonoBehaviour
{
    private Transform _cameraTransform;

    void Start()
    {
        _cameraTransform = Camera.main.transform;
    }

    
    void LateUpdate()
    {
        transform.LookAt(transform.position + _cameraTransform.forward);
    }
}
