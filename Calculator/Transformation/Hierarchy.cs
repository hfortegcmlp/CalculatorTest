using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Calculator.Contract;

namespace Calculator.Transformation
{
    internal class Hierarchy : IHierarchy
    {
        private const int p1 = 1009;
        private const int p2 = 1013;

        private ILookup<short, short> parents;
        private ILookup<short, short> children;
        private HashSet<int> relationHash;

        /// <summary>
        /// Where Tuple<int,int> represents a child_id|parent_id relationship
        /// </summary>
        /// <param name="relations"></param>
        public Hierarchy(List<Relation> relations)
        {
            if (relations == null)
                relations = new List<Relation>();

            for (int i = 0; i < relations.Count; i++)
            {
                var item = relations[i];

                if (item.Child == item.Parent)
                    continue;

                // the child is a relation of itself; the parent is a relation of itself, too                
                this.Add(relations, new Relation { Child = item.Child, Parent = item.Child });
                this.Add(relations, new Relation{ Child = item.Parent, Parent = item.Parent});
                
                // if this 'parent' is the child of another item G, then this 'child' is also a child of G.
                var grandparents = relations.Where(r => item.Parent == r.Child);
                var grandchildren = relations.Where(r => r.Parent == item.Child).Select(r => r.Child);
                var list = grandparents.Select(g => new Relation { Child = g.Child, Parent = item.Parent }).ToList();
                foreach (var g in list)
                    this.Add(relations, g);
            }

            //parents = relations.GroupBy(i => i.Item1, i => i.Item2).ToLookup(g => g.Key, g => g.ToList());
            parents = relations.ToLookup(i => i.Child, i => i.Parent);
            children = relations.ToLookup(i => i.Parent, i => i.Child);

            this.relationHash = new HashSet<int>(relations.Select(r => r.GetHashCode()));            
        }

        /// <summary>
        /// Wrap the 'add' function to ensure no duplicates are added
        /// </summary>
        /// <param name="relations"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool Add(List<Relation> relations, Relation item)
        {
            if (relations.Any(i => item.Child == i.Child && item.Parent == i.Parent))
                return false;

            relations.Add(item);
            return true;
        }

        public IEnumerable<short> GetChildren(short id)
        {
            return this.children.Contains(id) ? this.children[id] : new List<short>() { id };            
        }

        public IEnumerable<short> GetParents(short id)
        {
            return this.parents.Contains(id) ? this.parents[id] : new List<short>() { id };
        }

        public bool RelationExists(short child, short parent)
        {
            if (child == parent) return true;

            var hash = Relation.GetHashCode(child, parent);
            return this.relationHash.Contains(hash);
        }
    }
}
