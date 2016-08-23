using System;

namespace OneUpside.Reactive
{

  /// <summary>
  /// Helper functions for <see cref="LateObservable{A}"/>.
  /// </summary>
  public static class LateObservable
  {
    /// <summary>
    /// Construct a <see cref="LateObservable{A}"/>.
    /// </summary>
    /// <typeparam name="A">The event type.</typeparam>
    /// <param name="getBinding">
    /// Assigned to <see cref="LateObservable{A}.GetBinding"/>
    /// </param>
    /// <returns>The constructed <see cref="LateObservable{A}"/>.</returns>
    public static LateObservable<A> Create<A>(
      Func<IObservable<A>> getBinding)
    {
      return new LateObservable<A>(getBinding);
    }
  }

}
