using UnityEngine;

public class DestroyEffect : MonoBehaviour
{
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private Vector3 startScale = new Vector3(8f, 8f, 8f);
    
    private float _timer;

    void Start()
    {
        transform.localScale = startScale;
    }

    void Update()
    {
        _timer += Time.deltaTime;
        float progress = _timer / duration;
        
        transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);

        if (progress >= 1.0f)
        {
            Destroy(gameObject);
        }
    }
}
