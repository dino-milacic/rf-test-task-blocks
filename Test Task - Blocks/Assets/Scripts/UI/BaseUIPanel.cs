using UnityEngine;

namespace RFTestTaskBlocks.UI
{
    public abstract class BaseUIPanel : MonoBehaviour, IUIPanel
    {
        public virtual void OnOpen() {}

        public void Close()
        {
            OnClose();
            Destroy(gameObject);
        }
        
        protected virtual void OnClose() {}
    }
}