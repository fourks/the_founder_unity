using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProductWorker : MonoBehaviour {
    public float maxStamina;
    public float stamina;
    public float fatigue;
    public float laborProgress;
    public float debugging;
    private bool recovering;
    private float debugRate = 1f;
    private float staminaRate = 1f;
    private Worker worker;

    public GameObject[] laborPrefabs;
    private List<ProductLabor> labors;
    private ProductLabor.Type laborType;


    void Start() {
        labors = new List<ProductLabor>();
        laborType = ProductLabor.RandomType;
    }

    private GameObject workerGroup;
    public void Setup(Worker w, GameObject wg) {
        workerGroup = wg;
        maxStamina = w.productivity.value;
        debugRate = w.cleverness.value/100;
        stamina = maxStamina;
        worker = w;
        renderer.material = w.material;

        if (w.robot)
            staminaRate = 0;
        else
            staminaRate = 0.1f;
    }

    public void StartDebugging() {
        debugging = Random.value * 10;
        renderer.material.color = new Color(0, 0, 1f);
    }

    void Update() {
        // Debugging...
        if (!recovering && stamina > 0 && debugging > 0) {
            debugging -= debugRate * Time.deltaTime;

            if (debugging <= 0) {
                debugging = 0;
                renderer.material.color = new Color(1f, 1f, 1f);
            }

        // Working...
        } else if (!recovering && stamina > 0 && ((labors.Count < 1 && laborType == ProductLabor.Type.Breakthrough) || (labors.Count < 4 && laborType != ProductLabor.Type.Breakthrough))) {
            if (laborProgress < 1) {
                stamina -= staminaRate * Time.deltaTime;
                laborProgress += worker.productivity.value/10 * Time.deltaTime;

                renderer.material.color = new Color(stamina/maxStamina, stamina/maxStamina, stamina/maxStamina);

                // If stamina hits 0, there
                // is a recovery penalty.
                if (stamina <= 0)
                    fatigue = 1;

            } else {
                laborProgress = 0;

                // Spawn points above the employee.
                GameObject labor = Instantiate(laborPrefabs[(int)laborType]) as GameObject;
                labor.name = "Labor";
                labor.transform.parent = transform;

                Vector3 pos = Vector3.zero;
                pos.y = 0.6f + labors.Count * 0.2f;
                labor.transform.localPosition = pos;
                labor.rigidbody.isKinematic = true;

                ProductLabor pl = labor.GetComponent<ProductLabor>();
                pl.type = laborType;

                switch (laborType) {
                    case ProductLabor.Type.Creativity:
                        pl.points = Random.value * worker.creativity.value;
                        break;

                    case ProductLabor.Type.Charisma:
                        pl.points = Random.value * worker.charisma.value;
                        break;

                    case ProductLabor.Type.Cleverness:
                        pl.points = Random.value * worker.cleverness.value;
                        break;
                }

                pl.points = Mathf.Max(Mathf.Sqrt(pl.points), 0.5f);
                labors.Add(pl);
                labor.SetActive(true);
            }

        // Recovering...
        } else {
            if (fatigue > 0) {
                recovering = true;
                fatigue -= 1f * Time.deltaTime;
                // TO DO flash red
                renderer.material.color = new Color(1f, 0.5f, 0.5f);
            } else if (stamina < maxStamina) {
                stamina += staminaRate * Time.deltaTime;
                renderer.material.color = new Color(stamina/maxStamina, stamina/maxStamina, stamina/maxStamina);
            } else if (stamina >= maxStamina) {
                recovering = false;
            }
        }
    }

    void OnClick() {
        // Release points.
        foreach (ProductLabor l in labors) {
            l.Fire();
        }
        labors.Clear();

        if (Random.value < worker.happiness.value/100) {
            laborType = ProductLabor.Type.Breakthrough;
        } else {
            laborType = ProductLabor.RandomType;
        }
    }

    void OnDrag(Vector2 delta) {
        Vector3 drag = Vector3.zero;
        drag.x = delta.x * 0.01f;
        workerGroup.transform.localPosition = workerGroup.transform.localPosition + drag;
    }
}
