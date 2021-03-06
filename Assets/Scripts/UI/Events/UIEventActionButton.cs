using UnityEngine;
using System.Collections;

public class UIEventActionButton : MonoBehaviour {
    private GameEvent.Action action_;
    public GameEvent.Action action {
        get { return action_; }
        set {
            action_ = value;
            titleLabel.text = action_.name;
        }
    }

    public UILabel titleLabel;

    void OnClick() {
        if (action != null)
            action_.effects.Apply(GameManager.Instance.playerCompany);
    }
}


