using System;
using UnityEngine;
using UnityEngine.UI;

namespace MONEY
{
    public class RestoreControl : MonoBehaviour
    {
        public Button remove, restore;
        private void Start()
        {
            remove.onClick.RemoveAllListeners();
            restore.onClick.RemoveAllListeners();
            if (PlayerPrefs.GetInt(AdsManager.RemoveAds, 0) == 0)
            {
                remove.gameObject.SetActive(true);
                remove.onClick.AddListener(() => IAPManager.instance.BuyRemoveAds());
                restore.gameObject.SetActive(false);
            }
            else
            {
                remove.gameObject.SetActive(false);
                restore.gameObject.SetActive(true);
                restore.onClick.AddListener(() => IAPManager.instance.RestorePurchases());
            }
        }
    }
}
