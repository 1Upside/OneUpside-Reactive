using System;

namespace OneUpside.Reactive
{
  public static class Behaviour
  {
    /// <summary>
    /// The constant-value Behaviour.
    /// </summary>
    public static Behaviour<A> Create<A>(A value)
    {
      return new Behaviour<A>(value);
    }

    /// <summary>
    /// A Behaviour that begins with an initial value and steps to the value of
    /// the Event at the same time the Event occurs.
    /// </summary>
    public static Behaviour<A> Step<A>(A initial, IObservable<A> with)
    {
      return Behaviour<A>.Step(initial, with);
    }

    /// <summary>
    /// A Behaviour that begins with an initial value and steps when the Event
    /// occurs to a function-mapped value from the Event value and the previous
    /// Behaviour value.
    /// </summary>
    public static Behaviour<A> Scan<A, B>(
      A initial,
      IObservable<B> source,
      Func<A, B, A> next)
    {
      return Behaviour<A>.Scan(initial, source, next);
    }

    public static Behaviour<A> Join<A>(
      this Behaviour<Behaviour<A>> a)
    {
      return Behaviour<A>.Join(a);
    }

    /// <summary>
    /// The events of <paramref name="source"/> when <paramref name="gate"/> is 
    /// true.
    /// </summary>
    public static IObservable<A> Gate<A>(
      IObservable<A> source,
      Behaviour<bool> gate)
    {
      return Behaviour<A>.Gate(source, gate);
    }

    public static Tuple<UntypedConnectable, Behaviour<A>> LatelyBound<A>(
      A initial,
      Func<Behaviour<A>> getBinding)
    {
      return Behaviour<A>.LatelyBound(initial, getBinding);
    }

  }

}
