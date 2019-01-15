using System;
using UnityEngine;
using UnityEditor;

public class ScreenResizeEditorWindow : EditorWindow
{
    private ScreenOrientation _screenOrientation;
    private ScreenResizeManager.PhoneState _phoneState;

    private ScreenOrientation _cacheOrientation;
    private ScreenResizeManager.PhoneState _cachePhone;

    private static ScreenResizeEditorWindow _instance;
    [MenuItem("Custom/ScreenResize/Window")]
    private static void Open()
    {
        if (_instance == null)
        {
            _instance = GetWindow<ScreenResizeEditorWindow>();
            _instance.minSize = new Vector2(300,300);
            _instance.Show();
        }
        else
        {
            _instance.Close();
            _instance = null;
        }
    }

    private void Awake()
    {
        _screenOrientation = ScreenResizeManager.GetScreenOrientation();
        _phoneState = ScreenResizeManager.GetPhoneState();
    }


     void OnGUI()
    {
        _cachePhone = _phoneState;
        _phoneState = EnumPopup<ScreenResizeManager.PhoneState>("手机型号", _phoneState);
        if (_cachePhone != _phoneState)
        {
            ScreenResizeManager.SetPhoneState(_phoneState);
        }

        EditorGUILayout.Space();
        _cacheOrientation = _screenOrientation;
        _screenOrientation = EnumPopup<ScreenOrientation>("朝向", _screenOrientation);
        if (_cacheOrientation != _screenOrientation)
        {
            ScreenResizeManager.SetScreenOrientation(_screenOrientation);
        }
    }

    private T EnumPopup<T>(string title, Enum selectedEnum, GUIStyle style = null)
    {
        style = style ?? EditorStyles.popup;
        return (T) Convert.ChangeType(EditorGUILayout.EnumPopup(title, selectedEnum, style), typeof (T));
    }
}
