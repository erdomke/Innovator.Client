using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  internal enum PrecedenceLevel
  {
    Comma = 0,
    Or = 3,
    And = 4,
    Not = 5,
    Comparison = 7,
    SubComparison = 8,
    Additive = 9,
    Multiplicative = 10,
    Negation = 11,
    Parentheses = 15
  }
}
