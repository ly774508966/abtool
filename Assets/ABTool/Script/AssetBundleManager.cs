using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MsgPack;
using System.Linq;
using System.IO;

namespace ABTool
{
    /// <summary>
    /// Asetbundle manager.
    /// Manage: Download AssetBundles, Load object from AssetBundle
    /// </summary>
    public class AssetBundleManager : MonoBehaviour 
    {
        private static object _lock = new object();
        public static AssetBundleManager _instance;
        public static AssetBundleManager Instance {
            get {
                lock (_lock) {
                    if (_instance == null) {
                        _instance = (AssetBundleManager) FindObjectOfType(typeof(AssetBundleManager));
                        if (FindObjectsOfType(typeof(AssetBundleManager)).Length > 1) {
                            Debug.LogError("[Singleton] Something went really wrong - there should never be more than 1 singleton!");
                            return _instance;
                        }

                        if (_instance == null) {
                            GameObject singleton = new GameObject();
                            _instance = singleton.AddComponent<AssetBundleManager>();
                            singleton.name = "(singleton) "+ typeof(AssetBundleManager).ToString();
                            DontDestroyOnLoad(singleton);
                        }
                    }

                    return _instance;
                }
            }
        }


        AssetBundleSettings.AssetBundleTargetDB _assetBundleVersionDB;
        private ObjectPacker _objectPacker = new ObjectPacker ();

