using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Util;
using Scriptable_Objects.Upgrades;
using System;

namespace Util
{
    public static class Extensions
    {
        public static string Center(this string self, uint width, char space = ' ')
        {
            long low = (width - self.Length)/2;
            if (low < 0)
            {
                return self;
            }

            long high = width - self.Length - low;
            for (int i = 0; i < low; i++)
            {
                self = space + self;
            }
            for (int i = 0; i < high; i++)
            {
                self = self + space;
            }
            return self;
        }
        public static string Right(this string self, uint width, char space = ' ')
        {
            long low = (width - self.Length);
            if (low < 0)
            {
                return self;
            }

            long high = width - self.Length - low;
            for (int i = 0; i < low; i++)
            {
                self = space + self;
            }
            return self;
        }
    }
}
public class TextTable : MonoBehaviour
{
    public int rows;
    public int cols;

    private int _charWidth;
    private int _charHeight;
    private int _hScroll;

    private Text _text;
    private RectTransform _rTransform;
    
    private string[,] _grid;
    private uint[] _cWidths;
    private uint _curPos;
    private BaseWeapon?[] _equippedWeapons;

    public List<BaseUpgrade> upgrades;
    public List<BaseComponent> components;
    public bool isWeapons;

    public enum Select
    {
        None,
        Current,
        Selected,
    }
    public enum CorU
    {
        Component, Upgrade,
    }
    public CorU coru;

    public Select selectState;

    void UpdateText()
    {

        string nstr = "";
        nstr += "+-";
        for (int i = 0; i < _cWidths[0]; i++){nstr += '-';}
        nstr += "-+------+-----+\n";

        string label = coru == CorU.Component ? "Components" : "Upgrades";
        nstr += "| " + label.Center(_cWidths[0]) +" | Mass |     |\n";
        
        nstr += "+-";
        for (int i = 0; i < _cWidths[0]; i++) { nstr += '-'; }
        nstr += "-+------+-----+\n";
        nstr += "| ";
        nstr += "<>".Center(_cWidths[0]);
        nstr += " |      |     |\n";

        if (coru == CorU.Upgrade)
        {
            for (int i = 0; i < upgrades.Count; i++)
            {
                var upgrade = upgrades[i];
                string name = upgrade.name;
                name = (name.Length <= _cWidths[0] - 3)
                    ? name.Center(_cWidths[0] - 3)
                    : name.Substring(0, (int)_cWidths[0] - 6) + "...";
                string ind = (i == _curPos)
                    ? selectState switch { Select.None => "   ", Select.Selected => ">> ", Select.Current => ">  ", _ => "?? " }
                    : "   ";
                nstr += "| " + ind + name + " | " + upgrade.weight.ToString().Right(_cWidths[1]) + " |     |\n";
            }
        } else if (coru == CorU.Component)
        {
            for (int i = 0; i < components.Count; i++)
            {
                var component = components[i];
                string name = component.name;
                name = (name.Length <= _cWidths[0] - 3)
                    ? name.Center(_cWidths[0] - 3)
                    : name.Substring(0, (int)_cWidths[0] - 6) + "...";
                string ind = (i == _curPos)
                    ? selectState switch { Select.None => "   ", Select.Selected => ">> ", Select.Current => ">  ", _ => "?? " }
                    : "   ";
                string occ = isWeapons ? ( component.weaponID == null ? "   " : "("+component.weaponID+")" ) : "(*)"; 
                nstr += "| " + ind + name + " | " + component.compWeight.ToString().Right(_cWidths[1]) + " | "+ occ +" |\n";
            }
        }

        nstr += "+-";
        for (int i = 0; i < _cWidths[0]; i++) { nstr += '-'; }
        nstr += "-+------+-----+\n";
        nstr += "".Center((uint)_charHeight, '\n'); //hacky way of putting _charHeight newlines

        var rstr = nstr.Split("\n");
        _text.text = "";
        for (int i = _hScroll; i < _hScroll + _charHeight; i++)
        {
            _text.text += rstr[i]+"\n";
        }
    }

