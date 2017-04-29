using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MsgPack;

namespace ABTool
{
    /// <summary>
    /// Asset bundle tool editor.
    /// </summary>
    public class AssetBundleToolEditor : EditorWindow
    {
        int _selectedTabInfo = 0;

        //BUILD
        string _exportLocation = "";
        string _bundleFileExtension = "";
        BuildTarget buildTarget = BuildTarget.iOS;

        bool isShowAdvanceSetting = false;
        string passwordEncrypt = "";

        static float _windowWidth;
        static float _windowHeight;
        static float _wrapScrollViewHeight;

        static Rect lastRect;

        public static Color preColor;
        public static Color preBkgrColor;

        static AssetBundleToolEditor _toolViewWindow;

        Vector2 scrollPosViewDB = Vector2.zero;

        [MenuItem ("Tools/AssetBundle Tool/AssetBundle Window")]
        static void ShowAssetBundleList ()
        {
            _toolViewWindow = EditorWindow.GetWindow<AssetBundleToolEditor> ("AssetBundle");
        }

        void OnEnable ()
        {
            preColor = GUI.color;
            preBkgrColor = GUI.backgroundColor;
            if (_exportLocation == "")
                _exportLocation = AssetBundleSettings.DefaultExportAssetBundlePath;
            if (_bundleFileExtension == "")
                _bundleFileExtension = AssetBundleSettings.AssetBundleExtension;

            AssetDetailView.InitData ();
            AssetBundleDetailView.InitData ();
        }

        void OnGUI ()
        {
            if (_toolViewWindow == null) {
                _toolViewWindow = EditorWindow.GetWindow<AssetBundleToolEditor> ("AssetBundle");
            }
            _windowWidth = _toolViewWindow.position.width;
            _windowHeight = _toolViewWindow.position.height;

            InitAssetBundleBuild ();
            ShowResourcesDetail ();
        }

