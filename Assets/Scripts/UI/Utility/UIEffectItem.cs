using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class UIEffectItem : MonoBehaviour {
    public UIGrid effectGrid;
    private List<UIWidget> effectWidgets = new List<UIWidget>();

    public GameObject buffEffectPrefab;
    public GameObject unlockEffectPrefab;
    public GameObject productEffectPrefab;
    public void RenderEffects(EffectSet es) {
        // Clear out existing effect elements.
        while (effectGrid.transform.childCount > 0) {
            GameObject go = effectGrid.transform.GetChild(0).gameObject;
            NGUITools.DestroyImmediate(go);
        }

        RenderUnlockEffects(es);
        RenderBuffEffects(es);
        RenderProductEffects(es);
        RenderSpecialEffects(es);
        effectGrid.Reposition();
    }

    public void AdjustEffectsHeight() {
        int count = effectGrid.GetChildList().Count;

        // If there are effects, expand the height for them.
        if (count > 0)
            Extend((int)((count + 1) * effectGrid.cellHeight));
    }

    private void RenderBuffEffects(EffectSet es) {
        foreach (StatBuff buff in es.workerEffects) {
            RenderBuffEffect(buff, "workers");
        }

        if (es.research.value != 0) {
            RenderBuffEffect(es.research, null);
        }

        if (es.cash != 0) {
            RenderBuffEffect(new StatBuff("Cash", es.cash), null);
        }

        if (es.forgettingRate != 0) {
            RenderBuffEffect(new StatBuff("Forgetting Rate", es.forgettingRate), null);
        }

        if (es.spendingMultiplier != 0) {
            RenderBuffEffect(new StatBuff("Consumer Spending", es.spendingMultiplier), null);
        }

        if (es.wageMultiplier != 0) {
            RenderBuffEffect(new StatBuff("Wage Multiplier", es.wageMultiplier), null);
        }

        if (es.economicStability != 0) {
            RenderBuffEffect(new StatBuff("Economic Stability", es.economicStability), null);
        }

        if (es.taxRate != 0) {
            RenderBuffEffect(new StatBuff("Tax Rate", es.taxRate), null);
        }

        if (es.expansionCostMultiplier != 0) {
            RenderBuffEffect(new StatBuff("Expansion Costs", es.expansionCostMultiplier), null);
        }

        if (es.opinionEvent.opinion.value != 0) {
            RenderBuffEffect(es.opinionEvent.opinion, null);
        }

        if (es.opinionEvent.publicity.value != 0) {
            RenderBuffEffect(es.opinionEvent.publicity, null);
        }

        foreach (Infrastructure.Type t in Infrastructure.Types) {
            if (es.infrastructureCostMultiplier[t] != 0) {
                RenderBuffEffect(new StatBuff(string.Format("{0} costs", t), es.infrastructureCostMultiplier[t]/100f), null);
            }
        }
    }

    private void RenderUnlockEffects(EffectSet es) {
        // Render the unlock effects for this event.
        // Note that event unlocks are *not* rendered because
        // those are "hidden" effects. You don't know they can happen until they do happen.
        foreach (ProductType i in es.unlocks.productTypes) {
            RenderUnlockEffect(i.name + " products");
        }
        foreach (Vertical i in es.unlocks.verticals) {
            RenderUnlockEffect("the " + i.name + " vertical");
        }
        foreach (Perk i in es.unlocks.perks) {
            RenderUnlockEffect(i.name);
        }
        foreach (Location i in es.unlocks.locations) {
            RenderUnlockEffect(i.name);
        }
        foreach (MiniCompany i in es.unlocks.companies) {
            RenderUnlockEffect(i.name);
        }
        foreach (SpecialProject i in es.unlocks.specialProjects) {
            RenderUnlockEffect(i.name);
        }
    }

    private void RenderProductEffects(EffectSet es) {
        foreach (ProductEffect pe in es.productEffects) {
            GameObject effectObj = NGUITools.AddChild(effectGrid.gameObject, productEffectPrefab);
            effectObj.GetComponent<UIProductEffect>().Set(pe);
            effectWidgets.Add(effectObj.GetComponent<UIWidget>());
        }
    }

    private void RenderSpecialEffects(EffectSet es) {
        if (es.specialEffect != EffectSet.Special.None) {
            RenderSpecialEffect(es.specialEffect);
        }
    }

    private void RenderUnlockEffect(string name) {
        GameObject effectObj = NGUITools.AddChild(effectGrid.gameObject, unlockEffectPrefab);
        effectObj.GetComponent<UIUnlockEffect>().Set(name);
        effectWidgets.Add(effectObj.GetComponent<UIWidget>());
    }

    private void RenderSpecialEffect(EffectSet.Special effect) {
        GameObject effectObj = NGUITools.AddChild(effectGrid.gameObject, unlockEffectPrefab);
        effectObj.GetComponent<UIUnlockEffect>().SetSpecial(effect);
        effectWidgets.Add(effectObj.GetComponent<UIWidget>());
    }

    private void RenderBuffEffect(StatBuff buff, string target) {
        GameObject effectObj = NGUITools.AddChild(effectGrid.gameObject, buffEffectPrefab);
        effectObj.GetComponent<UIBuffEffect>().Set(buff, target);
        effectWidgets.Add(effectObj.GetComponent<UIWidget>());
    }

    public void Extend(int amount) {
        gameObject.GetComponent<UIWidget>().height += amount;
    }

    // Call this in the update loop to keep effects at full width.
    protected void UpdateEffectWidths() {
        int w = effectGrid.GetComponent<UIWidget>().width;
        foreach (UIWidget widget in effectWidgets) {
            widget.width = w;
        }
    }
}


