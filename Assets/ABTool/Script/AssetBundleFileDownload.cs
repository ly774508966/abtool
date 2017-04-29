using UnityEngine;
using System.Collections;
using System;
using System.IO;

namespace ABTool
{
    /// <summary>
    /// Asset bundle file download.
    /// </summary>
    public class AssetBundleFileDownload 
    {
        const int RETRY_DOWNLOAD_NUM = 3;
        WWW _www;
        int _retryCount;
        string _urlStr;

        /// <summary>
        /// AssetBundle file information
        /// </summary>
        /// <value>The file info.</value>
        public AssetBundleSettings.AssetBundleInfo fileInfo { get; private set; }
        public bool isProcessing { get; private set; }

        public AssetBundleFileDownload(string urlStr, AssetBundleSettings.AssetBundleInfo abFileInfo)
        {
            _www = null;
            _urlStr = urlStr;
            if (!_urlStr.EndsWith ("/")) {
                _urlStr += "/";
            }
            _retryCount = 0;
            isProcessing = false;
            fileInfo = abFileInfo;
        }

        public void Dispose() {
            if (_www != null) {
                _www.Dispose ();
                _www = null;
            }
        }

        /// <summary>
        /// Gets the size of the downloaded.
        /// </summary>
        /// <returns>The downloaded size.</returns>
        public long GetDownloadedSize()
        {
            if (_www == null) return 0;
            return fileInfo.size;
        }

        /// <summary>
        /// Downloads the data.
        /// </summary>
        /// <returns>The data.</returns>
        /// <param name="onFinish">On finish.</param>
        /// <param name="onFailed">On failed.</param>
        /// <param name="onRetry">On retry.</param>
        /// <param name="writeLocal">If set to <c>true</c> write local.</param>
        /// <param name="localPath">Local path.</param>
        public IEnumerator DownloadData(Action<WWW> onFinish, Action<string> onFailed, Action onRetry, bool writeLocal = true, string localPath = "") {
            isProcessing = true;
            string wwwStr = _urlStr + fileInfo.assetBundle + fileInfo.extension;

            _www = new WWW(wwwStr);
            yield return _www;

            if (_www == null || !string.IsNullOrEmpty (_www.error)) {
                if (_retryCount >= RETRY_DOWNLOAD_NUM) {
                    if (onFailed != null)
                        onFailed ("Can not download " + wwwStr + " ver:" + fileInfo.version);
                } else {
                    _retryCount++;
                    Dispose ();
                    if (onRetry != null)
                        onRetry ();
                }
            } else {
                if (writeLocal) {
                    SaveFileToLocal (localPath);
                }
                string logText = string.Format ("WWW success! url={0} size={1} bytesDownloaded={2}", _www.url, _www.bytes.Length, _www.bytesDownloaded);
                Debug.Log(logText);

                onFinish (_www);
            }
        }

        void SaveFileToLocal(string localPath) {
            if (!localPath.EndsWith ("/")) {
                localPath += "/";
            }
            string path = localPath + fileInfo.assetBundle + fileInfo.extension;
            int lastIdx = path.LastIndexOf ("/");
            string dirPath = path.Substring( 0, lastIdx );

            if (!Directory.Exists( dirPath)) {
                Directory.CreateDirectory(dirPath);
            }

            byte[] bytes = _www.bytes;
            File.WriteAllBytes(path, bytes);
            #if UNITY_IPHONE 
            //No backup to cloud
            UnityEngine.iOS.Device.SetNoBackupFlag(path);
            #endif
        }
    }
}