using UnityEngine;

public class DestroyOffScreen : MonoBehaviour
{

    public GameObject playArea;

    private bool _inPlayArea;
    private Renderer _renderer;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == playArea) _inPlayArea = false;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == playArea) _inPlayArea = true;
    }

    private void Update()
    {
        if (!_inPlayArea && !_renderer.isVisible) Destroy(gameObject);
    }
}
