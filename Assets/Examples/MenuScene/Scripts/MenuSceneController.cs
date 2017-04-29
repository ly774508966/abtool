using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using ABTool;
using System;
using UnityEngine.UI;
using System.IO;

public class MenuSceneController : MonoBehaviour 
{
    [SerializeField] Text _txtDownloadProgress;

    public void ClickDownloadAssetBundles () {
        //Download AssetBundles
        StartCoroutine(DownloadAllAssetBundles (()=>{
            _txtDownloadProgress.text = "Finished! " + AssetBundleManager.Instance.totalSize + "Byte(s)";
        }));
    }

    IEnumerator DownloadAllAssetBundles(Action onFinish) {
        bool isFinishDownload = false;
        bool isSuccess = false;
        _txtDownloadProgress.text = "Processing...";
        StartCoroutine(AssetBundleManager.Instance.DownloadAssetBundlesFromServer (
            ()=>{
                isFinishDownload = true;
                isSuccess = true;
            }, 
            (error)=>{
                isFinishDownload = true;
                isSuccess = false;
                Debug.Log("Error " + error);
            }));

        while (!isFinishDownload) {
            //Update progess
            if (AssetBundleManager.Instance.totalAssetBundleFilesNeedDownload > 0) {
                _txtDownloadProgress.text = "Processed: " + AssetBundleManager.Instance.totalDownloaded + "/" + AssetBundleManager.Instance.totalAssetBundleFilesNeedDownload;
                _txtDownloadProgress.text += "     " + AssetBundleManager.Instance.downloadedSize + "B / " + AssetBundleManager.Instance.totalSize + "B";
            }
            yield return null;
        }

        if (isSuccess) {
            onFinish ();
        }
    }

    public void OnClickDeleteAllDownloadedAssetBundles() {
        if (Directory.Exists (AssetBundleSettings.localFolderPathSaveAB)) {
            Directory.Delete (AssetBundleSettings.localFolderPathSaveAB, true);
        }
        Debug.Log ("<color=red>DONE! All downloaded Assetbundles were DELETED!</color>");
    }



    public void ClickLoadGameObject() {
        SceneManager.LoadSceneAsync ("CharacterScene", LoadSceneMode.Single);
    }

    public void ClickLoadSceneAssetBundle() {
        AssetManager.Instance.LoadScene ("assets/examples/assetbundleresources/scene/camfirescene", LoadSceneMode.Single, 
            (sceneName) => {
                Debug.Log("Load Scene Finished!");
            }, (err) => {
                
            }); 
    }
}
