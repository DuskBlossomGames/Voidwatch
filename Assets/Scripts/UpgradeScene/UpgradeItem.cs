using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeItem : MonoBehaviour
{
    public uint width;
    public uint height;
    public string shape;
    public UpgradeGridGenerator gridGenerator;
    public Vector2 targetPos;

    private bool[,] _shape;//[x,y] form
    private bool _isDragging;
    private Vector2 _grabPos;
    private bool _hasMouse = false;
    private Camera _mainCamera;
    private float _slotsize;
    private Vector2Int _gridPos;
    private bool _isValid;
    private bool _isPlanted;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _isPlanted = false;
        _mainCamera = Camera.main;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _slotsize = 2f / (gridGenerator.size - 1);
        transform.localScale = new Vector3(_slotsize, _slotsize, 1);

        string cshape = string.Copy(shape);

        _shape = new bool[width, height];

        for (int col = 0; col < width; col++)
        {
            for (int row = 0; row < height; row++)
            {
                if (cshape[0] == '.')
                {
                    _shape[row, col] = false;
                }
                else if (cshape[0] == 'X')
                {
                    _shape[row, col] = true;

                }
                else
                {
                    Debug.LogError("Expected . or X got: " + cshape + " @ row: " + row + " col: " + col);
                }
                cshape = cshape[1..];
            }
            if (cshape[0] != ';')
            {
                Debug.LogError("Expected ; got: " + cshape);
            }
            cshape = cshape[1..];
        }
    }

    private void Update()
    {
        Vector2 globalMousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 spriteMousePos = (globalMousePos - (Vector2)transform.position) / _slotsize;

        int spriteXPos = Mathf.FloorToInt(width * (spriteMousePos.x + 1) / 2);//convert to % of width then multiply by width and round to find cell
        int spriteYPos = Mathf.FloorToInt(height * (spriteMousePos.y + 1) / 2);

        _hasMouse = false; 
        if (0 <= spriteXPos && spriteXPos < width && 0 <= spriteYPos && spriteYPos < height)//is in the bounding box
        {
            _hasMouse = _shape[spriteXPos, spriteYPos];
        }



        if (!_isDragging)
        {
            if (_hasMouse && Input.GetMouseButtonDown(0))
            {
                _isDragging = true;
                if (_isPlanted)
                {
                    _isPlanted = false;
                    transform.position -= .5f * Vector3.forward;
                    gridGenerator.Free(_gridPos, width, height, _shape);
                }
                _grabPos = (Vector2)transform.position - globalMousePos;
            }
            else if (!_isPlanted)
            {
                _spriteRenderer.color = new Color(255, 255, 255);
                gridGenerator.Restore();
                Vector2 temp = UtilFuncs.LerpSafe((Vector2)transform.position, targetPos, 10 * Time.deltaTime);
                transform.position = new Vector3(temp.x, temp.y, transform.position.z);
            }
        }
        else
        {
            if (!Input.GetMouseButton(0))
            {
                _isDragging = false;
                if (_isValid)
                {
                    _isPlanted = true;
                    transform.position += .5f * Vector3.forward;
                    gridGenerator.Reserve(_gridPos, width, height, _shape);
                }
            }

            //_isValid = false;
            Vector3 tempPos = _grabPos + globalMousePos;
            tempPos.z = transform.position.z;
            transform.position = tempPos;

            if (-1 <= transform.position.x && transform.position.x <= 1 && -1 <= transform.position.y && transform.position.y <= 1)
            {
                Vector3 offsetTL = new Vector3(-((width / 2f - .5f) * _slotsize), ((height / 2f - .5f) * _slotsize), 0);
                transform.position = gridGenerator.NearestGridCell(transform.position + offsetTL) - offsetTL;
                Vector2Int ngridPos = gridGenerator.ToRoundedGridCoords(transform.position + offsetTL);
                if (ngridPos != _gridPos)//Moved to new grid cell, check for validity
                {
                    gridGenerator.Restore();
                    bool isValid = false;
                    _gridPos = ngridPos;

                    if (0 <= _gridPos.x && _gridPos.x + width < gridGenerator.size && 0 <= _gridPos.y && _gridPos.y + height < gridGenerator.size)
                    {

                        isValid = gridGenerator.IsFree(_gridPos, width, height, _shape);
                        
                        if (isValid)
                        {
                            _spriteRenderer.color = new Color(255, 255, 255);
                        }
                        else
                        {
                            _spriteRenderer.color = new Color(255, 0, 0);
                        }
                    }

                    _isValid = isValid;
                }
            }
            else
            {
                _isValid = false;
            }
        }
    }
}
