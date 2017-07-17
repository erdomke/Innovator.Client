using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  public interface IRange
  {
    object Minimum { get; }
    object Maximum { get; }
  }

  [DebuggerDisplay("{DebuggerDisplay,nq}")]
  public struct Range<T> : IRange where T : IComparable
  {
    private bool _hasValue;
    private T _min;
    private T _max;

    public Range(T min, T max)
    {
      if (min.CompareTo(max) <= 0)
      {
        _min = min;
        _max = max;
      }
      else
      {
        _min = max;
        _max = min;
      }
      _hasValue = true;
    }

    public bool HasValue { get { return _hasValue; } }
    public T Minimum { get { return _min; } }
    public T Maximum { get { return _max; } }

    private string DebuggerDisplay
    {
      get
      {
        if (_hasValue)
          return string.Format("[{0}, {1}]", _min, _max);
        return "{Empty}";
      }
    }

    object IRange.Minimum { get { return _min; } }
    object IRange.Maximum { get { return _max; } }

    public bool ContainsValue(T value)
    {
      return _min.CompareTo(value) <= 0 && value.CompareTo(_max) <= 0;
    }
  }
}
