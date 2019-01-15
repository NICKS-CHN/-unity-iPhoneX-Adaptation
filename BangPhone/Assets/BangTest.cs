using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BangTest : MonoBehaviour {

	void Start ()
	{
	    ScreenResizeManager.Instance.OnOrientationChanged += ResizeWindow;
	    ResizeWindow();
	}
	
    void OnDestroy()
    {
        ScreenResizeManager.Instance.OnOrientationChanged -= ResizeWindow;
    }

    private void ResizeWindow()
    {
        if(ScreenResizeManager.Instance.IsNeedResize())
            ScreenResizeManager.Instance.ResizePanel(this.gameObject);
    }
}
