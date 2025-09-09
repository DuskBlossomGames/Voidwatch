using System;
using Extensions;
using TMPro;
using UnityEngine;
using Util;
using static Singletons.Static_Info.PlayerData;

namespace Player
{
    public class ScrapDisplayController : MonoBehaviour
    {
        public float waitTime;
        public int transferPerSec;
    
        private TextMeshProUGUI _text;
        private TextMeshProUGUI _gain;
        private int _scrap;

        private bool _waited;
        private readonly Timer _wait = new();

        private readonly Timer _transfer = new();
    
        private void Start()
        {
            _text = GetComponentsInChildren<TextMeshProUGUI>()[0];
            _gain = GetComponentsInChildren<TextMeshProUGUI>()[1];
        
            _scrap = PlayerDataInstance.Scrap;
            _gain.SetAlpha(0);
        }

        public event Action FinishTransfer;
        void Update()
        {
            _wait.Update();
            _transfer.Update();
            _text.text = $"{_scrap:# ### ##0}";

            if (_scrap != PlayerDataInstance.Scrap)
            {
                if (_wait.IsFinished && !_waited)
                {
                    _waited = true;
                    _wait.Value = waitTime;
                }
                
                if (_wait.IsFinished && _transfer.IsFinished)
                {
                    _scrap += (int) Mathf.Sign(PlayerDataInstance.Scrap - _scrap);
                    if (_scrap == PlayerDataInstance.Scrap) FinishTransfer?.Invoke();
                    
                    var perSec = transferPerSec * (1 + Mathf.Abs(_scrap - PlayerDataInstance.Scrap)/50f);
                    _transfer.Value = 1 / perSec;
                }
                
                _gain.SetAlpha(1);
                _gain.text = (_scrap > PlayerDataInstance.Scrap ? "- " : "+") +
                             $"{Mathf.Abs(PlayerDataInstance.Scrap - _scrap):# ### ##0}";
            }
            else
            {
                _waited = false;
                _gain.SetAlpha(0);
            }
        }
    }
}
