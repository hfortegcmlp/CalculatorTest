using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator.Contract
{
    public class Relation
    {
        public short Child { get; set; }

        public short Parent { get; set; }

        /// <summary>
        /// we can multiply by two 'large' prime (must be larger than our max id) 
        /// to get a unique hash 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return GetHashCode(this.Child, this.Parent);
        }

        public static int GetHashCode(int child, int parent)
        {
            var p1 = 1871;
            var p2 = 1873;

            return child * p1 + parent * p2;
        }
    }
}
