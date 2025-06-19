using System.Linq;
using Player;
using TMPro;
using UnityEngine;
using Util;

namespace Tutorial
{
    public class RaceController : MonoBehaviour
    {
        public GameObject rings;
        public TextMeshProUGUI timer, ringCount, bestTime;
        public Movement player;
        public float resetWaitTime, winWaitTime;

        private CustomRigidbody2D _playerRb;
        
        private readonly Timer _resetTimer = new();
        private readonly Timer _winTimer = new();
        
        private bool _begun;
        private float _time, _bestTime = -1;
        private RingController[] _rings;

        public bool Completed { get; private set; }

        private void OnEnable() { timer.gameObject.SetActive(true); }

        private void OnDisable()
        {
            timer.gameObject.SetActive(false);
            player.inputBlocked = player.autoPilot = false;
        }

        private void Start()
        {
            _playerRb = player.GetComponent<CustomRigidbody2D>();
            
            _rings = rings.GetComponentsInChildren<RingController>();

            bestTime.text = "";

            GetComponent<LineRenderer>().positionCount = _rings.Length + 1;
            var positions = _rings.Select(r => r.transform.position).ToList();
            positions.Insert(0, new Vector3(35, 0, 0));
            GetComponent<LineRenderer>().SetPositions(positions.ToArray());

            Reset();
        }

        private void Reset()
        {
            _time = 0;
            timer.text = GetTimerText(0);
            ringCount.text = "0 / " + _rings.Length;
            _begun = false;

            player.transform.position = new Vector3(35, 0, -1);
            player.transform.rotation = Quaternion.identity;
            _playerRb.linearVelocity = Vector2.zero;

            foreach (var ring in _rings) ring.Completed = false;
            
            _resetTimer.Value = resetWaitTime;
            player.inputBlocked = true;
            player.autoPilot = false;
        }

        private void Update()
        {
            if (!_resetTimer.IsFinished)
            {
                _resetTimer.Update();
                if (_resetTimer.IsFinished) player.inputBlocked = false;
            }

            if (!_winTimer.IsFinished)
            {
                _winTimer.Update();
                if (_winTimer.IsFinished) Reset();
            }

            if (!_begun)
            {
                if (!player.autoPilot && _playerRb.linearVelocity.sqrMagnitude > 0) _begun = true;
                else return;
            }
            
            _time += Time.deltaTime;
            timer.text = GetTimerText(_time);
            ringCount.text = _rings.Sum(r=>r.Completed ? 1 : 0) + " / " + _rings.Length;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<Movement>(out var player)) return;
            if (!_rings.All(r => r.Completed)) return;
            
            player.autoPilot = true;
            Completed = true;
            _begun = false;

            _winTimer.Value = winWaitTime;

            if (_time < _bestTime || _bestTime == -1)
            {
                bestTime.text = GetTimerText(_bestTime = _time, 13);
            }
        }

        private string GetTimerText(float time, float mspace=16)
        {
            return $"<mspace={mspace}>{(time/60).ToString("00")}</mspace>:<mspace={mspace}>{((int) (time%60)).ToString("00")}</mspace>.<mspace={mspace}>{(100*(time%1)).ToString("00")}";
        }
    }
}