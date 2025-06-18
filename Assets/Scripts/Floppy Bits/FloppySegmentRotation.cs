using UnityEngine;

namespace Floppy_Bits
{
  public class FloppySegmentRotation : MonoBehaviour
  {

    public float speed;

    private Vector2 _direction;
    public Transform target;
    public float rigidity;
    public float segLength;

    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
      //  _direction = target.position - transform.position;

      //float angle = Mathf.Atan2(_direction.x,_direction.y) *Mathf.Rad2Deg;

      //  Quaternion rotation = Quaternion.AngleAxis(angle,Vector3.forward);
      //  transform.rotation = Quaternion.Slerp(transform.rotation,rotation,speed*Time.deltaTime);

    }


  }
}
