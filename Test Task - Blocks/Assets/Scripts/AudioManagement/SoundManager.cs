using System.Collections.Generic;
using UnityEngine;

namespace RFTestTaskBlocks
{
    public class SoundManager : MonoBehaviour, ISoundManager
    {
        [SerializeField] private AudioSource _sfxSource;

        private Dictionary<string, AudioClip> _preloadedClips;

        private void Awake()
        {
            if (_sfxSource == null)
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
            }

            _preloadedClips = new Dictionary<string, AudioClip>();
            
            Services.Register(this);
        }

        private void OnDestroy()
        {
            Services.Unregister(this);
        }

        public void PlaySFX(string soundAddress)
        {
            if (_preloadedClips.TryGetValue(soundAddress, out AudioClip clip))
            {
                _sfxSource.PlayOneShot(clip);
            }
            else
            {
                Services.Get<IAssetManager>().Load<AudioClip>(GetFullSoundAddress(soundAddress), PlaySFXAsync);
            }
        }

        private void PlaySFXAsync(AudioClip clip)
        {
            if (_sfxSource.isPlaying) return;
            _sfxSource.PlayOneShot(clip);
        }

        public void PreloadSFXList(params string[] addresses)
        {
            foreach (string address in addresses)
            {
                Services.Get<IAssetManager>().Load<AudioClip>(GetFullSoundAddress(address), clip => OnSFXAssetLoaded(address, clip));
            }
        }
        
        private void OnSFXAssetLoaded(string address, AudioClip clip)
        {
            _preloadedClips[address] = clip;
        }

        private string GetFullSoundAddress(string address) => string.Format(SoundAddress.AddressBase, address);

    }
}