using UnityEngine;
using UnityEngine.UI;

namespace RFTestTaskBlocks.UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonClickSFXPlayer : MonoBehaviour
    {
        private Button _button;
        private ISoundManager _soundManager;

        [SerializeField] private string _sfxAddress;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClicked);
            _soundManager = Services.Get<ISoundManager>();
        }

        private void OnButtonClicked()
        {
            if (_soundManager == null) return;

            string address = string.IsNullOrEmpty(_sfxAddress) ? SoundAddress.TinyButton : _sfxAddress;
            _soundManager.PlaySFX(address);
        }
    }
}