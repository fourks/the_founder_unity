using UnityEngine;
using System.Collections;

public class UISpecialProject : UIEffectItem {
    private SpecialProject _specialProject;
    public SpecialProject specialProject {
        get { return _specialProject; }
        set {
            _specialProject = value.Clone();

            // Check if the project has already been completed.
            SpecialProject completedProject = SpecialProject.Find(_specialProject, company.specialProjects);
            if (completedProject == null) {
                SetupNewProject();
            } else {
                SetupCompletedProject();
            }

            DisplayProject();
        }
    }

    private Company company;

    public UILabel nameLabel;
    public UILabel descLabel;
    public UILabel costLabel;
    public UIButton button;
    public UILabel buttonLabel;
    public GameObject projectObj;

    void DisplayProject() {
        nameLabel.text = _specialProject.name;
        descLabel.text = _specialProject.description;
        projectObj.GetComponent<MeshFilter>().mesh = _specialProject.mesh;

        RenderEffects(_specialProject.effects);
        AdjustEffectsHeight();
    }

    void SetupNewProject() {
        costLabel.gameObject.SetActive(true);
        costLabel.text = string.Format("{0:C0}", _specialProject.cost);
        buttonLabel.text = "Develop";
    }

    void SetupCompletedProject() {
        costLabel.gameObject.SetActive(false);
        button.isEnabled = false;
        buttonLabel.text = "Completed";
    }

    void Awake() {
        company = GameManager.Instance.playerCompany;
        UIEventListener.Get(button.gameObject).onClick += BeginProject;
    }

    void Update() {
        // Rotate the product, fancy.
        projectObj.transform.Rotate(0,0,-50*Time.deltaTime);
    }

    public void BeginProject(GameObject obj) {
        UIManager.Instance.Confirm("This project will cost " + costLabel.text + " to develop.", delegate() {
            if (!company.StartSpecialProject(_specialProject)) {
                UIManager.Instance.Alert("You can't afford the cost for this project.");
            }
        }, null);
    }
}