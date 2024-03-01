using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickHandler : Singleton<ClickHandler>
{
    private GameObject _hoveredObject;
    private Selectable _hoveredSelectable;

    private void Awake() {
        InitializeSingleton();
    }
    // Start is called before the first frame update
    void Start()
    {
        OnEnable();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnEnable() {
        if (CameraController.Instance != null) {
            Deactivate();
            Activate();
        }
    }
    private void OnDisable() {
        Deactivate();
    }

    public void Activate() {
        CameraController.Instance.ClickAction += Select;
        CameraController.Instance.HoverAction += HoverUnselected;
        _hoveredObject = null;
    }
    public void Deactivate() {
        CameraController.Instance.ClickAction -= Select;
        CameraController.Instance.HoverAction -= HoverUnselected;
    }

    private void Select(GameObject gO) {
        if (gO == null) {
            return;
        }
        /*
            Place to put UI checking as well
        */
        Selectable selectable = GameObjectHelper.GetSelectableObject(gO);
        if (selectable == null) {
            return;
        }
        selectable.Select();
    }
    private void HoverUnselected(GameObject gO) {
        //Debug.Log(gO);
        if (gO == null || gO.Equals(_hoveredObject)) {
            return;
        }
        Debug.Log("HoverUnselected Running");
        Selectable selectable = GameObjectHelper.GetSelectableObject(gO);
        if (selectable == null) {
            if (_hoveredSelectable != null) {
                _hoveredSelectable.HoverDeselect();
                _hoveredSelectable = null;
                _hoveredObject = null;
            }
            return;
        }
        selectable.HoverSelect();
        _hoveredObject = gO;
        _hoveredSelectable = selectable;
    }
}
