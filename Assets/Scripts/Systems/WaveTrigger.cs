using UnityEngine;

namespace CyberVeil.Systems
{
    public class WaveTrigger : MonoBehaviour
    {
        public WaveManager waveManager;

        void Update()
        {
            // Start run (going to change how it starts later)
            if (Input.GetKeyDown(KeyCode.N))
            {
                waveManager.StartRun();
            }
        }
    }
}
