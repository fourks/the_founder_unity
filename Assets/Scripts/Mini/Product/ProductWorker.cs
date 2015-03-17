using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProductWorker : MonoBehaviour {
    public float maxStamina;
    public float stamina;
    public float fatigue;
    public float laborProgress;
    private bool recovering;
    private float staminaRate = 1f;
    public GameObject[] laborPrefabs;
    private List<ProductLabor> labors;
    private ProductLabor.Type laborType;
    public float productivity;

    void Start() {
        labors = new List<ProductLabor>();
        laborType = ProductLabor.RandomType;
    }

    public void Setup(Worker w) {
        maxStamina = w.productivity.value;
        productivity = w.productivity.value/100;
        stamina = maxStamina;

        if (w.robot)
            staminaRate = 0;
        else
            staminaRate = 0.1f;
    }

    void Update() {
        // Working...
        if (!recovering && labors.Count < 4 && stamina > 0) {
            if (laborProgress < 1) {
                stamina -= staminaRate * Time.deltaTime;
                laborProgress += productivity * Time.deltaTime;

                renderer.material.color = new Color(stamina/maxStamina, stamina/maxStamina, stamina/maxStamina);

                // If stamina hits 0, there
                // is a recovery penalty.
                if (stamina <= 0)
                    fatigue = 1;

            } else {
                laborProgress = 0;

                // TO DO Generate a labor sphere.
                // Spawn points above the employee.
                GameObject labor = Instantiate(laborPrefabs[(int)laborType]) as GameObject;
                labor.name = "Labor";
                labor.transform.parent = transform;
                labor.SetActive(true);

                Vector3 pos = Vector3.zero;
                pos.y = 3.2f + labors.Count * 1.2f;
                labor.transform.localPosition = pos;
                labor.rigidbody.isKinematic = true;

                ProductLabor pl = labor.GetComponent<ProductLabor>();
                pl.type = laborType;
                labors.Add(pl);

                Debug.Log(string.Format("Worker has {0} labors.", labors.Count));
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
        Debug.Log("WAS CLICKED");

        // Release points.
        foreach (ProductLabor l in labors) {
            l.Fire();
        }
        labors.Clear();
        laborType = ProductLabor.RandomType;
    }
}
