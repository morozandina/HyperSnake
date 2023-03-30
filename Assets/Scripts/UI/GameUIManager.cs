using System;
using System.Collections;
using Game;
using Main;
using MONEY;
using Newtonsoft.Json.Linq;
using Snake;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class GameUIManager : MonoBehaviour
    {
        public static GameUIManager Instance;
        public Animator transition;
        public AudioClip buttonSound;
        
        private int _currentScore = 0;
        private int _currentStar = 0;
        [Header("For Settings :")]
        [SerializeField] private GameObject globalVolume;

        [Header("UI: ")]
        [SerializeField] private GameObject pause;
        [SerializeField] private GameObject lose;
        [SerializeField] private GameObject shield;
        [SerializeField] private GameObject super;
        [SerializeField] private GameObject showApple;

        [Header("Text: ")]
        [SerializeField] private TextMeshProUGUI score;
        [SerializeField] private TextMeshProUGUI bestScore;
        [SerializeField] private TextMeshProUGUI shieldTimer;
        [SerializeField] private TextMeshProUGUI superTimer;
        [SerializeField] private TextMeshProUGUI showAppleTimer;
        [SerializeField] private TextMeshProUGUI stars;

        [Header("Lose: ")]
        [SerializeField] private Button restart;
        [SerializeField] private Button revive;
        [SerializeField] private Button exit;

        [Header("Settings: ")]
        [SerializeField] private Toggle volume;
        [SerializeField] private Toggle sound;
        private void Awake()
        {
            Instance = this;
            _currentStar = StarManager.GetStar();
            stars.text = _currentStar.ToString();
            UpdateScore(_currentScore);
        }

        private void Start()
        {
            volume.isOn = SettingsPrefs.GetPrefs(SettingsPrefs.Volume) == 0;
            sound.isOn = SettingsPrefs.GetUpdatePrefs(SettingsPrefs.Sound) == 1;
            globalVolume.SetActive(SettingsPrefs.GetPrefs(SettingsPrefs.Volume) == 0);
            
            volume.onValueChanged.AddListener(x => ChangeVolume());
            sound.onValueChanged.AddListener(x => ChangeMusicVolume());
        }
        
        public void ChangeMusicVolume()
        {
            var soundState = SettingsPrefs.GetUpdatePrefs(SettingsPrefs.Sound);
            Sound.Instance.PlaySound(buttonSound);
            var curr = soundState == 0 ? 1 : 0;
            SettingsPrefs.SavePrefs(SettingsPrefs.Sound, curr);
            Sound.Instance.VolumeControl(curr);
        }
        
        public void ChangeVolume()
        {
            Sound.Instance.PlaySound(buttonSound);
            SettingsPrefs.SavePrefs(SettingsPrefs.Volume, volume.isOn ? 0 : 1);
            globalVolume.SetActive(volume.isOn);
        }

        public void Pause(bool status)
        {
            switch (status)
            {
                case true:
                    Sound.Instance.PlaySound(buttonSound);
                    pause.SetActive(true);

                    Time.timeScale = 0;
                    break;
                case false:
                    Sound.Instance.PlaySound(buttonSound);
                    pause.SetActive(false);
                    StyledSnakeControl.shiedSnake?.Invoke(3);
                    
                    Time.timeScale = 1;
                    break;
            }
        }
        
        // Lose
        public void Lose(Action<Action> onLose, Action onRevive)
        {
            ConfigureBestScore();
            lose.SetActive(true);
            var bestText = $"Best Score:\n{SettingsPrefs.GetPrefs(SettingsPrefs.Best)}";
            bestScore.text = bestText;

            restart.onClick.RemoveAllListeners();
            restart.onClick.AddListener(() =>
            {
                lose.SetActive(false);
                onLose.Invoke(Restart);
            });
            revive.onClick.RemoveAllListeners();
            revive.onClick.AddListener(() =>
            {
                if (AdsManager.GetInstance.IsRewardLoaded)
                {
                    AdsManager.GetInstance.ShowReward(() =>
                    {
                        revive.gameObject.SetActive(false);
                        lose.SetActive(false);
                        onRevive.Invoke();
                    });
                }
            });
            exit.onClick.RemoveAllListeners();
            exit.onClick.AddListener(() =>
            {
                lose.SetActive(false);
                onLose.Invoke(() => Exit(false));
            });
        }

        private void ConfigureBestScore()
        {
            if (_currentScore < SettingsPrefs.GetPrefs(SettingsPrefs.Best))
                return;
            
            SettingsPrefs.SavePrefs(SettingsPrefs.Best, _currentScore);
        }
        
        // Restart
        public void Restart()
        {
            Sound.Instance.PlaySound(buttonSound);
            Sound.Instance.StopMusic();
            Time.timeScale = 1;
            SaveStars();
            StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex));
        }
        
        // Exit
        public void Exit(bool state)
        {
            Sound.Instance.PlaySound(buttonSound);
            Sound.Instance.StopMusic();
            Time.timeScale = 1;
            SaveStars();
            if (state)
            {
                if (AdsManager.GetInstance.IsInterstitialLoaded)
                {
                    AdsManager.GetInstance.ShowInterstitial(() =>
                    {
                        StartCoroutine(LoadLevel(0));
                    }); 
                    return;
                }
                StartCoroutine(LoadLevel(0));
            }
            else
            {
                StartCoroutine(LoadLevel(0));
            }
        }
        
        // Score
        public void UpdateScore(int add)
        {
            _currentScore += add;
            score.text = _currentScore.ToString();
        }

        public void UpdateStars(int add)
        {
            _currentStar += add;
            stars.text = _currentStar.ToString();
        }

        public void SaveStars()
        {
            StarManager.RefreshStars(_currentStar);
        }

        public void ShieldTimer(int time)
        {
            StartCoroutine(Timer(shield, time, shieldTimer));
        }

        public void SuperTimer(int time)
        {
            StartCoroutine(Timer(super, time, superTimer));
        }

        public void AppleTimer(int time)
        {
            StartCoroutine(Timer(showApple, time, showAppleTimer));

        }

        private static IEnumerator Timer(int time, TMP_Text txt)
        {
            while (time > 0)
            {
                time--;
                var result = TimeSpan.FromHours(time);
                var fromTimeString = result.ToString("d':'hh");
                txt.text = fromTimeString;
                yield return new WaitForSeconds(1f);
            }
        }
        
        private static IEnumerator Timer(GameObject obj, int time, TMP_Text txt)
        {
            obj.SetActive(true);
            while (time > 0)
            {
                time--;
                var result = TimeSpan.FromHours(time);
                var fromTimeString = result.ToString("d':'hh");
                txt.text = fromTimeString;
                yield return new WaitForSeconds(1f);
            }
            obj.SetActive(false);
        }

        private IEnumerator LoadLevel(int levelIndex)
        {
            transition.SetTrigger("Start");
            yield return new WaitForSeconds(1.2f);
            SceneManager.LoadScene(levelIndex);
        }
    }
}