        void Awake() {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        void OnDestroy() {
            _instance = null;
        }

#region Download_AssetBundles_From_Server
        /// <summary>
        /// List AssetBundle files need to download
        /// </summary>
        List<AssetBundleSettings.AssetBundleInfo> _downloadList;

        /// <summary>
        /// Current index in list of files is downloading
        /// </summary>
        public int currentDownloadIndex { get; private set; }

        /// <summary>
        /// Gets the total AssetBundle files that need to be downloaded
        /// </summary>
        /// <value>The total asset bundle files.</value>
        public int totalAssetBundleFilesNeedDownload { 
            get { 
                if (_downloadList != null) {
                    return _downloadList.Count;
                } else {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Gets total files were downloaded
        /// </summary>
        public int totalDownloaded { get; private set; }

        /// <summary>
        /// Get total size need to be downloaded
        /// </summary>
        public long totalSize { get; private set; }

        /// <summary>
        /// Gets the total size of the downloaded AssetBundles
        /// </summary>
        public long downloadedSize { get; private set; }

        /// <summary>
        /// Downloads AssetBundles from server
        /// </summary>
        /// <returns>The assetbundles from server.</returns>
        /// <param name="onFinish">onFinish callback.</param>
        /// <param name="onFailed">onFailed callback. Return error reason</param>
        public IEnumerator DownloadAssetBundlesFromServer(Action onFinish, Action<string> onFailed) {
            currentDownloadIndex = 0;
            totalDownloaded = 0;
            downloadedSize = 0;
            totalSize = 0;

            bool isSuccess = false;
            bool isDownloadVersionFileFinish = false;
            StartCoroutine (GetAssetBundleVersionFile((fileUrl)=>{
                Debug.Log("Download version file finished: " + fileUrl);
                isSuccess = true;
                isDownloadVersionFileFinish = true;
            }, (downloadPath, error)=>{
                isDownloadVersionFileFinish = true;
                Debug.LogError("Can not download version file from: " + downloadPath);
                onFailed(error);
            }));

            yield return new WaitUntil (() => {
                return isDownloadVersionFileFinish;
            });
            if (!isSuccess) yield break;

            if (_downloadList.Count == 0) {
                Debug.Log ("All AssetBundles were downloaded!");
            }

            bool isError = false;
            List<AssetBundleFileDownload> _downloadProcessList = new List<AssetBundleFileDownload> ();
            //Process download file
            while (true) {
                if (_downloadProcessList.Count < AssetBundleSettings.DOWNLOAD_NUM_FILE_AT_TIME) {
                    if (currentDownloadIndex <= _downloadList.Count - 1) {
                        AssetBundleFileDownload dlObj = new AssetBundleFileDownload (AssetBundleSettings.GetAssetBundleServerURL (), _downloadList [currentDownloadIndex]);
                        _downloadProcessList.Add (dlObj);
                        currentDownloadIndex++;
                    }
                    if (currentDownloadIndex > _downloadList.Count) {
                        currentDownloadIndex = _downloadList.Count;
                    }
                }

                for (int i = 0; i < _downloadProcessList.Count; i++) {
                    AssetBundleFileDownload dlObj = _downloadProcessList[i];
                    if (!dlObj.isProcessing) {
                        DownloadFile (true, dlObj, 
                            (www) => {
                                totalDownloaded ++;
                                downloadedSize += dlObj.GetDownloadedSize();
                                dlObj.Dispose();
                                _downloadProcessList.Remove(dlObj);
                            }, (errMsg) => {
                                isError = true;
                                if(onFailed != null) onFailed(errMsg);
                            });
                    }
                }
                if (isError) {
                    yield break;
                }

                if (totalDownloaded == _downloadList.Count) {
                    yield return null;
                    onFinish ();
                    yield break;
                }
                yield return null;
            }
        }
#endregion

        /// <summary>
        /// Process download AssetBundle file. Each [AssetBundleFileDownload] have retryCount to process if AssetBundle can not be downloaded.
        /// While redownload's times is over retryCount, the downloading is stop, failed will be returned"
        /// </summary>
        void DownloadFile(bool saveDownloadedFileToLocal, AssetBundleFileDownload dlObj, Action<WWW> onFinish, Action<string> onFailed) {
            StartCoroutine (dlObj.DownloadData (
                onFinish, 
                onFailed, () =>{
                    //Retry download again
                    DownloadFile(saveDownloadedFileToLocal, dlObj, onFinish, onFailed);
                }, 
                saveDownloadedFileToLocal, 
                AssetBundleSettings.localFolderPathSaveAB));
        }

#region Load_Asset_Object
        /// <summary>
        /// Loads asset object from AssetBundles
        /// </summary>
        /// <returns>The asset async.</returns>
        /// <param name="assetBundle">AssetBundle file.</param>
        /// <param name="onFinish">onFinish callback. Return object was loaded.</param>
        /// <param name="onFailed">onFailed callback. Return error reason.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public IEnumerator LoadAssetAsync<T>(
            string assetBundle, 
            Action<T> onFinish, 
            Action<string> onFailed) where T : UnityEngine.Object
        {
            if (_assetBundleVersionDB == null) {
                //Load AssetBundle version first
                bool isSuccess = false;
                bool isDownloadVersionFileFinish = false;
                StartCoroutine (GetAssetBundleVersionFile((fileUrl)=>{
                    Debug.Log("Download version file OK " + fileUrl);
                    isSuccess = true;
                    isDownloadVersionFileFinish = true;
                }, (downloadPath, error)=>{
                    isDownloadVersionFileFinish = true;
                    Debug.LogError("Can not download version file from: " + downloadPath);
                    onFailed(error);
                }));

                yield return new WaitUntil (() => {
                    return isDownloadVersionFileFinish;
                });
                if (!isSuccess) yield break;
            }

            //Check AssetBundle existed in version database or not?
            AssetBundleSettings.AssetBundleInfo abInfo = _assetBundleVersionDB.lstAssetBundleInfo.Where(_=>_.assetBundle == assetBundle).FirstOrDefault();
            if (abInfo == null) {
                Debug.Log ("Not found『"+assetBundle+"』in AssetBundles Version Database");
                onFailed ("Not found『"+assetBundle+"』in AssetBundles Version Database");
                yield break;
            }

            //Load from local
            bool isFinish = false;
            bool isLoaded = false;
            StartCoroutine (LoadAssetBundleFromLocalPath<T>(abInfo, 
                (obj)=>{
                    isFinish = true;
                    isLoaded = true;
                    onFinish(obj);
                }, (err)=>{
                    isFinish = true;
                    isLoaded = false;
                    onFailed (err);
                }));
            yield return new WaitUntil (()=>{
                return isFinish;
            });
            if (isLoaded) yield break;

            //Can not be loaded from local, so need download from server
            AssetBundleFileDownload dlObj = new AssetBundleFileDownload (AssetBundleSettings.GetAssetBundleServerURL(), abInfo);
            DownloadFile (true, dlObj, 
                (www) => {
                    string path = AssetBundleSettings.localFolderPathSaveAB + "/" + abInfo.assetBundle + abInfo.extension;
                    StartCoroutine(ExtractAssetObjectFromAssetBundle(www, abInfo, path, onFinish, onFailed));
                    dlObj.Dispose();
                }, (errMsg) => {
                    onFailed (errMsg);
                });
        }

        IEnumerator LoadAssetBundleFromLocalPath<T>(
            AssetBundleSettings.AssetBundleInfo abInfo, 
            Action<T> onFinish, 
            Action<string> onFailed) where T : UnityEngine.Object
        {
            string path = AssetBundleSettings.localFolderPathSaveAB + "/" + abInfo.assetBundle + abInfo.extension;
            if (File.Exists (path)) {
                System.Uri url = new System.Uri (path);
                WWW _www = new WWW (url.AbsoluteUri);
                yield return new WaitUntil (() => {
                    return _www.isDone;
                });
                if (string.IsNullOrEmpty (_www.error)) {
                    StartCoroutine (ExtractAssetObjectFromAssetBundle (_www, abInfo, path, onFinish, onFailed));
                    _www.Dispose ();
                } else {
                    onFailed ("Not Loaded『" + abInfo.assetBundle + "』In LocalPath: " + path);
                }
            } else {
                onFailed ("Not Found『" + abInfo.assetBundle + "』In LocalPath: " + path);
            }
        }

        IEnumerator ExtractAssetObjectFromAssetBundle<T>(
            WWW www, 
            AssetBundleSettings.AssetBundleInfo abInfo, 
            string path, 
            Action<T> onFinish, 
            Action<string> onFailed) where T : UnityEngine.Object
        {
            string assetName = System.IO.Path.GetFileNameWithoutExtension (abInfo.assetBundle);
            AssetBundle assetBundle = null;
            if (AssetBundleSettings.IsEncryptAssetBundle) {
                byte[] decryptedBytes = EncryptDecryptUtil.DecryptBytesToBytes (www.bytes, AssetBundleSettings.PASSWORD_ENCRYPT_DECRYPT_ASSETBUNDLE); 
                AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync (decryptedBytes);
                yield return assetBundleCreateRequest;
                assetBundle = assetBundleCreateRequest.assetBundle;
            } else {
                assetBundle = www.assetBundle;
            }

            // Load the object asynchronously
            AssetBundleRequest request = assetBundle.LoadAssetAsync (assetName);
            // Wait for completion
            while (!request.isDone) {
                yield return null;
            }
            yield return new WaitForEndOfFrame ();
            UnityEngine.Object obj = request.asset;
            if (obj != null) {
                if (obj is Texture2D && typeof(T) == typeof(Sprite)) {
                    Texture2D texture = request.asset as Texture2D;
                    Sprite spr = Sprite.Create (texture, new Rect (0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    onFinish (spr as T);
                } else {
                    onFinish (obj as T);
                }
                new WaitForEndOfFrame ();
                assetBundle.Unload (false);
                yield break;
            } else {
                onFailed ("Can not extract『" + abInfo.assetBundle + "』from LocalPath: " + path);
            }
        }
#endregion

#region Load_Scene
        /// <summary>
        /// Loads the scene async from AssetBundle
        /// </summary>
        /// <returns>The scene async.</returns>
        /// <param name="assetBundle">AssetBundle file.</param>
        /// <param name="loadSceneMode">Load scene mode.</param>
        /// <param name="onFinish">onFinish callback. Return name of scene has just loaded</param>
        /// <param name="onFailed">onFailed callback Return error reason.</param>
        public IEnumerator LoadSceneAsync(
            string assetBundle,
            UnityEngine.SceneManagement.LoadSceneMode loadSceneMode,
            Action<string> onFinish, Action<string> onFailed) 
        {
            bool isSuccess = false;
            bool isProcessFinish = false;
            //Load AssetBundle version first
            if (_assetBundleVersionDB == null) {
                StartCoroutine (GetAssetBundleVersionFile((fileUrl)=>{
                    Debug.Log("Download version file OK " + fileUrl);
                    isSuccess = true;
                    isProcessFinish = true;
                }, (downloadPath, error)=>{
                    isProcessFinish = true;
                    Debug.LogError("Can not download version file from: " + downloadPath);
                    onFailed(error);
                }));

                yield return new WaitUntil (() => {
                    return isProcessFinish;
                });
                if (!isSuccess) yield break;
            }

            //Check AssetBundle existed in version database or not?
            AssetBundleSettings.AssetBundleInfo abInfo = _assetBundleVersionDB.lstAssetBundleInfo.Where(_=>_.assetBundle == assetBundle).FirstOrDefault();
            if (abInfo == null) {
                Debug.Log ("Not found『"+assetBundle+"』in AssetBundles Version Database");
                onFailed ("Not found『"+assetBundle+"』in AssetBundles Version Database");
                yield break;
            }

            //Load from local
            string path = AssetBundleSettings.localFolderPathSaveAB + "/" + abInfo.assetBundle + abInfo.extension;
            if (File.Exists (path)) {
                System.Uri url = new System.Uri (path);
                WWW _www = new WWW (url.AbsoluteUri);
                yield return new WaitUntil (() => {
                    return _www.isDone;
                });
                if (string.IsNullOrEmpty (_www.error)) {
                    isSuccess = false;
                    isProcessFinish = false;
                    StartCoroutine (ExtractSceneNameAndLoadSceneFromAssetBundle (_www, abInfo, path, loadSceneMode, 
                        (sceneName)=>{
                            isSuccess = true;
                            isProcessFinish = true;
                            onFinish(sceneName);
                        }, (err)=>{
                            isSuccess = false;
                            isProcessFinish = true;
                        }));
                    _www.Dispose ();
                    yield return new WaitUntil (() => {
                        return isProcessFinish;
                    });
                    if (isSuccess) yield break;
                } else {
                    onFailed ("Not Found『" + abInfo.assetBundle + "』In LocalPath: " + path);
                    yield break;
                }
            } else {
                Debug.Log ("NOT FOUND " + assetBundle);
            }

            //Can not be loaded from local, so need download from server
            AssetBundleFileDownload dlObj = new AssetBundleFileDownload (AssetBundleSettings.GetAssetBundleServerURL(), abInfo);
            DownloadFile (true, dlObj, 
                (www) => {
                    string pathLocal = AssetBundleSettings.localFolderPathSaveAB + "/" + abInfo.assetBundle + abInfo.extension;
                    StartCoroutine(ExtractSceneNameAndLoadSceneFromAssetBundle(www, abInfo, pathLocal, loadSceneMode, onFinish, onFailed));
                    dlObj.Dispose();
                }, (errMsg) => {
                    onFailed (errMsg);
                });
        }

        IEnumerator ExtractSceneNameAndLoadSceneFromAssetBundle(
            WWW www, 
            AssetBundleSettings.AssetBundleInfo abInfo, 
            string path, 
            UnityEngine.SceneManagement.LoadSceneMode loadSceneMode,
            Action<string> onFinish, 
            Action<string> onFailed)
        {
            // Load the object asynchronously
            AssetBundle assetBundle = null;
            if (AssetBundleSettings.IsEncryptAssetBundle) {
                byte[] decryptedBytes = EncryptDecryptUtil.DecryptBytesToBytes (www.bytes, AssetBundleSettings.PASSWORD_ENCRYPT_DECRYPT_ASSETBUNDLE); 
                AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync (decryptedBytes);
                yield return assetBundleCreateRequest;
                assetBundle = assetBundleCreateRequest.assetBundle;
            } else {
                assetBundle = www.assetBundle;
            }
            if (assetBundle != null) {
                string[] scenePath = assetBundle.GetAllScenePaths ();
                string sceneName = Path.GetFileNameWithoutExtension (scenePath [0]);
                var asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync (sceneName, loadSceneMode);
                yield return asyncLoad;
                onFinish (sceneName);
                assetBundle.Unload (false);
            } else {
                onFailed ("Can not extract『" + abInfo.assetBundle + "』from LocalPath: " + path);
            }
        }
#endregion

#region Get_AssetBundle_Version_File_For_CurrentTargetPlatform
        /// <summary>
        /// Gets the asset bundle version file.
        /// </summary>
        /// <returns>The asset bundle version file.</returns>
        /// <param name="success">Success.</param>
        /// <param name="failed">Failed.</param>
        IEnumerator GetAssetBundleVersionFile (Action<string> success, Action<string, string> failed)
        {
            string abVersionFileUrl = AssetBundleSettings.GetAssetBundleServerURL () + "/" + AssetBundleSettings.ASSETBUNDLE_VERSION_FILE_NAME;
            WWW _www = new WWW (abVersionFileUrl);
            yield return _www;
            if (string.IsNullOrEmpty (_www.error)) {
                if (AssetBundleSettings.IsEncryptAssetBundle) {
                    byte[] decryptedBytes = EncryptDecryptUtil.DecryptBytesToBytes (_www.bytes, AssetBundleSettings.PASSWORD_ENCRYPT_DECRYPT_ASSETBUNDLE); 
                    _assetBundleVersionDB = _objectPacker.Unpack<AssetBundleSettings.AssetBundleTargetDB> (decryptedBytes);
                } else {
                    _assetBundleVersionDB = _objectPacker.Unpack<AssetBundleSettings.AssetBundleTargetDB> (_www.bytes);
                }
                yield return null;
                yield return StartCoroutine(SettingDownloadABList (_www.bytes));
                if (success != null) {
                    success (abVersionFileUrl);
                }
            } else {
                if (failed != null) {
                    failed (abVersionFileUrl, _www.error);
                }
            }
            yield return null;
        }

        /// <summary>
        /// Settings the download AB list. Get total size needed to be downloaded
        /// </summary>
        IEnumerator SettingDownloadABList(byte[] newAssetBundleAssetVersion) {
            //Load local assetbundle version
            string path = AssetBundleSettings.localFolderPathSaveAB + "/" + AssetBundleSettings.ASSETBUNDLE_VERSION_FILE_NAME;
            if (File.Exists (path)) {
                System.Uri url = new System.Uri (path);
                WWW _www = new WWW (url.AbsoluteUri);
                yield return new WaitUntil (() => {
                    return _www.isDone;
                });

                AssetBundleSettings.AssetBundleTargetDB _oldVersionDB = null;
                if (AssetBundleSettings.IsEncryptAssetBundle) {
                    byte[] decryptedBytes = EncryptDecryptUtil.DecryptBytesToBytes (_www.bytes, AssetBundleSettings.PASSWORD_ENCRYPT_DECRYPT_ASSETBUNDLE); 
                    _oldVersionDB = _objectPacker.Unpack<AssetBundleSettings.AssetBundleTargetDB> (decryptedBytes);
                } else {
                    _oldVersionDB = _objectPacker.Unpack<AssetBundleSettings.AssetBundleTargetDB> (_www.bytes);
                }

                _downloadList = new List<AssetBundleSettings.AssetBundleInfo> ();
                foreach (var assetBundleInfo  in _assetBundleVersionDB.lstAssetBundleInfo) {
                    AssetBundleSettings.AssetBundleInfo oldABInfo = _oldVersionDB.lstAssetBundleInfo.Where (_ => _.assetBundle == assetBundleInfo.assetBundle).FirstOrDefault ();
                    if (oldABInfo == null) {
                        _downloadList.Add (assetBundleInfo);
                    } else {
                        if (assetBundleInfo.version != oldABInfo.version) {
                            _downloadList.Add (assetBundleInfo);
                        } else if(assetBundleInfo.hashAssetBundle != oldABInfo.hashAssetBundle) {
                            _downloadList.Add (assetBundleInfo);
                        }
                    }
                }
            } else {
                _downloadList = new List<AssetBundleSettings.AssetBundleInfo> (_assetBundleVersionDB.lstAssetBundleInfo);
            }

            if (!Directory.Exists(AssetBundleSettings.localFolderPathSaveAB)) {
                Directory.CreateDirectory(AssetBundleSettings.localFolderPathSaveAB);
            }
            #if UNITY_IPHONE 
            //No backup to cloud
            UnityEngine.iOS.Device.SetNoBackupFlag(path);
            #endif
            yield return null;
            File.WriteAllBytes(path, newAssetBundleAssetVersion);

            totalSize = 0;
            foreach (var item in _downloadList) {
                totalSize += item.size;
            }

            yield return null;
        }
#endregion
    }
}