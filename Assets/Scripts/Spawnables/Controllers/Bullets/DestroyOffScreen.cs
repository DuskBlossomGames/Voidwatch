using UnityEngine;

public class DestroyOffScreen : MonoBehaviour
{

    public float playRadius;

    private bool _inPlayArea;
    private Renderer _renderer;

    private void Start()
    {   
        _renderer = GetComponent<Renderer>();
    }
    private void Update()
    {
        _inPlayArea = ((Vector2)transform.position).sqrMagnitude < playRadius * playRadius;
        if (!_inPlayArea && !_renderer.isVisible) Destroy(gameObject);
    }
}
