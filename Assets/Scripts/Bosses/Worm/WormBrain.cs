using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Bosses.Worm
{
    public class WormBrain : MonoBehaviour
    {
        public GameObject head, middle, tail;
        public int middleLength;
        public SnakePathfinder pathfinder;

        public Vector3 targetPosition;
        public GameObject player;

        public float maxTurnAngleDeg;

        private GameObject[] _segments;
        private Rigidbody2D _headRigid;
        private float _speed;
        private Vector2 _targetMovePos;
        private float _segmentDist;
        private float _ouroborosRadius;
        private Vector2 _currdir;
        private float _swipe;
        public float swipetime;
        public float swipeangle;
        public float tailmomentum;
        public int swipestartindex;

        public float pursueSpeed;
        public float rushSpeed;
        public float pursueSnakiness;
        public float pursueTurnAngle;

        public float ouroborosSpeed;
        public float ouroborosSnakiness;
        public float ouroborosTurnAngle;

        public float wanderSpeed;
        public float wanderSnakiness;
        public float wanderTurnAngle;

        private float _tarSpeed;
        private float _tarSnakines;
        private float _tarTurnAngle;

        private TailController _tailController;
        private SpikeLinkedList _headSpike;

        public enum MoveMode
        {
            Portal,
            Direct,
            Circle,
            Wander,
        }
        public enum ActionGoal
        {
            Rush,
            Burrow,
            Tailspike,
            Idle,
            Ouroboros,
        }
        public MoveMode _moveMode;
        public ActionGoal actionGoal;
        private Util.Timer _actionUtilTimer;
        private bool _isStageTwo = false;


        public Vector2 portalIn;
        public Vector2 portalOut;
        public Vector2 pInNorm;
        public Vector2 pOutNorm;
        public Transform mirrorMiddle;
        public Transform mirrorMiddle2;
        public int portalID;



        private void Start()
        {
            _tailController = GetComponentInChildren<TailController>();
            
            _actionUtilTimer = new Util.Timer();
            actionGoal = ActionGoal.Idle;
            _segments = new GameObject[middleLength + 2];

            _segments[0] = head;
            _segments[1] = middle;
            _segments[^1] = tail;

            var tailPos = tail.transform.localPosition;
            tailPos.x -= middle.transform.localScale.x * (middleLength - 1);

            tail.transform.localPosition = tailPos;


            float totallength = head.transform.localScale.x + tail.transform.localScale.x;
            SpikeLinkedList? prevNode = null;

            for (var i = 2; i < middleLength + 1; i++)
            {
                var segment = (_segments[i] = Instantiate(middle)).transform;
                segment.SetParent(transform);
                
                segment.GetChild(2).GetComponent<SpikeLinkedList>().previous = prevNode;
                if (prevNode == null)
                {
                    _headSpike = prevNode = segment.GetChild(2).GetComponent<SpikeLinkedList>();
                } else {
                    prevNode = prevNode.next = segment.GetChild(2).GetComponent<SpikeLinkedList>();
                }
                
                segment.GetChild(3).GetComponent<SpikeLinkedList>().previous = prevNode;
                prevNode = prevNode.next = segment.GetChild(3).GetComponent<SpikeLinkedList>();

                var pos = _segments[i - 1].transform.localPosition;

                totallength += _segmentDist = segment.localScale.x * 0.9f;
                pos.x -= _segmentDist;
                segment.localPosition = pos;
            }

            _ouroborosRadius = totallength / (2 * Mathf.PI);

            _headRigid = head.GetComponent<Rigidbody2D>();
            _moveMode = MoveMode.Wander;

            // activate all the worm summoners
            foreach (var c in GetComponentsInChildren<SummonWorm>())
            {
                c.active = true;
            }


        }

        private void Update()
        {

            UpdateMovement();

            var _snakiness = pathfinder.snakeyness;


            /* Code for managing speeding up and slowing down, too small to really need a function especially since it occurs once */{
                _speed = Mathf.Clamp(_speed + 10 * Time.deltaTime * MathF.Sign(_tarSpeed - _speed), Mathf.Min(_speed, _tarSpeed), Mathf.Max(_speed, _tarSpeed));
                pathfinder.snakeyness = Mathf.Clamp(_snakiness + .1f * Time.deltaTime * MathF.Sign(_tarSnakines - _snakiness), Mathf.Min(_snakiness, _tarSnakines), Mathf.Max(_snakiness, _tarSnakines));
                maxTurnAngleDeg = Mathf.Clamp(maxTurnAngleDeg + 10 * Time.deltaTime * MathF.Sign(_tarTurnAngle - maxTurnAngleDeg), Mathf.Min(maxTurnAngleDeg, _tarTurnAngle), Mathf.Max(maxTurnAngleDeg, _tarTurnAngle));
            }

            _actionUtilTimer.Update();
            if (_actionUtilTimer.IsFinished)
            {
                if(actionGoal == ActionGoal.Tailspike)
                {
                    TailSwipe();
                }

                if (!_isStageTwo)
                {
                    StartCoroutine(_headSpike.TriggerDown());
                    float rand = UnityEngine.Random.Range(0f, 1f);
                    switch (rand)
                    {
                        case < .15f:
                            /*Do burrow*/
                            goto default;
                        
                        case < .50f:
                            /*Do Rush*/
                            actionGoal = ActionGoal.Rush;
                            _actionUtilTimer.Value = Random.Range(5f, 10f);
                            break;

                        case < 1.0f:
                            /*Do Tailspike*/
                            actionGoal = ActionGoal.Tailspike;
                            _actionUtilTimer.Value = Random.Range(1f, 4f);
                            break;

                        default:
                            actionGoal = ActionGoal.Idle;
                            _actionUtilTimer.Value = Random.Range(5f, 8f);
                            break;
                    }
                }
            }
            switch (actionGoal)
            {
                case ActionGoal.Rush:
                    if (player.GetComponent<Player.Movement>().inputBlocked)
                    {
                        actionGoal = ActionGoal.Idle;
                        goto case ActionGoal.Idle;
                    }
                    _moveMode = MoveMode.Direct;
                    break;

                case ActionGoal.Idle:
                case ActionGoal.Tailspike:
                    _moveMode = MoveMode.Wander;
                    break;

                case ActionGoal.Ouroboros:
                    _moveMode = MoveMode.Circle;
                    break;

                case ActionGoal.Burrow:
                    _moveMode = MoveMode.Wander;
                    break;

            }

        }

        private void RippleSegments()
        {
            for (var i = 1; i < middleLength + 1; i++)
            {
                Vector3 nextSegmentPos = _segments[i - 1].transform.position;
                Vector3 currSegmentPos = _segments[i].transform.position;
                Vector3 prevSegmentPos = _segments[i + 1].transform.position;

                Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;
                currSegmentPos = nextSegmentPos + -_segmentDist * curr2next;
                if(((Vector2)currSegmentPos).sqrMagnitude <= _ouroborosRadius * _ouroborosRadius)
                {
                    currSegmentPos += .1f * (Vector3)((Vector2)currSegmentPos).normalized;
                }
                _segments[i].transform.position = currSegmentPos;

                Vector3 prev2curr = (currSegmentPos - prevSegmentPos).normalized;
                Vector3 meanDir = .5f * (prev2curr + curr2next);

                var angle = Mathf.Atan2(meanDir.y, meanDir.x);
                _segments[i].transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
            }

            { //Scope naming stuffs
                Vector3 nextSegmentPos = _segments[^2].transform.position;
                Vector3 currSegmentPos = _segments[^1].transform.position;

                Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;
                _segments[^1].transform.position = currSegmentPos = nextSegmentPos + -_segmentDist * curr2next;

                var angle = Mathf.Atan2(curr2next.y, curr2next.x);
                _segments[^1].transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
            }
        }

        private void RippleSegmentsWithSwipe(float swipeAngle, int startpos)
        {
            for (var i = 1; i < middleLength + 1; i++)
            {
                Vector3 nextSegmentPos = _segments[i - 1].transform.position;
                Vector3 currSegmentPos = _segments[i].transform.position;
                Vector3 prevSegmentPos = _segments[i + 1].transform.position;

                Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;
                if (i >= startpos)
                {
                    Vector2 next2nnext = (_segments[i - 2].transform.position - nextSegmentPos).normalized;
                    Vector2 tarcurr2next = Util.UtilFuncs.Rot(next2nnext, swipeAngle);
                    curr2next = (tailmomentum * .03f / Time.deltaTime * curr2next + (Vector3)tarcurr2next).normalized;
                }
                currSegmentPos = nextSegmentPos + -_segmentDist * curr2next;
                if (((Vector2)currSegmentPos).sqrMagnitude <= _ouroborosRadius * _ouroborosRadius)
                {
                    currSegmentPos += .1f * (Vector3)((Vector2)currSegmentPos).normalized;
                }
                _segments[i].transform.position = currSegmentPos;

                Vector3 prev2curr = (currSegmentPos - prevSegmentPos).normalized;
                Vector3 meanDir = .5f * (prev2curr + curr2next);

                var angle = Mathf.Atan2(meanDir.y, meanDir.x);
                _segments[i].transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
            }

            { //Scope naming stuffs
                Vector3 nextSegmentPos = _segments[^2].transform.position;
                Vector3 currSegmentPos = _segments[^1].transform.position;

                Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;
                Vector2 next2nnext = (_segments[^3].transform.position - nextSegmentPos).normalized;
                Vector2 tarcurr2next = Util.UtilFuncs.Rot(next2nnext, swipeAngle);
                curr2next = (tailmomentum * .03f / Time.deltaTime * curr2next + (Vector3)tarcurr2next).normalized;

                _segments[^1].transform.position = currSegmentPos = nextSegmentPos + -_segmentDist * curr2next;

                var angle = Mathf.Atan2(curr2next.y, curr2next.x);
                _segments[^1].transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
            }
        }
        private void TailSwipe()
        {
            if(_moveMode == MoveMode.Wander)
            {
                _swipe = 4 * swipetime;
            } else
            {
                _swipe = 0;
            }

        }

        public void BiteCallback()
        {
            actionGoal = ActionGoal.Idle;
        }

        private int _spikesReleased = 0;
        private void UpdateMovement()
        {
            switch (_moveMode)
            {
                case MoveMode.Portal:
                    goto wander;
                case MoveMode.Wander:
                wander:
                    _tarSpeed = wanderSpeed;
                    _tarTurnAngle = wanderTurnAngle;
                    _tarSnakines = wanderSnakiness;
                    if ((head.transform.position - targetPosition).sqrMagnitude < 120)
                    {
                        while ((head.transform.position - targetPosition).sqrMagnitude < 4000)
                            targetPosition = UnityEngine.Random.Range(20, 70) * pathfinder.AngleToVector(UnityEngine.Random.Range(0, 6.28f));
                    }
                    break;
                case MoveMode.Direct:
                    if ((targetPosition - head.transform.position).sqrMagnitude < 30*30)
                    {
                        if(_speed < _tarSpeed)
                            _speed *= Mathf.Pow(1.10f,Time.deltaTime);
                        _tarSpeed = rushSpeed;
                    }
                    else {
                        _tarSpeed = pursueSpeed;
                    }
                    _tarTurnAngle = pursueTurnAngle;
                    _tarSnakines = pursueSnakiness;
                    targetPosition = player.transform.position;
                    break;
                case MoveMode.Circle:
                    _tarSpeed = ouroborosSpeed;
                    _tarTurnAngle = ouroborosTurnAngle;
                    _tarSnakines = ouroborosSnakiness;
                    targetPosition = Util.UtilFuncs.TangentPointOnCircleFromPoint(Vector2.zero, 20, head.transform.position);
                    break;
            }
            _targetMovePos = targetPosition;
            var dir = pathfinder.PathDirNorm(_segments[0].transform.position, _targetMovePos);
            Vector2 prevAngle = pathfinder.AngleToVector(Mathf.Deg2Rad * _segments[1].transform.rotation.eulerAngles.z);
            dir = pathfinder.ClampAngle(dir, prevAngle, Mathf.Deg2Rad * maxTurnAngleDeg);

            _currdir = dir = (.1f / Time.deltaTime * _currdir + dir).normalized;

            //_headRigid.AddForce(_speed * dir, ForceMode.VelocityChange)
            _headRigid.velocity = _speed * dir;

            var angle = Mathf.Atan2(dir.y, dir.x);
            _segments[0].transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);

            if (_swipe > 2 * swipetime)
            {
                RippleSegmentsWithSwipe(swipeangle, swipestartindex);
            }
            else if (_swipe > swipetime)
            {
                RippleSegmentsWithSwipe(-swipeangle, swipestartindex);
                
                if (_spikesReleased < 3 && _swipe - swipetime < swipetime - swipetime/6/3 * _spikesReleased)
                {
                    _tailController.ReleaseSpike(_spikesReleased++);
                }
            }
            else if (_swipe > 0)
            {
                RippleSegmentsWithSwipe(swipeangle, swipestartindex);

                if (_spikesReleased < 6 && _swipe < swipetime - swipetime/6/3 * (_spikesReleased-3))
                {
                    _tailController.ReleaseSpike(_spikesReleased++);
                }

            }
            else
            {
                if (_spikesReleased != 0)
                {
                    StartCoroutine(_tailController.RegrowSpikes());
                    _spikesReleased = 0;
                }
                /////////////////////////////////////////////////////
                //RippleSegments();
                RippleSegmentsWithTeleport();
            }
            _swipe -= Time.deltaTime;
        }

        private void RippleSegmentsWithTeleport()
        {
            if (portalID > middleLength) return;
            
            _segments[portalID + 1].transform.position = PortalOutofTransform(_segments[portalID + 1].transform.position);
            //Ripple forwards
            for (var i = 1; i <= portalID; i++)
            {
                Vector3 nextSegmentPos = _segments[i - 1].transform.position;
                Vector3 currSegmentPos = _segments[i].transform.position;
                Vector3 prevSegmentPos = _segments[i + 1].transform.position;

                Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;
                currSegmentPos = nextSegmentPos + -_segmentDist * curr2next;
                if (((Vector2)currSegmentPos).sqrMagnitude <= _ouroborosRadius * _ouroborosRadius)
                {
                    currSegmentPos += .1f * (Vector3)((Vector2)currSegmentPos).normalized;
                }
                _segments[i].transform.position = currSegmentPos;

                Vector3 prev2curr = (currSegmentPos - prevSegmentPos).normalized;
                Vector3 meanDir = .5f * (prev2curr + curr2next);

                var angle = Mathf.Atan2(meanDir.y, meanDir.x);
                _segments[i].transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
            }

            _segments[portalID + 1].transform.position = PortalIntoTransform(_segments[portalID + 1].transform.position);
            _segments[portalID + 1].transform.position = Vector3.Lerp(_segments[portalID + 1].transform.position, AlignwithInPortal(_segments[portalID + 1].transform.position), .01f);
            //_segments[portalID + 2].transform.position = Vector3.Lerp(_segments[portalID + 2].transform.position, AlignwithInPortal(_segments[portalID + 2].transform.position), .2f);


            Vector3 truemidpos = _segments[portalID].transform.position = AlignwithOutPortal(_segments[portalID].transform.position);

            //Ripple backwards
            for (var i = portalID - 1; i >= 1; i--)
            {
                Vector3 nextSegmentPos = _segments[i + 1].transform.position;
                Vector3 currSegmentPos = _segments[i].transform.position;
                Vector3 prevSegmentPos = _segments[i - 1].transform.position;

                Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;
                currSegmentPos = nextSegmentPos + -_segmentDist * curr2next;
                if (((Vector2)currSegmentPos).sqrMagnitude <= _ouroborosRadius * _ouroborosRadius)
                {
                    currSegmentPos += .1f * (Vector3)((Vector2)currSegmentPos).normalized;
                }
                _segments[i].transform.position = currSegmentPos;

                Vector3 prev2curr = (currSegmentPos - prevSegmentPos).normalized;
                Vector3 meanDir = .5f * (prev2curr + curr2next);

                var angle = Mathf.Atan2(meanDir.y, meanDir.x);
                _segments[i].transform.rotation = Quaternion.Euler(0, 0, 180 + Mathf.Rad2Deg * angle);
            }

            mirrorMiddle.position = _segments[portalID].transform.position = PortalIntoTransform(_segments[portalID].transform.position);
            mirrorMiddle.rotation = PortalInRotation(_segments[portalID].transform.rotation);
            for (var i = portalID + 1; i < middleLength + 1; i++)
            {
                Vector3 nextSegmentPos = _segments[i - 1].transform.position;
                Vector3 currSegmentPos = _segments[i].transform.position;
                Vector3 prevSegmentPos = _segments[i + 1].transform.position;

                Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;
                currSegmentPos = nextSegmentPos + -_segmentDist * curr2next;
                if (((Vector2)currSegmentPos).sqrMagnitude <= _ouroborosRadius * _ouroborosRadius)
                {
                    currSegmentPos += .1f * (Vector3)((Vector2)currSegmentPos).normalized;
                }
                _segments[i].transform.position = currSegmentPos;

                Vector3 prev2curr = (currSegmentPos - prevSegmentPos).normalized;
                Vector3 meanDir = .5f * (prev2curr + curr2next);

                var angle = Mathf.Atan2(meanDir.y, meanDir.x);
                _segments[i].transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
            }

            mirrorMiddle2.position = PortalOutofTransform(_segments[portalID + 1].transform.position);
            mirrorMiddle2.rotation = PortalOutRotation(_segments[portalID + 1].transform.rotation);

            { //Scope naming stuffs
                Vector3 nextSegmentPos = _segments[^2].transform.position;
                Vector3 currSegmentPos = _segments[^1].transform.position;

                Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;
                _segments[^1].transform.position = currSegmentPos = nextSegmentPos + -_segmentDist * curr2next;

                var angle = Mathf.Atan2(curr2next.y, curr2next.x);
                _segments[^1].transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
            }

            { //Scope naming stuffs
                Vector3 nextSegmentPos = _segments[0].transform.position;
                Vector3 currSegmentPos = _segments[1].transform.position;

                Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;
                _segments[0].transform.position = currSegmentPos + _segmentDist * curr2next;
            }

            _segments[portalID].transform.position = truemidpos;
            if (PortalOutAmt(mirrorMiddle2.position) > 0)
            {
                _segments[portalID + 1].transform.position = mirrorMiddle2.position;
                portalID += 1;
                
            }
        }

        //Stolen from ChatGPT
        private static Vector2 RotateVector(Vector2 vector, float angle)
        {
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            float newX = vector.x * cos - vector.y * sin;
            float newY = vector.x * sin + vector.y * cos;

            return new Vector2(newX, newY);
        }

        private Vector3 PortalIntoTransform(Vector3 orig)
        {
            float z = orig.z;
            float ang = Mathf.Deg2Rad * Vector2.SignedAngle(pInNorm, pOutNorm);
            Vector2 delta = (Vector2)orig - portalOut;
            Vector2 rotdelt = RotateVector(delta, -ang);
            return new Vector3(rotdelt.x + portalIn.x, rotdelt.y + portalIn.y, z);
        }

        private Vector3 PortalOutofTransform(Vector3 orig)
        {
            float z = orig.z;
            float ang = Mathf.Deg2Rad * Vector2.SignedAngle(pInNorm, pOutNorm);
            Vector2 delta = (Vector2)orig - portalIn;
            Vector2 rotdelt = RotateVector(delta, ang);
            return new Vector3(rotdelt.x + portalOut.x, rotdelt.y + portalOut.y, z);
        }

        private Vector3 AlignwithOutPortal(Vector3 orig)
        {
            float z = orig.z;
            Vector2 delta = (Vector2)orig - portalOut;
            Vector2 mdelt = pOutNorm * Vector2.Dot(pOutNorm, delta);
            return new Vector3(mdelt.x + portalOut.x, mdelt.y + portalOut.y, z);
        }

        private Vector3 AlignwithInPortal(Vector3 orig)
        {
            float z = orig.z;
            Vector2 delta = (Vector2)orig - portalIn;
            Vector2 mdelt = pInNorm * Vector2.Dot(pInNorm, delta);
            return new Vector3(mdelt.x + portalIn.x, mdelt.y + portalIn.y, z);
        }

        private float PortalOutAmt(Vector3 orig)
        {
            float z = orig.z;
            Vector2 delta = (Vector2)orig - portalOut;
            return Vector2.Dot(pOutNorm, delta);
        }

        private Quaternion PortalOutRotation(Quaternion rots)
        {
            float ang = (Vector2.SignedAngle(pInNorm, pOutNorm));
            return rots * Quaternion.AngleAxis(ang, Vector3.forward);
        }

        private Quaternion PortalInRotation(Quaternion rots)
        {
            float ang = (Vector2.SignedAngle(pInNorm, pOutNorm));
            return rots * Quaternion.AngleAxis(-ang, Vector3.forward);
        }
    }
}
