using System;
using System.Collections;
using System.Linq;
using Extensions;
using LevelPlay;
using Player;
using Singletons;
using Singletons.Static_Info;
using Spawnables;
using Spawnables.Controllers;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util;
using static Singletons.Static_Info.Statistics;
using static Singletons.Static_Info.LevelSelectData;

namespace Menus
{
    public class GameOverController : MonoBehaviour
    {
        public CustomRigidbody2D player;
        
        public Image fadeOut;
        public float fadeOutTime;
        public Image winImage;
        public TextMeshProUGUI winTitle, winSubtitle, eliteWinTitle, loseTitle, loseSubtitle;
        public GameObject time, enemy, clickContinue;
        public GameObject[] statistics;
        public float fadeInTime;
        public float statisticsBeforeTime, statisticsFadeTime, statisticsBetweenTime;
        public Image fadeOutAgain;
        public GameObject instagram;
        
        public DialogueController dialogue;

        public IEnumerator Run(bool won, DeathInfo diedTo)
        {
            var firstWin = SettingsInterface.rank < SettingsInterface.Rank.Captain;
            var firstEliteWin = SettingsInterface.rank < SettingsInterface.Rank.General && LevelSelectDataInstance.hardMode;
            if (won)
            {
                SettingsInterface.SetMinRank(LevelSelectDataInstance.hardMode
                    ? SettingsInterface.Rank.General
                    : SettingsInterface.Rank.Captain);
            }
            else
            {
                SettingsInterface.SetMinRank(SettingsInterface.Rank.Lieutenant);
            }
            
            var es = FindAnyObjectByType<EnemySpawner>();
            es.enabled = false;
            
            var timePlayed = Time.time - StatisticsInstance.startTime;
            var hour = (int) timePlayed / 3600;
            var minute = (int) (timePlayed % 3600) / 60;
            var second = (int) (timePlayed % 3600) % 60;
            time.GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"{hour:00}:{minute:00}:{second:00}";

            if (!won)
            {
                if (diedTo != null)
                {
                    enemy.GetComponentsInChildren<TextMeshProUGUI>()[1].text = diedTo.title;

                    var icon = enemy.GetComponentsInChildren<Image>()[1];
                    icon.sprite = diedTo.icon;
                    icon.rectTransform.offsetMin = -diedTo.offsetMin;
                    icon.rectTransform.offsetMax = diedTo.offsetMax;
                    if (diedTo.additionalChildren != null)
                    {
                        foreach (Transform child in diedTo.additionalChildren)
                        {
                            Instantiate(child, icon.transform, false);
                        }
                    }
                }
                else
                {
                    enemy.GetComponentsInChildren<TextMeshProUGUI>()[1].text = "Unclear";
                    enemy.GetComponentInChildren<Image>().enabled = false;
                }
            } 
            
            fadeOut.gameObject.SetActive(true);
            fadeOut.SetAlpha(0);
            for (float t = 0; t < fadeOutTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                fadeOut.SetAlpha(t/fadeOutTime);
            }
            player.transform.position = Vector3.zero;
            player.linearVelocity = Vector2.zero;
            es.SpawnedEnemies.ForEach(Destroy);
            
            var title = won ? winTitle : loseTitle;
            var subtitle = won ? winSubtitle : loseSubtitle;
            
            title.gameObject.SetActive(true);
            subtitle.gameObject.SetActive(true);
            title.SetAlpha(0);
            subtitle.SetAlpha(0);
            if (won)
            {
                winImage.gameObject.SetActive(true);
                winImage.SetAlpha(0);
            }

            for (float t = 0; t < fadeInTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                title.SetAlpha(t / fadeInTime);
                subtitle.SetAlpha(t / fadeInTime);
                if (won) winImage.SetAlpha(t/fadeInTime);;
            }
            
            yield return new WaitForSeconds(statisticsBeforeTime);
            SetText(statistics.Select(o => o.GetComponentsInChildren<TextMeshProUGUI>()[1]).ToArray());
                
            for (var i = LevelSelectDataInstance.hardMode ? -1 : 0; i < statistics.Length + 1; i++)
            {
                var stats = i == statistics.Length ? new[] { time, enemy, clickContinue } : 
                    i == -1 ? Array.Empty<GameObject>() : new[] { statistics[i] };
                if (i == statistics.Length && won) stats = stats.Append(instagram).ToArray();

                if (i == -1) eliteWinTitle.gameObject.SetActive(true);
                foreach (var stat in stats) stat.SetActive(true);
                for (float t = 0; t < statisticsFadeTime; t += Time.fixedDeltaTime)
                {
                    if (i == -1) eliteWinTitle.SetAlpha(t / statisticsFadeTime);
                    foreach (var stat in stats) stat.GetComponent<CanvasGroup>().alpha = t / statisticsFadeTime;
                    yield return new WaitForFixedUpdate();
                }

                yield return new WaitForSeconds(statisticsBetweenTime);
            }
            
            while (!InputManager.GetKey(KeyCode.Return) && !InputManager.GetKey(KeyCode.Mouse0) 
                                                        && !InputManager.GetKey(KeyCode.Space))
            {
                yield return new WaitForFixedUpdate();
            }
            
            fadeOutAgain.gameObject.SetActive(true);
            fadeOutAgain.SetAlpha(0);
            for (float t = 0; t < fadeOutTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                fadeOutAgain.SetAlpha(t/fadeOutTime);
            }

            if (won && (firstWin || firstEliteWin))
            {
                dialogue.Continue += () => StartCoroutine(Close(true, true));

                yield return dialogue.Open(DialogueController.People.General);
                
                dialogue.ShowText(firstEliteWin
                        ? "We appreciate your dedication, General. One day, we hope to reward you with larger galaxies, and more advanced missions. Until then, it's up to you to expand the Voidwatch Academy. Thank you."
                        : "Congratulations, Captain. You have done the impossible, and liberated this sector. But the Void lives on, requiring more... <b>Elite Campaigns</b>. Are you up for the challenge?", true);
            }
            else
            {
                StartCoroutine(Close(false, won));
            }
        }

        private IEnumerator Close(bool closeBox, bool won)
        {
            if (closeBox) yield return dialogue.Close();
            Destroy(StaticInfoHolder.Instance.gameObject);
            if (won) DontDestroyOnLoad(new GameObject("RollCredits")); // TODO: this is so scuffed ahhhh
            SceneManager.LoadScene("TitleScreen");
        }
    }
}