using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NSubstitute;

namespace UnityTest
{
    [TestFixture]
    internal class EffectTests
    {
        private GameObject gameObj;
        private GameData gd;
        private GameManager gm;

        private Company c;
        private EffectSet e;

        [SetUp]
        public void SetUp() {
            gameObj = new GameObject("Game Manager");
            gm = gameObj.AddComponent<GameManager>();
            gd = GameData.New("DEFAULTCORP");
            gm.Load(gd);

            c = new Company("Foo Inc").Init();
            e = new EffectSet();
        }

        [TearDown]
        public void TearDown() {
            UnityEngine.Object.DestroyImmediate(gameObj);
            gm = null;
            c = null;
        }

        [Test]
        public void Cash() {
            e.cash = 2000;

            float start = c.cash.baseValue;
            e.Apply(c);
            Assert.AreEqual(c.cash.baseValue, start + e.cash);
        }

        [Test]
        public void Opinion() {
            e.opinion = 2000;
            e.hype = 4000;

            float start_o = c.opinion;
            float start_h = c.hype;
            e.Apply(c);
            Assert.AreEqual(c.opinion,   start_o + e.opinion);
            Assert.AreEqual(c.hype, start_h + e.hype);
        }

        [Test]
        public void Event() {
            GameEvent ev  = ScriptableObject.CreateInstance<GameEvent>();
            ev.name       = "TESTEVENT";
            e.gameEvent   = ev;
            e.eventProbability = 0.5f;

            e.Apply(c);
            AGameEvent ev_ = gd.eventsPool.Where(evn => evn.name == ev.name).First();
            Assert.AreEqual(ev_.name, ev.name);
            Assert.AreEqual(ev_.probability, e.eventProbability);
        }

        [Test]
        public void EnablingAICompanies() {
            AICompany a = (AICompany)GameObject.Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Editor/Tests/Resources/TestAICompanyDisabled.asset", typeof(AICompany)));
            AAICompany aic = new AAICompany(a);
            aic.disabled = true;
            AICompany.all = new List<AAICompany>() { aic };

            e.aiCompany = aic.aiCompany;
            e.Apply(c);
            Assert.IsFalse(aic.disabled);
            Assert.IsTrue(a.disabled);
        }

        [Test]
        public void Worker() {
            e.workerEffects = new List<StatBuff>();
            e.workerEffects.Add(new StatBuff("Happiness", 100));

            Worker worker = (Worker)GameObject.Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Editor/Tests/Resources/TestWorker.asset", typeof(Worker)));
            AWorker w = new AWorker(worker);
            c.HireWorker(w);
            Assert.IsTrue(c.workers.Contains(w));

            float start = w.happiness;

            e.Apply(c);
            Assert.AreEqual(w.happiness, start + e.workerEffects[0].value);
        }

        // TODO update
        //[Test]
        //public void Product() {
            //ProductEffect pe = new ProductEffect("Design");
            //pe.buff = new StatBuff("Design", 100);

            //e.productEffects = new List<ProductEffect>();
            //e.productEffects.Add(pe);

            //ProductType pt = ScriptableObject.CreateInstance<ProductType>();
            //pt.requiredVerticals = new List<Vertical>();
            //ProductType pt_ = ScriptableObject.CreateInstance<ProductType>();
            //pt_.requiredVerticals = new List<Vertical>();
            //List<ProductType> pts = new List<ProductType>() { pt, pt_ };

            //c.StartNewProduct(pts, 0, 0, 0);
            //Product p = c.products[0];
            //p.requiredProgress = 0;

            //float start = p.design.value;

            //e.Apply(c);

            //// The effect should not apply until the product is launched.
            //Assert.IsTrue(p.developing);
            //Assert.AreEqual(start, p.design.value);

            //p.Complete(c);

            //// The effect should after the product is launched.
            //Assert.IsTrue(p.launched);
            //Assert.AreEqual(p.design.value, start + pe.buff.value);

            //// Test applying the effect to an already launched product.
            //pe = new ProductEffect("Engineering");
            //pe.buff.value = 100;
            //e.productEffects[0] = pe;

            //start = p.engineering.value;

            //e.Apply(c);
            //Assert.AreEqual(p.engineering.value, start + pe.buff.value);
        //}
    }
}