using UnityEngine;
using System.Collections;

public class LoadingController : MonoBehaviour 
{
    private static object _lock = new object();
    public static LoadingController _instance;
    public static LoadingController Instance {
        get {
            lock (_lock) {
                if (_instance == null) {
                    _instance = (LoadingController) FindObjectOfType(typeof(LoadingController));
                    if (FindObjectsOfType(typeof(LoadingController)).Length > 1) {
                        Debug.LogError("[Singleton] Something went really wrong - there should never be more than 1 singleton!");
                        return _instance;
                    }

                    if (_instance == null) {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<LoadingController>();
                        singleton.name = "(singleton) "+ typeof(LoadingController).ToString();
                        DontDestroyOnLoad(singleton);
                    }
                }

                return _instance;
            }
        }
    }

    [SerializeField] GameObject _loadingPrefab;
    GameObject loadingObj;


    public void ShowLoading() {
    
    }

    public void HideLoading() {
        
    }
}
