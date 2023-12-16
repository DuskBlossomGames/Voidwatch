using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeGridGenerator : MonoBehaviour
{
    public int size;
    public string layout;
    public bool inEditMode;

    public GameObject slotPrefab;

    public Color occupiedColor;
    public Color vacantColor;

    private GameObject[,] _gridObjects;
    public UpgradeSlot.Mode[,] modeGrid;
    private List<((int, int), UpgradeSlot.Mode)> _restoreList;

    private void Start()
    {
        _restoreList = new List<((int, int), UpgradeSlot.Mode)>();
        float dp = 2f / (size - 1);
        float x, y;
        string clayout = string.Copy(layout);
        bool cState;

        modeGrid = new UpgradeSlot.Mode[size, size];
        _gridObjects = new GameObject[size, size];

        for (int row = 0; row < size; row++)
        {
            y = 1 - dp * row;
            for (int col = 0; col < size; col++)
            {
                x = dp * col - 1;

                if (clayout[0] == '.')
                {
                    cState = false;
                }
                else if (clayout[0] == 'X')
                {
                    cState = true;

                }
                else
                {
                    cState = false;
                    Debug.LogError("Expected . or X got: " + clayout + " @ row: "+row+" col: "+col);
                }
                clayout = clayout[1..];

                modeGrid[col, row] = cState ? UpgradeSlot.Mode.Vacant : UpgradeSlot.Mode.Space;
                GameObject spawn = Instantiate(slotPrefab, new Vector3(x, y, -1), Quaternion.identity, transform);
                _gridObjects[col, row] = spawn;
                spawn.GetComponent<UpgradeSlot>().Ready();
                spawn.GetComponent<UpgradeSlot>().SetMode(modeGrid[col, row]);
                spawn.name = string.Format("Grid Cell X: {0}, Y: {1}", col, row);
                spawn.transform.localScale = new Vector3(dp, dp, 1);
            }
            if (clayout[0] != ';')
            {
                Debug.LogError("Expected ; got: " + clayout);
            }
            clayout = clayout[1..];
        }
    }

    public Vector3 NearestGridCell(Vector3 pos)
    {
        float dp = 2f / (size - 1);
        Vector3 gridOrig = new Vector3(-1, 1, 0);
        Vector3 gridPos = (pos - gridOrig) / dp;
        Vector3 roundedPos = new Vector3(Mathf.Round(gridPos.x), Mathf.Round(gridPos.y), 0);
        Vector3 returnPos = roundedPos * dp + gridOrig;
        returnPos.z = pos.z;
        return returnPos;
    }
    public Vector2Int ToRoundedGridCoords(Vector3 pos)
    {
        float dp = 2f / (size - 1);
        Vector3 gridOrig = new Vector3(-1, 1, 0);
        Vector3 gridPos = (pos - gridOrig) / dp;
        Vector2Int roundedPos = new Vector2Int(Mathf.RoundToInt(gridPos.x), -Mathf.RoundToInt(gridPos.y));
        return roundedPos;
    }
    public void Reserve(Vector2Int pos, uint width, uint height, bool[,] shape)
    {
        SetShape(UpgradeSlot.Mode.Occupied, pos, width, height, shape);
    }
    public void Free(Vector2Int pos, uint width, uint height, bool[,] shape)
    {
        SetShape(UpgradeSlot.Mode.Vacant, pos, width, height, shape);
    }
    public void SetShape(UpgradeSlot.Mode newMode, Vector2Int pos, uint width, uint height, bool[,] shape)
    {
        for (int dy = 0; dy < height; dy++) 
        {
            for (int dx = 0; dx < width; dx++)
            {
                //bool oldVal = occupationGrid[pos.x + dx, pos.y + dy];
                modeGrid[pos.x + dx, pos.y + dy] = newMode;
                _gridObjects[pos.x + dx, pos.y + dy].GetComponent<UpgradeSlot>().SetMode(newMode);
                
            }
        }
    }

    public bool IsFree(Vector2Int pos, uint width, uint height, bool[,] shape)
    {
        bool isFree = true;
        for (int dy = 0; dy < height; dy++)
        {
            for (int dx = 0; dx < width; dx++)
            {
                if(shape[dx, dy])
                {
                    bool isCellFree = modeGrid[pos.x + dx, pos.y + dy] == UpgradeSlot.Mode.Vacant;
                    isFree &= isCellFree;
                    if (!isCellFree)//Cell is invalid
                    {
                        if(modeGrid[pos.x + dx, pos.y + dy] == UpgradeSlot.Mode.Occupied)
                        {
                            _restoreList.Add( ((pos.x + dx, pos.y + dy), UpgradeSlot.Mode.Occupied) );
                            modeGrid[pos.x + dx, pos.y + dy] = UpgradeSlot.Mode.Collision;
                            _gridObjects[pos.x + dx, pos.y + dy].transform.position -= 2f * Vector3.forward;
                            _gridObjects[pos.x + dx, pos.y + dy].GetComponent<UpgradeSlot>().SetMode(UpgradeSlot.Mode.Collision);
                        }
                    }
                }
                
            }
        }
        return isFree;
    }

    public void Restore()
    {
        foreach (var ((x,y),state) in _restoreList)
        {
            modeGrid[x,y] = state;
            _gridObjects[x, y].GetComponent<UpgradeSlot>().SetMode(state);
            _gridObjects[x, y].transform.position += 2f * Vector3.forward;
        }
        _restoreList.Clear();
    }

    public bool RawIsFree(Vector2Int pos, uint width, uint height, bool[,] shape)
    {
        bool isFree = true;
        for (int dy = 0; dy < height; dy++)
        {
            for (int dx = 0; dx < width; dx++)
            {
                bool isCellFree = modeGrid[pos.x + dx, pos.y + dy] == UpgradeSlot.Mode.Vacant;
                isFree &= !shape[dx, dy] || shape[dx, dy] && isCellFree;
            }
        }
        return isFree;
    }
}
