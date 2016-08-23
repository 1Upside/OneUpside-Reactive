using System;
using System.Collections;
using System.Collections.Generic;

namespace OneUpside.Reactive
{
  /// <summary>
  /// An ordered collection of <see cref="IDisposable"/>'s.
  /// </summary>
  public sealed class MultiDisposable : IDisposable, IEnumerable<IDisposable>
  {

    /// <summary>
    /// The ordered <see cref="IDisposable"/>'s.
    /// </summary>
    private readonly IEnumerable<IDisposable> Disposables;

    /// <summary>
    /// Construct a <see cref="MultiDisposable"/>.
    /// </summary>
    /// <param name="disposables">
    ///   The ordered <see cref="IDisposable"/>'s.
    /// </param>
    public MultiDisposable(IEnumerable<IDisposable> disposables)
    {
      Disposables = disposables;
    }

    /// <summary>
    ///   Construct a <see cref="MultiDisposable"/>.
    /// </summary>
    /// <param name="disposables">
    ///   The ordered <see cref="IDisposable"/>'s.
    /// </param>
    public MultiDisposable(params IDisposable[] disposables)
    {
      Disposables = disposables;
    }

    /// <summary>
    ///   Call .Dispose() on every <see cref="IDisposable"/> in order.
    /// </summary>
    public void Dispose()
    {
      foreach (var d in Disposables)
      {
        d.Dispose();
      }
    }

    public IEnumerator<IDisposable> GetEnumerator()
    {
      return Disposables.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

  }

}