    private void Start()
    {
        _equippedWeapons = new BaseWeapon?[3];
        _text = GetComponent<Text>();
        _rTransform = GetComponent<RectTransform>();

        _grid = new string[cols, rows];
        _cWidths = new uint[3];
        _hScroll = 0;
        _curPos = 0;

        int fontSize = GetComponent<TextScalar>().CurrFontSize();

        _charWidth = Mathf.FloorToInt(_rTransform.rect.width / (fontSize / 2.5f));
        _charHeight = Mathf.FloorToInt(_rTransform.rect.height /fontSize);

        _cWidths[0] = (uint) _charWidth - 17;
        _cWidths[1] = 4;
        _cWidths[2] = 3;
        
    }
    private void Update()
    {
        if(selectState == Select.Current)
        {
            if (Input.GetKeyDown("w") || Input.GetKeyDown(KeyCode.UpArrow))
            {
                _curPos = _curPos==0 ? 0 : _curPos - 1;
            } else if (Input.GetKeyDown("s") || Input.GetKeyDown(KeyCode.DownArrow))
            {
                int maxrow = coru == CorU.Component ? components.Count : upgrades.Count;
                _curPos = _curPos == maxrow - 1 ? _curPos : _curPos + 1;
            }

            if(coru == CorU.Component && isWeapons)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (Input.GetKeyDown( (i+1).ToString() ))
                    {
                        int? oldInd = components[(int)_curPos].weaponID;
                        if (oldInd != null) { _equippedWeapons[(int)oldInd-1] = null; }
                        if (_equippedWeapons[i] != null)
                        {
                            _equippedWeapons[i].weaponID = null;
                        }
                        _equippedWeapons[i] = (BaseWeapon)components[(int)_curPos];
                        components[(int)_curPos].weaponID = i+1;
                    }
                }
                if (Input.GetKeyDown("0"))
                {
                    int? oldInd = components[(int)_curPos].weaponID;
                    if (oldInd != null) { _equippedWeapons[(int)oldInd - 1] = null; }
                    components[(int)_curPos].weaponID = null;
                }

            }
        }
        UpdateText();
    }
}


/* Old stuff
 * [System.Flags]
    enum WallMap : byte //Order is NESW
    {
        None = 0,
        N = 1,
        E = 2,
        S = 4,
        W = 8,
        All = 15,

    }

    struct cell
    {
        public WallMap walls;
        public string content;
    }
 * cell[,] verti = new cell[cols+1, rows+1]; //Theres 1 more vertex along each row and column 
        string nstr = "";
        for (int row = 0; row < rows; row++)
        {
            string rstr = "";
            for (int col = 0; col < cols; col++)
            {
                / *
                 * The following will set weather or not each vertex goes in the given direction
                 * Consider the case of:
                 *   | 
                 * --+..
                 *   |
                 * Here the bottom right cell doesnt go up, so the vertex doesnt go right.
                 * The rest of the cells however have edges that make the vertex go N S & W
                 * /
if ((_grid[col, row].walls & WallMap.N) != WallMap.None) { _grid[col, row].walls |= WallMap.E; _grid[col + 1, row].walls |= WallMap.W; }
if ((_grid[col, row].walls & WallMap.E) != WallMap.None) { _grid[col + 1, row].walls |= WallMap.S; _grid[col + 1, row + 1].walls |= WallMap.N; }
if ((_grid[col, row].walls & WallMap.S) != WallMap.None) { _grid[col, row + 1].walls |= WallMap.E; _grid[col + 1, row + 1].walls |= WallMap.W; }
if ((_grid[col, row].walls & WallMap.W) != WallMap.None) { _grid[col, row].walls |= WallMap.S; _grid[col, row + 1].walls |= WallMap.N; }

string tL = verti[col, row].walls switch
{
    WallMap.None => " ",
    WallMap.N => "|",
    WallMap.N | WallMap.S => "|",
    WallMap.S => "|",
    WallMap.W => "-",
    WallMap.W | WallMap.E => "-",
    WallMap.E => "-",
    _ => "+", //catch all funky corner
};
rstr += tL;
            }
        }

*/