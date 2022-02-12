using UnityEngine;

namespace RFTestTaskBlocks.UI
{
    public class UIManager : MonoBehaviour, IUIManager
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Transform _menuLayer;
    
        private void Awake()
        {
            Services.Register(this);
        }

        private void OnDestroy()
        {
            Services.Unregister(this);
        }

        public void ShowMenu<T>() where T : Object, IUIPanel
        {
            string menuAddress = GetFullMenuAddress<T>();
            Debug.LogFormat("ShowMenu <b>{0}</b>", menuAddress);
            Services.Get<IAssetManager>().InstantiatePrefab<T>(menuAddress, OnMenuLoaded, _menuLayer);
        }

        private void OnMenuLoaded<T>(T menu) where T : Object, IUIPanel
        {
            Debug.LogFormat("Menu <b>{0}</b> loaded!", GetFullMenuAddress<T>());
            menu.OnOpen();
        }

        private string GetFullMenuAddress<T>() => $"UI.{typeof(T).Name}";
    }

    public interface IUIManager : IGameService
    {
        void ShowMenu<T>() where T : Object, IUIPanel;
    }

    public interface IUIPanel
    {
        void OnOpen();
        void Close();
    }

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