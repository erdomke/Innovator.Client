using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel.Functions
{
  public class Abs : FunctionExpression
  {
    public Abs() : base(1) { }

    public IExpression Value { get => _args[0]; set => _args[0] = value; }
  }

  public class Ceiling : FunctionExpression
  {
    public Ceiling() : base(1) { }

    public IExpression Value { get => _args[0]; set => _args[0] = value; }
  }

  public class Floor : FunctionExpression
  {
    public Floor() : base(1) { }

    public IExpression Value { get => _args[0]; set => _args[0] = value; }
  }

  public class Power : FunctionExpression
  {
    public Power() : base(2) { }

    public IExpression Value { get => _args[0]; set => _args[0] = value; }
    public IExpression Exponent { get => _args[1]; set => _args[1] = value; }
  }

  public class Round : FunctionExpression
  {
    public Round() : base(2) { }

    public IExpression Value { get => _args[0]; set => _args[0] = value; }
    public IExpression Digits { get => _args[1]; set => _args[1] = value; }
  }

  public class Truncate : FunctionExpression
  {
    public Truncate() : base(2) { }

    public IExpression Value { get => _args[0]; set => _args[0] = value; }
    public IExpression Digits { get => _args[1]; set => _args[1] = value; }
  }
}
