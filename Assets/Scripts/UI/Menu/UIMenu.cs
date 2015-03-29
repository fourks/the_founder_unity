using UnityEngine;
using System.Collections;

public class UIMenu : MonoBehaviour {
    public UISimpleGrid grid;
    public UIMenuButton menuButton;
    public UIMenuItem[] hudButtons;
    public Material lockedMat;

    void OnEnable() {
        grid.Reposition();
    }

    public void Activate(string item) {
        UIMenuItem menuItem = GetItem(item);
        menuItem.wiggle = true;
        menuItem.locked = false;
        if (item == "New Product" || item == "Research" || item == "Communications") {
            Show(item);
        } else {
            menuItem.display.renderer.material = menuItem.mat;
            menuItem.transform.Find("Locked").gameObject.SetActive(false);
            grid.Reposition();
            menuButton.Wiggle();
        }
    }

    public void Deactivate(string item) {
        if (item == "New Product" || item == "Research" || item == "Communications") {
            Hide(item);
        } else {
            UIMenuItem menuItem = GetItem(item);
            menuItem.locked = true;
            menuItem.display.renderer.material = lockedMat;
            menuItem.transform.Find("Locked").gameObject.SetActive(true);
            grid.Reposition();
        }
    }

    public void Hide(string item) {
        GetItem(item).gameObject.SetActive(false);
        grid.Reposition();
    }

    public void Show(string item) {
        GetItem(item).gameObject.SetActive(true);
        grid.Reposition();
    }

    private UIMenuItem GetItem(string item) {
        switch (item) {
            case "New Product":
                return hudButtons[0];
            case "Research":
                return hudButtons[1];
            case "Communications":
                return hudButtons[2];
            default:
                return grid.transform.Find(item).gameObject.GetComponent<UIMenuItem>();
        }
    }
}
