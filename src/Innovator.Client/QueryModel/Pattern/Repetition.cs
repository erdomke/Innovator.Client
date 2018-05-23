using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client.QueryModel
{
  public class Repetition : IPatternSegment
  {
    public bool Greedy { get; set; }
    public int MinCount { get; set; }
    public int MaxCount { get; set; }

    public Repetition()
    {
      this.Greedy = true;
      this.MinCount = 1;
      this.MaxCount = 1;
    }

    public override string ToString()
    {
      string result;
      
      if (this.MinCount == 1 && this.MaxCount == 1)
      {
        return string.Empty;
      }
      else if (this.MinCount == 0 && this.MaxCount == 1)
      {
        result = "?";
      }
      else if (this.MinCount == 1 && this.MaxCount == int.MaxValue)
      {
        result = "+";
      }
      else if (this.MinCount == 0 && this.MaxCount == int.MaxValue)
      {
        result = "*";
      }
      else if (this.MinCount == this.MaxCount)
      {
        result = "{" + this.MinCount.ToString() + "}";
      }
      else if (this.MaxCount == int.MaxValue)
      {
        result = "{" + this.MinCount.ToString() + ",}";
      }
      else
      {
        result = "{" + this.MinCount.ToString() + "," + this.MaxCount.ToString() + "}";
      }

      if (!this.Greedy) result += "?";
      return result;
    }

    public void Visit(IPatternVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
