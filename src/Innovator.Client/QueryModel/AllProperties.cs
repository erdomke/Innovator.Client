using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  /// <summary>
  /// An expression representing all properties in a select statement (e.g. <code>select *</code>)
  /// </summary>
  /// <seealso cref="Innovator.Client.QueryModel.IOperand" />
  public class AllProperties : IOperand, ITableProvider
  {
    /// <summary>
    /// Gets the table for which all properties are returned.
    /// </summary>
    /// <value>
    /// The table.
    /// </value>
    public QueryItem Table { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether all extended properties (xProperties) should be
    /// returned as opposed to all normal properties 
    /// </summary>
    /// <value>
    ///   <c>true</c> if all extended properties (xProperties) should be returned; otherwise,
    ///   <c>false</c>.
    /// </value>
    public bool XProperties { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AllProperties"/> class.
    /// </summary>
    /// <param name="table">The table.</param>
    public AllProperties(QueryItem table)
    {
      Table = table;
    }

    /// <summary>
    /// Tell the specified visitor to process this expression component.
    /// </summary>
    /// <param name="visitor">The visitor.</param>
    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
