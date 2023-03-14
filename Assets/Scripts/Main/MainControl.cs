using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using Game.Spawner;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Main
{
    public class MainControl : MonoBehaviour
    {
        public AudioClip buttonSound;
        [SerializeField] private GameObject globalVolume;
        public List<PlanetSettings> planets = new List<PlanetSettings>();
        private bool _volumeState;
        private int _soundState;
        public Animator transition;

        [Header("Settings: ")]
        [SerializeField] private GameObject settings;
        [SerializeField] private TextMeshProUGUI bestScore;
        [SerializeField] private TextMeshProUGUI totalDeath;
        [SerializeField] private TextMeshProUGUI totalApple;
        [SerializeField] private Toggle volume;
        [SerializeField] private Toggle sound;

        [Header("Choose: ")]
        [SerializeField] private Button easy;
        [SerializeField] private Button medium;
        [SerializeField] private Button hard;

        [Header("Upgrade: ")]
        [SerializeField] private Color maxColor;
        [SerializeField] private Color disabledColor;
        [SerializeField] private Button power, seekers, snake;
        [SerializeField] private TextMeshProUGUI powerText, seekersText, snakeText;

        [Space(20)] [SerializeField] private TextMeshProUGUI totalStars;
        private void Start()
        {
            Spawner.planetSettings = planets[0];
            _volumeState = SettingsPrefs.GetPrefs(SettingsPrefs.Volume) == 0;
            _soundState = SettingsPrefs.GetUpdatePrefs(SettingsPrefs.Sound);
            globalVolume.SetActive(_volumeState);
            Sound.Instance.VolumeControl(_soundState);
            ConfigureMap();
            ConfigureUpdate();
            RefreshCoins();
        }

        private void RefreshCoins()
        {
            totalStars.text = StarManager.GetStar().ToString();
        }

        public void ChangeVolume()
        {
            Sound.Instance.PlaySound(buttonSound);
            _volumeState = volume.isOn;
            SettingsPrefs.SavePrefs(SettingsPrefs.Volume, _volumeState ? 0 : 1);
            globalVolume.SetActive(_volumeState);
        }

        public void ChangeMusicVolume()
        {
            Sound.Instance.PlaySound(buttonSound);
            var curr = _soundState == 0 ? 1 : 0;
            SettingsPrefs.SavePrefs(SettingsPrefs.Sound, curr);
            Sound.Instance.VolumeControl(curr);
            _soundState = curr;
        }
        
        // Settings
        public void EnterToSettings()
        {
            Sound.Instance.PlaySound(buttonSound);
            volume.isOn = _volumeState;
            sound.isOn = _soundState == 1;

            volume.onValueChanged.AddListener(x => ChangeVolume());
            sound.onValueChanged.AddListener(x => ChangeMusicVolume());
            
            
            bestScore.text = SettingsPrefs.GetPrefs(SettingsPrefs.Best).ToString();
            totalDeath.text = SettingsPrefs.GetPrefs(SettingsPrefs.Death).ToString();
            totalApple.text = StarManager.GetApple().ToString();
            
            settings.SetActive(true);
        }

        public void ExitFromSettings()
        {
            Sound.Instance.PlaySound(buttonSound);
            volume.onValueChanged.RemoveAllListeners();
            sound.onValueChanged.RemoveAllListeners();
            settings.SetActive(false);
        }
        
        // Map
        private void ConfigureMap()
        {
            if (SettingsPrefs.GetPrefs(SettingsPrefs.Level1) == 0)
            {
                medium.transform.GetChild(2).gameObject.SetActive(false);
                medium.transform.GetChild(3).gameObject.SetActive(true);
            }
            if (SettingsPrefs.GetPrefs(SettingsPrefs.Level2) == 0)
            {
                hard.transform.GetChild(2).gameObject.SetActive(false);
                hard.transform.GetChild(3).gameObject.SetActive(true);
            }
            
            easy.onClick.AddListener(() => ChangeMap(planets[0]));
            medium.onClick.AddListener(() => ChangeMap(planets[1], SettingsPrefs.Level1, 100, medium.gameObject));
            hard.onClick.AddListener(() => ChangeMap(planets[2], SettingsPrefs.Level2, 1000, hard.gameObject));
        }
        
        private void ChangeMap(PlanetSettings planet)
        {
            Spawner.planetSettings = planet;
            Sound.Instance.PlaySound(buttonSound);
            ChangeScene(1);
        }

        private void ChangeMap(PlanetSettings planet, string prefs, int price, GameObject obj)
        {
            Sound.Instance.PlaySound(buttonSound);
            if (SettingsPrefs.GetPrefs(prefs) > 0)
            {
                Spawner.planetSettings = planet;
                ChangeScene(1);
                return;
            }
            obj.transform.GetChild(2).gameObject.SetActive(false);
            obj.transform.GetChild(3).gameObject.SetActive(true);
            var stars = StarManager.GetStar();
            if (price > stars)
                return;
            
            StarManager.AddStars(-price);
            RefreshCoins();
            SettingsPrefs.SavePrefs(prefs, 1);
            obj.transform.GetChild(2).gameObject.SetActive(true);
            obj.transform.GetChild(3).gameObject.SetActive(false);
            ChangeMap(planet, prefs, price, obj);
        }
        
        // Upgrade
        private void ConfigureUpdate()
        {
            ButtonUpgrade(power, SettingsPrefs.Powers, powerText);
            ButtonUpgrade(seekers, SettingsPrefs.Seeker, seekersText);
            ButtonUpgrade(snake, SettingsPrefs.Snake, snakeText);
        }

        private void ButtonUpgrade(Button btn, string prefs, TextMeshProUGUI txt)
        {
            if (SettingsPrefs.GetUpdatePrefs(prefs) > 10)
            {
                txt.color = maxColor;
                txt.text = "MAX";
                return;
            }

            var price = 100 * SettingsPrefs.GetUpdatePrefs(prefs);
            var stars = StarManager.GetStar();
            txt.text = price.ToString();
            btn.onClick.RemoveAllListeners();

            if (price < stars)
            {
                btn.interactable = true;
                txt.color = Color.white;
                txt.transform.GetChild(0).GetComponent<Image>().color = Color.white;
                btn.onClick.AddListener(() =>
                {
                    Sound.Instance.PlaySound(buttonSound);
                    SettingsPrefs.SavePrefs(prefs, SettingsPrefs.GetUpdatePrefs(prefs) + 1);
                    StarManager.AddStars(-price);
                    RefreshCoins();
                    ButtonUpgrade(btn, prefs, txt);
                });
            }
            else
            {
                btn.interactable = false;
                txt.color = disabledColor;
                txt.transform.GetChild(0).GetComponent<Image>().color = disabledColor;
            }
        }

        // Scene
        public void ChangeScene(int scene)
        {
            StartCoroutine(LoadLevel(scene));
        }
        
        private IEnumerator LoadLevel(int levelIndex)
        {
            transition.SetTrigger("Start");
            yield return new WaitForSeconds(1.2f);
            SceneManager.LoadScene(levelIndex);
        }

        public void PlaySoundButton()
        {
            Sound.Instance.PlaySound(buttonSound);
        }
    }
}
