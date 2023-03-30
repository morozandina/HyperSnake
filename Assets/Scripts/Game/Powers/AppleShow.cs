using System;
using UnityEngine;

namespace Game.Powers
{
    public class AppleShow : MonoBehaviour
    {
        public static Action<bool> UseViewPower;
        private GameObject view;

        private void Awake()
        {
            view = transform.GetChild(0).gameObject;
            UseViewPower += UsePower;
        }

        private void OnDestroy()
        {
            UseViewPower -= UsePower;
        }
        public void UsePower(bool state) => view.SetActive(state);
    }
}
