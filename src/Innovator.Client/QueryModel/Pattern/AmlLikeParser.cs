using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class AmlLikeParser : PatternParser
  {
    private static readonly PatternParser _writePattern = new PatternParser('*', '\0', '\0', '\0', '^', '-');

    public static PatternParser Instance { get; } = new AmlLikeParser('%', '\0', '\0', '\0', '^', '-');
    internal static PatternParser NoCharSet { get; } = new AmlLikeParser('%', '\0', '\0', '\0', '\0', '\0');

    public AmlLikeParser(char anything, char singleChar, char singleDigit, char escape, char inverseSet, char setRange)
      : base(anything, singleChar, singleDigit, escape, inverseSet, setRange) { }

    public override PatternList Parse(string str)
    {
      return base.Parse(str.Replace('*', '%'));
    }

    public override string Render(PatternList pattern)
    {
      var writer = new AmlPatternWriter();
      pattern.Visit(writer);
      return writer.ToString();
    }

    private class AmlPatternWriter : SqlPatternWriter
    {
      public AmlPatternWriter() : base(_writePattern) { }
      public AmlPatternWriter(TextWriter writer) : base(writer, _writePattern) { }

      public override void Visit(StringMatch value)
      {
        if (value.Match.ToString().IndexOf('*') >= 0)
          throw new NotSupportedException();
        base.Visit(value);
      }
    }
  }
}
