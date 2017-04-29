using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ABTool
{
    /// <summary>
    /// AssetBundle settings
    /// </summary>
    public class AssetBundleSettings
    {
#region Build_AssetBundle_Setting
        /// <summary>
        /// After AssetBundles were build with AssetBundle Tool, this folder will be created automatically.
        /// And all built Assetbunles were put inside this folder (for all platform).
        /// Please copy this folder and it's content to server.
        /// You can change this folder name to fit with your game
        /// </summary>
        public const string ASSETBUNDLE_ROOT_STORAGE = "MyGameNameAssetBundle";

        /// <summary>
        /// After AssetBundles were build with AssetBundle Tool with encryption, this folder will be created automatically.
        /// And all built Assetbunles were put inside this folder (for all platform).
        /// Please copy this folder and it's content to server.
        /// You can change this folder name to fit with your game
        /// </summary>
        public const string ASSETBUNDLE_ROOT_STORAGE_ENCRYPT = "MyGameNameAssetBundleEncrypt";

        /// <summary>
        /// This file manages version, name, hash... of AssetBundles
        /// This file will be downloaded to local device first then 
        /// system will check AssetBundles's information and determine to download other AssetBundle.
        /// You can change filename to fit with your game
        /// </summary>
        public const string ASSETBUNDLE_VERSION_FILE_NAME = "assetsBundleVersion.dat";

        /// <summary>
        /// AssetBundle file extension.
        /// You can change extension to fit with your game
        /// </summary>
        public const string AssetBundleExtension = "unity3d";

        /// <summary>
        /// After assetbundles were built, ASSETBUNDLE_ROOT_STORAGE and ASSETBUNDLE_ROOT_STORAGE_ENCRYPT folder will be created at this path as default path
        /// So please select your own path in AssetBundle Tool Editor Window
        /// </summary>
        public static string DefaultExportAssetBundlePath {
            get {
                return Application.dataPath.Replace ("/Assets", "");
            }
        }

        //Each target build will have each folder that contain built assetbundles
        #if UNITY_EDITOR
        public static string GetPlatformFolderForBuildTarget(BuildTarget buildTarget)
        {
            switch (buildTarget) {
            case BuildTarget.iOS:
                return "iOS";
            case BuildTarget.Android:
                return "Android";

            case BuildTarget.PS3:
                return "PS3";
            case BuildTarget.XBOX360:
                return "XBOX360";
            case BuildTarget.XboxOne:
                return "XboxOne";
            case BuildTarget.PSP2:
                return "PSP2";
            case BuildTarget.PS4:
                return "PS4";
            case BuildTarget.PSM:
                return "PSM";
            case BuildTarget.Tizen:
                return "Tizen";
            case BuildTarget.SamsungTV:
                return "SamsungTV";
            case BuildTarget.WiiU:
                return "WiiU";
            case BuildTarget.tvOS:
                return "tvOS";
            default:
                return "StandAlone";
            }
        }
        #endif
#endregion


#region DownLoad_&_Load_AssetBundle_Setting

        /// <summary>
        /// Encrypt AssetBundles flag
        /// </summary>
        public static bool IsEncryptAssetBundle = true;

        /// <summary>
        /// Password to decrypt encrypted AssetBundle files. You can change to fit with your game
        /// </summary>
        public const string PASSWORD_ENCRYPT_DECRYPT_ASSETBUNDLE = "!*2rwMnr8*ov84";

        /// <summary>
        /// If it is setted to TRUE, we can simulator interacting with AssetBundle from StreamingAssets folder for local test
        /// Please save your exported folder [ASSETBUNDLE_ROOT_STORAGE] or [ASSETBUNDLE_ROOT_STORAGE_ENCRYPT] to StreamingAssets folder and load from it without server
        /// If it is setted to FALSE, we will interact with Assetbunle that saved in server
        /// Please save your exported folder [ASSETBUNDLE_ROOT_STORAGE] or [ASSETBUNDLE_ROOT_STORAGE_ENCRYPT] to server and set value to [hostURL]
        /// </summary>
        public static bool IsStreamingAssetsFolderLoad = true;

        /// <summary>
        /// Url that contain exported folder [ASSETBUNDLE_ROOT_STORAGE] or [ASSETBUNDLE_ROOT_STORAGE_ENCRYPT] at server
        /// Ex: http://game-dev.com/
        /// </summary>
        public static string hostURL = "http://10qpalzma.000webhostapp.com/";

        /// <summary>
        /// Full path URL to folder that contain AssetBundle files, Assetbunder version database
        /// Ex: http://game-dev.com/[ASSETBUNDLE_ROOT_STORAGE or ASSETBUNDLE_ROOT_STORAGE_ENCRYPT]/iOS/
        /// </summary>
        public static string GetAssetBundleServerURL () {
            if (IsStreamingAssetsFolderLoad) {
                if (IsEncryptAssetBundle) {
                    return "file://" + Application.streamingAssetsPath + "/" + ASSETBUNDLE_ROOT_STORAGE_ENCRYPT + "/" + GetPlatformFolder ();
                } else {
                    return "file://" + Application.streamingAssetsPath + "/" + ASSETBUNDLE_ROOT_STORAGE + "/" + GetPlatformFolder ();
                }
            } else {
                if (IsEncryptAssetBundle) {
                    return hostURL + ASSETBUNDLE_ROOT_STORAGE_ENCRYPT + "/" + GetPlatformFolder ();
                } else {
                    return hostURL + ASSETBUNDLE_ROOT_STORAGE + "/" + GetPlatformFolder ();
                }
            }
        }

        /// <summary>
        /// For download AssetBundle files.
        /// How many Assetbunde files that we want to download at the same time
        /// </summary>
        public const int DOWNLOAD_NUM_FILE_AT_TIME = 10;

        /// <summary>
        /// Downloaded Assetbunde files will be stored in this path of local device
        /// </summary>
        public static string localFolderPathSaveAB {
            get { return Application.persistentDataPath + "/" + LOCAL_DOWNLOADED_ASSETBUNDLE_FOLDER; }
        }

        /// <summary>
        /// After assetbundle files was downloaded, it will be stored in local device's path, inside this folder name
        /// </summary>
        public const string LOCAL_DOWNLOADED_ASSETBUNDLE_FOLDER = "assetbundlefolder";


        /// <summary>
        /// Base on current Platform, we get folder name that contain assetbundle
        /// </summary>
        /// <returns>The platform folder.</returns>
        public static string GetPlatformFolder () {
            switch (Application.platform) {
            #if UNITY_EDITOR
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.WindowsEditor:
                return GetPlatformFolderForBuildTarget(UnityEditor.EditorUserBuildSettings.activeBuildTarget);
            #endif

            case RuntimePlatform.Android:
                return "Android";
            case RuntimePlatform.IPhonePlayer:
                return "iOS";
            case RuntimePlatform.PS3:
                return "PS3";
            case RuntimePlatform.XBOX360:
                return "XBOX360";
            case RuntimePlatform.XboxOne:
                return "XboxOne";
            case RuntimePlatform.PSP2:
                return "PSP2";
            case RuntimePlatform.PS4:
                return "PS4";
            case RuntimePlatform.PSM:
                return "PSM";
            case RuntimePlatform.TizenPlayer:
                return "Tizen";
            case RuntimePlatform.SamsungTVPlayer:
                return "SamsungTV";
            case RuntimePlatform.WiiU:
                return "WiiU";
            case RuntimePlatform.tvOS:
                return "tvOS";
            default:
                return "StandAlone";
            }
        }
#endregion

        /// <summary>
        /// Manage AssetBundles's info, size, version
        /// Can calculate fize size, time needed to download
        /// Can check new version of assetbundle to know we need to update or not
        /// </summary>
        public class AssetBundleInfo
        {
            public string assetBundle = "";
            public string hashAssetBundle = "";
            public string hashAssetsContent = "";
            public int version = 0;
            public long size = 0;
            public string extension = "";
        }

        /// <summary>
        /// With different targets we have different info about AssetBundles
        /// </summary>
        public class AssetBundleTargetDB
        {
            public string buildTarget;
            public List<AssetBundleInfo> lstAssetBundleInfo;
        }

        #region AssetBundle_Database_Store_AllTarget_AssetBundle_Information
        /// <summary>
        /// After Assetbunle files were built, AssetBundles Database will be generated and will be stored at this path
        /// </summary>
        public static string ExportAssetBundleDatabasePath {
            get {
                return Application.persistentDataPath;
            }
        }

        /// <summary>
        /// AssetBundle database name
        /// Store all information about target's build: assetbundles version, hash
        /// </summary>
        public const string AssetBundleDatabaseName = "AssetBundleDB";

        /// <summary>
        /// Asetbundle database extension
        /// </summary>
        public const string AssetBundleDatabaseExtension = "dbo";
        #endregion
    }
}