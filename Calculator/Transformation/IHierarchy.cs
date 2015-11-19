using System;
using System.Collections.Generic;
namespace Calculator.Transformation
{
    public interface IHierarchy
    {
        IEnumerable<short> GetChildren(short id);
        IEnumerable<short> GetParents(short id);

        bool RelationExists(short child, short parent);
    }
}
