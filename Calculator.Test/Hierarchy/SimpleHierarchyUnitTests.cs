using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Calculator.Transformation;
using Calculator.Contract;

namespace Calculator.Test
{
    [TestClass]
    public class SimpleHierarchyUnitTests
    {
        /// <summary>
        /// null hierarchy is one where every item is only related to itself
        /// </summary>
        [TestMethod]
        public void NullParameterTest()
        {
            var hierarchy = new Hierarchy(null);
            Assert.IsNotNull(hierarchy);
            Assert.AreEqual(1, hierarchy.GetChildren(1).Single());
            Assert.AreEqual(1, hierarchy.GetParents(1).Single());
        }

        [TestMethod]
        public void SingleElement_GetChildren()
        {
            short id1 = 1;
            short id2 = 2;

            var items = new List<Relation>();
            items.Add(new Relation { Child = id1, Parent = id2 });

            var result = new Hierarchy(items);

            var children = result.GetChildren(id1);
            Assert.AreEqual(1, children.Count());
            Assert.IsTrue(children.Contains(id1));

            var children0 = result.GetChildren(id2);
            Assert.AreEqual(2, children0.Count());
            Assert.IsTrue(children0.Contains(id1));
            Assert.IsTrue(children0.Contains(id2));
        }

        [TestMethod]
        public void SingleElement_GetParents()
        {
            short id1 = 1;
            short id2 = 2;

            var items = new List<Relation>();
            items.Add(new Relation { Child = id1, Parent = id2 });

            var result = new Hierarchy(items);

            var parents = result.GetParents(id1);
            Assert.AreEqual(2, parents.Count());
            Assert.IsTrue(parents.Contains(id1));
            Assert.IsTrue(parents.Contains(id2));

            var parents0 = result.GetParents(id2);
            Assert.AreEqual(1, parents0.Count());
            Assert.IsTrue(parents0.Contains(id2));
        }

        [TestMethod]
        public void TwoElementHierarchy_GetChildren()
        {
            short id1 = 1;
            short id2 = 2;
            short id3 = 3;

            var items = new List<Relation>();
            items.Add(new Relation { Child = id1, Parent = id2 });
            items.Add(new Relation { Child = id2, Parent = id3 });            

            var result = new Hierarchy(items);

            var children1 = result.GetChildren(id1);
            Assert.AreEqual(1, children1.Count());
            Assert.IsTrue(children1.Contains(id1));

            var children2 = result.GetChildren(id2);
            Assert.AreEqual(2, children2.Count());
            Assert.IsTrue(children2.Contains(id1));
            Assert.IsTrue(children2.Contains(id2));

            var children3 = result.GetChildren(id3);
            Assert.AreEqual(3, children3.Count());
            Assert.IsTrue(children3.Contains(id1));
            Assert.IsTrue(children3.Contains(id2));
            Assert.IsTrue(children3.Contains(id3));
        }

        [TestMethod]
        public void TwoElementHierarchy_GetParents()
        {
            short id1 = 1;
            short id2 = 2;
            short id3 = 3;

            var items = new List<Relation>();
            items.Add(new Relation { Child = id1, Parent = id2 });
            items.Add(new Relation { Child = id2, Parent = id3 });

            var result = new Hierarchy(items);

            var parents1 = result.GetParents(id1);
            Assert.AreEqual(3, parents1.Count());
            Assert.IsTrue(parents1.Contains(id1));
            Assert.IsTrue(parents1.Contains(id2));
            Assert.IsTrue(parents1.Contains(id3));

            var parents2 = result.GetParents(id2);
            Assert.AreEqual(2, parents2.Count());
            Assert.IsTrue(parents2.Contains(id2));
            Assert.IsTrue(parents2.Contains(id3));

            var parents3 = result.GetParents(id3);
            Assert.AreEqual(1, parents3.Count());
            Assert.IsTrue(parents3.Contains(id3));
        }
    }
}
