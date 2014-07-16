using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class GameManager : Singleton<GameManager> {

    // Disable the constructor so that
    // this must be a singleton.
    protected GameManager() {}

    [HideInInspector]
    public Company playerCompany;

    private int weekTime = 15;
    private Month _month = Month.January;
    public string month {
        get { return _month.ToString(); }
    }
    private int _year = 1;
    public int year {
        get { return 2014 + _year; }
    }
    [HideInInspector]
    public int week = 0;

    private enum Month {
        January,
        February,
        March,
        April,
        May,
        June,
        July,
        August,
        September,
        October,
        November,
        December
    }

    public UnlockSet unlocked = new UnlockSet();

    public void NewGame(string companyName) {
        playerCompany = new Company(companyName);
        Application.LoadLevel("Game");
    }

    public void HireWorker(Worker worker) {
        playerCompany.HireWorker(worker);

        // TO DO worker shouldn't be removed from unlockedWorkers
        // but instead from availableWorkers.
        //unlockedWorkers.Remove(worker);
    }

    void Awake() {
        DontDestroyOnLoad(gameObject);

        if (playerCompany == null) {
            playerCompany = new Company("Foobar Inc");
        }
    }

    void OnEnable() {
        GameEvent.EventTriggered += OnEvent;
    }

    void OnDisable() {
        GameEvent.EventTriggered -= OnEvent;
    }

    void Start() {
        StartCoroutine(Weekly());
        StartCoroutine(Monthly());
        StartCoroutine(Yearly());

        StartCoroutine(ProductDevelopmentCycle());
        StartCoroutine(ProductRevenueCycle());
    }

    void Update() {
    }

    IEnumerator Yearly() {
        int yearTime = weekTime*4*12;
        yield return new WaitForSeconds(yearTime);
        while(true) {
            _year++;
            yield return new WaitForSeconds(yearTime);
        }
    }

    IEnumerator Monthly() {
        int monthTime = weekTime*4;
        yield return new WaitForSeconds(monthTime);
        while(true) {

            if (_month == Month.December) {
                _month = Month.January;
            } else {
                _month++;
            }

            playerCompany.Pay();
            yield return new WaitForSeconds(monthTime);
        }
    }

    IEnumerator Weekly() {
        yield return new WaitForSeconds(weekTime);
        while(true) {
            if (week == 3) {
                week = 0;
            } else {
                week++;
            }
            yield return new WaitForSeconds(weekTime);
        }
    }

    IEnumerator ProductDevelopmentCycle() {
        yield return new WaitForSeconds(weekTime/14);
        while(true) {
            playerCompany.DevelopProducts();

            // Temporarily placed here
            GameEvent.Roll(unlocked.events);

            // Add a bit of randomness to give things
            // a more "natural" feel.
            yield return new WaitForSeconds(weekTime/14 * Random.Range(0.4f, 1.4f));
        }
    }

    IEnumerator ProductRevenueCycle() {
        yield return new WaitForSeconds(weekTime/14);
        while(true) {
            float elapsedTime = weekTime/14 * Random.Range(0.4f, 1.4f);
            playerCompany.HarvestProducts(elapsedTime);

            // Add a bit of randomness to give things
            // a more "natural" feel.
            yield return new WaitForSeconds(elapsedTime);
        }
    }


    void OnEvent(GameEvent e) {
        playerCompany.ApplyBuffs(e.companyEffects);

        foreach (Worker worker in playerCompany.workers) {
            worker.ApplyBuffs(e.workerEffects);
        }

        foreach (ProductEffect pe in e.productEffects) {
            playerCompany.ApplyProductEffect(pe);
        }

        unlocked.Unlock(e.unlocks);
    }
}


