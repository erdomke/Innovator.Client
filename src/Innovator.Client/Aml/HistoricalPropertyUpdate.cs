using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  public class HistoricalPropertyUpdate
  {
    public string Name { get; set; }
    public string Original { get; set; }
    public string Final { get; set; }

    public override string ToString()
    {
      if (string.IsNullOrEmpty(Original))
        return Name + ": \"" + (Final ?? "") + "\"";
      return Name + ": \"" + (Original ?? "") + "\" > \"" + (Final ?? "") + "\"";
    }

    private enum State
    {
      PropertyName,
      QuotedValue,
      Invalid
    }

    public static IEnumerable<HistoricalPropertyUpdate> Parse(string update)
    {
      if (string.IsNullOrEmpty(update))
        yield break;

      var state = State.PropertyName;
      var last = 0;
      var curr = new HistoricalPropertyUpdate();
      for (var i = 0; i < update.Length; i++)
      {
        switch (state)
        {
          case State.PropertyName:
            if (update[i] == ':')
            {
              if ((i + 3) < update.Length && update[i + 1] == ' ' && update[i + 2] == '"')
              {
                curr.Name = update.Substring(last, i - last);
                i += 2;
                last = i + 1;
                state = State.QuotedValue;
              }
              else
              {
                yield break;
              }
            }
            else if (update[i] == '\r' || update[i] == '\n')
            {
              yield break;
            }
            break;
          case State.QuotedValue:
            if (update[i] == '"')
            {
              if ((i + 5) < update.Length
                && update[i + 1] == ' '
                && update[i + 2] == '>'
                && update[i + 3] == ' '
                && update[i + 4] == '"')
              {
                curr.Original = update.Substring(last, i - last);
                i += 4;
                last = i + 1;
                state = State.QuotedValue;
              }
              else
              {
                var validEnd = i == update.Length - 1;
                if ((i + 1) < update.Length && (update[i + 1] == '\r' || update[i + 1] == '\n'))
                {
                  var colonIdx = update.IndexOf(':', i);
                  var lineFeedIdx = update.IndexOf('\n', Math.Min(update.Length - 1, i + 3));
                  if (lineFeedIdx < 0)
                    lineFeedIdx = update.Length;
                  validEnd = colonIdx > 0 && colonIdx < lineFeedIdx;
                }

                if (validEnd)
                {
                  curr.Final = update.Substring(last, i - last);
                  yield return curr;
                  curr = new HistoricalPropertyUpdate();
                  if (i < update.Length - 1)
                  {
                    i++;
                    while (i < update.Length && char.IsWhiteSpace(update[i]))
                      i++;
                    last = i;
                    i--;
                  }
                  state = State.PropertyName;
                }
              }
            }
            break;
        }
      }
    }
  }
}
