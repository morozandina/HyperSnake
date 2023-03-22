using System;
using System.Collections;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

namespace MONEY
{
    public class AdsManager : MonoBehaviour
    {
        public static string RemoveAds = "RemoveAds";
        
        public static AdsManager GetInstance { get; private set; }
        // Keys
        [Header("Android : "), Space(5)]
        public string androidBannerID;
        public string androidInterstitialID;
        public string androidRewardedVideoID;
        public string androidAppOpenID;
        // Ad Component's
        private BannerView _banner;
        private InterstitialAd _interstitial;
        private RewardedAd _rewarded;
        private AppOpenAd _appOpen;

        // Verification
        public bool IsBannerLoaded => _banner != null;
        public bool IsInterstitialLoaded => _interstitial.IsLoaded();
        public bool IsRewardLoaded => _rewarded.IsLoaded();

        private bool IsAdAvailable => _appOpen != null && (System.DateTime.UtcNow - _loadTime).TotalSeconds >= 6;
        private bool _isShowingAd = false;
        private static bool isFistDate = false;
        private DateTime _loadTime;
        private bool _isInitialized;

        public void Awake()
        {
            if (GetInstance == this) return;
            if (GetInstance != null)
            {
                Destroy(gameObject);
                return;
            }
            GetInstance = this;
            DontDestroyOnLoad(this);
        }
        
        private void Start()
        {
            InitiationAds();
            AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
        }

        public void InitiationAds()
        {
            try
            {
                if (!_isInitialized)
                {
                    _isInitialized = true;
                
                    RequestConfiguration requestConfiguration = new RequestConfiguration.Builder().SetSameAppKeyEnabled(true).build();
                    MobileAds.SetRequestConfiguration(requestConfiguration);
                    MobileAds.Initialize((initStatus) =>
                    {
                        print($"MobileAds is initialized");
                        RequestReward();
                        RequestBanner();
                        RequestAppOpen();
                        RequestInterstitial();
                    });
                }
            }
            catch (Exception)
            {
                InitiationAds();
            }
        }

        // Banner
        public void RequestBanner()
        {
            if (PlayerPrefs.GetInt(RemoveAds, 0) != 0)
                return;

            DestroyBanner();
            var adRequest = new AdRequest.Builder().Build();

#if UNITY_ANDROID
            _banner = new BannerView(androidBannerID, AdSize.Banner, AdPosition.Bottom);
#elif UNITY_IOS
            _banner = new BannerView(iosBannerID, AdSize.Banner, AdPosition.Bottom);
#endif
            
            _banner.OnAdLoaded += HandleOnBannerAdLoaded;
            _banner.LoadAd(adRequest);
        }

        private void HandleOnBannerAdLoaded(object sender, EventArgs args) => _banner.Show();
        public void DestroyBanner() => _banner?.Destroy();
        private void HideBanner() => _banner?.Hide();
        
        // Interstitial
        private void RequestInterstitial()
        {
#if UNITY_ANDROID
            _interstitial = new InterstitialAd(androidInterstitialID);
#elif UNITY_IOS
            _interstitial = new InterstitialAd(iosInterstitialID);
#endif
            var adRequest = new AdRequest.Builder().Build();
            _interstitial.OnAdClosed += HandleOnInterstitialAdClosed;
            _interstitial.LoadAd(adRequest);
        }

        private Action _interstitialCallback;
        public void ShowInterstitial(Action callback)
        {
            if (PlayerPrefs.GetInt(RemoveAds, 0) != 0)
            {
                callback?.Invoke();
                return;
            }

            _interstitialCallback = callback;
            _interstitial?.Show();
        }
    
        private void HandleOnInterstitialAdClosed(object sender, EventArgs args) => StartCoroutine(HandleOnInterstitialAdClosed());
        private IEnumerator HandleOnInterstitialAdClosed()
        {
            yield return new WaitForEndOfFrame();
        
            _interstitialCallback?.Invoke();
            _interstitial.Destroy();
            RequestInterstitial();
        }

        // Reward
        private void RequestReward()
        {
#if UNITY_ANDROID
            _rewarded = new RewardedAd(androidRewardedVideoID);
#elif UNITY_IOS
            _rewarded = new RewardedAd(iosRewardedVideoID);
#endif
            var adRequest = new AdRequest.Builder().Build();
            _rewarded.OnAdClosed += HandleRewardedAdClosed;
            _rewarded.OnAdFailedToShow += HandleRewardedFailed;
            _rewarded.LoadAd(adRequest);
        }

        private Action _rewardCallback;
        public void ShowReward(Action callback)
        {
            _rewardCallback = callback;
            _rewarded?.Show();
        }

        private void HandleRewardedAdClosed(object sender, EventArgs args) => StartCoroutine(HandleRewardedAdClosed());
        private IEnumerator HandleRewardedAdClosed()
        {
            yield return new WaitForEndOfFrame();
        
            _rewardCallback?.Invoke();
            _rewarded.Destroy();
            RequestReward();
        }

        private void HandleRewardedFailed(object sender, AdErrorEventArgs e)
        {
            print("[SUKA NU SE INCARCA] " + e.AdError);
            RequestReward();
        }
        
        // App Open Ad
        private void RequestAppOpen()
        {
            var request = new AdRequest.Builder().Build();
#if UNITY_ANDROID
            AppOpenAd.LoadAd(androidAppOpenID, ScreenOrientation.Portrait, request, (appOpenAd, error) =>
#elif UNITY_IOS
            AppOpenAd.LoadAd(iosAppOpenID, ScreenOrientation.Portrait, request, (appOpenAd, error) =>
#endif
            {
            if (error != null)
            {
                Debug.LogFormat("Failed to load the ad. (reason: {0})", error.LoadAdError.GetMessage());
                return;
            }
                // App open ad is loaded.
                _appOpen = appOpenAd;
                _loadTime = DateTime.UtcNow;
            });
        }


        public void ShowAdIfAvailable()
        {
            if (PlayerPrefs.GetInt(RemoveAds, 0) != 0)
                return;
            
            if (!IsAdAvailable || _isShowingAd)
                return;
            
            
            _appOpen.OnAdDidDismissFullScreenContent += HandleAdDidDismissFullScreenContent;
            _appOpen.OnAdFailedToPresentFullScreenContent += HandleAdFailedToPresentFullScreenContent;
            _appOpen.OnAdDidPresentFullScreenContent += HandleAdDidPresentFullScreenContent;
            
            _appOpen.Show();
            isFistDate = true;
        }
    
        private void HandleAdDidDismissFullScreenContent(object sender, EventArgs args)
        {
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
            _appOpen = null;
            _isShowingAd = false;
            RequestAppOpen();
        }

        private void HandleAdFailedToPresentFullScreenContent(object sender, AdErrorEventArgs args)
        {
            Debug.LogFormat("Failed to present the ad (reason: {0})", args.AdError.GetMessage());
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
            _appOpen = null;
            RequestAppOpen();
        }

        private void HandleAdDidPresentFullScreenContent(object sender, EventArgs args)
        {
            Debug.Log("Displayed app open ad");
            _isShowingAd = true;
        }
        
        // When show app open
        private void OnAppStateChanged(AppState state)
        {
            if (state == AppState.Foreground)
                ShowAdIfAvailable();
        }
    }
}
