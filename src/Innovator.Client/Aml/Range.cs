using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  /// <summary>
  /// A range of values between a minimum and maximum (inclusive)
  /// </summary>
  public interface IRange
  {
    /// <summary>
    /// The minimum value of the range
    /// </summary>
    object Minimum { get; }
    /// <summary>
    /// The maximum value of the range
    /// </summary>
    object Maximum { get; }
  }

  /// <summary>
  /// A strongly-typed range of values between a minimum and maximum (inclusive)
  /// </summary>
  /// <typeparam name="T"></typeparam>
  [DebuggerDisplay("{DebuggerDisplay,nq}")]
  public struct Range<T> : IRange where T : IComparable
  {
    private readonly bool _hasValue;
    private T _min;
    private T _max;

    /// <summary>
    /// Initializes a new instance of the <see cref="Range{T}"/> struct.
    /// </summary>
    /// <param name="min">The minimum.</param>
    /// <param name="max">The maximum.</param>
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

    /// <summary>
    /// Whether the range has been initialized with a value
    /// </summary>
    public bool HasValue { get { return _hasValue; } }
    /// <summary>
    /// The minimum value of the range
    /// </summary>
    public T Minimum { get { return _min; } }
    /// <summary>
    /// The maximum value of the range
    /// </summary>
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

    /// <summary>
    /// Whether or not the range contains the specified value
    /// </summary>
    public bool ContainsValue(T value)
    {
      return _min.CompareTo(value) <= 0 && value.CompareTo(_max) <= 0;
    }
  }
}
