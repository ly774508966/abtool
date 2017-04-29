# Unity AssetBundle tool
## What is this
A handly tool for building, managing, downloading AssetBundle
## Features
1. Add assets to AssetBundle with one click (select folder or select assets)
2. Remove assets from AssetBundle with one click (select folder or select assets)
3. Manager AssetBundle file and Build AssetBundle with ecrypted mode or non-encrypted mode in editor mode
4. Download Assetbundle, manage and check new version of AssetBundle file

## Used
* Unity UGUI to create sample UI
* EditorGUILayout, GUILayout... to create AssetBundle Window editor, Encrypt files in Folder window
* WWW to handle with download AssetBundle
* Free character, effect, scene at Unity Asset Store

## Other Library
+ msgpack-unity (https://github.com/masharada/msgpack-unity) to save and load AssetBundle database

## How to use
### Prepair
* Please create one folder for storing all assets that needed to become AssetBundle under `Assets/` path. Like: `Assets/AssetBundleResources`
* And move all assets that needed to become AssetBundle to this folder. Like: https://gyazo.com/7a717c46e79d38406cb6edc1dd2e5acc
```
Assets/AssetBundleResources/Characters/Minotaur.prefab
Assets/AssetBundleResources/Effects/Snow.preafb
Assets/AssetBundleResources/Weapon/weapons_a.preafb
Assets/AssetBundleResources/Weapon/weapons_b.preafb
Assets/AssetBundleResources/Weapon/weapons_c.preafb
```

### 1. Add assets to AssetBundle with one click (select folder or select assets)
* Select Folder that contain Assets (or Assets) needed to become AssetBundle
* `Right click / AssetBundle Tool/ Add To AssetBundle` -> DONE!
<br/> https://gyazo.com/caeca9ff703fcac602f6eafad6d70403

### 2. Remove assets from AssetBundle with one click (select folder or select assets)
* Select Folder that contain Assets (or Assets) needed to remove from AssetBundle
* `Right click / AssetBundle Tool/ Remove From AssetBundle` -> DONE!
<br/> https://gyazo.com/caeca9ff703fcac602f6eafad6d70403

### 3. Manager AssetBundle file and Build AssetBundle with ecrypted mode or non-encrypted mode in editor mode
* Go `Tools/ AssetBundle Tool/ AssetBundle Window` to open AssetBundle Manager Window (https://gyazo.com/95caeb73757869b4f08cbc3961ac6d19)
* At Assetbundle Manager Window, there are two area: `BUILD ASSETBUNDLE` and `RESOURCES'S DETAIL`(https://gyazo.com/e95413e0c72ee08cc7741eaf69541811)
#### BUILD ASSETBUNDLE
<br/>Used to build assetbundle
1. `Export path`: Where built assetbundle will be exported
2. `File extention`: file extension. Like: *.unity3d or *.abc, *.something
3. `Build Target`: Select target. iOS or Android or StandAlone...
4. Encrypt Assetbundle by select `Encrypt AssetBundle` and input password. 
<br/>https://gyazo.com/9ff0c040ed8b26fbc07102f2b85e592d
<br/>There are two option for setting password. `Load from setting` or `Random`
* `Load from setting`
<br/>Open `AssetBundleSettings.cs` and find `PASSWORD_ENCRYPT_DECRYPT_ASSETBUNDLE` to setup password
* `Random`:
<br/>Just click to it. It will generate random password with lenght in range 8-30 characters
5. When you are ready for building AssetBundle, Just click `Build AssetBundle` and wait
<br/>Building: https://gyazo.com/755ae75ce63dcc3da43435d97e39411c
<br/>Success: https://gyazo.com/df52a5e96f58f87fc6681792b2fa6a80
<br/>Export folder:https://gyazo.com/6509f9a80823d087db7ee3fbcc075a81
<br/>Note: All of default setting were setted in `AssetBundleSettings.cs`. Please edit it to change default setting.
#### RESOURCES'S DETAIL
<br/>Manager AssetBundle file by version, filesize, dependencies of AssetBundle file
https://gyazo.com/073057cc5295f7d7231e73dd2884466f
1. Refresh Data
<br/> Refresh data when new file was add or remove from AssetBundle
2. Clear Local Database
<br/> ABTool create local database to manager AssetBundle and version. So we can clear it and create new. Just carefully!
3. Open Saved AssetBundle Folder 
<br/> Open Local storage at editor mode to check something ...
4. Assets in AssetBundle
<br/>https://gyazo.com/410437539f0de8fb2000e61a87e9470c
<br/> When a new file was added to AssetBundle, this window will show a list of files were added to AssetBundle. It show file path, size of file.. We can sort by size to optimize file.. Just click to cell in list to focus to object.
5. Prebuild AssetBundle Information
<br/>https://gyazo.com/d2f575e41f194d367039fbff87ae6c7a
<br/>Show list of AssetBundle file. File was added to AssetBundle, Dependencies and size of file.
<br/>At this window, we can remove file that no need to be built
6. Local AssetBundle Database
<br/>https://gyazo.com/27c09c925608f6db22e9aeab7e512a4f
<br/>Manager built AssetBundle, size and version of AssetBundle

### 4. Download Assetbundle, manage and check new version of AssetBundle file
<br/>Open `MenuScene` in this project to know how to download AssetBundle
<br/>https://gyazo.com/03bb8d02a6b257528d694f370aeedb5a
<br/>There are two mode: Local or Remote server
1. Local test (Load AssetBundle from StreamingAssets Folder)
* Go to `AssetBundleSettings.cs`, set `IsStreamingAssetsFolderLoad` to `true`
* Copy your exported folder to StreamingAssets Folder
* https://gyazo.com/e5d33e57fe72d046eeec00ad036477ff
2. Remote server
* Please upload exported folder to your host server. For ex: "http://10qpalzma.000webhostapp.com/"
* Go to `AssetBundleSettings.cs`. set `IsStreamingAssetsFolderLoad` to `false`, set `hostURL= your_host_server`. For ex: "http://10qpalzma.000webhostapp.com/"

### Sample code:
* Download All AssetBundle
```
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
```
* Load Object
```
//Load Character
string assetbundle = "assets/examples/assetbundleresources/characters/minotaur";
AssetManager.Instance.GetAssetBundle<GameObject> (assetbundle, 
    (obj)=>{
        GameObject chaObj = GameObject.Instantiate(obj, _unitRoot.transform) as GameObject;
        chaObj.transform.localPosition = Vector3.zero;
    }, (err)=>{
        Debug.Log(err);
    });

//Load effect
string assetbundle = "assets/examples/assetbundleresources/effects/snow";
AssetManager.Instance.GetAssetBundle<GameObject> (assetbundle,
    (obj) => {
        GameObject effect = GameObject.Instantiate(obj, this.transform) as GameObject;
        effect.name = obj.name;
    }, (err) => {
        Debug.Log(err);
    });
```
* Load Scene
```
string assetPath = "assets/examples/assetbundleresources/scene/camfirescene";
UnityEngine.SceneManagement.LoadSceneMode loadSceneMode = LoadSceneMode.Single;
StartCoroutine (AssetBundleManager.Instance.LoadSceneAsync (assetPath, loadSceneMode, onFinish, onFailed));
```
