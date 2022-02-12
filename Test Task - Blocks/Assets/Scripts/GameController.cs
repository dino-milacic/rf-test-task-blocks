using RFTestTaskBlocks.UI;

namespace RFTestTaskBlocks
{
    public class GameController
    {
        public void StartGame()
        {
            Services.Get<ISoundManager>().PreloadSFXList(SoundAddress.RobotBlip01, SoundAddress.RobotBlip02,
                SoundAddress.TinyButton, SoundAddress.ScannerBeep);
            Services.Get<IUIManager>().ShowMenu<MainMenu>();
        }

        public void PauseGame()
        {
        }

        public void ResumeGame()
        {
        }

        public void QuitGame()
        {
        }
    }
}