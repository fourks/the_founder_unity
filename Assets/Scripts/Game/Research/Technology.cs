using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Technology : ScriptableObject, IHasPrereqs {
    public string description;

    public float requiredResearch = 1000;

    public EffectSet effects = new EffectSet();

    public Vertical requiredVertical;
    public List<Technology> requiredTechnologies;

    public bool isAvailable(Company company) {
        // The technology's vertical must be active on the company.
        if (!company.verticals.Contains(requiredVertical))
            return false;

        // The technology's prerequisite technologies must have been researched.
        foreach (Technology t in requiredTechnologies) {
            if (!company.technologies.Contains(t))
                return false;
        }

        return true;
    }
}