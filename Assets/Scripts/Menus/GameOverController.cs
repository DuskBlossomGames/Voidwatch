using System.Collections;
using System.Linq;
using Extensions;
using Spawnables;
using Spawnables.Controllers;
using Static_Info;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util;
using static Static_Info.Statistics;

namespace Menus
{
    public class GameOverController : MonoBehaviour
    {
        public Image fadeOut;
        public float fadeOutTime;
        public Image winImage;
        public TextMeshProUGUI winTitle, winSubtitle, loseTitle, loseSubtitle;
        public GameObject time, enemy, clickContinue;
        public GameObject[] statistics;
        public float fadeInTime;
        public float statisticsBeforeTime, statisticsFadeTime, statisticsBetweenTime;
        public Image fadeOutAgain;

        public IEnumerator Run(bool won, DeathInfo diedTo)
        {
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
                    enemy.GetComponentInChildren<Image>().sprite = diedTo.icon;
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
                
            for (var i = 0; i < statistics.Length + 1; i++)
            {
                var stats = i == statistics.Length ? new[] { time, enemy, clickContinue } : new[] { statistics[i] };
                
                foreach (var stat in stats) stat.SetActive(true);
                for (float t = 0; t < statisticsFadeTime; t += Time.fixedDeltaTime)
                {
                    foreach (var stat in stats)
                    {
                        stat.GetComponentInChildren<Image>()?.SetAlpha(t/statisticsFadeTime);
                        foreach (var text in stat.GetComponentsInChildren<TextMeshProUGUI>()) text.SetAlpha(t/statisticsFadeTime);
                    }
                        
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
            
            Destroy(StaticInfoHolder.Instance.gameObject);
            if (won) DontDestroyOnLoad(new GameObject("RollCredits")); // TODO: this is so scuffed ahhhh
            SceneManager.LoadScene("TitleScreen");
        }
    }
}