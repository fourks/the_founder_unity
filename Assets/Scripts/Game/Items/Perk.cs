/*
 * Any item which can be purchased from the market.
 */

using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Perk : TemplateResource<Perk> {
    [System.Serializable]
    public class Upgrade {
        public string name;
        public float cost = 1000;
        public string description;
        public Office.Type requiredOffice;

        // For the physical representation of the product.
        public Mesh mesh;

        public EffectSet effects = new EffectSet();
        public List<Technology> requiredTechnologies = new List<Technology>();

        public bool Available(Company c) {
            if (c.office < requiredOffice)
                return false;
            foreach (Technology t in requiredTechnologies) {
                if (!c.technologies.Contains(t))
                    return false;
            }
            return true;
        }
    }

    // The index of the currently-active upgrade.
    public int upgradeLevel = 0;
    public List<Upgrade> upgrades = new List<Upgrade>();

    public Upgrade current {
        get { return upgrades[upgradeLevel]; }
    }
    public Upgrade next {
        get { return hasNext ? upgrades[upgradeLevel + 1] : null; }
    }
    public bool hasNext {
        get { return upgradeLevel < upgrades.Count - 1; }
    }
    public bool NextAvailable(Company c) {
        // A perks availability depends on whether or not the company
        // has the necessary technologies and a high enough office level upgrade.
        return !hasNext ? false : next.Available(c);
    }
    public float cost {
        get { return current.cost; }
    }
    public EffectSet effects {
        get { return current.effects; }
    }
    public string description {
        get { return current.description; }
    }
    public Mesh mesh  {
        get { return current.mesh; }
    }
}
