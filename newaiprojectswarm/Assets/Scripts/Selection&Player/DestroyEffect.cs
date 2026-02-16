using UnityEngine;

public class DestroyEffect : MonoBehaviour
{
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private Vector3 startScale = new Vector3(8f, 8f, 8f);
    
    private float _timer;

    void Start()
    {
        transform.localScale = startScale;
        duration = 0.8f;
    }

    void Update()
    {
        _timer += Time.deltaTime;
        float progress = _timer / duration;
        //                                 (8, 8, 8)    (0, 0, 0)       0.5
        transform.localScale = Vector3.Lerp(new Vector3(20, 20, 20), Vector3.zero, progress);

        if (progress >= 1.0f)
        {
            Destroy(gameObject);
        }
    }
}
