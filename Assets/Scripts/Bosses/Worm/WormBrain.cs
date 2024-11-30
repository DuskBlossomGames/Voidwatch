using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using JetBrains.Annotations;
using ProgressBars;
//using Unity.VersionControl.Git.ICSharpCode.SharpZipLib.Zip;
using UnityEditor;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace Bosses.Worm
{
    public class WormBrain : MonoBehaviour
    {
        public ProgressBar bossBar;
        
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

        public float circleSpeed;
        public float ouroborosSpeed;
        public float ouroborosSnakiness;
        public float ouroborosTurnAngle;
        public float ouroborosTime;
        public float ouroborosProgressTime;
        private Timer _ouroborosProgressTimer = null;
        private int _ouroborosProgress;
        private bool _isInCircle = false;

        public float wanderSpeed;
        public float wanderSnakiness;
        public float wanderTurnAngle;

        private float _tarSpeed;
        private float _tarSnakines;
        private float _tarTurnAngle;

        private TailController _tailController;
        private SpikeLinkedList _headSpike, _tailSpike;
        private JawGrab _jawGrab;

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
        public bool _isStageTwo = false;

        struct PortalPair
        {
            public Transform pin;
            public Transform pout;
        };

        struct PortalID
        {
            public int segID;
            public int pairID;
        }

        private int exportals;
        public int numportals = 0;
        PortalPair[] _portals = new PortalPair[10];
        PortalID[] _portalIDs = new PortalID[10];

        public int secondsPerSummon;
        private SummonWorm[] _wormSummoners;
        private Timer _summonTimer;

        public Transform portalIn;
        public Transform portalOut;

        private int _eyesAlive;
        
        private void Start()
        {
            _eyesAlive = 2 * middleLength;
            
            _tailController = GetComponentInChildren<TailController>();

            _ouroborosProgressTimer = new Timer();

            _summonTimer = new Timer();
            _wormSummoners = GetComponentsInChildren<SummonWorm>();

            _jawGrab = GetComponentInChildren<JawGrab>();

            _actionUtilTimer = new Util.Timer();
            actionGoal = ActionGoal.Idle;
            _segments = new GameObject[middleLength + 2];

            _segments[0] = head;
            _segments[1] = middle;
            _segments[^1] = tail;

            var tailPos = tail.transform.localPosition;
            tailPos.x -= middle.transform.lossyScale.x * (middleLength - 1);

            tail.transform.localPosition = tailPos;


            float totallength = head.transform.lossyScale.x + middle.transform.lossyScale.x + tail.transform.lossyScale.x;
            var prevNode = _headSpike = middle.transform.GetChild(3).GetComponentInChildren<SpikeLinkedList>().previous = middle.transform.GetChild(2).GetComponentInChildren<SpikeLinkedList>();
            prevNode.next = prevNode = middle.transform.GetChild(3).GetComponentInChildren<SpikeLinkedList>();

            for (var i = 2; i < middleLength + 1; i++)
            {
                var segment = (_segments[i] = Instantiate(middle)).transform;
                segment.SetParent(transform, false);

                segment.GetChild(2).GetComponent<SpikeLinkedList>().previous = prevNode;
                prevNode = prevNode.next = segment.GetChild(2).GetComponent<SpikeLinkedList>();

                segment.GetChild(3).GetComponent<SpikeLinkedList>().previous = prevNode;
                prevNode = prevNode.next = segment.GetChild(3).GetComponent<SpikeLinkedList>();

                var pos = _segments[i - 1].transform.localPosition;

                totallength += _segmentDist = segment.lossyScale.x * 0.9f;
                pos.x -= _segmentDist;
                segment.localPosition = pos;
            }

            var tailLists = tail.GetComponentsInChildren<SpikeLinkedList>();
            prevNode!.next = tailLists[0];
            tailLists[0].previous = prevNode;
            tailLists[0].next = _tailSpike = tailLists[1];
            tailLists[1].previous = tailLists[0];

            _ouroborosRadius = totallength / (2 * Mathf.PI);

            _headRigid = head.GetComponent<Rigidbody2D>();
            _moveMode = MoveMode.Wander;
        }

        public void EyeDead()
        {
            _eyesAlive--;
            if (_eyesAlive == 0) _isStageTwo = true;

            bossBar.UpdatePercentage(middleLength*2 + _eyesAlive, middleLength * 4);
        }

        private void Update()
        {

            UpdateMovement();

            var _snakiness = pathfinder.snakeyness;


            /* Code for managing speeding up and slowing down, too small to really need a function especially since it occurs once */
            {
                _speed = Mathf.Clamp(_speed + 10 * Time.deltaTime * MathF.Sign(_tarSpeed - _speed), Mathf.Min(_speed, _tarSpeed), Mathf.Max(_speed, _tarSpeed));
                pathfinder.snakeyness = Mathf.Clamp(_snakiness + .1f * Time.deltaTime * MathF.Sign(_tarSnakines - _snakiness), Mathf.Min(_snakiness, _tarSnakines), Mathf.Max(_snakiness, _tarSnakines));
                maxTurnAngleDeg = Mathf.Clamp(maxTurnAngleDeg + 10 * Time.deltaTime * MathF.Sign(_tarTurnAngle - maxTurnAngleDeg), Mathf.Min(maxTurnAngleDeg, _tarTurnAngle), Mathf.Max(maxTurnAngleDeg, _tarTurnAngle));
            }

            if (!_isStageTwo)
            {
                _summonTimer.Update();
                if (_summonTimer.IsFinished)
                {
                    var summon = _wormSummoners[Random.Range(0, _wormSummoners.Length)];
                    if (summon != null) summon.TrySummon();

                    _summonTimer.Value = secondsPerSummon;
                }
            }

            if (actionGoal == ActionGoal.Ouroboros)
            {
                if (!_isInCircle)
                {
                    _isInCircle = _segments.All(seg => seg == head ||
                        ((Vector2)seg.transform.position).sqrMagnitude - _ouroborosRadius * _ouroborosRadius < 6);
                }

                if (_isInCircle)
                {
                    _ouroborosProgressTimer.Update();
                    if (_ouroborosProgressTimer.IsFinished)
                    {
                        var spike = _tailSpike;
                        for (var i = 0; i < _ouroborosProgress; i++) spike = spike!.previous;

                        if (spike != null)
                        {
                            spike!.isAnim = true;

                            if (_ouroborosProgress >= 2 && _ouroborosProgress % 2 == 1)
                            {
                                var laser = _segments[^(1 + _ouroborosProgress / 2)].GetComponentInChildren<OuroborosLaserControl>();

                                if (!laser.isShooting) StartCoroutine(laser.Shoot(player.GetComponent<PlayerGunHandler>().playRadius - _ouroborosRadius - middle.transform.lossyScale.y / 2, _actionUtilTimer.Value + 1));
                            }

                            _ouroborosProgress++;
                        }
                        else
                        {
                            _ouroborosProgress = 0;
                        }

                        _ouroborosProgressTimer.Value = ouroborosProgressTime;
                    }
                }
            }

            //actionGoal = ActionGoal.Burrow;

            _actionUtilTimer.Update();
            if (_actionUtilTimer.IsFinished)
            {
                if (actionGoal == ActionGoal.Tailspike)
                {
                    TailSwipe();
                }

                StartCoroutine(_headSpike.TriggerDown());
                float rand = UnityEngine.Random.Range(0f, 1f);
                if (!_isStageTwo)
                {
                    switch (rand)
                    {
                        case < 0 * .15f: //Disabled till fixed
                            /*Do burrow*/
                            actionGoal = ActionGoal.Burrow;
                            _actionUtilTimer.Value = Random.Range(8f, 12f);
                            exportals = Random.Range(0, 0);
                            SpawnPortal();
                            break;
                        case < .25f:
                            /*Do Rush*/
                            actionGoal = ActionGoal.Rush;
                            _actionUtilTimer.Value = Random.Range(5f, 10f);
                            break;

                        case < .5f:
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
                else
                {
                    switch (rand)
                    {
                        case < .15f:
                            /*Do Central laser*/
                            goto default;
                        case < .50f:
                            /*Do Ouroboros*/
                            actionGoal = ActionGoal.Ouroboros;
                            _actionUtilTimer.Value = ouroborosTime;
                            _ouroborosProgressTimer.Value = 0;
                            _ouroborosProgress = 0;
                            _isInCircle = false;

                            _jawGrab.enabled = false;
                            StartCoroutine(EnableJaw());
                            break;
                        case < .75f:
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
                    _moveMode = MoveMode.Portal;
                    break;

            }

        }

        private IEnumerator EnableJaw()
        {
            yield return new WaitForSeconds(ouroborosTime);
            _jawGrab.enabled = true;
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
            if (_moveMode == MoveMode.Wander)
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
            if (_moveMode != MoveMode.Portal && numportals != 0)
            {
                for (int i = 0; i < numportals; i++)
                {
                    Destroy(_portals[i].pin.gameObject);
                    Destroy(_portals[i].pout.gameObject);
                }
                _portals = new PortalPair[10];
                numportals = 0;
            }
            switch (_moveMode)
            {
                case MoveMode.Portal:
                    //targetPosition = _portals[numportals-1].pin.position;
                    _tarSpeed = rushSpeed;
                    _tarTurnAngle = 40;
                    _tarSnakines = 0;
                    targetPosition = _portals[numportals - 1].pin.position;
                    break;
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
                    targetPosition = player.transform.position;
                    if ((targetPosition - head.transform.position).sqrMagnitude < 30 * 30)
                    {
                        if (_speed < _tarSpeed)
                            _speed *= Mathf.Pow(1.10f, Time.deltaTime);
                        _tarSpeed = rushSpeed;
                    }
                    else {
                        _tarSpeed = pursueSpeed;
                    }
                    _tarTurnAngle = pursueTurnAngle;
                    _tarSnakines = pursueSnakiness;
                    break;
                case MoveMode.Circle:
                    _tarSpeed = _isInCircle ? circleSpeed : ouroborosSpeed;
                    _tarTurnAngle = ouroborosTurnAngle;
                    _tarSnakines = ouroborosSnakiness;
                    targetPosition = Util.UtilFuncs.TangentPointOnCircleFromPoint(Vector2.zero, _ouroborosRadius, head.transform.position);
                    break;
            }
            _targetMovePos = targetPosition;
            var dir = pathfinder.PathDirNorm(_segments[0].transform.position, _targetMovePos);
            Vector2 prevAngle = pathfinder.AngleToVector(Mathf.Deg2Rad * _segments[0].transform.rotation.eulerAngles.z);
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

                if (_spikesReleased < 3 && _swipe - swipetime < swipetime - swipetime / 6 / 3 * _spikesReleased)
                {
                    _tailController.ReleaseSpike(_spikesReleased++);
                }
            }
            else if (_swipe > 0)
            {
                RippleSegmentsWithSwipe(swipeangle, swipestartindex);

                if (_spikesReleased < 6 && _swipe < swipetime - swipetime / 6 / 3 * (_spikesReleased - 3))
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
                // RippleSegments();
                RippleSegmentsWithTeleport();
            }
            _swipe -= Time.deltaTime;
        }

        public void SpawnPortal()
        {
            var newPin = Instantiate(portalIn);
            var newPout = Instantiate(portalOut);
            newPin.gameObject.SetActive(true);
            newPout.gameObject.SetActive(true);

            float oldZ = newPin.position.z;
            Vector3 newpos = (Vector3)(Vector2)head.transform.position + Random.Range(20, 80) * head.transform.right + oldZ * Vector3.forward;
            newPin.position = newpos;
            newPin.rotation = head.transform.rotation;
            Debug.LogFormat("Head rot is {0}", head.transform.rotation.eulerAngles.z);


            do newpos = (Vector3)(Random.Range(20, 70) * pathfinder.AngleToVector(UnityEngine.Random.Range(0, 6.28f))) + oldZ * Vector3.forward;
                while ((head.transform.position - newpos).sqrMagnitude < 4000);
            newPout.position = newpos;
            newPout.rotation = Quaternion.Euler(0, 0, -Mathf.Rad2Deg * Mathf.Atan2(newpos.y, newpos.x));

            var pair = new PortalPair { pin = newPin , pout = newPout};
            _portals[numportals] = pair;
            var id = new PortalID { segID = 0, pairID = numportals };
            _portalIDs[numportals] = id;
            
            numportals += 1;
            exportals -= 1;

            targetPosition = _portals[numportals - 1].pin.position;
        }
        public void RippleSegmentsWithTeleport()
        {
            //RippleSegments();
            Vector3 currSegmentPos = _segments[0].transform.position;
            bool teled = false;
            for (int idx = 0; idx < numportals; idx++)
            {
                PortalID id = _portalIDs[idx];
                if (id.segID == 0)
                {
                    var pair = _portals[id.pairID];
                    //currSegmentPos = OutofPortal(pair, currSegmentPos);
                    currSegmentPos = SnapAlongPortal(pair.pin, currSegmentPos);
                    float along = ValueAlongPortal(pair.pin, currSegmentPos);
                    //Debug.LogFormat("Along: {0}", along);
                    if (along > 0)
                    {
                        currSegmentPos = OutofPortal(pair, currSegmentPos);
                        head.transform.rotation = Quaternion.Euler(0, 0, 180+pair.pout.rotation.z);
                        //Debug.LogFormat("Head Along: {0}", along);
                        _portalIDs[idx].segID += 1;
                        teled = true;
                    }
                    break;
                }
            }
            _segments[0].transform.position = currSegmentPos;
            for (var i = 1; i < middleLength + 1; i++)
            {
                Vector3 nextSegmentPos = _segments[i - 1].transform.position;
                currSegmentPos = _segments[i].transform.position;
                Vector3 prevSegmentPos = _segments[i + 1].transform.position;

                for (int idx = 0; idx < numportals; idx++)
                {
                    //print("loop");
                    PortalID id = _portalIDs[idx];
                    if (id.segID == i)
                    {
                        //print("Ran");
                        var pair = _portals[id.pairID];
                        currSegmentPos = OutofPortal(pair, currSegmentPos);
                    }
                }
                
                Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;
                currSegmentPos = nextSegmentPos + -_segmentDist * curr2next;
                
                for (int idx = 0; idx < numportals; idx++)
                {
                    PortalID id = _portalIDs[idx];
                    if (id.segID == i)
                    {
                        var pair = _portals[id.pairID];
                        currSegmentPos = IntoPortal(pair, currSegmentPos);
                        curr2next = ((Vector3)IntoPortal(pair, nextSegmentPos) - currSegmentPos).normalized;
                        currSegmentPos = SnapAlongPortal(pair.pin, currSegmentPos);
                        float along = ValueAlongPortal(pair.pin, currSegmentPos);
                        //Debug.LogFormat("Along: {0}", along);
                        if (along > 0)
                        {
                            currSegmentPos = OutofPortal(pair, currSegmentPos);
                            //Debug.LogFormat("Seg_{1} Along: {0}", along, i);
                            _portalIDs[idx].segID += 1;
                        }
                        break;
                    }
                    if (id.segID == i + 1)
                    {
                        var pair = _portals[id.pairID];
                        prevSegmentPos = OutofPortal(pair, prevSegmentPos);
                    }
                }

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
                currSegmentPos = _segments[^1].transform.position;

                Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;
                _segments[^1].transform.position = currSegmentPos = nextSegmentPos + -_segmentDist * curr2next;

                var angle = Mathf.Atan2(curr2next.y, curr2next.x);
                _segments[^1].transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
            }

            if (teled && exportals > 0) SpawnPortal();

            for (var i = 0; i < middleLength + 2; i++)
            {
                foreach (var rend in _segments[i].GetComponentsInChildren<SpriteRenderer>())
                {
                    rend.sortingLayerName = "Boss";
                    for (int idx = 0; idx < numportals; idx++)
                    {
                        if (_portalIDs[idx].segID == i) rend.sortingLayerName = "Teleport";
                    }
                }
            }
        }

        Vector2 IntoPortal(PortalPair pair, Vector2 position)
        {
            return FromLocalSpace(pair.pin, -IntoLocalSpace(pair.pout, position));
        }

        Vector2 OutofPortal(PortalPair pair, Vector2 position)
        {
            return FromLocalSpace(pair.pout, -IntoLocalSpace(pair.pin, position));
        }

        float OutofPortalRot(PortalPair pair, float rot)
        {
            float nrot = rot - pair.pin.eulerAngles.z + pair.pout.eulerAngles.z;
            return nrot;
        }

        Vector2 SnapAlongPortal(Transform transform, Vector2 position)
        {
            var local = IntoLocalSpace(transform, position);
            return FromLocalSpace(transform, new Vector2(local.x, 0));
        }

        float ValueAlongPortal(Transform transform, Vector2 position)
        {
            var local = IntoLocalSpace(transform, position);
            return local.x;
        }

        Vector2 IntoLocalSpace(Transform transform, Vector2 position)
        {
            Vector2 delta = position - (Vector2)transform.position;
            Vector2 norm = transform.right;
            Vector2 perp = transform.up;
            float x = Vector2.Dot(delta, norm);
            float y = Vector2.Dot(delta, perp);
            return new Vector2(x, y);
        }

        Vector2 FromLocalSpace(Transform transform, Vector2 position)
        {
            Vector2 norm = transform.right;
            Vector2 perp = transform.up;
            Vector2 x = norm * position.x;
            Vector2 y = perp * position.y;
            return x + y + (Vector2)transform.position;
        }

        /*
        //private void RippleSegmentsWithTeleport()
        //{
        //    if (portalID > middleLength) return;

        //    _segments[portalID + 1].transform.position = PortalOutofTransform(_segments[portalID + 1].transform.position);
        //    //Ripple forwards
        //    for (var i = 1; i <= portalID; i++)
        //    {
        //        Vector3 nextSegmentPos = _segments[i - 1].transform.position;
        //        Vector3 currSegmentPos = _segments[i].transform.position;
        //        Vector3 prevSegmentPos = _segments[i + 1].transform.position;

        //        Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;
        //        currSegmentPos = nextSegmentPos + -_segmentDist * curr2next;
        //        if (((Vector2)currSegmentPos).sqrMagnitude <= _ouroborosRadius * _ouroborosRadius)
        //        {
        //            currSegmentPos += .1f * (Vector3)((Vector2)currSegmentPos).normalized;
        //        }
        //        _segments[i].transform.position = currSegmentPos;

        //        Vector3 prev2curr = (currSegmentPos - prevSegmentPos).normalized;
        //        Vector3 meanDir = .5f * (prev2curr + curr2next);

        //        var angle = Mathf.Atan2(meanDir.y, meanDir.x);
        //        _segments[i].transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
        //    }

        //    _segments[portalID + 1].transform.position = PortalIntoTransform(_segments[portalID + 1].transform.position);
        //    _segments[portalID + 1].transform.position = Vector3.Lerp(_segments[portalID + 1].transform.position, AlignwithInPortal(_segments[portalID + 1].transform.position), .01f);
        //    //_segments[portalID + 2].transform.position = Vector3.Lerp(_segments[portalID + 2].transform.position, AlignwithInPortal(_segments[portalID + 2].transform.position), .2f);


        //    Vector3 truemidpos = _segments[portalID].transform.position = AlignwithOutPortal(_segments[portalID].transform.position);

        //    //Ripple backwards
        //    for (var i = portalID - 1; i >= 1; i--)
        //    {
        //        Vector3 nextSegmentPos = _segments[i + 1].transform.position;
        //        Vector3 currSegmentPos = _segments[i].transform.position;
        //        Vector3 prevSegmentPos = _segments[i - 1].transform.position;

        //        Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;
        //        currSegmentPos = nextSegmentPos + -_segmentDist * curr2next;
        //        if (((Vector2)currSegmentPos).sqrMagnitude <= _ouroborosRadius * _ouroborosRadius)
        //        {
        //            currSegmentPos += .1f * (Vector3)((Vector2)currSegmentPos).normalized;
        //        }
        //        _segments[i].transform.position = currSegmentPos;

        //        Vector3 prev2curr = (currSegmentPos - prevSegmentPos).normalized;
        //        Vector3 meanDir = .5f * (prev2curr + curr2next);

        //        var angle = Mathf.Atan2(meanDir.y, meanDir.x);
        //        _segments[i].transform.rotation = Quaternion.Euler(0, 0, 180 + Mathf.Rad2Deg * angle);
        //    }

        //    mirrorMiddle.position = _segments[portalID].transform.position = PortalIntoTransform(_segments[portalID].transform.position);
        //    mirrorMiddle.rotation = PortalInRotation(_segments[portalID].transform.rotation);
        //    for (var i = portalID + 1; i < middleLength + 1; i++)
        //    {
        //        Vector3 nextSegmentPos = _segments[i - 1].transform.position;
        //        Vector3 currSegmentPos = _segments[i].transform.position;
        //        Vector3 prevSegmentPos = _segments[i + 1].transform.position;

        //        Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;
        //        currSegmentPos = nextSegmentPos + -_segmentDist * curr2next;
        //        if (((Vector2)currSegmentPos).sqrMagnitude <= _ouroborosRadius * _ouroborosRadius)
        //        {
        //            currSegmentPos += .1f * (Vector3)((Vector2)currSegmentPos).normalized;
        //        }
        //        _segments[i].transform.position = currSegmentPos;

        //        Vector3 prev2curr = (currSegmentPos - prevSegmentPos).normalized;
        //        Vector3 meanDir = .5f * (prev2curr + curr2next);

        //        var angle = Mathf.Atan2(meanDir.y, meanDir.x);
        //        _segments[i].transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
        //    }

        //    mirrorMiddle2.position = PortalOutofTransform(_segments[portalID + 1].transform.position);
        //    mirrorMiddle2.rotation = PortalOutRotation(_segments[portalID + 1].transform.rotation);

        //    { //Scope naming stuffs
        //        Vector3 nextSegmentPos = _segments[^2].transform.position;
        //        Vector3 currSegmentPos = _segments[^1].transform.position;

        //        Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;
        //        _segments[^1].transform.position = currSegmentPos = nextSegmentPos + -_segmentDist * curr2next;

        //        var angle = Mathf.Atan2(curr2next.y, curr2next.x);
        //        _segments[^1].transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
        //    }

        //    { //Scope naming stuffs
        //        Vector3 nextSegmentPos = _segments[0].transform.position;
        //        Vector3 currSegmentPos = _segments[1].transform.position;

        //        Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;
        //        _segments[0].transform.position = currSegmentPos + _segmentDist * curr2next;
        //    }

        //    _segments[portalID].transform.position = truemidpos;
        //    if (PortalOutAmt(mirrorMiddle2.position) > 0)
        //    {
        //        _segments[portalID + 1].transform.position = mirrorMiddle2.position;
        //        portalID += 1;

        //    }
        //}

        ////Stolen from ChatGPT
        //private static Vector2 RotateVector(Vector2 vector, float angle)
        //{
        //    float cos = Mathf.Cos(angle);
        //    float sin = Mathf.Sin(angle);

        //    float newX = vector.x * cos - vector.y * sin;
        //    float newY = vector.x * sin + vector.y * cos;

        //    return new Vector2(newX, newY);
        //}

        //private Vector3 PortalIntoTransform(Vector3 orig)
        //{
        //    float z = orig.z;
        //    float ang = Mathf.Deg2Rad * Vector2.SignedAngle(pInNorm, pOutNorm);
        //    Vector2 delta = (Vector2)orig - portalOut;
        //    Vector2 rotdelt = RotateVector(delta, -ang);
        //    return new Vector3(rotdelt.x + portalIn.x, rotdelt.y + portalIn.y, z);
        //}

        //private Vector3 PortalOutofTransform(Vector3 orig)
        //{
        //    float z = orig.z;
        //    float ang = Mathf.Deg2Rad * Vector2.SignedAngle(pInNorm, pOutNorm);
        //    Vector2 delta = (Vector2)orig - portalIn;
        //    Vector2 rotdelt = RotateVector(delta, ang);
        //    return new Vector3(rotdelt.x + portalOut.x, rotdelt.y + portalOut.y, z);
        //}

        //private Vector3 AlignwithOutPortal(Vector3 orig)
        //{
        //    float z = orig.z;
        //    Vector2 delta = (Vector2)orig - portalOut;
        //    Vector2 mdelt = pOutNorm * Vector2.Dot(pOutNorm, delta);
        //    return new Vector3(mdelt.x + portalOut.x, mdelt.y + portalOut.y, z);
        //}

        //private Vector3 AlignwithInPortal(Vector3 orig)
        //{
        //    float z = orig.z;
        //    Vector2 delta = (Vector2)orig - portalIn;
        //    Vector2 mdelt = pInNorm * Vector2.Dot(pInNorm, delta);
        //    return new Vector3(mdelt.x + portalIn.x, mdelt.y + portalIn.y, z);
        //}

        //private float PortalOutAmt(Vector3 orig)
        //{
        //    float z = orig.z;
        //    Vector2 delta = (Vector2)orig - portalOut;
        //    return Vector2.Dot(pOutNorm, delta);
        //}

        //private Quaternion PortalOutRotation(Quaternion rots)
        //{
        //    float ang = (Vector2.SignedAngle(pInNorm, pOutNorm));
        //    return rots * Quaternion.AngleAxis(ang, Vector3.forward);
        //}

        //private Quaternion PortalInRotation(Quaternion rots)
        //{
        //    float ang = (Vector2.SignedAngle(pInNorm, pOutNorm));
        //    return rots * Quaternion.AngleAxis(-ang, Vector3.forward);
        //}
        */
    }
}
