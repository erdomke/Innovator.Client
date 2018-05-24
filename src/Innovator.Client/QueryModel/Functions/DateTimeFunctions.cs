using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel.Functions
{
  public class AddNanoseconds : FunctionExpression
  {
    public AddNanoseconds() : base(2) { }

    public IExpression Expression { get => _args[0]; set => _args[0] = value; }
    public IExpression Number { get => _args[1]; set => _args[1] = value; }
  }

  public class AddMicroseconds : FunctionExpression
  {
    public AddMicroseconds() : base(2) { }

    public IExpression Expression { get => _args[0]; set => _args[0] = value; }
    public IExpression Number { get => _args[1]; set => _args[1] = value; }
  }

  public class AddMilliseconds : FunctionExpression
  {
    public AddMilliseconds() : base(2) { }

    public IExpression Expression { get => _args[0]; set => _args[0] = value; }
    public IExpression Number { get => _args[1]; set => _args[1] = value; }
  }

  public class AddSeconds : FunctionExpression
  {
    public AddSeconds() : base(2) { }

    public IExpression Expression { get => _args[0]; set => _args[0] = value; }
    public IExpression Number { get => _args[1]; set => _args[1] = value; }
  }

  public class AddMinutes : FunctionExpression
  {
    public AddMinutes() : base(2) { }

    public IExpression Expression { get => _args[0]; set => _args[0] = value; }
    public IExpression Number { get => _args[1]; set => _args[1] = value; }
  }

  public class AddHours : FunctionExpression
  {
    public AddHours() : base(2) { }

    public IExpression Expression { get => _args[0]; set => _args[0] = value; }
    public IExpression Number { get => _args[1]; set => _args[1] = value; }
  }

  public class AddDays : FunctionExpression
  {
    public AddDays() : base(2) { }

    public IExpression Expression { get => _args[0]; set => _args[0] = value; }
    public IExpression Number { get => _args[1]; set => _args[1] = value; }
  }

  public class AddMonths : FunctionExpression
  {
    public AddMonths() : base(2) { }

    public IExpression Expression { get => _args[0]; set => _args[0] = value; }
    public IExpression Number { get => _args[1]; set => _args[1] = value; }
  }

  public class AddYears : FunctionExpression
  {
    public AddYears() : base(2) { }

    public IExpression Expression { get => _args[0]; set => _args[0] = value; }
    public IExpression Number { get => _args[1]; set => _args[1] = value; }
  }

  public class CurrentDateTime : FunctionExpression
  {
    public CurrentDateTime() : base(0) { }

    public override IExpression Evaluate() => new DateTimeLiteral(DateTime.Now);
  }

  public class CurrentUtcDateTime : FunctionExpression
  {
    public CurrentUtcDateTime() : base(0) { }

    public override IExpression Evaluate() => new DateTimeLiteral(DateTime.UtcNow);
  }

  public class Day : FunctionExpression
  {
    public Day() : base(1) { }

    public IExpression Expression { get => _args[0]; set => _args[0] = value; }
  }

  public class DayOfYear : FunctionExpression
  {
    public DayOfYear() : base(1) { }

    public IExpression Expression { get => _args[0]; set => _args[0] = value; }
  }

  public class DiffNanoseconds : FunctionExpression
  {
    public DiffNanoseconds() : base(2) { }

    public IExpression StartExpression { get => _args[0]; set => _args[0] = value; }
    public IExpression EndExpression { get => _args[1]; set => _args[1] = value; }
  }

  public class DiffMilliseconds : FunctionExpression
  {
    public DiffMilliseconds() : base(2) { }

    public IExpression StartExpression { get => _args[0]; set => _args[0] = value; }
    public IExpression EndExpression { get => _args[1]; set => _args[1] = value; }
  }

  public class DiffMicroseconds : FunctionExpression
  {
    public DiffMicroseconds() : base(2) { }

    public IExpression StartExpression { get => _args[0]; set => _args[0] = value; }
    public IExpression EndExpression { get => _args[1]; set => _args[1] = value; }
  }

  public class DiffSeconds : FunctionExpression
  {
    public DiffSeconds() : base(2) { }

    public IExpression StartExpression { get => _args[0]; set => _args[0] = value; }
    public IExpression EndExpression { get => _args[1]; set => _args[1] = value; }
  }

  public class DiffMinutes : FunctionExpression
  {
    public DiffMinutes() : base(2) { }

    public IExpression StartExpression { get => _args[0]; set => _args[0] = value; }
    public IExpression EndExpression { get => _args[1]; set => _args[1] = value; }
  }

  public class DiffHours : FunctionExpression
  {
    public DiffHours() : base(2) { }

    public IExpression StartExpression { get => _args[0]; set => _args[0] = value; }
    public IExpression EndExpression { get => _args[1]; set => _args[1] = value; }
  }

  public class DiffDays : FunctionExpression
  {
    public DiffDays() : base(2) { }

    public IExpression StartExpression { get => _args[0]; set => _args[0] = value; }
    public IExpression EndExpression { get => _args[1]; set => _args[1] = value; }
  }

  public class DiffMonths : FunctionExpression
  {
    public DiffMonths() : base(2) { }

    public IExpression StartExpression { get => _args[0]; set => _args[0] = value; }
    public IExpression EndExpression { get => _args[1]; set => _args[1] = value; }
  }

  public class DiffYears : FunctionExpression
  {
    public DiffYears() : base(2) { }

    public IExpression StartExpression { get => _args[0]; set => _args[0] = value; }
    public IExpression EndExpression { get => _args[1]; set => _args[1] = value; }
  }

  public class Hour : FunctionExpression
  {
    public Hour() : base(1) { }

    public IExpression Expression { get => _args[0]; set => _args[0] = value; }
  }

  public class Millisecond : FunctionExpression
  {
    public Millisecond() : base(1) { }

    public IExpression Expression { get => _args[0]; set => _args[0] = value; }
  }

  public class Minute : FunctionExpression
  {
    public Minute() : base(1) { }

    public IExpression Expression { get => _args[0]; set => _args[0] = value; }
  }

  public class Month : FunctionExpression
  {
    public Month() : base(1) { }

    public IExpression Expression { get => _args[0]; set => _args[0] = value; }
  }

  public class Second : FunctionExpression
  {
    public Second() : base(1) { }

    public IExpression Expression { get => _args[0]; set => _args[0] = value; }
  }

  public class TruncateTime : FunctionExpression
  {
    public TruncateTime() : base(1) { }

    public IExpression Expression { get => _args[0]; set => _args[0] = value; }
  }

  public class Year : FunctionExpression
  {
    public Year() : base(1) { }

    public IExpression Expression { get => _args[0]; set => _args[0] = value; }
  }
}
