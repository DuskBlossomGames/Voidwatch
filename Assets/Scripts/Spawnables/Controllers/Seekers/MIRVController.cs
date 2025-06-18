using Spawnables.Controllers.Misslers;
using UnityEngine;
using Util;

namespace Spawnables.Controllers.Seekers
{
    public class MIRVController : MonoBehaviour
    {
        MissleAim _ma;
        public Transform target;
        public float splitDist;
        public int splitCount;
        public GameObject splitPrefab;
        public float scatterForce;
        public float prepTime = 1;

        bool _canPop = false;

        Timer _timer;

        CustomRigidbody2D _crb;

        // Start is called before the first frame update
        void Start()
        {
            target = null;
            _ma = GetComponent<MissleAim>();
            _crb = GetComponent<CustomRigidbody2D>();
            _timer = new Timer();
        }

        // Update is called once per frame
        void Update()
        {
            while(target == null)
            {
                if (_ma.target != null)
                {
                    target = _ma.target.transform;
                    _timer.Value = prepTime;
                    _canPop = true;
                }
                else
                {
                    return;
                }
            }

            _timer.Update();

            if (_canPop && _timer.IsFinished && (transform.position - target.position).sqrMagnitude < splitDist * splitDist)
            {
                for (int i = 0; i < splitCount; i++)
                {
                    var nchild = Instantiate(splitPrefab);
                    var nma = nchild.GetComponent<MissleAim>();
                    nma.target = target.gameObject;
                    var ncrb = nchild.GetComponent<CustomRigidbody2D>();
                    ncrb.velocity = _crb.velocity;
                    ncrb.AddForce(scatterForce * Random.insideUnitCircle);
                    nchild.transform.position = transform.position;
                }
                Destroy(gameObject);
            }
        }
    }
}
