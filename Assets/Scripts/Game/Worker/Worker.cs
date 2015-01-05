/*
 * A worker which contributes to the development of products.
 * "Worker" here is used abstractly - it can mean an employee, or a location, or a planet.
 * i.e. a worker is some _productive_ entity.
 */

using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[System.Serializable]
public class Worker : HasStats {
    public static List<Worker> LoadAll() {
        // Load workers as _copies_ so any changes don't get saved to the actual resources.
        return Resources.LoadAll<Worker>("Workers").ToList().Select(w => {
                Worker worker = Instantiate(w) as Worker;
                worker.name = w.name;
                return worker;
        }).ToList();
    }

    private Levels levels;

    // Each worker has their own style!
    public Texture texture;

    public float salary;
    public string bio;
    public string description;
    public string title;
    public float baseMinSalary;
    public float minSalary {
        get {
            // If the employee is currently hired, i.e. has a salary > 0,
            // their minimum acceptable salary depends on their happiness at their current company.
            // If happiness is below 5, the employee will actually accept a lower salary to move.
            if (salary > 0)
                return salary * (1 + (happiness.value - 5)/10);
            return baseMinSalary;
        }
    }

    // How many weeks the worker is off the job market for.
    // Recent offers the player has made.
    public int offMarketTime;
    public int recentPlayerOffers;

    public Stat happiness;
    public Stat productivity;
    public Stat charisma;
    public Stat creativity;
    public Stat cleverness;

    void Start() {
        Init("Default Worker");
    }
    public void Init(string name_) {
        name          = name_;
        salary        = 0;
        baseMinSalary = 30000;
        happiness     = new Stat("Happiness",    0);
        productivity  = new Stat("Productivity", 0);
        charisma      = new Stat("Charisma",     0);
        creativity    = new Stat("Creativity",   0);
        cleverness    = new Stat("Cleverness",   0);

        offMarketTime = 0;
        recentPlayerOffers = 0;
        //levels     = this.gameObject.GetComponent<Levels>();
    }

    public void OnEnable() {
        if (levels) {
            levels.LevelUp += LeveledUp;
        }
    }
    public void OnDisable() {
        if (levels) {
            levels.LevelUp -= LeveledUp;
        }
    }

    void LeveledUp(int level) {
        //print("Leveled");
    }

    public void ApplyItem(Item item) {
        ApplyBuffs(item.effects.workers);
    }

    public void RemoveItem(Item item) {
        RemoveBuffs(item.effects.workers);
    }

    public override Stat StatByName(string name) {
        switch (name) {
            case "Happiness":
                return happiness;
            case "Productivity":
                return productivity;
            case "Charisma":
                return charisma;
            case "Creativity":
                return creativity;
            case "Cleverness":
                return cleverness;
            default:
                return null;
        }
    }

    private static Dictionary<string, Dictionary<string, string[]>> bioMap = new Dictionary<string, Dictionary<string, string[]>> {
        {
            "Creativity", new Dictionary<string, string[]> {
                { "low", new string[] {"is not very imaginative"} },
                { "mid", new string[] {"is artistic", "has an eye for trends", "can be quite original"} },
                { "high", new string[] {"is a genius", "is a visionary", "is extremely talented"} }
            }
        },
        {
            "Cleverness", new Dictionary<string, string[]> {
                { "low", new string[] {"is quite dim", "isn't the sharpest tool in the shed", "isn't the brightest bulb of the bunch"} },
                { "mid", new string[] {"is an adept problem solver", "is technically gifted", "solves problems well", "is inventive"} },
                { "high", new string[] {"has a beautiful mind", "is absolutely brilliant", "is always on the cutting edge"} }
            }
        },
        {
            "Charisma", new Dictionary<string, string[]> {
                { "low", new string[] {"is alienating", "is hard to be around", "has a weird smell", "is very awkward", "creeps me out"} },
                { "mid", new string[] {"is fun to be around", "can be charming", "is easy to talk to", "is friendly"} },
                { "high", new string[] {"has a magnetic personality", "is hypnotic", "is a natural-born leader"} }
            }
        },
        {
            "Productivity", new Dictionary<string, string[]> {
                { "low", new string[] {"is kind of lazy", "needs a lot of motivation"} },
                { "mid", new string[] {"is a diligent worker", "is dedicated", "works hard", "is efficient with time"} },
                { "high", new string[] {"is really 'heads-down'", "multitasks very effectively", "gets a lot done in a short time"} }
            }
        },
        {
            "Happiness", new Dictionary<string, string[]> {
                { "low", new string[] {"is a real bummer", "brings everyone down", "reminds me of Eeyore"} },
                { "mid", new string[] {"brightens up the office", "cheers everyone up"} },
                { "high", new string[] {"would be a great cultural fit", "is a real pleasure to work with", "has an infectious energy"} }
            }
        }
    };

    public static string BuildBio(Worker w) {
        // Randomize order of stats.
        string[] stats = new string[] {"Creativity", "Cleverness", "Charisma", "Productivity", "Happiness"};
        stats = stats.OrderBy(x => Random.value).ToArray();

        float prevVal = -1;
        int maxConj = 3;
        List<string> conjs = new List<string>() {};
        List<string> bio = new List<string> { w.name };

        foreach (string stat in stats) {
            float val = w.StatByName(stat).baseValue;
            string level = SkillLevel(val);

            if (prevVal >= 0) {
                string conj;
                if (System.Math.Abs(val - prevVal) >= 4) {
                    conj = "but";
                } else {
                    conj = "and";
                }

                if (conjs.Count >= maxConj || (conjs.Count > 0 && conjs[conjs.Count - 1] == conj)) {
                    bio[bio.Count - 1] += ".";
                    bio.Add(w.name);
                    conjs.Clear();
                } else {
                    bio.Add(conj);
                    conjs.Add(conj);
                }
            }

            string[] descs = bioMap[stat][level];
            int idx = Random.Range(0,(descs.Length - 1));
            bio.Add(descs[idx]);
            prevVal = val;
        }
        return string.Join(" ", bio.ToArray()) + ".";
    }

    private static string SkillLevel(float val) {
        if (val <= 4)
            return "low";
        if (val <= 8)
            return "mid";
        else
            return "high";
    }


}


