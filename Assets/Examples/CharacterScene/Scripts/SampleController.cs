using UnityEngine;
using System.Collections;
using ABTool;
using System;
using UnityEngine.SceneManagement;

public class SampleController : MonoBehaviour 
{
    [SerializeField] GameObject _unitRoot;
    [SerializeField] GameObject _btnChangeWeapon;
    [SerializeField] GameObject _btnLoadCharacter;
    UnitController _unitCtrl = null;

    string[] weaponArr = new string[] {
        "assets/examples/assetbundleresources/weapon/weapons_a",
        "assets/examples/assetbundleresources/weapon/weapons_b",
        "assets/examples/assetbundleresources/weapon/weapons_c",
    };
    int iCurrentWeaponIndex = 0;

    public void OnClickLoadCharacter () {
        _btnLoadCharacter.SetActive (false);
        //Load Character
        string assetbundle = "assets/examples/assetbundleresources/characters/minotaur";
        AssetManager.Instance.GetAssetBundle<GameObject> (assetbundle, 
            (obj)=>{
                GameObject chaObj = GameObject.Instantiate(obj, _unitRoot.transform) as GameObject;
                chaObj.transform.localPosition = Vector3.zero;
                _unitCtrl = chaObj.GetComponent<UnitController>();
                _unitCtrl.LoadWeapon(weaponArr[iCurrentWeaponIndex]);
                _btnChangeWeapon.SetActive(true);
            }, (err)=>{
                Debug.Log(err);
            });
    }

    public void OnClickLoadEffect () {
        //Load effect
        string assetbundle = "assets/examples/assetbundleresources/effects/snow";

        AssetManager.Instance.GetAssetBundle<GameObject> (assetbundle,
            (obj) => {
                GameObject effect = GameObject.Instantiate(obj, this.transform) as GameObject;
                effect.name = obj.name;
            }, (err) => {
                Debug.Log(err);
            });
    }

    public void ChangeWeapon () {
        if (_unitCtrl != null) {
            iCurrentWeaponIndex++;
            if (iCurrentWeaponIndex >= weaponArr.Length) iCurrentWeaponIndex = 0; 
            _unitCtrl.LoadWeapon (weaponArr[iCurrentWeaponIndex]);
        }
    }


    public void OnClickBack() {
        SceneManager.LoadSceneAsync ("MenuScene", LoadSceneMode.Single);
    }
}
