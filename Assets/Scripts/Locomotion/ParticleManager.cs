namespace Utils
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ParticleManager : Singleton<ParticleManager>
    {
        [SerializeField] ParticleSystem[] _jets;

        void startParticles(ParticleSystem system)
        {
            if (system.isPlaying)
                return;
            system.Play();
        }

        public void startJets()
        {
            foreach (ParticleSystem jet in _jets)
            {
                startParticles(jet);
            }
        }

        public void stopJets()
        {
            foreach (ParticleSystem jet in _jets)
            {
                jet.Stop();
            }
        }
    } 
}
