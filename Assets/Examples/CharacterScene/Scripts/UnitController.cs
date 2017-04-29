using UnityEngine;
using System.Collections;
using ABTool;

public class UnitController : MonoBehaviour 
{
    UnitModel _unitModel;
    GameObject _weaponObj;

    Animation _anim;

    void Awake() {
        _anim = GetComponent<Animation> ();
    }

    public void LoadWeapon(string weaponAssetBundle) {
        if (_unitModel == null) _unitModel = GetComponent<UnitModel> ();

        AssetManager.Instance.GetAssetBundle<GameObject> (weaponAssetBundle, 
            (obj) => {
                if(_weaponObj != null) Destroy(_weaponObj);
                _weaponObj = GameObject.Instantiate(obj, _unitModel.weaponRoot.transform) as GameObject;
                _weaponObj.transform.localPosition = obj.transform.localPosition;
                _weaponObj.transform.localRotation = obj.transform.localRotation;
                _weaponObj.transform.localScale = obj.transform.localScale;
            }, (error) => {
                Debug.Log("ERROR " + error);
            });
    }


    void OnGUI () {

        float fY = Screen.height - 100;

        if (GUI.Button(new Rect(10, fY,100,50),"RunCycle")){
            PlayRun ();
        }

        if (GUI.Button(new Rect(120,fY,50,50),"Idle_1")){
            PlayIdle_1 ();
        }

        if (GUI.Button(new Rect(180,fY,50,50),"Idle_2")){
            PlayIdle_2 ();
        }

        if (GUI.Button(new Rect(240,fY,80,50),"Attack_1")){
            PlayAtk_1 ();
        }
        if (GUI.Button(new Rect(330,fY,80,50),"Attack_2")){
            PlayAtk_2 ();
        }
        if (GUI.Button(new Rect(420,fY,80,50),"Attack_3")){
            PlayAtk_3 ();
        }
        if (GUI.Button(new Rect(510,fY,60,50),"GetHit")){
            PlayHit ();
        }
        if (GUI.Button(new Rect(580,fY,60,50),"Die")){
            PlayDie ();
        }
    }

    public void PlayIdle_1 () {
        _anim.CrossFade ("Idle_1");
    }

    public void PlayIdle_2 () {
        _anim.CrossFade ("Idle_2");
    }

    public void PlayRun () {
        _anim.CrossFade ("RunCycle");
    }

    public void PlayAtk_1 () {
        _anim.CrossFade ("Attack_1");
    }

    public void PlayAtk_2 () {
        _anim.CrossFade ("Attack_2");
    }

    public void PlayAtk_3 () {
        _anim.CrossFade ("Attack_3");
    }

    public void PlayHit () {
        _anim.CrossFade ("GetHit");
    }

    public void PlayDie () {
        _anim.CrossFade ("Die");
    }
	
}
