using UnityEngine;
using System.Collections;

public class RotateObject : MonoBehaviour {

    private Transform thisTransform;

    // Use this for initialization
    void Start () {
        thisTransform = transform;
    }

#if !UNITY_EDITOR
    void Update () { 
        if (Input.touchCount == 1)
        {
            // GET TOUCH 0
            Touch touch0 = Input.GetTouch(0);

            // APPLY ROTATION
            if (touch0.phase == TouchPhase.Moved)
            {
                thisTransform.transform.Rotate(0f, -touch0.deltaPosition.x, 0f);
            }

        }
    }
#endif

#if UNITY_EDITOR
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButton(0)){
        	thisTransform.Rotate(Vector3.up *-15* Input.GetAxis("Mouse X"));
      	}
	}
#endif
  
}
