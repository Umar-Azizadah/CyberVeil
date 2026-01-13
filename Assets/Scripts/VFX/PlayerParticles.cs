using UnityEngine;

namespace CyberVeil.VFX
{
    public class PlayerParticles : MonoBehaviour
    {

        public void ShowParticle()
        {
            gameObject.SetActive(true); //enable the GameObject
        }

        public void HideParticle()
        {
            gameObject.SetActive(false); //disable the GameObject
        }

    }
}
