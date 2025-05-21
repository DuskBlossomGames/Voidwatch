using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FloppyBrain : MonoBehaviour
{

    public int length;
    private Vector3[] _segmentV;
    public float targetDist;
    public float smoothSpeed;
    public float rotSpeed;
    public float segLenthScale;


    public List<Transform> segments;
    [NonSerialized] public List<Vector3> Positions = new();

    public GameObject anchorForward;

    public GameObject anchorBase;

    public AnimationCurve speedMultiplier; // this should have positive slope

    private Transform GetCorresponding(FloppyBrain otherBrain, Transform other)
    {
        var childIndices = new List<int>();
        do
        {
            childIndices.Insert(0, other.GetSiblingIndex());
            other = other.transform.parent;
        } while (other != otherBrain.transform.parent);

        return childIndices.Aggregate(transform.parent, (current, idx) => current.GetChild(idx));
    }

    public void Clone(FloppyBrain other)
    {
        length = other.length;
        _segmentV = other._segmentV;
        targetDist = other.targetDist;
        smoothSpeed = other.smoothSpeed;
        rotSpeed = other.rotSpeed;
        segLenthScale = other.segLenthScale;
        segments = other.segments.Select(t => GetCorresponding(other, t)).ToList();
        Positions = new (other.Positions);
        anchorForward = GetCorresponding(other, other.anchorForward.transform).gameObject;
        anchorBase = GetCorresponding(other, other.anchorBase.transform).gameObject;
        speedMultiplier = other.speedMultiplier;
    }

    public void Ready()
    {
        Positions.Add(Vector3.zero);
        foreach (var t in segments) Positions.Add(t.position);
    }

    private void Update(){
        Positions[0] = anchorBase.transform.position;
        Positions[1] = anchorForward.transform.position;

        for(var i = 2; i<Positions.Count; i++){
            var localSegLength = segments[i-1].GetComponent<FloppySegmentRotation>().segLength;


            var tarDir = (Positions[i-1] - Positions[i-2]).normalized;
            var tarPos = tarDir * (segLenthScale * localSegLength) + Positions[i-1];
            var currDir = (Positions[i] - Positions[i-1]).normalized;
            var snapPos = currDir * (segLenthScale * localSegLength) + Positions[i-1];
            
            Positions[i]= Vector3.Lerp(snapPos, tarPos,smoothSpeed * speedMultiplier.Evaluate(localSegLength));
        }

        for(var i = 0; i<segments.Count; i++){
            segments[i].position = (Positions[i] + Positions[i+1])/2;


            Quaternion tarRot = Quaternion.identity;
            tarRot.eulerAngles = new Vector3(0,0,Mathf.Atan2((Positions[i+1]-Positions[i]).y,(Positions[i+1]-Positions[i]).x)*Mathf.Rad2Deg);

            segments[i].rotation = tarRot;
        }
    }
}
