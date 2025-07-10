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

            StartCoroutine(Open(true));
        }
        
        public void ShowText(string text, bool continuable)
        {
            text = new Regex("{(.*)}").Replace(text, m => InputManager.VALID_KEY_CODES[InputAction.Parse(m.Groups[1].Value)!]);
            
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
            
            if (InputManager.GetKeyDown(KeyCode.LeftShift) || InputManager.GetKeyDown(KeyCode.RightShift) || InputManager.GetKeyDown(KeyCode.Return) || InputManager.GetKeyDown(KeyCode.Space) || InputManager.GetKeyDown(KeyCode.Mouse0))
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

        public IEnumerator Open(float speedMod=1)
        {
            yield return Open(false, speedMod);
        }
        
        public void SetClosed() // get the end result of Close() instantly
        {
            _person.SetAlpha(0);
            _personBox.SetAlpha(0);
            continueText.SetAlpha(0);
            text.text = _text = "";
            text.SetAlpha(1);
            _transform.anchorMin = _transform.anchorMax = new Vector2(_startAnchorX, _startAnchorY);
            _opened = false;
        }
        
        public IEnumerator Close(float speedMod=1)
        {
            _opened = false;
            
            for (float t = 0; t < personFadeInTime/speedMod; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                
                _person.SetAlpha(Mathf.SmoothStep(1, 0, t / personFadeInTime*speedMod));
                _personBox.SetAlpha(Mathf.SmoothStep(1, 0, t / personFadeInTime*speedMod));
                text.SetAlpha(Mathf.SmoothStep(1, 0, t / personFadeInTime*speedMod));
                continueText.SetAlpha(Mathf.SmoothStep(1, 0, t / personFadeInTime*speedMod));
            }
            text.text = _text = "";
            text.SetAlpha(1);
            
            for (float t = 0; t < openTime/speedMod; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                
                _transform.anchorMin = new Vector2(_anchorMinX, Mathf.Lerp(_anchorMinY, _startAnchorY - barHeight/2, t/openTime*speedMod));
                _transform.anchorMax = new Vector2(_anchorMaxX, Mathf.Lerp(_anchorMaxY, _startAnchorY + barHeight/2, t/openTime*speedMod));
            }

            for (float t = 0; t < barExpandTime/speedMod; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                
                _transform.anchorMin = new Vector2(Mathf.Lerp(_anchorMinX, _startAnchorX, t / barExpandTime*speedMod), _startAnchorY - barHeight / 2);
                _transform.anchorMax = new Vector2(Mathf.Lerp(_anchorMaxX, _startAnchorX, t / barExpandTime*speedMod), _startAnchorY + barHeight / 2);
            }

            _transform.anchorMin = _transform.anchorMax = new Vector2(_startAnchorX, _startAnchorY);
        }

        private IEnumerator Open(bool flash, float speedMod=1)
        {
            yield return new WaitForSeconds(waitTime);
            
            for (var i = 0; i < numberOfFlashes/speedMod; i++)
            {
                var wait = i == 0 ? waitBeforeFlash/speedMod : 0;
                for (float t = 0; t < flashTime/speedMod + wait; t += Time.fixedDeltaTime)
                {
                    yield return new WaitForFixedUpdate();

                    var a = Mathf.SmoothStep(t - wait < flashTime/speedMod / 2 ? 0 : 1, t - wait < flashTime/speedMod / 2 ? 1 : 0, 2 * (t - wait) / flashTime/speedMod % 1);
                    if (flash && t > wait) circle.SetAlpha(a);
                    if (i == 0 && t < barExpandTime)
                    {
                        _transform.anchorMin = new Vector2(Mathf.Lerp(_startAnchorX, _anchorMinX, t / barExpandTime*speedMod),
                            Mathf.Lerp(_startAnchorY, _startAnchorY - barHeight / 2,
                                Mathf.Clamp01(5 * t / barExpandTime*speedMod)));
                        _transform.anchorMax = new Vector2(Mathf.Lerp(_startAnchorX, _anchorMaxX, t / barExpandTime*speedMod),
                            Mathf.Lerp(_startAnchorY, _startAnchorY + barHeight / 2,
                                Mathf.Clamp01(5 * t / barExpandTime*speedMod)));
                    }
                    else if (!flash) break; 
                    else if (i < numberOfFlashes - 1 || t < flashTime/speedMod / 2) _box.SetAlpha(a); 
                }
            }
            _box.SetAlpha(1);
            _person.SetAlpha(0);
            _personBox.SetAlpha(0);

            if (flash) yield return new WaitForSeconds(waitBeforeOpen/speedMod);

            for (float t = 0; t < openTime/speedMod; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                
                _transform.anchorMin = new Vector2(_anchorMinX, Mathf.Lerp(_startAnchorY - barHeight/2, _anchorMinY, t/openTime*speedMod));
                _transform.anchorMax = new Vector2(_anchorMaxX, Mathf.Lerp(_startAnchorY + barHeight/2, _anchorMaxY, t/openTime*speedMod));
            }

            for (float t = 0; t < personFadeInTime/speedMod; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                
                _person.SetAlpha(Mathf.SmoothStep(0, 1, t / personFadeInTime*speedMod));
                _personBox.SetAlpha(Mathf.SmoothStep(0, 1, t / personFadeInTime*speedMod));
            }

            _opened = true;
        }
    }
}