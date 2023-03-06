using UnityEngine;

namespace Game
{
    public class FpsShow : MonoBehaviour
    {
        private void Awake()
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 300;
        }
    }
}
