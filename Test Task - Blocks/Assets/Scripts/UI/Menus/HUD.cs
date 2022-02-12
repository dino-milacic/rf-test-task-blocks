namespace RFTestTaskBlocks.UI
{
    public class HUD : BaseUIPanel
    {
        public void Restart()
        {
            Services.Get<ISceneManager>().Reset();
            Services.Get<IUIManager>().ShowMenu<MainMenu>();
            Close();
        }
    }
}