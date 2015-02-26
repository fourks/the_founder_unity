using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Product : HasStats {
    public enum State {
        DEVELOPMENT,
        LAUNCHED,
        RETIRED
    }

    public string description {
        get {
            return recipe.description != null ? recipe.description : "This combination didn't make any sense. This product is incoherent!";
        }
    }

    // A generic name just based on the product types.
    public string genericName {
        get {
            return string.Join(" + ", productTypes.Select(pt => pt.name).ToArray());
        }
    }

    [SerializeField]
    private float _progress = 0;
    public float progress {
        get { return _progress/requiredProgress; }
    }

    public float marketScore = 0;
    public float marketShare = 0;

    public bool killsPeople;
    public bool debtsPeople;
    public bool techPenalty;
    public bool synergy;

    public Mesh mesh {
        get {
            // Fallback to first product type's mesh.
            return recipe.mesh != null ? recipe.mesh : productTypes[0].mesh;
        }
    }

    public float requiredProgress;

    [SerializeField]
    private State _state = State.DEVELOPMENT;
    public State state {
        get { return _state; }
    }

    // A product may be disabled if the company
    // has less infrastructure than necessary to support it.
    // A disabled product generates no revenue and does not continue developing.
    public bool disabled = false;

    // Infrastructure, in points, used by the product.
    public int points {
        get { return productTypes.Sum(p => p.points); }
    }
    public Infrastructure requiredInfrastructure {
        get {
            Infrastructure infras = new Infrastructure();
            foreach (ProductType pt in productTypes) {
                if (pt.requiredInfrastructure != null)
                    infras += pt.requiredInfrastructure;
            }
            return infras;
        }
    }
    public List<Vertical> requiredVerticals {
        get {
            List<Vertical> verts = new List<Vertical>();
            foreach (ProductType pt in productTypes) {
                verts.AddRange(pt.requiredVerticals);
            }
            return verts.Distinct().ToList();
        }
    }
    public EffectSet effects {
        get { return recipe.effects; }
    }

    public bool launched { get { return _state == State.LAUNCHED; } }
    public bool developing { get { return _state == State.DEVELOPMENT; } }
    public bool retired { get { return _state == State.RETIRED; } }

    // All the data about how well
    // this ProductType combination does.
    [SerializeField]
    private ProductRecipe recipe;
    public ProductRecipe Recipe {
        get { return recipe; }
    }

    // This is identifies what combination of product types
    // the product is. This is meant for quicker comparisons
    // between products to see if they are of the same combo.
    public string comboID;

    public float difficulty {
        get { return recipe.difficulty; }
    }

    public float timeSinceLaunch = 0;

    // Revenue earned over the product's lifetime.
    public float revenueEarned = 0;

    // Revenue earned during the last cycle.
    public float lastRevenue = 0;

    // The revenue model for the product.
    [SerializeField]
    private AnimationCurve revenueModel;

    // How long the product lasts at its peak plateau.
    [SerializeField]
    private float longevity;

    [SerializeField]
    private float maxRevenue;

    public List<ProductType> productTypes;

    public Stat design;
    public Stat marketing;
    public Stat engineering;

    public void Init(List<ProductType> pts, int design_, int marketing_, int engineering_, Company c) {
        productTypes = pts;
        comboID = string.Join(".", productTypes.OrderBy(pt => pt.name).Select(pt => pt.name).ToArray());

        design =      new Stat("Design",      (float)design_);
        marketing =   new Stat("Marketing",   (float)marketing_);
        engineering = new Stat("Engineering", (float)engineering_);

        recipe = ProductRecipe.LoadFromTypes(pts);

        // Load default if we got nothing.
        if (recipe == null) {
            recipe = ProductRecipe.LoadDefault();
        }
        requiredProgress = TotalProgressRequired(c);
        revenueModel = recipe.revenueModel;

        foreach (Vertical v in requiredVerticals) {
            if (v.name == "Defense") {
                killsPeople = true;
            } else if (v.name == "Finance") {
                debtsPeople = true;
            }
        }

        // A product recipe can be built without the required techs,
        // but it will operate at a penalty.
        techPenalty = false;
        foreach (Technology t in recipe.requiredTechnologies) {
            if (!c.technologies.Contains(t))
                techPenalty = true;
        }

        name = GenerateName(c);
    }

    // Generate a product name.
    private string GenerateName(Company c) {
        // If the company already has products of this combo,
        // use "versioning" for the product name.
        IEnumerable<Product> existing = c.products.Where(p => p.comboID == comboID);
        int version = existing.Count();
        if (version > 0)
            return string.Format("{0} {1}.0", existing.First().name, version + 1);

        if (recipe.names != null) {
            // TO DO this can potentially lead to products with duplicate names. Should keep track of which names are used,
            string[] names = recipe.names.Split(new string[] { ", ", "," }, System.StringSplitOptions.None);
            if (names.Length > 0)
                return names[Random.Range(0, names.Length-1)];
        }

        // Fallback to a rather generic name.
        return genericName;
    }


    static public event System.Action<Product, Company> Completed;
    public bool Develop(float newProgress, Company company) {
        if (developing && !disabled) {
            _progress += newProgress;

            if (_progress >= requiredProgress) {
                Launch();

                // Trigger completed event.
                if (Completed != null) {
                    Completed(this, company);
                }
                return true;
            }
        }
        return false;
    }

    public void Launch() {
        // Calculate the revenue model's parameters
        // based on the properties of the product.

        // +1 because the minimum value is 1, not 0.
        float A = design.value + 1;
        float U = marketing.value + 1;
        float P = engineering.value + 1;

        // Weights
        float a_w = recipe.design_W;
        float u_w = recipe.marketing_W;
        float p_w = recipe.engineering_W;

        // Ideals
        float a_i = recipe.design_I;
        float u_i = recipe.marketing_I;
        float p_i = recipe.engineering_I;

        // Calculate the score, i.e. the percent achieve of the ideal product values.
        // The maximum score is 1.0. We cap each value individually so that
        // they don't "bleed over" into others.
        float A_ = Mathf.Min((A/a_i) * a_w, 1f);
        float U_ = Mathf.Min((U/u_i) * u_w, 1f);
        float P_ = Mathf.Min((P/p_i) * p_w, 1f);
        float score = (A_ + U_ + P_)/(a_w + u_w + p_w);

        // Revenue model modifications:
        longevity = recipe.maxLongevity;

        // Maxmimum lifetime revenue of the product.
        maxRevenue = recipe.maxRevenue * score;

        Debug.Log(string.Format("Score {0}", score));
        Debug.Log(string.Format("Design Value {0}", A));
        Debug.Log(string.Format("Marketing Value {0}", U));
        Debug.Log(string.Format("Engineering Value {0}", P));
        Debug.Log(string.Format("Max Revenue {0}", maxRevenue));
        Debug.Log(string.Format("Longevity {0}", longevity));

        _state = State.LAUNCHED;
    }

    public float Revenue(float elapsedTime, Company company) {
        timeSinceLaunch += elapsedTime;

        float t = timeSinceLaunch/longevity;
        float revenue = 0;
        Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        Debug.Log(string.Format("Time {0}", t));
        if (launched && !disabled && t <= 1f) {
            revenue = revenueModel.Evaluate(t) * maxRevenue * Random.Range(0.95f, 1.05f);
            Debug.Log(string.Format("Raw revenue: {0}", revenue));

            // Economy's impact.
            revenue *= GameManager.Instance.economyMultiplier;
            Debug.Log(string.Format("After economy: {0}", revenue));

            // Consumer spending impact.
            revenue *= GameManager.Instance.spendingMultiplier;
            Debug.Log(string.Format("After consumer spending: {0}", revenue));

            // Public opinion's impact.
            revenue *= 1 + company.opinion.value/100f;
            Debug.Log(string.Format("After opinion: {0}", revenue));

            revenue *= marketShare;
            Debug.Log(string.Format("After market share: {0}", revenue));

            if (techPenalty)
                revenue *= 0.1f;

            synergy = true;
            List<ProductRecipe> activeRecipes = company.activeProducts.Select(p => p.recipe).ToList();
            foreach (ProductRecipe r in recipe.synergies) {
                if (!activeRecipes.Contains(r)) {
                    synergy = false;
                    break;
                }
            }
            if (synergy)
                revenue *= 1.5f;
        }


        revenueEarned += revenue;
        lastRevenue = revenue;
        return Mathf.Max(revenue, 0);
    }

    public override Stat StatByName(string name) {
        switch (name) {
            case "Design":
                return design;
            case "Marketing":
                return marketing;
            case "Engineering":
                return engineering;
            default:
                return null;
        }
    }

    // Product death
    public void Shutdown() {
        if (_state == State.DEVELOPMENT) {
            // Give it a basic name; incomplete products aren't christened!
            name = genericName;
        }

        _state = State.RETIRED;
    }


    // Progress required for the nth point.
    public static int baseProgress = 500;
    public float ProgressRequired(string feature, int n, Company c) {
        float reqProgress = Fibonacci(n+2) * baseProgress;
        float aggStat = 0;
        reqProgress *= difficulty;

        switch (feature) {
            case "Design":
                aggStat = c.AggregateWorkerStat("Creativity");
                break;
            case "Engineering":
                aggStat = c.AggregateWorkerStat("Cleverness");
                break;
            case "Marketing":
                aggStat = c.AggregateWorkerStat("Charisma");
                break;
            default:
                break;
        }

        if (aggStat == 0)
            return reqProgress;
        return reqProgress/aggStat;
    }

    public float TotalProgressRequired(Company c) {
        float reqProgress = 0;

        // We only count the base value of these stats, since bonuses to them
        // should not penalize development time.
        reqProgress += ProgressRequired("Design",      (int)design.baseValue, c);
        reqProgress += ProgressRequired("Engineering", (int)engineering.baseValue, c);
        reqProgress += ProgressRequired("Marketing",   (int)marketing.baseValue, c);

        // Required progress can't be 0, so set to 1 if it is.
        return Mathf.Max(1, reqProgress);
    }

    public int EstimatedCompletionTime(Company c) {
        float aggProductivity = c.AggregateWorkerStat("Productivity");
        float reqProgress = TotalProgressRequired(c);

        // Products are developed per development cycle.
        // Int to round down because of Hofstadter's Law: "It always takes longer than you expect, even when you take into account Hofstadter's Law."
        return (int)((reqProgress/aggProductivity) * GameManager.CycleTime);
    }

    public int EstimatedCompletionTime(string feature, int n, Company c) {
        float aggProductivity = c.AggregateWorkerStat("Productivity");
        float reqProgress = ProgressRequired(feature, n, c);
        return (int)((reqProgress/aggProductivity) * GameManager.CycleTime);
    }


    private static int Fibonacci(int n) {
        if (n == 0)
            return 0;
        else if (n == 1)
            return 1;
        else
            return Fibonacci(n-1) + Fibonacci(n-2);
    }
    private static float Gaussian(float x, float mean, float sd) {
        return ( 1 / ( sd * (float)System.Math.Sqrt(2 * (float)System.Math.PI) ) ) * (float)System.Math.Exp( -System.Math.Pow(x - mean, 2) / ( 2 * System.Math.Pow(sd, 2) ) );
    }
}


