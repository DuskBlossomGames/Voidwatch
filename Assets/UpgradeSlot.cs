using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeSlot : MonoBehaviour
{
    public enum Mode
    {
        Space, Vacant, Occupied, Collision
    };

    public Sprite emptySprite;
    public Color emptyColor;
    public Sprite vacantSprite;
    public Color vacantColor;
    public Sprite occupiedSprite;
    public Color occupiedColor;
    public Sprite collisionSprite;
    public Color collisionColor;

    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetMode(Mode mode) {
        switch (mode){
            case Mode.Space:
                _spriteRenderer.sprite = emptySprite;
                _spriteRenderer.color = emptyColor;
                break;
            case Mode.Vacant:
                _spriteRenderer.sprite = vacantSprite;
                _spriteRenderer.color = vacantColor;
                break;
            case Mode.Occupied:
                _spriteRenderer.sprite = occupiedSprite;
                _spriteRenderer.color = occupiedColor;
                break;
            case Mode.Collision:
                _spriteRenderer.sprite = collisionSprite;
                _spriteRenderer.color = collisionColor;
                break;
        }
    }
}
