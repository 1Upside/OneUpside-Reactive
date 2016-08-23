using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace OneUpside.Reactive
{
  /// <summary>
  /// An observable that binds to an observable that is not yet defined. When 
  /// <see cref="LateObservable{A}"/> is connected the bound observable must be 
  /// defined. This can be used to construct circularities.
  /// </summary>
  /// <typeparam name="A">The event type</typeparam>
  public sealed class LateObservable<A> : IConnectableObservable<A>
  {
    /// <summary>
    /// Get the bound observable. This only called when <see cref="Connect"/> 
    /// is called.
    /// </summary>
    public readonly Func<IObservable<A>> GetBinding;

    /// <summary>
    /// <see cref="Event"/> converted to an observable. All our observers
    /// are subscribed to this.
    /// </summary>
    private readonly IObservable<A> EventObservable;

    /// <summary>
    /// Construct a <see cref="LateObservable{A}"/>.
    /// </summary>
    /// <param name="getBinding">Assigned to <see cref="GetBinding"/></param>
    public LateObservable(Func<IObservable<A>> getBinding)
    {
      EventObservable = Observable.FromEventPattern<A>(
          h => Event += h,
          h => Event -= h
        )
        .Select(x => x.EventArgs);
      GetBinding = getBinding;
    }

    /// <summary>
    /// Once connected, all events from the bound observable are published
    /// through this event.
    /// </summary>
    private event EventHandler<A> Event;

    /// <summary>
    /// Begin publishing events from the bound observable to subscribed 
    /// observers. The bound observable must be defined when 
    /// <see cref="Connect"/> is called.
    /// </summary>
    /// <returns>When disposed, stops the publication of events from the
    /// bound observable to subscribed observers.</returns>
    public IDisposable Connect()
    {
      return GetBinding().Subscribe(x => { Event?.Invoke(this, x); });
    }

    /// <summary>
    /// Subscribe to receive events from the bound observable. Events will not
    /// be received until <see cref="Connect"/> is called, and not after the
    /// <see cref="Connect"/> disposable is disposed.
    /// </summary>
    /// <param name="observer">The observer to subscribe with.</param>
    /// <returns>When disposed, stops the publication of events from the
    /// bound observable to <paramref name="observer"/>.</returns>
    public IDisposable Subscribe(IObserver<A> observer)
    {
      return EventObservable.Subscribe(observer);
    }
  }

}
