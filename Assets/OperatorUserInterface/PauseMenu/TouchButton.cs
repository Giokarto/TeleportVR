using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TouchButton : MonoBehaviour
{
    public string text
    {
        get { return _text; }
        set
        {
            _text = value;
            textMeshPro.SetText(_text);
        }
    }

    public int fontSize
    {
        get { return _fontSize; }
        set
        {
            _fontSize = value;
            textMeshPro.fontSize = _fontSize;
        }
    }
    public Color textColor
    {
        get { return _textColor; }
        set
        {
            _textColor = value;
            textMeshPro.color = _textColor;
        }
    }

    // do not use directly, only for internal storage.
    // use above getter / setter for interacting.
    private string _text;
    private int _fontSize;
    private Color _textColor;
    public new bool enabled
    {
        get { return activationVolume.enabled; }
        set { activationVolume.enabled = value; }
    }

    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private TouchButtonActivationVolume activationVolume;

    // Start is called before the first frame update
    void Start()
    {
        // Find relevant children
        textMeshPro = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        activationVolume = gameObject.GetComponentInChildren<TouchButtonActivationVolume>();
    }

    public void OnTouchEnter(System.Action<string> callback, bool once = false)
    {
        activationVolume.enterCallbacks.Add(callback, once);
    }

    public void ClearOnTouchEnter()
    {
        activationVolume.enterCallbacks.Clear();
    }

    public void OnTouchExit(System.Action<string> callback, bool once = false)
    {
        activationVolume.exitCallbacks.Add(callback, once);
    }

    public void ClearOnTouchExit()
    {
        activationVolume.exitCallbacks.Clear();
    }

}
