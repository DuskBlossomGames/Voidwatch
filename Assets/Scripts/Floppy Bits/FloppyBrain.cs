using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FloppyBrain : MonoBehaviour
{

  public int length;
  private Vector3[] _segmentV;
  public float targetDist;
  public float smoothSpeed;
  public float rotSpeed;
  //public float wiggleSpeed;
  //public float wiggleMagnitude;
  //public Transform wiggleDir;
  public float segLenthScale;


  public List<Transform> segments;
  List<Vector3> positions = new List<Vector3>();

  public GameObject anchorForward;

  public GameObject anchorBase;

[SerializeField] public AnimationCurve speedMultiplier; // this should have positive slope

    // Start is called before the first frame update
    public void Ready()
    {
      //_segmentV = new Vector3[length];

      positions.Add(Vector3.zero);
      for(int i = 0;i < segments.Count;i++){
        positions.Add(segments[i].position);
      }



    }

    void Update(){
      positions[0] = anchorBase.transform.position;
      positions[1] = anchorForward.transform.position;

      for(int i = 2; i<positions.Count; i++){

        if(segments[i-1] == null){
          print("aofhja;sjhdf" + i);
        }
        float localSegLength = segments[i-1].GetComponent<FloppySegmentRotation>().segLength;


        Vector3 tarDir = (positions[i-1] - positions[i-2]).normalized;
        Vector3 tarPos = tarDir * segLenthScale* localSegLength + positions[i-1];
        Vector3 currDir = (positions[i] - positions[i-1]).normalized;
        Vector3 snapPos = currDir * segLenthScale* localSegLength + positions[i-1];
        //positions[i] = Vector3.SmoothDamp(snapPos,tarPos,ref _segmentV[i],smoothSpeed * speedMultiplier.Evaluate(localSegLength));
        positions[i]= Vector3.Lerp(snapPos,tarPos,smoothSpeed * speedMultiplier.Evaluate(localSegLength));
        //positions[i]= snapPos;
      }

      for(int i = 0; i<segments.Count;i++){
          segments[i].position = (positions[i] + positions[i+1])/2;


          Quaternion tarRot = Quaternion.identity;
          tarRot.eulerAngles = new Vector3(0,0,Mathf.Atan2((positions[i+1]-positions[i]).y,(positions[i+1]-positions[i]).x)*Mathf.Rad2Deg);

          //segments[i].rotation = Quaternion.Slerp(segments[i].rotation,tarRot,rotSpeed);
          segments[i].rotation = tarRot;
      }


    }




    /*void Update(){

      float singleStep = flopSpeed * Time.deltaTime;

      Vector2 brainPos = this.gameObject.transform.position;
      Quaternion brainRot = this.gameObject.transform.rotation; //on skibidi fr fr
      Vector2 targetPos = brainPos + (-brainPos + (Vector2)segments[0].position).normalized * targetDist *segments[0].gameObject.GetComponent<FloppySegmentRotation>().segLength;
      segments[0].position = Vector3.forward * segments[0].position.z + (Vector3)Vector2.SmoothDamp((Vector2)segments[0].position,targetPos,ref _segmentV[0],smoothSpeed);
      Vector3.RotateTowards(segments[0].position, Quaternion.AngleAxis(-baseRigidity*segments[0].gameObject.GetComponent<FloppySegmentRotation>().rigidity,Vector3.forward) * -brainPos,singleStep,0.0f);

      //brainRot.eulerAngles -= Quaternion.Euler(0,0,segments[0].gameObject.GetComponent<FloppySegmentRotation>().rigidity * baseRigidity).eulerAngles;
      brainRot.eulerAngles += new Vector3(0,0,baseRigidity * segments[0].gameObject.GetComponent<FloppySegmentRotation>().rigidity);
      //Quaternion.RotateTowards(segments[0].rotation,brainRot,singleStep);
      segments[0].rotation = Quaternion.Slerp(segments[0].rotation,brainRot,rotSpeed);
      //segments[0].rotation = brainRot;

      for( int i = 1; i<segments.Count;i++){
        targetPos = (Vector2)segments[i-1].position + (-(Vector2)segments[i-1].position + (Vector2)segments[i].position).normalized * targetDist*segments[i].gameObject.GetComponent<FloppySegmentRotation>().segLength;
        segments[i].position = Vector3.forward * segments[0].position.z + (Vector3)Vector2.SmoothDamp((Vector2)segments[i].position,targetPos,ref _segmentV[i],smoothSpeed);

        Vector3.RotateTowards(segments[i].position, Quaternion.AngleAxis(-baseRigidity*segments[i].gameObject.GetComponent<FloppySegmentRotation>().rigidity,Vector3.forward) * -segments[i-1].position,singleStep,0.0f);

        Quaternion modRotation = segments[i-1].rotation;
        //modRotation.eulerAngles -= Quaternion.Euler(0,0,segments[i].gameObject.GetComponent<FloppySegmentRotation>().rigidity * baseRigidity).eulerAngles;
        modRotation.eulerAngles += new Vector3(0,0,baseRigidity*segments[i].gameObject.GetComponent<FloppySegmentRotation>().rigidity);
        //Quaternion.RotateTowards(segments[i].rotation,modRotation,singleStep);

        segments[i].rotation = Quaternion.Slerp(segments[i].rotation,modRotation,rotSpeed);
        //segments[i].rotation = modRotation;

      }


    }



    // Update is called once per frame
    void Update()
    {
      //wiggleDir.localRotation = Quaternion.Euler(0,0,Mathf.Sin(Time.time * wiggleSpeed) * wiggleMagnitude);

      segmentPoses[0] = targetDir.position;

      for( int i =1; i < segmentPoses.Length; i++){
      //  Vector3 dir = new Vector3(-1*Mathf.Cos(segments[i].rotation.eulerAngles.z),-1*Mathf.Sin(segments[i].rotation.eulerAngles.z),0);
          Vector3 targetPos = segmentPoses[i-1] +
          (segmentPoses[i] + segmentPoses[i-1]).normalized
          //dir
          * targetDist;
          segmentPoses[i] = Vector3.SmoothDamp(segmentPoses[i],targetPos,ref _segmentV[i], smoothSpeed);
          segments[i-1].transform.position = segmentPoses[i];
      }
      lineRend.SetPositions(segmentPoses);

    }*/


}
