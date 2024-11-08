using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateUniformFloppy : MonoBehaviour
{
    public int length;
    public bool degredation;
    public float degredationMagnitude;
    public float masterRigidity;
    public bool flippedY;
    public FloppyBrain brain;

    public float rigidityDegredation;

    public GameObject segmentPrefab;

    public GameObject anchor;

    // Start is called before the first frame update
    void Start()
    {
      brain.segments.Add(anchor.transform);
      brain.segments.Add(this.gameObject.transform);

      GameObject child,oldChild;
      oldChild = this.gameObject;

      Vector3 relPos = Vector3.zero;
      Vector3 degredationScale = new Vector3 (degredationMagnitude,degredationMagnitude,0);
      float rigidity = masterRigidity;
      var segLength = segmentPrefab.GetComponent<FloppySegmentRotation>().segLength;

      for(int i = 0; i < length - 1; i++){

        child = Instantiate(segmentPrefab, transform);
        child.transform.localRotation = Quaternion.Euler(0, 0, 0);
        child.transform.localPosition = relPos;
        child.GetComponent<FloppySegmentRotation>().rigidity = rigidity;
        child.GetComponent<FloppySegmentRotation>().target = oldChild.transform;
        child.GetComponent<FloppySegmentRotation>().segLength = segLength;
        if(flippedY){
          child.GetComponent<SpriteRenderer>().flipY = true;
        }
        if(degredation){
          child.transform.localScale -= degredationScale;
          degredationScale += new Vector3 (degredationMagnitude,degredationMagnitude,0);
          rigidity -=rigidityDegredation;
          segLength -= degredationMagnitude;
        }
        brain.segments.Add(child.transform);


        oldChild = child;
        relPos.x += segLength;
      }

      brain.Ready();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
