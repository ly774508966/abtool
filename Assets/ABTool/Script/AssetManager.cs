using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace ABTool
{
    /// <summary>
    /// Asset manager. Process load object or scene from AssetBundle, Resources path.
    /// </summary>
    public class AssetManager : MonoBehaviour
    {
        private static object _lock = new object();
        public static AssetManager _instance;
        public static AssetManager Instance {
            get {
                lock (_lock) {
                    if (_instance == null) {
                        _instance = (AssetManager) FindObjectOfType(typeof(AssetManager));
                        if (FindObjectsOfType(typeof(AssetManager)).Length > 1) {
                            Debug.LogError("[Singleton] Something went really wrong - there should never be more than 1 singleton!");
                            return _instance;
                        }

                        if (_instance == null) {
                            GameObject singleton = new GameObject();
                            _instance = singleton.AddComponent<AssetManager>();
                            singleton.name = "(singleton) "+ typeof(AssetManager).ToString();
                            DontDestroyOnLoad(singleton);
                        }
                    }

                    return _instance;
                }
            }
        }

        /// <summary>
        /// Loaded Assets will be cached
        /// </summary>
        Dictionary<string, UnityEngine.Object> _assetCache = new Dictionary<string, UnityEngine.Object> ();

        void Awake ()
        {
            //For caching so dont delete this!
            DontDestroyOnLoad (this);
        }

        void OnDestroy ()
        {
            _instance = null;
        }

        /// <summary>
        /// Clears local cache object
        /// </summary>
        public void ClearCache ()
        {
            List<string> assetKeyList = new List<string> (_assetCache.Keys);
            foreach (var key in assetKeyList) {
                _assetCache [key] = null;
            }
            _assetCache.Clear ();
        }

#region Load_Asset
        /// <summary>
        /// Get asset as Object.
        /// It will check asset in cache, Resources path, or load from AssetBundle 
        /// </summary>
        /// <param name="assetPath">Asset path.</param>
        /// <param name="onLoaded">onLoaded callback. Return object was loaded</param>
        /// <param name="onFailed">onFailed callback. Return error reason</param>
        /// <param name="needCache">If set to <c>true</c> need cache.</param>
        public void GetAsset<T> (
            string assetPath,
            Action<T> onLoaded,
            Action<string> onFailed, bool needCache = true) where T : UnityEngine.Object
        {
            StartCoroutine (GetCoroutine<T> (assetPath, onLoaded, onFailed, needCache));
        }

        IEnumerator GetCoroutine<T> (
            string assetPath,
            Action<T> onLoaded,
            Action<string> onFailed, bool needCache = true) where T : UnityEngine.Object
        {
            //Check cache
            if (_assetCache.ContainsKey (assetPath)) {
                if (onLoaded != null)
                    onLoaded (_assetCache [assetPath] as T);
                yield break;
            }

            //Check resources
            bool isFinish = false;
            bool isExistAsset = false;
            StartCoroutine (LoadAsyncFromResouces<T> (assetPath,
                (obj) => {
                    isFinish = true;
                    isExistAsset = true;
                    onLoaded (obj);
                }, (err) => {
                isFinish = true;
                isExistAsset = false;
            }, needCache));
            yield return new WaitUntil (() => {
                return isFinish;
            });
            if (isExistAsset)
                yield break;

            //Load from assetbundle
            isFinish = false;
            isExistAsset = false;
            GetAssetBundle<T> (assetPath,
                (obj) => {
                    isFinish = true;
                    isExistAsset = true;
                    onLoaded (obj);
                }, (err) => {
                    isFinish = true;
                    isExistAsset = false;
            }, needCache);
            yield return new WaitUntil (() => {
                return isFinish;
            });
            if (!isExistAsset) {
                onFailed ("Can not load asset 『" + assetPath + "』. Asset was not existed in Resources or AssetBundles. Please check again!");
            }
        }

        /// <summary>
        /// Gets asset from AssetBundle.
        /// </summary>
        /// <returns>The asset bundle.</returns>
        /// <param name="assetPath">Asset path.</param>
        /// <param name="onLoaded">onLoaded callback. Return object was loaded</param>
        /// <param name="onFailed">onFailed callback Return error reason</param>
        /// <param name="needCache">If set to <c>true</c> need cache.</param>
        public void GetAssetBundle<T> (
            string assetPath,
            Action<T> onLoaded,
            Action<string> onFailed,
            bool needCache = true) where T : UnityEngine.Object
        {
            //Check cache
            if (_assetCache.ContainsKey (assetPath)) {
                if (onLoaded != null) onLoaded (_assetCache [assetPath] as T);
                return;
            }

            StartCoroutine (AssetBundleManager.Instance.LoadAssetAsync<T> (assetPath, (obj) => {
                if (needCache) {
                    SaveAssetToCache (assetPath, obj);
                }
                onLoaded (obj);
            }, onFailed));
        }
#endregion

#region Load_Scene_From_AssetBundle
        /// <summary>
        /// Load scene from AssetBundle.
        /// </summary>
        /// <param name="assetPath">Asset path.</param>
        /// <param name="loadSceneMode">Load scene mode.</param>
        /// <param name="onFinish">onFinish callback. Return name of scene has just loaded</param>
        /// <param name="onFailed">onFailed callback. Return error reason</param>
        public void LoadScene (
            string assetPath,
            UnityEngine.SceneManagement.LoadSceneMode loadSceneMode,
            Action<string> onFinish, Action<string> onFailed)
        {
            StartCoroutine (AssetBundleManager.Instance.LoadSceneAsync (assetPath, loadSceneMode, onFinish, onFailed));
        }
#endregion

#region Load_From_Resource
        /// <summary>
        /// Loads the async object from resouces.
        /// </summary>
        /// <returns>The object was loaded (async) from resouces.</returns>
        /// <param name="assetPath">Asset path.</param>
        /// <param name="onLoaded">onLoaded callback. Return object was loaded</param>
        /// <param name="onFailed">onFailed callback. Return error reason</param>
        /// <param name="needCache">If set to <c>true</c> need cache.</param>
        public IEnumerator LoadAsyncFromResouces<T> (
            string assetPath,
            Action<T> onLoaded,
            Action<string> onFailed, bool needCache = true) where T : UnityEngine.Object
        {
            if (_assetCache.ContainsKey (assetPath)) {
                if (onLoaded != null)
                    onLoaded (_assetCache [assetPath] as T);
                yield break;
            }
            ResourceRequest rq = Resources.LoadAsync (assetPath);
            yield return new WaitUntil (() => {
                return rq.isDone;
            });
            if (rq.asset != null) {
                if (needCache) {
                    SaveAssetToCache (assetPath, rq.asset);
                }

                if (rq.asset is Texture2D) {
                    Texture2D texture = rq.asset as Texture2D;
                    Sprite spr = Sprite.Create (texture, new Rect (0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    onLoaded (spr as T);
                } else {
                    onLoaded (rq.asset as T);
                }
            } else {
                onFailed ("Can not load『" + assetPath + "』from Resources");
            }
        }

        /// <summary>
        /// Loads object from resouces.
        /// </summary>
        /// <returns>The object was loaded from resouces.</returns>
        /// <param name="assetPath">Asset path.</param>
        /// <param name="needCache">If set to <c>true</c> need cache.</param>
        public T LoadFromResouces<T> (string assetPath, bool needCache = true) where T : UnityEngine.Object
        {
            if (_assetCache.ContainsKey (assetPath)) {
                return _assetCache [assetPath] as T;
            }
            UnityEngine.Object obj = Resources.Load<UnityEngine.Object> (assetPath);
            if (needCache) {
                SaveAssetToCache (assetPath, obj);
            }

            if (obj is Texture2D && typeof(T) == typeof(Sprite)) {
                Texture2D texture = obj as Texture2D;
                Sprite spr = Sprite.Create (texture, new Rect (0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                return (spr as T);
            }

            return (obj as T);
        }
#endregion

        void SaveAssetToCache (string assetPath, UnityEngine.Object asset)
        {
            if (_assetCache.ContainsKey (assetPath)) {
                _assetCache [assetPath] = asset;
            } else {
                _assetCache.Add (assetPath, asset);
            }
        }
    }
}