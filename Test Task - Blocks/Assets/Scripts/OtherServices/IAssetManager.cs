using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace RFTestTaskBlocks
{
    public interface IAssetManager : IGameService
    {
        void Load<T>(string address, Action<T> onAssetLoaded = null) where T : Object;

        void InstantiatePrefab<T>(string address, Action<T> onPrefabLoaded = null,
            Transform parent = null, bool worldSpace = false) where T : Object;

        void Release(GameObject gameObject);
        void Release<T>(AsyncOperationHandle<T> handle) where T : Object;
        void UnloadAll();
    }
}