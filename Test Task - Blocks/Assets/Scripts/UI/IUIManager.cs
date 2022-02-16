using UnityEngine;

namespace RFTestTaskBlocks.UI
{
    public interface IUIManager : IGameService
    {
        void ShowMenu<T>() where T : Object, IUIPanel;
    }
}