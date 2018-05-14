using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class ODataVisitor : IQueryVisitor
  {
    private TextWriter _writer;
    private Stack<ILogical> _logicals = new Stack<ILogical>();
    private IQueryWriterSettings _settings;

    public void Visit(QueryItem query)
    {
      throw new NotImplementedException();
    }

    public void Visit(AndOperator op)
    {
      _logicals.Push(op);

      op.Left.Visit(this);
      _writer.Write(" and ");
      op.Right.Visit(this);

      _logicals.Pop();
    }

    public void Visit(BetweenOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(BooleanLiteral op)
    {
      if (op.Value)
        _writer.Write("true");
      else
        _writer.Write("false");
    }

    public void Visit(DateTimeLiteral op)
    {
      if (op.Value.TimeOfDay.Equals(TimeSpan.Zero))
      {
        _writer.Write(op.Value.ToString("yyyy-MM-dd"));
      }
      else
      {
        _writer.Write(ElementFactory.Local.LocalizationContext.AsDateTimeUtc(op.Value).Value.ToString("s"));
        _writer.Write('Z');
      }
    }

    public void Visit(EqualsOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" eq ");
      op.Right.Visit(this);
    }

    public void Visit(FloatLiteral op)
    {
      _writer.Write(op.Value.ToString());
    }

    public void Visit(FunctionExpression op)
    {
      var name = op.Name;
      switch (name.ToLowerInvariant())
      {
        case "getdate":
          name = "now";
          break;
      }

      _writer.Write(name);
      _writer.Write('(');
      var first = true;
      foreach (var arg in op.Args)
      {
        if (!first)
          _writer.Write(", ");
        first = false;
        arg.Visit(this);
      }
      _writer.Write(')');
    }

    public void Visit(GreaterThanOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" gt ");
      op.Right.Visit(this);
    }

    public void Visit(GreaterThanOrEqualsOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" ge ");
      op.Right.Visit(this);
    }

    public void Visit(InOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" in ");
      op.Right.Visit(this);
    }

    public void Visit(IntegerLiteral op)
    {
      _writer.Write(op.Value.ToString());
    }

    public void Visit(IsOperator op)
    {
      op.Left.Visit(this);
      switch (op.Right)
      {
        case IsOperand.Defined:
        case IsOperand.NotNull:
          _writer.Write(" ne ");
          break;
        default:
          _writer.Write(" eq ");
          break;
      }
      _writer.Write("null");
    }

    public void Visit(LessThanOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" lt ");
      op.Right.Visit(this);
    }

    public void Visit(LessThanOrEqualsOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" le ");
      op.Right.Visit(this);
    }

    public void Visit(LikeOperator op)
    {
      if (!(op.Left is PropertyReference prop && op.Right is StringLiteral str))
        throw new NotSupportedException();

      var val = str.Value;
      var start = 0;
      var length = val.Length;
      var func = "";
      if (val.StartsWith("%") && val.EndsWith("%"))
      {
        start++;
        length--;
        func = "contains";
      }
      else if (val.StartsWith("%"))
      {
        start++;
        func = "endswith";
      }
      else if (val.EndsWith("%"))
      {
        length--;
        func = "startswith";
      }

      var output = new StringBuilder();
      for (var i = start; i < length; i++)
      {
        switch (val[i])
        {
          case '%':
            throw new NotSupportedException();
          case '[':
            if (i + 2 < length && val[i + 2] == ']')
            {
              output.Append(val[i + 1]);
              i += 2;
            }
            else
            {
              throw new NotSupportedException();
            }
            break;
          default:
            output.Append(val[i]);
            break;
        }
      }

      if (string.IsNullOrEmpty(func))
      {
        new EqualsOperator()
        {
          Left = prop,
          Right = new StringLiteral(output.ToString())
        }.Visit(this);
      }
      else
      {
        new FunctionExpression()
        {
          Name = func,
          Args = {
            prop, new StringLiteral(output.ToString())
          }
        }.Visit(this);
      }
    }

    public void Visit(ListExpression op)
    {
      _writer.Write('(');
      var first = true;
      foreach (var arg in op.Values)
      {
        if (!first)
          _writer.Write(", ");
        first = false;
        arg.Visit(this);
      }
      _writer.Write(')');
    }

    public void Visit(NotBetweenOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(NotEqualsOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" ne ");
      op.Right.Visit(this);
    }

    public void Visit(NotInOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(NotLikeOperator op)
    {
      new NotOperator()
      {
        Arg = new LikeOperator()
        {
          Left = op.Left,
          Right = op.Right
        }
      }.Visit(this);
    }

    public void Visit(NotOperator op)
    {
      _logicals.Push(op);
      _writer.Write('(');
      op.Arg.Visit(this);
      _writer.Write(')');
      _logicals.Pop();
    }

    public void Visit(ObjectLiteral op)
    {
      var dataType = default(string);
      if (!string.IsNullOrEmpty(op.TypeProvider?.Table.Type))
      {
        var props = _settings.GetProperties(op.TypeProvider?.Table.Type);
        if (props != null && props.TryGetValue(op.TypeProvider.Name, out var propDefn))
        {
          dataType = propDefn.DataType().Value;
          if (dataType == "foreign")
            dataType = null;
        }
      }

      if (dataType == "boolean")
      {
        Visit(new BooleanLiteral(op.Value == "1"));
      }
      else if ((dataType == null || dataType == "date")
        && DateTime.TryParse(op.Value, out DateTime date))
      {
        Visit(new DateTimeLiteral(date));
      }
      else if ((dataType == null || dataType == "integer")
        && long.TryParse(op.Value, out long lng))
      {
        Visit(new IntegerLiteral(lng));
      }
      else if ((dataType == null || dataType == "float" || dataType == "decimal")
        && double.TryParse(op.Value, out double dbl))
      {
        Visit(new FloatLiteral(dbl));
      }
      else
      {
        if (dataType == "item" || dataType == "md5" || op.Value.IsGuid())
          _writer.Write('\'');
        else
          _writer.Write("N'");
        _writer.Write(op.Value.Replace("'", "''"));
        _writer.Write('\'');
      }
    }

    public void Visit(OrOperator op)
    {
      var paren = _logicals.Count > 0 && !(_logicals.Peek() is OrOperator);

      if (paren)
        _writer.Write('(');
      _logicals.Push(op);

      op.Left.Visit(this);
      _writer.Write(" or ");
      op.Right.Visit(this);

      _logicals.Pop();
      if (paren)
        _writer.Write(')');
    }

    public void Visit(PropertyReference op)
    {
      foreach (var prop in GetPropTree(op).Reverse())
      {
        var first = true;
        if (!first)
          _writer.Write("/");
        _writer.Write(op.Name);
      }
    }

    public void Visit(StringLiteral op)
    {
      _writer.Write('\'');
      _writer.Write(op.Value.Replace("'", "''"));
      _writer.Write('\'');
    }

    public void Visit(MultiplicationOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(DivisionOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(ModulusOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(AdditionOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(SubtractionOperator op)
    {
      throw new NotImplementedException();
    }

    private IEnumerable<PropertyReference> GetPropTree(PropertyReference curr)
    {
      while (curr != null)
      {
        yield return curr;
        curr = curr.Table?.TypeProvider;
      }
    }
  }
}
