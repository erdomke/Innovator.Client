using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client.QueryModel
{
  public class Anchor : IMatch
  {
    private Repetition _repeat = new Repetition();

    public Repetition Repeat 
    { 
      get
      {
        return _repeat;
      }
    }
    public AnchorType Type {get; set;}

    public Anchor() {}
    public Anchor(char type)
    {
      switch (type) 
      {
        case '^':
          this.Type = AnchorType.Start_Line;
          break;
        case '$':
          this.Type = AnchorType.End_Line;
          break;
        case 'A':
          this.Type = AnchorType.Start_Absolute;
          break;
        case 'b':
          this.Type = AnchorType.WordBoundary;
          break;
        case 'Z':
          this.Type = AnchorType.End_BeforeNewline;
          break;
        case 'z':
          this.Type = AnchorType.End_Absolute;
          break;
      }
    }

    public override string ToString()
    {
      switch (this.Type)
      {
        case AnchorType.End_Absolute:
          return @"\z";
        case AnchorType.End_BeforeNewline:
          return @"\Z";
        case AnchorType.End_Line:
          return @"$";
        case AnchorType.Start_Absolute:
          return @"\A";
        case AnchorType.Start_Line:
          return @"^";
        case AnchorType.WordBoundary:
          return @"\b";
      }
      return string.Empty;
    }

    public void Visit(IPatternVisitor visitor)
    {
      visitor.Visit(this);
    }
    public bool ContentEquals(IMatch value)
    {
      return false;
    }
  }
}
