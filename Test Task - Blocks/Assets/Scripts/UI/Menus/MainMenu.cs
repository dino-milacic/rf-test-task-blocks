
using System.Collections.Generic;

namespace RFTestTaskBlocks.UI
{
    public class MainMenu : BaseUIPanel
    {
        public void StartDefaultMode()
        {
            ISceneManager sceneManager = Services.Get<ISceneManager>();

            sceneManager.Configure(12, 1, 10, new List<BlockColor>{BlockColor.Red, BlockColor.Blue});
            sceneManager.SpawnContainer(0f, BlockColor.Red, ContainerController.ContainerOrientation.Right);
            sceneManager.SpawnContainer(1f, BlockColor.Blue, ContainerController.ContainerOrientation.Left);

            for (int i = 0; i < sceneManager.NumberOfRobots; i++)
            {
                sceneManager.SpawnRobot(i);
            }

            for (int i = 0; i < sceneManager.NumberOfBlocks; i++)
            {
                sceneManager.SpawnBlock();
            }
            
            Services.Get<IUIManager>().ShowMenu<HUD>();
            Close();
        }

        public void StartCustomMode()
        {
            Services.Get<IUIManager>().ShowMenu<CustomModeMenu>();
            Close();
        }
    }
}