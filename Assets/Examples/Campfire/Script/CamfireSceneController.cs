using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CamfireSceneController : MonoBehaviour 
{
    public void OnClickBack() {
        SceneManager.LoadSceneAsync ("MenuScene", LoadSceneMode.Single);
    }
}
