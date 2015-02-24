using UnityEngine;
using System.Collections;

public class UIEmployee : MonoBehaviour {
    public GameObject HUDgroup;
    public Transform HUDtarget;
    public HUDText hudtext;
    public UILabel happinessLabel;
    public Color workColor;
    public Color breakthroughColor;
    public Color unhappyColor;
    public Color happyColor;
    public Worker worker;
    private NavMeshAgent agent;
    private Company company;
    private bool idling;

    public Vector3 target;

    void Start() {
        company = GameManager.Instance.playerCompany;
        StartCoroutine(Working());

        agent = GetComponent<NavMeshAgent>();
        target = RandomTarget();
    }

    void Update() {
        if (!idling) {
            agent.SetDestination(target);

            // Check if we've reached the destination
            // For this to work, the stoppingDistance has to be about 1.
            if (Vector3.Distance(agent.nextPosition, agent.destination) <= agent.stoppingDistance) {
                StartCoroutine(Pause());
                target = RandomTarget();
            }
        }
    }

    IEnumerator Pause() {
        idling = true;
        yield return new WaitForSeconds(1f + Random.value * 3f);
        idling = false;
    }

    void OnEnable() {
        // On enable, reset the target.
        target = RandomTarget();
    }

    void OnDestroy() {
        Destroy(HUDgroup);
    }

    // Temporary, to get employees moving about the office.
    Vector3 RandomLocation() {
        return transform.parent.TransformDirection(UIOfficeManager.Instance.RandomLocation());
    }

    Vector3 RandomTarget() {
        return transform.parent.TransformDirection(UIOfficeManager.Instance.RandomTarget());
    }

    IEnumerator Working() {
        while(true) {
            float happy = worker.happiness.value;
            if (happy >= 20) {
                happinessLabel.text = ":D";
                happinessLabel.color = happyColor;
            } else if (happy >= 14) {
                happinessLabel.text = ":)";
                happinessLabel.color = happyColor;
            } else if (happy >= 8) {
                happinessLabel.text = ":\\";
                happinessLabel.color = happyColor;
            } else if (happy >= 4) {
                happinessLabel.text = ":(";
                happinessLabel.color = unhappyColor;
            } else {
                happinessLabel.text = ">:(";
                happinessLabel.color = unhappyColor;
            }

            if (company.developing) {
                // Breakthrough!
                // TO DO this should be based on employee happiness
                if (Random.value < 0.4) {
                    hudtext.Add("BRK!", breakthroughColor, 0f);
                } else {
                    hudtext.Add(worker.productivity.value, workColor, 0f);
                }
            }

            yield return new WaitForSeconds(2 * Random.value);
        }
    }
}
