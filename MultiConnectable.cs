using System;
using System.Collections.Generic;
using System.Linq;

namespace OneUpside.Reactive
{
  /// <summary>
  /// An ordered collection of <see cref="IConnectable"/>'s.
  /// </summary>
  public sealed class MultiConnectable : IConnectable
  {
    /// <summary>
    /// The ordered <see cref="IConnectable"/>'s.
    /// </summary>
    private readonly IEnumerable<IConnectable> Connectables;

    /// <summary>
    /// Construct a <see cref="MultiConnectable"/>.
    /// </summary>
    /// <param name="connectables">
    ///   The ordered <see cref="IConnectable"/>'s.
    /// </param>
    public MultiConnectable(IEnumerable<IConnectable> connectables)
    {
      Connectables = connectables;
    }

    /// <summary>
    /// Construct a MultiConnectable.
    /// </summary>
    /// <param name="connectables">
    ///   The ordered <see cref="IConnectable"/>'s.
    /// </param>
    public MultiConnectable(params IConnectable[] connectables)
    {
      Connectables = connectables;
    }

    /// <summary>
    /// Call .Connect() on every <see cref="IConnectable"/> in order.
    /// </summary>
    /// <returns>
    ///   An <see cref="IDisposable"/> that, when disposed, calls .Dispose()
    ///   on each <see cref="IDisposable"/> created from .Connect() in order.
    /// </returns>
    public IDisposable Connect()
    {
      // We .ToArray() to force evaluation, to connect immediately.
      return new MultiDisposable(
        Connectables.Select(c => c.Connect()).ToArray());
    }

  }

}
