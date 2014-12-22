using UnityEngine;
using UnityEditor;
using System;
using System.Threading;
using NUnit.Framework;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace UnityTest
{
	[TestFixture]
	internal class ProductTests
	{
        private GameObject gameObj;
        private GameData gd;
        private GameManager gm;

        private Product p = null;
        private ProductRecipe pr = null;
        private ProductType pt;
        private Item item;

        [SetUp]
        public void SetUp() {
            pt = ProductType.Load("Social Network");

            gameObj = new GameObject("Game Manager");
            gm = gameObj.AddComponent<GameManager>();
            gd = GameData.New("DEFAULTCORP");
            gm.Load(gd);

            List<ProductType> pts = new List<ProductType>() { pt };

            p = ScriptableObject.CreateInstance<Product>();
            p.Init(pts, 0, 0, 0);
            pr = ProductRecipe.LoadFromTypes(pts);

            item = (Item)GameObject.Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Editor/Tests/Resources/TestItem.asset", typeof(Item)));
        }

        [TearDown]
        public void TearDown() {
            UnityEngine.Object.DestroyImmediate(gameObj);
            gm = null;
            p = null;
            pt = null;
            pr = null;
            item = null;
        }

		[Test]
		public void ProductConstructor() {
            Assert.IsNotNull(p);

            // Creates a name.
            Assert.AreNotEqual(p.name, "");
		}

		[Test]
		public void Develop() {
            Assert.AreEqual(p.state, Product.State.DEVELOPMENT);
            Assert.AreEqual(p.progress, 0);

            p.Develop(100000);

            Assert.AreEqual(p.progress, 100000/p.progressRequired);
            Assert.AreEqual(p.state, Product.State.LAUNCHED);
        }

		[Test]
		public void Revenue_NotLaunched() {
            Assert.AreNotEqual(p.state, Product.State.LAUNCHED);
            Assert.AreEqual(p.Revenue(10), 0);
        }

		[Test]
		public void Revenue_Launched() {
            p.design.baseValue = 100;
            p.marketing.baseValue = 100;
            p.engineering.baseValue = 100;

            p.Launch();

            Assert.IsTrue(p.Revenue(4) > 0);
        }

		[Test]
		public void Revenue_ZeroStats() {
            p.design.baseValue = 0;
            p.marketing.baseValue = 0;
            p.engineering.baseValue = 0;

            p.Launch();

            float zeroRev = p.Revenue(4);

            // You still make a little money.
            Assert.IsTrue(zeroRev > 0);


            // But it should be less than a product with more stats.
            p.design.baseValue = 100;
            p.marketing.baseValue = 100;
            p.engineering.baseValue = 100;

            p.Launch();

            Assert.IsTrue(p.Revenue(4) > zeroRev);
        }

		[Test]
		public void Shutdown() {
            p.Shutdown();

            Assert.AreEqual(p.state, Product.State.RETIRED);
        }

        [Test]
        public void Points() {
            Assert.AreEqual(p.points, p.productTypes.Sum(t => t.points));
        }

        [Test]
        public void Difficulty() {
            ProductType pt_  = ScriptableObject.CreateInstance<ProductType>();
            pt_.difficulty  = 1f;

            ProductType pt__ = ScriptableObject.CreateInstance<ProductType>();
            pt__.difficulty = 2f;

            Product prod = ScriptableObject.CreateInstance<Product>();
            prod.Init( new List<ProductType> { pt_, pt__ }, 0, 0, 0);

            Assert.AreEqual(prod.difficulty, 1.5);
        }

        [Test]
        public void RequiredVerticals() {
            ProductType pt_  = ScriptableObject.CreateInstance<ProductType>();
            Vertical vert_   = ScriptableObject.CreateInstance<Vertical>();
            pt_.requiredVerticals = new List<Vertical>() { vert_ };

            ProductType pt__ = ScriptableObject.CreateInstance<ProductType>();
            Vertical vert__  = ScriptableObject.CreateInstance<Vertical>();
            pt__.requiredVerticals = new List<Vertical>() { vert__ };

            Product prod = ScriptableObject.CreateInstance<Product>();
            prod.Init( new List<ProductType> { pt_, pt__ }, 0, 0, 0 );

            Assert.AreEqual(prod.requiredVerticals, new List<Vertical>() { vert_, vert__ });
        }

        [Test]
        public void RequiredInfrastructure() {
            ProductType pt_   = ScriptableObject.CreateInstance<ProductType>();
            Infrastructure i_ = new Infrastructure();
            i_[Infrastructure.Type.Datacenter] = 10;
            pt_.requiredInfrastructure = i_;

            ProductType pt__   = ScriptableObject.CreateInstance<ProductType>();
            Infrastructure i__ = new Infrastructure();
            i__[Infrastructure.Type.Studio] = 3;
            pt__.requiredInfrastructure = i__;

            Product prod = ScriptableObject.CreateInstance<Product>();
            prod.Init( new List<ProductType> { pt_, pt__ }, 0, 0, 0 );

            Infrastructure i = new Infrastructure();
            i[Infrastructure.Type.Datacenter] = 10;
            i[Infrastructure.Type.Studio] = 3;
            Assert.IsTrue(prod.requiredInfrastructure.Equals(i));
        }

        [Test]
        public void RequiredProgress() {
            int baseProgress = Product.baseProgress;
            float difficulty = 2;
            pt.difficulty = difficulty;

            Assert.AreEqual(p.difficulty, difficulty);

            float cre = 2;
            float cle = 3;
            float cha = 4;

            Worker w = ScriptableObject.CreateInstance<Worker>();
            w.Init("Yoyo Ma");
            w.creativity.baseValue = cre;
            w.cleverness.baseValue = cle;
            w.charisma.baseValue   = cha;
            gd.company.HireWorker(w);

            Assert.AreEqual(gd.company.AggregateWorkerStat("Creativity"), cre);
            Assert.AreEqual(gd.company.AggregateWorkerStat("Cleverness"), cle);
            Assert.AreEqual(gd.company.AggregateWorkerStat("Charisma"),   cha);

            int n = 3;
            float expected;
            float required;

            // Fibonacci value for n+2:
            int fib = 5;

            expected = (fib * baseProgress * difficulty)/cre;
            required = p.ProgressRequired("Design", n, gd.company);
            Assert.AreEqual(expected, required);

            expected = (fib * baseProgress * difficulty)/cle;
            required = p.ProgressRequired("Engineering", n, gd.company);
            Assert.AreEqual(expected, required);

            expected = (fib * baseProgress * difficulty)/cha;
            required = p.ProgressRequired("Marketing", n, gd.company);
            Assert.AreEqual(expected, required);
        }
    }
}