        void ShowResourcesDetail ()
        {
            EditorGUILayout.TextArea ("", GUI.skin.horizontalSlider);
            GUILayout.BeginHorizontal ();
            {
                GUILayout.FlexibleSpace ();
                GUILayout.Label ("RESOURCES'S DETAIL", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace ();
            }
            GUILayout.EndHorizontal ();
            EditorGUILayout.TextArea ("", GUI.skin.horizontalSlider);

            GUILayout.BeginHorizontal ();
            {
                if (GUILayout.Button ("Refresh Data", GUILayout.Height (20))) {
                    AssetDatabase.RemoveUnusedAssetBundleNames ();
                    AssetDetailView.InitData ();
                    AssetBundleDetailView.InitData ();
                    LocalAssetBundleView.InitData ();
                    Debug.Log ("Refresh data DONE!");
                }
                if (GUILayout.Button ("Clear Local Database", GUILayout.Height (20))) {
                    if (EditorUtility.DisplayDialog (
                        "Delete local AssetBundle Database", 
                        "Do you want to DELETE local Database? (All AssetBundle files 's version will be cleared!)", "Delete", "Close")) {
                        string dbLocaltion = AssetBundleSettings.ExportAssetBundleDatabasePath + "/" + AssetBundleSettings.AssetBundleDatabaseName + "." + AssetBundleSettings.AssetBundleDatabaseExtension;
                        File.Delete (dbLocaltion);
                        LocalAssetBundleView.InitData ();
                        Debug.Log ("<size=20>Local database was deleted...</size>");
                    }
                }
                if (GUILayout.Button ("Open Saved AssetBundles Folder", GUILayout.Height (20))) {
                    EditorUtility.RevealInFinder (AssetBundleSettings.ExportAssetBundleDatabasePath);
                }
            }
            GUILayout.EndHorizontal ();
            EditorGUILayout.TextArea ("", GUI.skin.horizontalSlider);

            _selectedTabInfo = Tabs (new string[] {
                "Assets In AssetBundle",
                "Prebuild AssetBundle Information",
                "Local AssetBundle Database"
            }, _selectedTabInfo);

            if(Event.current.type == EventType.Repaint) lastRect = GUILayoutUtility.GetLastRect();
            _wrapScrollViewHeight = _windowHeight - lastRect.y - lastRect.height;

            scrollPosViewDB = EditorGUILayout.BeginScrollView (scrollPosViewDB, false, false, GUILayout.Width (_windowWidth), GUILayout.Height (_wrapScrollViewHeight));
            {
                if (_selectedTabInfo == 0) {
                    AssetDetailView.ShowView ();
                } else if (_selectedTabInfo == 1) {
                    AssetBundleDetailView.ShowView ();
                } else if (_selectedTabInfo == 2) {
                    LocalAssetBundleView.ShowView ();
                }
            }
            EditorGUILayout.EndScrollView ();
        }

        //Build
        void InitAssetBundleBuild ()
        {
            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            GUILayout.Label ("BUILD ASSETBUNDLE", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal ();
            GUILayout.Label ("", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal ();
            _exportLocation = EditorGUILayout.TextField ("Export Path", _exportLocation);
            if (GUILayout.Button ("...", GUILayout.Width (30), GUILayout.Height (15))) {
                GUI.FocusControl (null);
                _exportLocation = EditorUtility.SaveFolderPanel ("Select Output Folder", AssetBundleSettings.DefaultExportAssetBundlePath, "AssetBundles");
            }
            GUILayout.EndHorizontal ();
            _bundleFileExtension = EditorGUILayout.TextField ("File Extension", _bundleFileExtension);

            buildTarget = (BuildTarget)EditorGUILayout.EnumPopup ("Build Target", buildTarget);

            EditorGUILayout.BeginVertical (EditorStyles.helpBox);
            isShowAdvanceSetting = EditorGUILayout.Toggle ("Encrypt AssetBundles", isShowAdvanceSetting);
            if (isShowAdvanceSetting) {
                GUILayout.BeginHorizontal ();
                passwordEncrypt = EditorGUILayout.TextField ("Password Encrypt", passwordEncrypt);
                if (GUILayout.Button ("LoadFromSetting", GUILayout.Width (120), GUILayout.Height (15))) {
                    passwordEncrypt = AssetBundleSettings.PASSWORD_ENCRYPT_DECRYPT_ASSETBUNDLE;
                }
                if (GUILayout.Button ("Random", GUILayout.Width (90), GUILayout.Height (15))) {
                    GUI.FocusControl (null);
                    int ranlen = Random.Range (8, 30);
                    passwordEncrypt = EncryptDecryptUtil.RandomString (ranlen);
                }
                GUILayout.EndHorizontal ();
            }
            GUILayout.EndVertical ();

            GUILayout.Label ("", EditorStyles.boldLabel);
            if (GUILayout.Button ("Build AssetBundle", GUILayout.Height (40))) {
                bool readyBuild = true;
                if (_exportLocation == "") {
                    Debug.LogError ("『Export AB Path』Was Empty. Please Input Location to Export AssetBundle");
                    readyBuild = false;
                }

                if (readyBuild) {
                    if (string.IsNullOrEmpty (_bundleFileExtension)) {
                        ABBuilder.BuildAssetBundles (
                            _exportLocation,
                            buildTarget,
                            _bundleFileExtension,
                            AssetBundleSettings.ExportAssetBundleDatabasePath,
                            AssetBundleSettings.AssetBundleDatabaseName + "." + AssetBundleSettings.AssetBundleDatabaseExtension, isShowAdvanceSetting, passwordEncrypt);
                    } else {
                        ABBuilder.BuildAssetBundles (
                            _exportLocation,
                            buildTarget,
                            "." + _bundleFileExtension,
                            AssetBundleSettings.ExportAssetBundleDatabasePath,
                            AssetBundleSettings.AssetBundleDatabaseName + "." + AssetBundleSettings.AssetBundleDatabaseExtension, isShowAdvanceSetting, passwordEncrypt);
                    }
                }
            }
        }

        public static int Tabs (string[] options, int selected)
        {
            GUILayout.BeginHorizontal ();
            for (int i = 0; i < options.Length; ++i) {
                if (GUILayout.Toggle (selected == i, options [i], EditorStyles.toolbarButton)) {
                    selected = i; //Tab click
                }
            }
            GUILayout.EndHorizontal ();
            return selected;
        }

        #region AssetDetailView

        public class AssetDetailView
        {
            public class AssetInfo
            {
                public string assetPath;
                public long size;
                public Object obj;

                public bool isDependecies = false;
                public bool isEditFlagOn;

                public AssetInfo (string assetPath, long size, Object obj)
                {
                    this.assetPath = assetPath;
                    this.size = size;
                    this.obj = obj;
                }
            }

            public class AssetStatisticGroupInfo
            {
                public string classType;
                public int totalFile;
                public long totalSize;
            }

            static List<AssetInfo> _assetInfoList;
            static List<AssetInfo> _assetInfoListSearch;
            static List<AssetInfo> _assetInfoListSorted;

            static int _sizeMode = 0;           //!< 0: no sort, 2:large to small, 1; small to large
            static string _sizeModeText = "Size";
            static string _searchString = "";
            static string _preSearchString = "";

            public static void InitData ()
            {
                if (_assetInfoList == null)
                    _assetInfoList = new List<AssetInfo> ();
                _assetInfoList.Clear ();

                foreach (var item in AssetDatabase.GetAllAssetBundleNames()) {
                    var assetPathList = AssetDatabase.GetAssetPathsFromAssetBundle (item);
                    if (assetPathList.Length > 0) {
                        foreach (var asset in assetPathList) {
                            string assetPath = asset;
                            long size = GetSizeInBytesOfAsset (asset);
                            Object obj = AssetDatabase.LoadAssetAtPath<Object> (assetPath);
                            AssetInfo assetInfo = new AssetInfo (assetPath, size, obj);
                            _assetInfoList.Add (assetInfo);
                        }
                    }
                }

                _searchString = "";
                _assetInfoListSearch = new List<AssetInfo> (_assetInfoList);
                _sizeMode = 0;
                _assetInfoListSorted = new List<AssetInfo> (_assetInfoListSearch);
            }

            public static void ShowView ()
            {
                if (_assetInfoList.Count == 0) {
                    GUILayout.Label ("", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal ();
                    {
                        GUILayout.FlexibleSpace ();
                        GUILayout.Label ("NO DATA", EditorStyles.boldLabel);
                        GUILayout.FlexibleSpace ();
                    }
                    EditorGUILayout.EndHorizontal ();
                    GUILayout.Label ("", EditorStyles.boldLabel);
                    return;
                }

                EditorGUILayout.BeginVertical (EditorStyles.helpBox);
                {
                    SearchUI ();
                    UpdateSortMode ();
                    var indexWidth = 30f;
                    var assetSizeWidth = 50;
                    var assetRemoveFlagWidth = 20;
                    var assetPathWidth = _windowWidth - indexWidth - assetSizeWidth - assetRemoveFlagWidth - 50;

                    //Header
                    EditorGUILayout.BeginHorizontal ();
                    {
                        GUILayout.Label (" ID", EditorStyles.boldLabel, GUILayout.Width (indexWidth));
                        GUILayout.Label ("  AssetPath", EditorStyles.boldLabel, GUILayout.Width (assetPathWidth));
                        if (GUILayout.Button (_sizeModeText, GUILayout.Width (assetSizeWidth), GUILayout.Height (20))) {
                            _sizeMode++;
                            if (_sizeMode > 2)
                                _sizeMode = 0;
                            UpdateSortMode ();
                        }
                        GUILayout.Label ("", EditorStyles.boldLabel, GUILayout.Width (assetRemoveFlagWidth/2));
                        GUILayout.Label ("X", EditorStyles.boldLabel, GUILayout.Width (assetRemoveFlagWidth));
                    }
                    EditorGUILayout.EndHorizontal ();

                    for (int i = 0; i < _assetInfoListSorted.Count; i++) {
                        EditorGUILayout.BeginHorizontal (EditorStyles.helpBox, GUILayout.Width (_windowWidth - 50));

                        string index = (i + 1).ToString ();
                        string path = _assetInfoListSorted [i].assetPath;
                        string size = GetFormatSize (_assetInfoListSorted [i].size);
                        Object obj = _assetInfoListSorted [i].obj;

                        GUILayout.Label (index, GUILayout.Width (indexWidth));
                        if (GUILayout.Button (path, EditorStyles.boldLabel, GUILayout.Width (assetPathWidth))) {
                            EditorGUIUtility.PingObject (obj);
                        }

                        GUILayout.Label (size, GUILayout.Width (assetSizeWidth));

                        GUI.color = Color.red;
                        if (GUILayout.Button ("X", GUILayout.Width (assetRemoveFlagWidth))) {
                            ABFileAddRemoveTool.RemoveAssetPath (path);
                        }
                        GUI.color = preColor;

                        EditorGUILayout.EndHorizontal ();
                    }
                }
                EditorGUILayout.EndVertical (); 

                EditorGUILayout.BeginVertical (EditorStyles.helpBox);
                {
                    //Statistic
                    GUILayout.Label ("Total Files's Size:  " + GetTotalAssetsSizeFormat (_assetInfoList), EditorStyles.boldLabel);
                    var statistic = _assetInfoList
                    .GroupBy (_ => _.obj.GetType ().ToString ())
                    .Select (group => new AssetStatisticGroupInfo {
                        classType = group.First ().obj.GetType ().ToString (),
                        totalFile = group.Count (),
                        totalSize = group.Sum (assetInfo => assetInfo.size)
                    });
                    if (statistic.Count() > 0) {
                        var classSizeWidth = 200;
                        var totalFileSizeWidth = 80;
                        var totalSizeWidth = 50;

                        EditorGUILayout.BeginHorizontal ();
                        GUILayout.Label ("Object type", EditorStyles.boldLabel, GUILayout.Width (classSizeWidth));
                        GUILayout.Label ("Total file", EditorStyles.boldLabel, GUILayout.Width (totalFileSizeWidth));
                        GUILayout.Label ("Size", EditorStyles.boldLabel, GUILayout.Width (totalSizeWidth));
                        EditorGUILayout.EndHorizontal ();

                        foreach (var info in statistic) {
                            EditorGUILayout.BeginHorizontal ();
                            GUILayout.Label ("   " + info.classType, GUILayout.Width (classSizeWidth));
                            GUILayout.Label (info.totalFile.ToString (), GUILayout.Width (totalFileSizeWidth));
                            GUILayout.Label (GetFormatSize (info.totalSize), GUILayout.Width (totalSizeWidth));
                            EditorGUILayout.EndHorizontal ();
                        }
                    }
                }
                EditorGUILayout.EndVertical (); 
            }

            static void SearchUI ()
            {
                GUILayout.BeginHorizontal (GUI.skin.FindStyle ("Toolbar"));
                _searchString = GUILayout.TextField (_searchString, GUI.skin.FindStyle ("ToolbarSeachTextField"));
                if (GUILayout.Button ("", GUI.skin.FindStyle ("ToolbarSeachCancelButton"))) {
                    // Remove focus if cleared
                    _searchString = "";
                    GUI.FocusControl (null);
                }
                GUILayout.EndHorizontal ();

                if (!string.IsNullOrEmpty (_searchString)) {
                    if (_preSearchString != _searchString) {
                        _preSearchString = _searchString;
                        _assetInfoListSearch.Clear ();
                        foreach (var aInfo in _assetInfoList) {
                            if (aInfo.assetPath.ToLower().Contains (_searchString.ToLower())) {
                                _assetInfoListSearch.Add (aInfo);
                            }
                        }
                    }
                } else {
                    _assetInfoListSearch = new List<AssetInfo> (_assetInfoList);
                }
            }

            static void UpdateSortMode ()
            {
                if (_sizeMode == 0) {
                    _assetInfoListSorted = new List<AssetInfo> (_assetInfoListSearch);
                    _sizeModeText = "Size";
                }
                if (_sizeMode == 1) {
                    _assetInfoListSorted = new List<AssetInfo> (_assetInfoListSearch.OrderBy (_ => _.size).ToList ());
                    _sizeModeText = "Size ▲";
                }
                if (_sizeMode == 2) {
                    _assetInfoListSorted = new List<AssetInfo> (_assetInfoListSearch.OrderByDescending (_ => _.size).ToList ());
                    _sizeModeText = "Size ▼";
                }
            }
        }

        #endregion

        #region PreBuildAssetBundleInfoView

        public class AssetBundleDetailView
        {
            public class AssetBundleDetail
            {
                public string assetBundleName;
                public bool showAssets;
                public List<AssetDetailView.AssetInfo> assetInfoList;

                public AssetBundleDetail (string assetBundleName)
                {
                    this.assetBundleName = assetBundleName;
                    showAssets = false;
                    assetInfoList = new List<AssetDetailView.AssetInfo> ();

                    var assetPathList = AssetDatabase.GetAssetPathsFromAssetBundle (assetBundleName);
                    string[] bundleDependencies = AssetDatabase.GetDependencies (assetPathList, true);

                    if (assetPathList.Length > 0) {
                        foreach (var asset in assetPathList) {
                            long size = GetSizeInBytesOfAsset (asset);
                            Object obj = AssetDatabase.LoadAssetAtPath<Object> (asset);
                            AssetDetailView.AssetInfo assetInfo = new AssetDetailView.AssetInfo (asset, size, obj);
                            assetInfoList.Add (assetInfo);
                        }

                        foreach (var assetDep in bundleDependencies) {
                            if (assetInfoList.Where (_ => _.assetPath == assetDep).Count () == 0) {
                                long size = GetSizeInBytesOfAsset (assetDep);
                                Object obj = AssetDatabase.LoadAssetAtPath<Object> (assetDep);
                                AssetDetailView.AssetInfo assetInfo = new AssetDetailView.AssetInfo (assetDep, size, obj);
                                assetInfo.isDependecies = true;
                                assetInfoList.Add (assetInfo);
                            }
                        }
                    }
                }
            }

            static List<AssetBundleDetail> _assetBundleInfoList;
            static List<AssetBundleDetail> _assetBundleInfoSearch;
            static Vector2 _scrollPosAssetList;
            static string _searchString = "";
            static string _preSearchString = "";

            public static void InitData ()
            {
                if (_assetBundleInfoList == null)
                    _assetBundleInfoList = new List<AssetBundleDetail> ();
                _assetBundleInfoList.Clear ();

                foreach (var assetBundleName in AssetDatabase.GetAllAssetBundleNames()) {
                    _assetBundleInfoList.Add (new AssetBundleDetail (assetBundleName));
                }
                _searchString = "";
                _assetBundleInfoSearch = new List<AssetBundleDetail> (_assetBundleInfoList);
            }

            public static void ShowView ()
            {
                if (_assetBundleInfoList.Count == 0) {
                    GUILayout.Label ("", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal ();
                    GUILayout.FlexibleSpace ();
                    GUILayout.Label ("NO DATA", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace ();
                    EditorGUILayout.EndHorizontal ();
                    return;
                }

                EditorGUILayout.BeginVertical (EditorStyles.helpBox);
                {
                    SearchUI ();
                    float indexWidth = 30f;
                    float totalAssetsWidth = 50;
                    float assetRemoveFlagWidth = 20;
                    float assetPathWidth = _windowWidth - indexWidth - totalAssetsWidth - assetRemoveFlagWidth - 50;
                    float assetObjectWidth = _windowWidth - totalAssetsWidth - assetPathWidth - indexWidth - 50;

                    //Header
                    EditorGUILayout.BeginHorizontal ();
                    {
                        GUILayout.Label ("  ID", EditorStyles.boldLabel, GUILayout.Width (indexWidth));
                        GUILayout.Label ("  AssetBundle Name", EditorStyles.boldLabel, GUILayout.Width (assetPathWidth));
                        GUILayout.Label ("", EditorStyles.boldLabel, GUILayout.Width (totalAssetsWidth));
                        GUILayout.Label ("", EditorStyles.boldLabel, GUILayout.Width (assetObjectWidth));
                    }
                    EditorGUILayout.EndHorizontal ();

                    //Content
                    for (int i = 0; i < _assetBundleInfoSearch.Count; i++) {
                        string index = (i + 1).ToString ();
                        string path = _assetBundleInfoSearch [i].assetBundleName;
                        string total = _assetBundleInfoSearch [i].assetInfoList.Where (_ => !_.isDependecies).Count ().ToString ();

                        EditorGUILayout.BeginHorizontal (EditorStyles.helpBox);
                        {
                            GUILayout.Label (index, GUILayout.Width (indexWidth));
                            if (GUILayout.Button (path, EditorStyles.boldLabel, GUILayout.Width (assetPathWidth))) {
                                _assetBundleInfoSearch [i].showAssets = !_assetBundleInfoSearch [i].showAssets;
                            }
                            GUILayout.Label ("", GUILayout.Width (totalAssetsWidth));

                            string btnTitle = _assetBundleInfoSearch [i].showAssets ? "▲" : "▼";
                            if (GUILayout.Button (btnTitle, GUILayout.Width (assetObjectWidth))) {
                                _assetBundleInfoSearch [i].showAssets = !_assetBundleInfoSearch [i].showAssets;
                            }
                        }
                        EditorGUILayout.EndHorizontal ();


                        if (_assetBundleInfoSearch [i].showAssets) {
                            GUI.backgroundColor = Color.yellow;
                            List<AssetDetailView.AssetInfo> lstDepend = _assetBundleInfoSearch [i].assetInfoList.Where (_ => _.isDependecies).ToList();
                            List<AssetDetailView.AssetInfo> lstAssetsInAB = _assetBundleInfoSearch [i].assetInfoList.Where (_ => !_.isDependecies).ToList();

                            if (lstAssetsInAB.Count > 0) {
                                bool wasShowAssetsInBundle = false;
                                EditorGUILayout.BeginVertical (EditorStyles.helpBox);
                                {
                                    if (!wasShowAssetsInBundle) {
                                        wasShowAssetsInBundle = true;
                                        long sizeDependencies = _assetBundleInfoSearch [i].assetInfoList.Where (_ => !_.isDependecies).Sum (_ => _.size);
                                        GUILayout.Label ("► ASSETS IN ASSETBUNDLE      " + GetFormatSize (sizeDependencies) + "   Total: " + total + " Assets");
                                    }

                                    for (int m = 0; m < lstAssetsInAB.Count; m++) {
                                        AssetDetailView.AssetInfo assetInfo = lstAssetsInAB [m];
                                        string assetPath = assetInfo.assetPath;
                                        string size = GetFormatSize (assetInfo.size);
                                        Object obj = assetInfo.obj;

                                        string subIndex = (m + 1).ToString ();
                                        EditorGUILayout.BeginHorizontal ();
                                        {
                                            if (GUILayout.Button ("  " + subIndex + "  " + assetPath, EditorStyles.boldLabel, GUILayout.Width (assetPathWidth))) {
                                                EditorGUIUtility.PingObject (obj);
                                            }
                                            if (GUILayout.Button (size, EditorStyles.boldLabel, GUILayout.Width (totalAssetsWidth))) {}
                                            GUILayout.Label ("", GUILayout.Width (1));
                                            GUI.color = Color.red;
                                            if (GUILayout.Button ("X", GUILayout.Width (assetRemoveFlagWidth))) {
                                                ABFileAddRemoveTool.RemoveAssetPath (assetPath);
                                            }
                                            GUI.color = preColor;
                                        }
                                        EditorGUILayout.EndHorizontal ();
                                    }
                                }
                                EditorGUILayout.EndVertical ();
                            }

                            if (lstDepend.Count > 0) {
                                bool wasShowDependencies = false;
                                int depIndex = 0;

                                EditorGUILayout.BeginVertical (EditorStyles.helpBox);
                                {
                                    if (!wasShowDependencies) {
                                        wasShowDependencies = true;
                                        long sizeDependencies = _assetBundleInfoSearch [i].assetInfoList.Where (_ => _.isDependecies).Sum (_ => _.size);
                                        GUILayout.Label ("► DEPENDENCIES                       " + GetFormatSize (sizeDependencies));
                                    }

                                    for (int m = 0; m < lstDepend.Count; m++) {
                                        AssetDetailView.AssetInfo assetInfo = lstDepend [m];
                                        string assetPath = assetInfo.assetPath;
                                        string size = GetFormatSize (assetInfo.size);
                                        Object obj = assetInfo.obj;

                                        depIndex += 1;

                                        EditorGUILayout.BeginHorizontal ();
                                        {
                                            if (GUILayout.Button ("  " + depIndex + "  " + assetPath, EditorStyles.boldLabel, GUILayout.Width (assetPathWidth))) {
                                                EditorGUIUtility.PingObject (obj);
                                            }
                                            if (GUILayout.Button (size, EditorStyles.boldLabel, GUILayout.Width (totalAssetsWidth))) {}
                                            EditorGUILayout.ObjectField (obj, typeof(Object), true);
                                        }
                                        EditorGUILayout.EndHorizontal ();
                                    }
                                }
                                EditorGUILayout.EndVertical ();
                            }

                            GUI.backgroundColor = preBkgrColor;
                        }
                    }
                }
                EditorGUILayout.EndVertical ();
            }

            static void SearchUI ()
            {
                GUILayout.BeginHorizontal (GUI.skin.FindStyle ("Toolbar"));
                _searchString = GUILayout.TextField (_searchString, GUI.skin.FindStyle ("ToolbarSeachTextField"));
                if (GUILayout.Button ("", GUI.skin.FindStyle ("ToolbarSeachCancelButton"))) {
                    // Remove focus if cleared
                    _searchString = "";
                    GUI.FocusControl (null);
                }
                GUILayout.EndHorizontal ();

                if (!string.IsNullOrEmpty (_searchString)) {
                    if (_preSearchString != _searchString) {
                        _preSearchString = _searchString;
                        _assetBundleInfoSearch.Clear ();
                        foreach (var abDetail in _assetBundleInfoList) {
                            bool needAdd = false;
                            if (abDetail.assetBundleName.ToLower().Contains (_searchString.ToLower())) {
                                needAdd = true;
                            } else {
                                foreach (var assetInfo in abDetail.assetInfoList) {
                                    if (assetInfo.assetPath.ToLower().Contains (_searchString.ToLower())) {
                                        needAdd = true;
                                    }
                                }
                            }
                            if (needAdd) {
                                _assetBundleInfoSearch.Add (abDetail);
                            }
                        }
                    }
                } else {
                    _assetBundleInfoSearch = new List<AssetBundleDetail> (_assetBundleInfoList);
                }
            }
        }

        #endregion

        #region LocalAssetBundleDatabase

        public class LocalAssetBundleView
        {
            static List<AssetBundleSettings.AssetBundleTargetDB> _assetBundleDB;
            static List<TargetABView> _targetABViewList;
            static ObjectPacker _dataPacker = new ObjectPacker ();

            static string dbLocaltion = "";
            static bool isOpenDBInfor = true;

            public static void InitData ()
            {
                isOpenDBInfor = true; 
                if(dbLocaltion =="") dbLocaltion = AssetBundleSettings.ExportAssetBundleDatabasePath + "/" + AssetBundleSettings.AssetBundleDatabaseName + "." + AssetBundleSettings.AssetBundleDatabaseExtension;
                if (File.Exists (dbLocaltion)) {
                    FileStream fstream = File.OpenRead (dbLocaltion);
                    _assetBundleDB = _dataPacker.Unpack<List<AssetBundleSettings.AssetBundleTargetDB>> (fstream);
                    if (_assetBundleDB == null)
                        _assetBundleDB = new List<AssetBundleSettings.AssetBundleTargetDB> ();
                    fstream.Close ();
                } else {
                    _assetBundleDB = new List<AssetBundleSettings.AssetBundleTargetDB> ();
                }

                if (_targetABViewList == null)
                    _targetABViewList = new List<TargetABView> ();
                _targetABViewList.Clear ();

                for (int i = 0; i < _assetBundleDB.Count; i++) {
                    TargetABView targetView = new TargetABView ();
                    targetView.Init (_assetBundleDB [i].lstAssetBundleInfo, _assetBundleDB [i].buildTarget);
                    _targetABViewList.Add (targetView);
                }
            } 

            public static void ShowView ()
            {
                //NO DATA
                if (_assetBundleDB == null || _assetBundleDB.Count == 0) {
                    GUILayout.Label ("", EditorStyles.boldLabel);
                    GUILayout.BeginHorizontal ();
                    GUILayout.FlexibleSpace ();
                    GUILayout.Label ("NO DATA", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace ();
                    GUILayout.EndHorizontal ();

                    InitData ();
                    return;
                }

                for (int a = 0; a < _targetABViewList.Count; a++) {
                    _targetABViewList [a].ShowView();
                }
            }

            static void ShowLocalDB() {
                isOpenDBInfor = EditorGUILayout.Toggle("Show Local Database Info", isOpenDBInfor);
                if (isOpenDBInfor) {
                    GUILayout.BeginVertical ();
                    {
                        var titleWidth = 120;
                        var contentWidth = _windowWidth - 150;

                        EditorGUILayout.BeginHorizontal ();
                        {
                            GUILayout.Label ("Database Name", GUILayout.Width (titleWidth));
                            GUILayout.Label (AssetBundleSettings.AssetBundleDatabaseName + "." + AssetBundleSettings.AssetBundleDatabaseExtension, EditorStyles.boldLabel, GUILayout.Width (contentWidth));
                        }
                        EditorGUILayout.EndHorizontal ();

                        EditorGUILayout.BeginHorizontal ();
                        {
                            GUILayout.Label ("Local Database Path", GUILayout.Width (titleWidth));
                            if (dbLocaltion == "")
                                dbLocaltion = AssetBundleSettings.ExportAssetBundleDatabasePath + "/" + AssetBundleSettings.AssetBundleDatabaseName + "." + AssetBundleSettings.AssetBundleDatabaseExtension;
                            GUILayout.Label (dbLocaltion, EditorStyles.boldLabel, GUILayout.Width (contentWidth));
                        }
                        EditorGUILayout.EndHorizontal ();

                        if (GUILayout.Button ("Open Folder Path", GUILayout.Width (120), GUILayout.Height (20))) {
                            EditorUtility.RevealInFinder (AssetBundleSettings.ExportAssetBundleDatabasePath);
                        }
                    }
                    GUILayout.EndVertical ();   
                }
            }

            public class TargetABView
            {
                string _buildTarget = "";
                List<AssetBundleSettings.AssetBundleInfo> _lstAssetBundleInfo;
                List<AssetBundleSettings.AssetBundleInfo> _lstAssetBundleInfoSearch;
                List<AssetBundleSettings.AssetBundleInfo> _lstAssetBundleInfoSort;
                bool _isOpen = false;
                int _sizeMode = 0;
                string _sizeModeText = "Size";

                string _searchString = "";
                string _preSearchString = "";

                public void Init (List<AssetBundleSettings.AssetBundleInfo> lstAssetBundleInfo, string buildTarget)
                {
                    _lstAssetBundleInfo = lstAssetBundleInfo;
                    _buildTarget = buildTarget;

                    _lstAssetBundleInfoSearch = new List<AssetBundleSettings.AssetBundleInfo> (_lstAssetBundleInfo);
                    _lstAssetBundleInfoSort = new List<AssetBundleSettings.AssetBundleInfo> (_lstAssetBundleInfoSearch);
                }

                public void ShowView ()
                {
                    EditorGUILayout.BeginVertical (EditorStyles.helpBox);

                    string totalSize = GetFormatSize (_lstAssetBundleInfo.Sum (_ => _.size));
                    string statistics = totalSize + ", " + _lstAssetBundleInfo.Count + " Files";
                    string btnTargetTitle = _isOpen ? "►" +_buildTarget : "▼" + _buildTarget;
                    btnTargetTitle += "   (" + statistics + ")";
                    if (GUILayout.Button (btnTargetTitle, EditorStyles.boldLabel, GUILayout.Width (_windowWidth - 25), GUILayout.Height (15))) {
                        _isOpen = !_isOpen;
                    }
                       
                    if (_isOpen) {
                        if (_lstAssetBundleInfoSort.Count == 0) {
                            GUILayout.BeginHorizontal ();
                            {
                                GUILayout.FlexibleSpace ();
                                GUILayout.Label ("NO DATA", EditorStyles.boldLabel);
                                GUILayout.FlexibleSpace ();
                            }
                            GUILayout.EndHorizontal ();

                            EditorGUILayout.EndVertical ();
                            return;
                        }

                        SearchUI ();
                        UpdateModeSort ();

                        var indexWidth = 30f;
                        var fileSizeWidth = 100;
                        var versionWidth = 100;
                        var assetPathWidth = _windowWidth - versionWidth - fileSizeWidth - indexWidth - 80;

                        //Header
                        EditorGUILayout.BeginHorizontal ();
                        GUILayout.Label ("ID", EditorStyles.boldLabel, GUILayout.Width (indexWidth));
                        GUILayout.Label ("AssetBundle", EditorStyles.boldLabel, GUILayout.Width (assetPathWidth));
                        GUILayout.Label ("Version", EditorStyles.boldLabel, GUILayout.Width (versionWidth));
                        if (GUILayout.Button (_sizeModeText, GUILayout.Width (50), GUILayout.Height (20))) {
                            _sizeMode++;
                            UpdateModeSort ();
                        }
                        EditorGUILayout.EndHorizontal ();

                        for (int i = 0; i < _lstAssetBundleInfoSort.Count; i++) {
                            AssetBundleSettings.AssetBundleInfo assetBundleInfo = _lstAssetBundleInfoSort [i];

                            EditorGUILayout.BeginHorizontal (EditorStyles.helpBox);
                            string index = (i + 1).ToString ();
                            string path = assetBundleInfo.assetBundle + assetBundleInfo.extension;
                            string version = assetBundleInfo.version.ToString ();
                            string size = GetFormatSize (assetBundleInfo.size);
                            GUILayout.Label (index, GUILayout.Width (indexWidth));
                            GUILayout.Label (path, GUILayout.Width (assetPathWidth));
                            GUILayout.Label (version, GUILayout.Width (versionWidth));
                            GUILayout.Label (size, GUILayout.Width (fileSizeWidth));
                            EditorGUILayout.EndHorizontal ();
                        }
                    }

                    EditorGUILayout.EndVertical ();
                }

                void UpdateModeSort ()
                {
                    if (_sizeMode > 2)
                        _sizeMode = 0;

                    if (_sizeMode == 0) {
                        _lstAssetBundleInfoSort = new List<AssetBundleSettings.AssetBundleInfo> (_lstAssetBundleInfoSearch);
                        _sizeModeText = "Size";
                    }
                    if (_sizeMode == 1) {
                        _lstAssetBundleInfoSort = new List<AssetBundleSettings.AssetBundleInfo> (_lstAssetBundleInfoSearch.OrderBy (_ => _.size).ToList ());
                        _sizeModeText = "Size ▲";
                    }
                    if (_sizeMode == 2) {
                        _lstAssetBundleInfoSort = new List<AssetBundleSettings.AssetBundleInfo> (_lstAssetBundleInfoSearch.OrderByDescending (_ => _.size).ToList ());
                        _sizeModeText = "Size ▼";
                    }
                }

                void SearchUI ()
                {
                    GUILayout.BeginHorizontal (GUI.skin.FindStyle ("Toolbar"));
                    _searchString = GUILayout.TextField (_searchString, GUI.skin.FindStyle ("ToolbarSeachTextField"));
                    if (GUILayout.Button ("", GUI.skin.FindStyle ("ToolbarSeachCancelButton"))) {
                        // Remove focus if cleared
                        _searchString = "";
                        GUI.FocusControl (null);
                    }
                    GUILayout.EndHorizontal ();

                    if (!string.IsNullOrEmpty (_searchString)) {
                        if (_preSearchString != _searchString) {
                            _preSearchString = _searchString;

                            _lstAssetBundleInfoSearch.Clear ();
                            foreach (var abDetail in _lstAssetBundleInfo) {
                                if (abDetail.assetBundle.ToLower().Contains (_searchString.ToLower())) {
                                    _lstAssetBundleInfoSearch.Add (abDetail);
                                }
                            }
                        }
                    } else {
                        _lstAssetBundleInfoSearch = new List<AssetBundleSettings.AssetBundleInfo> (_lstAssetBundleInfoSearch);
                    }
                }
            }
        }

        #endregion

        #region FILE_SIZE_UTIL

        static string GetTotalAssetsSizeFormat (List<AssetDetailView.AssetInfo> lst)
        {
            long size = 0;
            foreach (var asset in lst) {
                size += asset.size;
            }
            string ret = GetFormatSize (size);
            return ret;
        }

        static long GetSizeInBytesOfAsset (string assetBundlePath)
        {
            var fileInfo = new System.IO.FileInfo (assetBundlePath);
            return fileInfo.Length;
        }

        static string GetSizeFormatOfFile (string assetBundlePath)
        {
            long size = GetSizeInBytesOfAsset (assetBundlePath);
            string ret = GetFormatSize (size);
            return ret;
        }

        static string GetFormatSize (long size)
        {
            string ret = "";
            if (size < 1024)
                ret += size + "B";
            else if (size < 1024 * 1024)
                ret += (size / 1024) + "KB";
            else
                ret += (size / (1024 * 1024)) + "MB";
            return ret;
        }

        #endregion
    }

    /// <summary>
    /// AssetBundle builder 
    /// </summary>
    public class ABBuilder
    {
        static List<AssetBundleSettings.AssetBundleTargetDB> _assetBundleDB;
        //DATA OF ALL TARGET
        static AssetBundleSettings.AssetBundleTargetDB assetBundleTargetDB;
        //DATA OF TARGET
        static string[] assetBundleNames;

        static ObjectPacker _dataPacker = new ObjectPacker ();

        public static void BuildAssetBundles (
            string exportRootLocation,
            BuildTarget target, 
            string extention,
            string outputDBPath, string dbName,
            bool isShowAdvanceSetting, string passwordEncrypt)
        {
            AssetDatabase.RemoveUnusedAssetBundleNames ();
            string targetPath = Path.Combine (exportRootLocation + "/" + AssetBundleSettings.ASSETBUNDLE_ROOT_STORAGE, AssetBundleSettings.GetPlatformFolderForBuildTarget(target));
            if (!Directory.Exists (targetPath)) {
                Directory.CreateDirectory (targetPath);
            }
            assetBundleNames = AssetDatabase.GetAllAssetBundleNames ();

            LoadAssetBundlesDataOfBuildTarget (target, outputDBPath, dbName);//1 Get db
            AssetBundleBuild[] abBuilds = GetAssetBundlesBuild (targetPath, target, extention);//2 Get AssetBundle Build
            if (abBuilds.Length > 0) {
                BuildPipeline.BuildAssetBundles (targetPath, abBuilds, BuildAssetBundleOptions.ChunkBasedCompression, target);//3 build
                GenerateOutputAssetBundleVersionFile (targetPath, extention, target);//4 Create AssetBundles version database
                SaveAssetBundlesDatabase (outputDBPath, dbName);//5 save db
                CleanUpOutputUnusedAssetBundle (targetPath, extention, target);//6 Remove unused AssetBundles in folder

                if (isShowAdvanceSetting) {
                    string inputPath = Path.Combine (exportRootLocation, AssetBundleSettings.ASSETBUNDLE_ROOT_STORAGE);
                    string outputEncryptedFolder = Path.Combine (exportRootLocation, AssetBundleSettings.ASSETBUNDLE_ROOT_STORAGE_ENCRYPT);
                    EncryptFiles (inputPath, outputEncryptedFolder, passwordEncrypt);
                }
                    
                if (EditorUtility.DisplayDialog (
                        "AssetBundle was built successfully!", 
                    "Do you want to open Export Folder?", "Open", "Cancel")) {
                    EditorUtility.RevealInFinder (exportRootLocation);
                }
            } else {
                EditorUtility.DisplayDialog (
                    "AssetBundleBuild not found!", 
                    "Maybe your prebuilt AssetBundles have no change or you have not add assets to AssetBundle", "Close");
            }
        }

        static void LoadAssetBundlesDataOfBuildTarget (BuildTarget target, string outputDBPath, string dbName)
        {
            string datPath = Path.Combine (outputDBPath, dbName);
            if (File.Exists (datPath)) {
                FileStream fstream = File.OpenRead (datPath);
                _assetBundleDB = _dataPacker.Unpack<List<AssetBundleSettings.AssetBundleTargetDB>> (fstream);
                fstream.Close ();
            } else {
                _assetBundleDB = new List<AssetBundleSettings.AssetBundleTargetDB> ();
            }

            assetBundleTargetDB = _assetBundleDB.Where (_ => _.buildTarget == AssetBundleSettings.GetPlatformFolderForBuildTarget(target)).FirstOrDefault ();
            if (assetBundleTargetDB == null) {
                //Add new targetDB
                assetBundleTargetDB = new AssetBundleSettings.AssetBundleTargetDB {
                    buildTarget = AssetBundleSettings.GetPlatformFolderForBuildTarget (target),
                    lstAssetBundleInfo = new List<AssetBundleSettings.AssetBundleInfo> ()
                };
                _assetBundleDB.Add (assetBundleTargetDB);
            } else {
                //Update db, remove unused assetbundle
                for (int i = 0; i < assetBundleTargetDB.lstAssetBundleInfo.Count; i++) {
                    var abInfo = assetBundleTargetDB.lstAssetBundleInfo[i];

                    //New version of AssetBundles not contain AssetBundle In DB, remove it
                    if (!assetBundleNames.Contains (abInfo.assetBundle)) {
                        assetBundleTargetDB.lstAssetBundleInfo.RemoveAt (i);
                        i--;
                    }
                }
            }
        }

        static AssetBundleBuild[] GetAssetBundlesBuild (string targetFolder, BuildTarget target, string extention)
        {
            List<AssetBundleBuild> assetBundleBuildList = new List<AssetBundleBuild> ();
            for (int i = 0; i < assetBundleNames.Length; i++) {
                string abName = assetBundleNames [i];
                string currentAssetHash = "";
                string newAssetHash = "";

                if (!UnityEditorInternal.InternalEditorUtility.inBatchMode) {
                    EditorUtility.DisplayProgressBar ("Process AssetBundle Build", "Processing: " + abName, (float)(i+1) / assetBundleNames.Length);
                }

                AssetBundleSettings.AssetBundleInfo assetBundleInfo = assetBundleTargetDB.lstAssetBundleInfo.Where (_ => _.assetBundle == abName).FirstOrDefault ();
                if (assetBundleInfo != null) {
                    currentAssetHash = assetBundleInfo.hashAssetsContent;
                }

                string[] assetsInBundle = AssetDatabase.GetAssetPathsFromAssetBundle (abName);
                foreach (string assetPath in assetsInBundle) {
                    newAssetHash += EncryptDecryptUtil.ComputeHashForAsset (assetPath);
                }
                string[] bundleDependencies = AssetDatabase.GetDependencies (assetsInBundle, true);
                foreach (string assetDepend in bundleDependencies) {
                    newAssetHash += EncryptDecryptUtil.ComputeHashForAsset (assetDepend);
                }

                //AssetBundle updated, need rebuild, version up
                if (currentAssetHash != newAssetHash.ToString ()) {
                    AssetBundleBuild abBuild = new AssetBundleBuild ();
                    abBuild.assetBundleName = abName + extention;
                    abBuild.assetNames = assetsInBundle;
                    assetBundleBuildList.Add (abBuild);

                    if (assetBundleInfo == null) {
                        assetBundleInfo = new AssetBundleSettings.AssetBundleInfo ();
                        assetBundleTargetDB.lstAssetBundleInfo.Add (assetBundleInfo);
                    }
                    assetBundleInfo.assetBundle = abName;
                    assetBundleInfo.version += 1;
                    assetBundleInfo.hashAssetsContent = newAssetHash;
                }
            }

            //Recheck AssetBundle in localpath
            for (int i = 0; i < assetBundleTargetDB.lstAssetBundleInfo.Count; i++) {
                AssetBundleSettings.AssetBundleInfo assetBundleInDB = assetBundleTargetDB.lstAssetBundleInfo[i];
                string abNameWithExtension = assetBundleInDB.assetBundle + extention;
                string filePath = Path.Combine (targetFolder, abNameWithExtension);
                bool needToBuild = !File.Exists (filePath);
                if (!needToBuild) {
                    string hash = EncryptDecryptUtil.ComputeHashForAsset (filePath);
                    needToBuild = hash != assetBundleInDB.hashAssetBundle;
                }

                if (needToBuild) {
                    bool isExits = assetBundleBuildList.Where ((a) => a.assetBundleName == abNameWithExtension).Count () != 0;
                    if (!isExits) {
                        AssetBundleBuild newBuild = new AssetBundleBuild ();
                        newBuild.assetBundleName = abNameWithExtension;
                        newBuild.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle (assetBundleInDB.assetBundle);
                        assetBundleBuildList.Add (newBuild);
                    }
                }   
            }

            if (!UnityEditorInternal.InternalEditorUtility.inBatchMode) {
                EditorUtility.ClearProgressBar ();
            }
            return assetBundleBuildList.ToArray ();
        }

        static void SaveAssetBundlesDatabase (string outputDBPath, string dbName)
        {
            string datPath = Path.Combine (outputDBPath, dbName);
            FileStream fstream = File.Create (datPath);
            _dataPacker.Pack (fstream, _assetBundleDB);
            fstream.Close ();
        }

        static void GenerateOutputAssetBundleVersionFile (string targetPath, string extention, BuildTarget target)
        {
            int bundleNum = assetBundleTargetDB.lstAssetBundleInfo.Count;
            for (int i = 0; i < assetBundleTargetDB.lstAssetBundleInfo.Count; i++) {
                AssetBundleSettings.AssetBundleInfo abInfo = assetBundleTargetDB.lstAssetBundleInfo[i];
                if (!UnityEditorInternal.InternalEditorUtility.inBatchMode) {
                    EditorUtility.DisplayProgressBar ("Generating output AssetBundle version file", "Processing: " + abInfo.assetBundle, ((float)(i+1) / (float)bundleNum));
                }
                string path = Path.Combine (targetPath, abInfo.assetBundle + extention);
                string fileHash = EncryptDecryptUtil.ComputeHashForAsset (path);
                abInfo.hashAssetBundle = fileHash;

                var fileInfo = new System.IO.FileInfo (path);
                abInfo.size = fileInfo.Length;
                abInfo.extension = extention;
            }
            if (!UnityEditorInternal.InternalEditorUtility.inBatchMode) {
                EditorUtility.ClearProgressBar ();
            }
            string outVerFile = Path.Combine (targetPath, AssetBundleSettings.ASSETBUNDLE_VERSION_FILE_NAME);
            FileStream ofstream = File.Create (outVerFile);
            _dataPacker.Pack (ofstream, assetBundleTargetDB);
            ofstream.Close ();
        }

        static void CleanUpOutputUnusedAssetBundle (string targetPath, string extention, BuildTarget target)
        {
            string[] allAssetsInFolder = Directory.GetFiles(targetPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(extention)).ToArray();
            if (!targetPath.EndsWith ("/")) {
                targetPath += "/";
            }
            int totalFiles = allAssetsInFolder.Length;
            int index = 0;
            foreach (string file in allAssetsInFolder) {
                string abNameWithExtension = file.Replace (targetPath, "");
                string abName = abNameWithExtension.Replace (extention, "");
                if (!UnityEditorInternal.InternalEditorUtility.inBatchMode) {
                    EditorUtility.DisplayProgressBar ("Remove Unused AssetBundles In Output Folder", "Processing: " + abName, ((float)index / (float)totalFiles));
                }
                AssetBundleSettings.AssetBundleInfo abInfo = assetBundleTargetDB.lstAssetBundleInfo.Where (_ => _.assetBundle == abName).FirstOrDefault ();
                if (abInfo == null) {
                    if (File.Exists (file)) {
                        File.Delete (file);
                    }
                    if (File.Exists (file + ".manifest")) {
                        File.Delete (file + ".manifest");
                    }
                }
                index++;
            }
            if (!UnityEditorInternal.InternalEditorUtility.inBatchMode) {
                EditorUtility.ClearProgressBar ();
            }
        }

        static void EncryptFiles (string inputFolder, string outputFolder, string passwordEncrypt) {
            string[] allAssetsInFolder = Directory.GetFiles (inputFolder, "*.*", SearchOption.AllDirectories)
                .Where(s => !s.EndsWith(".meta") && !s.EndsWith(".DS_Store") && s.Contains(".")).ToArray();
            if (!inputFolder.EndsWith ("/")) {
                inputFolder += "/";
            }
            if (!outputFolder.EndsWith ("/")) {
                outputFolder += "/";
            }
            foreach (var file in allAssetsInFolder) {
                string extension = string.Empty;
                string fileWithoutExtension = string.Empty;
                if (Path.HasExtension (file)) {
                    extension = Path.GetExtension (file);
                    fileWithoutExtension = file.Replace (extension, "");
                }
                fileWithoutExtension = fileWithoutExtension.Replace (inputFolder, "");

                string ecryptFileWithNewExtension = "";
                ecryptFileWithNewExtension = outputFolder + fileWithoutExtension + extension;

                int lastIdx = ecryptFileWithNewExtension.LastIndexOf ("/");
                string dirPath = ecryptFileWithNewExtension.Substring( 0, lastIdx );
                if ( !Directory.Exists( dirPath ) )
                {
                    Directory.CreateDirectory( dirPath );
                }

                EncryptDecryptUtil.EncryptFileToFile (file, ecryptFileWithNewExtension, passwordEncrypt);
            }

            Debug.Log ("Files in folder were encrypted successfully!");
            EditorUtility.RevealInFinder (outputFolder);
        }
    }
        
    /// <summary>
    /// Encrypt folder tool.
    /// </summary>
    public class EncryptFolderToolView : EditorWindow
    {
        //Encrypt
        string inputFolder = "";
        string outputEncryptLocation = "";
        string passwordEncrypt = "";

        [MenuItem ("Tools/AssetBundle Tool/Encrypt Files In Folder Window")]
        static void ShowAssetBundleList ()
        {
            EditorWindow.GetWindow<EncryptFolderToolView> ("Encrypt Bundles");
        }

        void OnEnable ()
        {
            if (inputFolder == "")
                inputFolder = Application.dataPath + "/AssetBundles/";
            if (outputEncryptLocation == "")
                outputEncryptLocation = Application.dataPath + "/AssetBundles/";
        }

        void OnGUI ()
        {
            EditorGUILayout.TextArea ("", GUI.skin.horizontalSlider);
            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            GUILayout.Label ("ENCRYPT FILES IN FOLDER", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal ();
            EditorGUILayout.TextArea ("", GUI.skin.horizontalSlider);

            GUILayout.BeginHorizontal ();
            inputFolder = EditorGUILayout.TextField ("Input Folder", inputFolder);
            if (GUILayout.Button ("...", GUILayout.Width (30), GUILayout.Height (15))) {
                GUI.FocusControl (null);
                inputFolder = EditorUtility.SaveFolderPanel ("Select InputFolder", Application.dataPath, "AssetBundles");
            }
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            outputEncryptLocation = EditorGUILayout.TextField ("Output Folder", outputEncryptLocation);
            if (GUILayout.Button ("...", GUILayout.Width (30), GUILayout.Height (15))) {
                GUI.FocusControl (null);
                outputEncryptLocation = EditorUtility.SaveFolderPanel ("Select InputFolder", Application.dataPath, "AssetBundles");
            }
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            passwordEncrypt = EditorGUILayout.TextField ("Password Encrypt", passwordEncrypt);
            if (GUILayout.Button ("Random", GUILayout.Width (100), GUILayout.Height (15))) {
                GUI.FocusControl (null);
                int ranlen = Random.Range (8, 30);
                passwordEncrypt = EncryptDecryptUtil.RandomString (ranlen);
            }
            GUILayout.EndHorizontal ();

            GUILayout.Label ("", EditorStyles.boldLabel);
            if (GUILayout.Button ("Encrypt All Files In Input Folder", GUILayout.Height (40))) {
                EncryptFiles (inputFolder, outputEncryptLocation, AssetBundleSettings.AssetBundleExtension);
            }
            EditorGUILayout.TextArea ("", GUI.skin.horizontalSlider);
        }

        void EncryptFiles (string inputFolder, string outputFolder, string newExtension)
        {
            string[] allAssetsInFolder = Directory.GetFiles (inputFolder, "*.*", SearchOption.AllDirectories)
                .Where (s => !s.EndsWith (".meta") && !s.EndsWith (".DS_Store")).ToArray ();
            if (!inputFolder.EndsWith ("/")) {
                inputFolder += "/";
            }
            if (!outputFolder.EndsWith ("/")) {
                outputFolder += "/";
            }
            foreach (var file in allAssetsInFolder) {
                string fileWithoutExtension = file.Replace (Path.GetExtension (file), "");
                fileWithoutExtension = fileWithoutExtension.Replace (inputFolder, "");

                string ecryptFileWithNewExtension = outputFolder + fileWithoutExtension + "." + newExtension;
                int lastIdx = ecryptFileWithNewExtension.LastIndexOf ("/");
                string dirPath = ecryptFileWithNewExtension.Substring (0, lastIdx);
                if (!Directory.Exists (dirPath)) {
                    Directory.CreateDirectory (dirPath);
                }

                EncryptDecryptUtil.EncryptFileToFile (file, ecryptFileWithNewExtension, passwordEncrypt);
            }

            Debug.Log ("Files in folder were encrypted Successfully!");
            EditorUtility.RevealInFinder (outputFolder);
        }
    }
        
    /// <summary>
    /// Add Selected objects, objects in selected folder to AssetBundle or remove with 1 click
    /// </summary>
    public class ABFileAddRemoveTool
    {
        static List<string> _assetsInAssetBundleList;

        [MenuItem ("Assets/AssetBundle Tool/Add To AssetBundle", false, 0)]
        public static void AddToAssetBundle() {
            foreach (string assetID in Selection.assetGUIDs) {
                if (!string.IsNullOrEmpty (assetID)) {
                    string assetPath = AssetDatabase.GUIDToAssetPath (assetID);
                    if (IsFolder (assetPath)) { //Process add content of folder to assetbundle
                        var assetFiles = GetAssetsFilesInPath (assetPath)
                            .Where (s => s.Contains (".meta") == false && s.Contains (".DS_Store") == false);
                        foreach (string f in assetFiles) {
                            AddAssetFromFilePath (f);
                        }
                    } else {
                        AddAssetFromFilePath (assetPath);
                    }
                }
            }
            Debug.Log ("<size=20><color=blue>ADD TO ASSETBUNDLE FINISH</color></size>");
            AssetDatabase.RemoveUnusedAssetBundleNames ();
            ABTool.AssetBundleToolEditor.AssetDetailView.InitData ();
            ABTool.AssetBundleToolEditor.AssetBundleDetailView.InitData ();
            ABTool.AssetBundleToolEditor.LocalAssetBundleView.InitData ();
        }

        [MenuItem ("Assets/AssetBundle Tool/Remove From AssetBundle", false, 0)]
        public static void RemoveAssetBundle() {
            foreach (string assetID in Selection.assetGUIDs) {
                if (!string.IsNullOrEmpty (assetID)) {
                    string assetPath = AssetDatabase.GUIDToAssetPath (assetID);
                    if (IsFolder (assetPath)) {
                        var assetFiles = GetAssetsFilesInPath (assetPath)
                            .Where (s => s.Contains (".meta") == false && s.Contains (".DS_Store") == false);
                        foreach (string f in assetFiles) {
                            RemoveAssetFromFilePath (f);
                        }
                    } else {
                        RemoveAssetFromFilePath (assetPath);
                    }
                }
            }
            Debug.Log ("<size=20><color=red>REMOVE FROM ASSETBUNDLE FINISH</color></size>");
            AssetDatabase.RemoveUnusedAssetBundleNames ();
            ABTool.AssetBundleToolEditor.AssetDetailView.InitData ();
            ABTool.AssetBundleToolEditor.AssetBundleDetailView.InitData ();
            ABTool.AssetBundleToolEditor.LocalAssetBundleView.InitData ();
        }

        public static void RemoveAssetPath(string assetPath) {
            if (IsFolder (assetPath)) {
                var assetFiles = GetAssetsFilesInPath (assetPath)
                    .Where (s => s.Contains (".meta") == false && s.Contains (".DS_Store") == false);
                foreach (string f in assetFiles) {
                    RemoveAssetFromFilePath (f);
                }
            } else {
                RemoveAssetFromFilePath (assetPath);
            }

            AssetDatabase.RemoveUnusedAssetBundleNames ();
            ABTool.AssetBundleToolEditor.AssetDetailView.InitData ();
            ABTool.AssetBundleToolEditor.AssetBundleDetailView.InitData ();
            ABTool.AssetBundleToolEditor.LocalAssetBundleView.InitData ();
        }

        static void AddAssetFromFilePath (string assetPath) {
            InitAssetsInAssetBundleList ();
            if (!_assetsInAssetBundleList.Contains (assetPath)) {
                string assetExtension = Path.GetExtension (assetPath);
                if (!string.IsNullOrEmpty (assetExtension)) {
                    string assetBundleName = assetPath.Replace (assetExtension, "");
                    AssetImporter importer = AssetImporter.GetAtPath (assetPath);
                    importer.assetBundleName = assetBundleName;
                    AssetDatabase.ImportAsset (assetPath, ImportAssetOptions.ForceUpdate);

                    _assetsInAssetBundleList.Add (assetPath);
                }
            }
        }

        static void RemoveAssetFromFilePath (string assetPath) {
            InitAssetsInAssetBundleList ();
            if (_assetsInAssetBundleList.Contains (assetPath)) {
                string assetExtension = Path.GetExtension (assetPath);
                if (!string.IsNullOrEmpty (assetExtension)) {
                    AssetImporter importer = AssetImporter.GetAtPath (assetPath);
                    importer.assetBundleName = null;

                    _assetsInAssetBundleList.Remove (assetPath);
                }
            }
        }

        static void InitAssetsInAssetBundleList() {
            _assetsInAssetBundleList = new List<string> ();
            foreach (var item in AssetDatabase.GetAllAssetBundleNames()) {
                var assetPathList = AssetDatabase.GetAssetPathsFromAssetBundle (item);
                if (assetPathList.Length > 0) {
                    foreach (var asset in assetPathList) {
                        _assetsInAssetBundleList.Add (asset);
                    }
                }
            }
        }

        static bool IsFolder (string assetPath) {
            if (Directory.Exists (assetPath)) {
                return true;
            } else {
                return false;
            }
        }

        static void ConvertAssetNameToLower(string assetPath) {
            string newName = Path.GetFileNameWithoutExtension (assetPath).ToLower ();
            AssetDatabase.RenameAsset (assetPath, newName);
            AssetDatabase.SaveAssets ();
            AssetDatabase.Refresh ();
        }

        /// <summary>
        /// Recursively gather all files under the given path including all its subfolders.
        /// </summary>
        static IEnumerable<string> GetAssetsFilesInPath (string path) {
            Stack<string> stack = new Stack<string> ();
            stack.Push (path);
            while (stack.Count > 0) {
                path = stack.Pop ();
                try {
                    foreach (string subDir in Directory.GetDirectories(path)) {
                        stack.Push (subDir);
                    }
                } catch (System.Exception ex) {
                    Debug.LogError (ex.Message);
                }
                string[] files = null;
                try {
                    files = Directory.GetFiles (path);
                } catch (System.Exception ex) {
                    Debug.LogError (ex.Message);
                }
                if (files != null) {
                    for (int i = 0; i < files.Length; i++) {
                        yield return files [i];
                    }
                }
            }
        }
    }
}