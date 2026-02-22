using UnityEngine;

// scripot that simply points text text towards the camera so we can see it
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
