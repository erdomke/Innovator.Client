using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  /// <summary>
  /// A model representing a relationship where the <c>related_id</c> is of type <typeparamref name="TRelated"/>
  /// </summary>
  /// <typeparam name="TRelated">The type of the related item.</typeparam>
  /// <seealso cref="Innovator.Client.IReadOnlyItem" />
  public interface IRelationship<TRelated> : IReadOnlyItem { }
  /// <summary>
  /// A model representing a relationship where the <c>source_id</c> is of type <typeparamref name="TSource"/>
  /// </summary>
  /// <typeparam name="TSource">The type of the source item.</typeparam>
  /// <seealso cref="Innovator.Client.IReadOnlyItem" />
  public interface INullRelationship<TSource> : IReadOnlyItem { }
}
