using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace RFTestTaskBlocks
{
    public interface IAssetManager : IGameService
    {
        AsyncOperationHandle<T> Load<T>(string address, Action<T> onAssetLoaded = null) where T : Object;

        AsyncOperationHandle InstantiatePrefab<T>(string address, Action<T> onPrefabLoaded = null,
            Transform parent = null, bool worldSpace = false) where T : Object;

        void Release(GameObject gameObject);
        void Release<T>(AsyncOperationHandle<T> handle) where T : Object;
        void UnloadAll();
    }
    
    
    public class AssetManager : IAssetManager, IDisposable
    {
        private readonly List<AsyncOperationHandle> _loadHandles = new List<AsyncOperationHandle>();
        
        public AssetManager()
        {
            Services.Register(this);
        }
        
        public void Dispose()
        {
            Services.Unregister(this);
            UnloadAll();
        }

        public AsyncOperationHandle<T> Load<T>(string address, Action<T> onAssetLoaded = null) where T : Object
        {
            AsyncOperationHandle<T> operation = Addressables.LoadAssetAsync<T>(address);
            operation.Completed += (o) => AssetLoaded(o, onAssetLoaded);
            _loadHandles.Add(operation);
            return operation;
        }

        public AsyncOperationHandle InstantiatePrefab<T>(string address, Action<T> onPrefabLoaded = null,
            Transform parent = null, bool worldSpace = false) where T : Object
        {
            AsyncOperationHandle<GameObject> operation = Addressables.InstantiateAsync(address, parent, worldSpace);
            operation.Completed += (o) => PrefabAssetLoaded(o, onPrefabLoaded);
            return operation;
        }

        public void Release(GameObject gameObject)
        {
            Transform transform = gameObject.transform;
            if (transform != null) transform.SetParent(null);

            if (!Addressables.ReleaseInstance(gameObject))
            {
                Object.Destroy(gameObject);
            }
        }

        public void Release<T>(AsyncOperationHandle<T> handle) where T : Object
        {
            if (Addressables.ReleaseInstance(handle)) return;
            
            Resources.UnloadAsset(handle.Result);
        }

        public void UnloadAll()
        {
            foreach (AsyncOperationHandle handle in _loadHandles)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            
            _loadHandles.Clear();
        }

        private void AssetLoaded<T>(AsyncOperationHandle<T> operation, Action<T> onLoaded) where T : Object
        {
            if (operation.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogWarningFormat("Failed to load: {0}", operation);
                return;
            }

            onLoaded?.Invoke(operation.Result);
            return;
        }

        private void PrefabAssetLoaded<T>(AsyncOperationHandle<GameObject> operation, Action<T> onLoaded) where T : UnityEngine.Object
        {
            if (operation.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogWarningFormat("Failed to load: {0}", operation);
                return;
            }
            
            onLoaded?.Invoke(operation.Result.GetComponent<T>());
            return;
        }
    }
}