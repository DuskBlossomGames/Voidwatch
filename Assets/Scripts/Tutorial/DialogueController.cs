using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace Tutorial
{
    public class DialogueController : MonoBehaviour
    {
        public TextMeshProUGUI text;
        public TextMeshProUGUI continueText;

        public float continueFlashTime;
        public uint charPerSec;
        public float periodPauseTime;
        
        public float waitTime;
        public float barExpandTime;
        public float waitBeforeFlash;
        public float flashTime;
        public float numberOfFlashes;
        public float barHeight; // in anchor points
        public float waitBeforeOpen;
        public float openTime;
        public float personFadeInTime;
        public Image circle;

        public event Action Continue;
        
        private string _text;
        private readonly Dictionary<int, string> _tagStartLocs = new(), _tagEndLocs = new();
        private bool _continuable;
        private readonly Timer _progress = new();
        private readonly Timer _periodPause = new();
        private int _idxPaused = -1;
        private int _continueFlashDir = 1;
        private bool _opened;
        
        private RectTransform _transform;
        private Image _personBox, _person, _box;
        private float _anchorMinX, _anchorMaxX, _anchorMinY, _anchorMaxY;
        private float _startAnchorX, _startAnchorY;
        
        private void Start()
        {
            _transform = GetComponent<RectTransform>();
            _personBox = transform.GetChild(0).GetComponent<Image>();
            _person = transform.GetChild(0).GetChild(0).GetComponent<Image>();
            _box = GetComponent<Image>();
            
            _anchorMinX = _transform.anchorMin.x;
            _anchorMaxX = _transform.anchorMax.x;
            _anchorMinY = _transform.anchorMin.y;
            _anchorMaxY = _transform.anchorMax.y;

            _transform.anchorMin = _transform.anchorMax = new Vector2(_startAnchorX = (_anchorMinX+_anchorMaxX)/2,
                _startAnchorY = (_anchorMinY+_anchorMaxY)/2);
            circle.SetAlpha(0);
            _person.SetAlpha(0);
            
            continueText.SetAlpha(0);

            StartCoroutine(Open());
        }
        
        public void ShowText(string text, bool continuable)
        {
            var openRegex = "(<[^/].*?>)";
            var closeRegex = "(</.+?>)";

            _text = new Regex(closeRegex).Replace(new Regex(openRegex).Replace(text, ""), "");
            _continuable = continuable;

            _tagStartLocs.Clear();
            _tagEndLocs.Clear();
            
            for (var i = 0; i < text.Length; i++)
            {
                var open = new Regex("^"+openRegex+".*").Match(text.Substring(i));
                var close = new Regex("^"+closeRegex+".*").Match(text.Substring(i));
                if (open.Value != string.Empty) _tagStartLocs.Add(i, open.Groups[1].Value);
                else if (close.Value != string.Empty) _tagEndLocs.Add(i, close.Groups[1].Value);
            }
            
            _progress.Value = (float) _text.Length / charPerSec;
            _periodPause.Value = 0;
            _idxPaused = -1;
            continueText.SetAlpha(0);
        }

        private void Update()
        {
            if (!_opened) return;
            
            if (InputManager.GetKeyDown(KeyCode.LeftShift) || InputManager.GetKeyDown(KeyCode.RightShift) || InputManager.GetKeyDown(KeyCode.Return))
            {
                if (!_progress.IsFinished)
                {
                    _progress.SetValue(Time.deltaTime/2); // make it proc one more time
                    _periodPause.Value = 0;
                }
                else if (_continuable)
                {
                    Continue?.Invoke();
                }
            }
            
            if (!_progress.IsFinished)
            {
                _periodPause.Update();
                if (!_periodPause.IsFinished) return;
                
                _progress.Update();

                var sub = _text[..Mathf.CeilToInt(_text.Length * (1 - _progress.Progress))];
                var i = 0;
                while (i <= sub.Length)
                {
                    if (_tagStartLocs.TryGetValue(i, out var tagStart)) sub = sub.Insert(i, tagStart);
                    else if (_tagEndLocs.TryGetValue(i, out var tagEnd)) sub = sub.Insert(i, tagEnd);

                    i++;
                }

                text.SetText(sub);
                if (sub.Length > 0 && sub.Length != _idxPaused && sub[^1] == '.')
                {
                    _periodPause.Value = periodPauseTime;
                    _idxPaused = sub.Length;
                }
            }
            else if (_continuable)
            {
                var a = Mathf.Clamp01(continueText.color.a + _continueFlashDir * Time.deltaTime / continueFlashTime);
                continueText.SetAlpha(a);
                if (a % 1 == 0) _continueFlashDir *= -1;
            }
        }

        private IEnumerator Open()
        {
            yield return new WaitForSeconds(waitTime);
            
            for (var i = 0; i < numberOfFlashes; i++)
            {
                var wait = i == 0 ? waitBeforeFlash : 0;
                for (float t = 0; t < flashTime + wait; t += Time.fixedDeltaTime)
                {
                    yield return new WaitForFixedUpdate();

                    var a = Mathf.SmoothStep(t - wait < flashTime / 2 ? 0 : 1, t - wait < flashTime / 2 ? 1 : 0, 2 * (t - wait) / flashTime % 1);
                    if (t > wait) circle.SetAlpha(a);
                    if (i == 0 && t < barExpandTime)
                    {
                        _transform.anchorMin = new Vector2(Mathf.Lerp(_startAnchorX, _anchorMinX, t/barExpandTime),
                            Mathf.Lerp(_startAnchorY, _startAnchorY - barHeight/2, Mathf.Clamp01(5*t/barExpandTime)));
                        _transform.anchorMax = new Vector2(Mathf.Lerp(_startAnchorX, _anchorMaxX, t/barExpandTime),
                            Mathf.Lerp(_startAnchorY, _startAnchorY + barHeight/2, Mathf.Clamp01(5*t/barExpandTime)));
                    } else if (i < numberOfFlashes - 1 || t < flashTime / 2) _box.SetAlpha(a); 
                }
            }
            _box.SetAlpha(1);
            _person.SetAlpha(0);
            _personBox.SetAlpha(0);

            yield return new WaitForSeconds(waitBeforeOpen);

            for (float t = 0; t < openTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                
                _transform.anchorMin = new Vector2(_anchorMinX, Mathf.Lerp(_startAnchorY - barHeight/2, _anchorMinY, t/openTime));
                _transform.anchorMax = new Vector2(_anchorMaxX, Mathf.Lerp(_startAnchorY + barHeight/2, _anchorMaxY, t/openTime));
            }

            for (float t = 0; t < personFadeInTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                
                _person.SetAlpha(Mathf.SmoothStep(0, 1, t / personFadeInTime));
                _personBox.SetAlpha(Mathf.SmoothStep(0, 1, t / personFadeInTime));
            }

            _opened = true;
        }
    }
}