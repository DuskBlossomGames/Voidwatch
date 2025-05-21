using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public class NewMapGen : MonoBehaviour
{
    public int lowerClusterCount;
    public int upperClusterCount;
    public int lowerPlanetsPerCluster;
    public int upperPlanetsPerCluster;

    List<Cluster> GenerateMap()
    {
        var clusters = List<Cluster>();
        var numClusters = Random.Range(lowerClusterCount, upperClusterCount);
        for (int cID = 0; cID < numClusters; cID++)
        {
            var cluster = new Cluster();
            cluster.id = cID;
            //do { cluster.clusterPos = Random.}
            var numPlanets = Random.Range(lowerPlanetsPerCluster, upperPlanetsPerCluster);

        }
    }

    List<Triangle> RawTriangles(List<Vector2> points)
    {
        //Assume len of list > 3;
        var tris = new List<Triangle>();
        var edges = new List<Vector2Int>();
        tris.Add(new Triangle(0, 1, 2));
        edges.Add(new Vector2Int(0, 1));
        edges.Add(new Vector2Int(1, 2));
        edges.Add(new Vector2Int(2, 0));

        for (int i = 3; i < points.Count; i++)
        {
            int p0 = -1;
            for (int j = 0; j < i; j++)
            {
                foreach (var edge in edges)
                {
                    var k = edge.x;
                    var l = edge.y;
                    var didInter = DoesIntersect(points[i], points[j], points[k], points[l]);
                    if (!didInter) continue;
                    p0 = j;
                    goto skip;
                }
                continue;
            skip:
                break;
            }

            int p1 = -1;
            for (int j = p0; j < i; j++)
            {
                foreach (var edge in edges)
                {
                    var k = edge.x;
                    var l = edge.y;
                    var didInter = DoesIntersect(points[i], points[j], points[k], points[l]);
                    if (!didInter) continue;
                    p1 = j;
                    goto skip;
                }
                continue;
            skip:
                break;
            }

            tris.Add(new Triangle(i, p0, p1));
            edges.Add(new Vector2Int(p0, i));
            edges.Add(new Vector2Int(p1, i));
        }
        return tris;
    }

    Vector2 Circumcenter(Vector2 a, Vector2 b, Vector2 c)
    {
        var d1 = b - a;
        var d2 = c - a;
        var ab = (a + b) / 2;
        var ac = (a + c) / 2;
        var m1 = -d1.x / d1.y; //Could crash here, could fix by offsetting it by a tiny amt
        var m2 = -d2.x / d2.y;
        var b1 = m1 * -ab.x + ab.y;
        var b2 = m2 * -ac.x + ac.y;
        return Intersect(m1, b1, m2, b2);
    }

    Vector2 Intersect(float m1, float b1, float m2, float b2)
    {
        var x = (b2 - b1) / (m1 - m2);
        var y = m1 * x + b1;
        return new Vector2(x, y);
    }

    bool DoesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        var da = a2 - a1;
        var db = b2 - b1;
        var m1 = da.y / da.x;
        var m2 = db.y / db.x;
        var _b1 = m1 * -a1.x + a1.y;
        var _b2 = m2 * -b1.x + b1.y;
        var inter = Intersect(m1, _b1, m2, _b2);
        var ain = (inter - a1).magnitude + (inter - a2).magnitude;
        var bin = (inter - b1).magnitude + (inter - b2).magnitude;
        return ain < .99f * da.magnitude || bin < .99f * db.magnitude;
    }

    public float RandNormal(float mean = 0, float stdDev = 0)
    {
        var u1 = 1.0f - Random.Range(0f, 1f); //uniform(0,1] random doubles
        var u2 = 1.0f - Random.Range(0f, 1f);
        var randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                     Mathf.Sin(2.0f * Mathf.PI * u2); //random normal(0,1)
        var randNormal =
                     mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
        return randNormal;
    }
}

class Triangle
{
    public int[] vertices = new int[3];
    public Triangle(int a, int b, int c)
    {
        this.vertices[0] = a;
        this.vertices[1] = b;
        this.vertices[2] = c;
    }
}

public class Cluster
{
    public int id;
    public Vector2 clusterPos;
    public List<Planet> planets;
    public List<int> connections;
}

public class Planet
{
    public int clusterID;
    public int planetID;
    public Vector2 inClusterPos;
    public List<int> connections;
}
*/
