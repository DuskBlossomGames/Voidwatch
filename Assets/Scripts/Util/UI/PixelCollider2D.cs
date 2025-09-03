using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Util.UI
{
    [RequireComponent(typeof(PolygonCollider2D), typeof(SpriteRenderer))]
    public class PixelCollider2D : MonoBehaviour
    {
        private Texture2D _texture;
        private void Reset()
        {
            _texture = GetComponent<SpriteRenderer>().sprite.texture;
            
            ParseTexture();

            // don't need this script after it sets up the collider
            StartCoroutine(Destroy());
        }

        private IEnumerator Destroy()
        {
            yield return new WaitForSeconds(0.1f);
            DestroyImmediate(this);
        }
        
        private bool Colored(Vector2 px) => px is { x: >= 0, y: >= 0 } // check bounds
                                            && px.x < _texture.width && px.y < _texture.height // check bounds
                                            && _texture.GetPixel((int)px.x, _texture.height - 1 - (int)px.y).a > 0; // we know background is transparent

        private void ParseTexture()
        {
            for (var y = 0; y < _texture.height; y++)
            {
                for (var x = 0; x < _texture.width; x++)
                {
                    var pos = new Vector2(x, y);
                    if (!Colored(pos) // we only care about edges, so gotta be colored
                        || Dirs.All(d=>Colored(pos+d)) // if it's "land-locked", it's not an edge 
                        || _edgePixels.Any(e=>e.Contains(pos))) continue; // if it's on an already found edge, disregard

                    // this big block tries to remove any internal edges
                    // isn't perfect, but should catch most cases (and I don't feel like perfecting it because it shouldn't ever even trigger)
                    var inObj = false;
                    foreach (var edge in _edgePixels)
                    {
                        var up = 0;
                        var down = 0;
                        var left = 0;
                        var right = 0;

                        bool wasUp, wasDown, wasRight, wasLeft;
                        wasUp = wasDown = wasRight = wasLeft = false;
                        foreach (var loc in edge)
                        {
                            if ((int) loc.x == (int) pos.x)
                            {
                                if (!wasUp & (wasUp=loc.y < pos.y)) up++;
                                if (!wasDown & (wasDown=loc.y > pos.y)) down++;
                            }
                            else
                            {
                                wasUp = wasDown = false;
                            }

                            if ((int)loc.y == (int)pos.y)
                            {
                                if (!wasRight & (wasRight=loc.x > pos.x)) right++;
                                if (!wasLeft & (wasLeft=loc.x < pos.x)) left++;
                            }
                            else
                            {
                                wasRight = wasLeft = false;
                            }
                        }

                        if (up % 2 == 1 || down % 2 == 1 || left % 2 == 1 || right % 2 == 1)
                        {
                            inObj = true;
                            break;
                        }
                    }

                    if (inObj) continue;

                    // we found an edge; run the tracer algorithm
                    FillEdge(pos);
                }
            }

            // fill the paths into the collider
            var col = GetComponent<PolygonCollider2D>();
            col.pathCount = _edges.Count;
            for (var i = 0; i < _edges.Count; i++)
            {
                col.SetPath(i, _edges[i].Select(pos => new Vector2(
                    -0.5f + pos.x/_texture.width,
                    0.5f - pos.y/_texture.height
                )).ToList());
            }
        }

        private static readonly Vector2[] Dirs = { Vector2.right, -Vector2.down, Vector2.left, -Vector2.up };
        
        private readonly List<List<Vector2>> _edges = new(); // list of pixels such that the top left corners in sequence form the desired path
        private readonly List<List<Vector2>> _edgePixels = new(); // list of actual pixels on the edge
        
        // trace an edge clockwise
        private void FillEdge(Vector2 start)
        {
            var edge = new List<Vector2> { start };
            var edgePixels = new List<Vector2>();
            
            // pos + corner together give a 'cursor'; we are currently at the `corner` corner of the `pos` pixel
            var pos = start;
            var corner = 0; // 0 = top left, 1 = top right, 2 = bottom right, 3 = bottom left
            do
            {
                var startIdx = corner;

                // if the counterclockwise direction is colored, we need to reverse course
                //                 we're not actually changing where the cursor is, though,
                //                                   so no need to add anything to `_edges`
                if (Colored(pos + Dirs[UtilFuncs.Mod(corner-1, 4)]))
                {
                    pos += Dirs[UtilFuncs.Mod(corner-1, 4)];
                    edgePixels.Add(pos);
                    corner = UtilFuncs.Mod(corner - 1, 4);
                }
                else
                {
                    // essentially, keep rotating about the current pixel (adding the traced path to `_edges`,
                    //                                until we find a straight path to continue tracing along
                    // at non-corners, this for loop will run once; at corners, it should run 2 times, at knobs it should run three
                    //                                                (if the edge is a single pixel, it will run 4 times and exit)
                    for (var i = 0; i < 4; i++)
                    {
                        var j = (startIdx + i) % 4;
                        // rotate the cursor about `pos` clockwise `corner` times, and assume we're tracing in the `Dirs[j]` direction 
                        edge.Add(pos + Dirs[j] + Dirs[..corner].Aggregate(Vector2.zero, (a,b)=>a+b));
                        // if that direction is colored, then we actually are; increment `pos`, and exit
                        if (Colored(pos + Dirs[j]))
                        {
                            pos += Dirs[j];
                            edgePixels.Add(pos);
                            break;
                        }

                        // otherwise, rotate our viewpoint and try again
                        corner = (corner + 1) % 4;
                    }
                }
            } while (pos != start); // go until we make a loop

            _edges.Add(edge);
            _edgePixels.Add(edgePixels);
        }
    }
}