using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client
{
  /// <summary>
  /// A promise to return two results
  /// </summary>
  /// <typeparam name="T1">The type of the 1.</typeparam>
  /// <typeparam name="T2">The type of the 2.</typeparam>
  public class PromiseResult<T1, T2>
  {
    /// <summary>
    /// Gets the first result.
    /// </summary>
    public T1 Result1 { get; internal set; }
    /// <summary>
    /// Gets the second result.
    /// </summary>
    public T2 Result2 { get; internal set; }

    internal PromiseResult() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PromiseResult{T1, T2}"/> class.
    /// </summary>
    /// <param name="result1">The result1.</param>
    /// <param name="result2">The result2.</param>
    public PromiseResult(T1 result1, T2 result2)
    {
      this.Result1 = result1;
      this.Result2 = result2;
    }
  }

  /// <summary>
  /// A promise to return three results
  /// </summary>
  /// <typeparam name="T1">The type of the 1.</typeparam>
  /// <typeparam name="T2">The type of the 2.</typeparam>
  /// <typeparam name="T3">The type of the 3.</typeparam>
  public class PromiseResult<T1, T2, T3>
  {
    /// <summary>
    /// Gets the first result.
    /// </summary>
    public T1 Result1 { get; internal set; }
    /// <summary>
    /// Gets the second result.
    /// </summary>
    public T2 Result2 { get; internal set; }
    /// <summary>
    /// Gets the third result.
    /// </summary>
    public T3 Result3 { get; internal set; }

    internal PromiseResult() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PromiseResult{T1, T2, T3}"/> class.
    /// </summary>
    /// <param name="result1">The result1.</param>
    /// <param name="result2">The result2.</param>
    /// <param name="result3">The result3.</param>
    public PromiseResult(T1 result1, T2 result2, T3 result3)
    {
      this.Result1 = result1;
      this.Result2 = result2;
      this.Result3 = result3;
    }
  }

  /// <summary>
  /// A promise to return four results
  /// </summary>
  /// <typeparam name="T1">The type of the 1.</typeparam>
  /// <typeparam name="T2">The type of the 2.</typeparam>
  /// <typeparam name="T3">The type of the 3.</typeparam>
  /// <typeparam name="T4">The type of the 4.</typeparam>
  public class PromiseResult<T1, T2, T3, T4>
  {
    /// <summary>
    /// Gets the first result.
    /// </summary>
    public T1 Result1 { get; internal set; }
    /// <summary>
    /// Gets the second result.
    /// </summary>
    public T2 Result2 { get; internal set; }
    /// <summary>
    /// Gets the third result.
    /// </summary>
    public T3 Result3 { get; internal set; }
    /// <summary>
    /// Gets the fourth result.
    /// </summary>
    public T4 Result4 { get; internal set; }

    internal PromiseResult() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PromiseResult{T1, T2, T3, T4}"/> class.
    /// </summary>
    /// <param name="result1">The result1.</param>
    /// <param name="result2">The result2.</param>
    /// <param name="result3">The result3.</param>
    /// <param name="result4">The result4.</param>
    public PromiseResult(T1 result1, T2 result2, T3 result3, T4 result4)
    {
      this.Result1 = result1;
      this.Result2 = result2;
      this.Result3 = result3;
      this.Result4 = result4;
    }
  }
}
