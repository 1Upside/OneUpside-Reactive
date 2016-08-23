using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Reactive.Concurrency;
using OneUpside.Data;

namespace OneUpside.Reactive
{
  /// <summary>
  /// A time-varying value.
  /// </summary>
  /// <typeparam name="A"></typeparam>
  public sealed class Behaviour<A>
  {
    /// <summary>
    /// Internally we delegate to Rx.
    /// </summary>
    private BehaviorSubject<A> Subject;

    /// <summary>
    /// The constant-value behaviour.
    /// </summary>
    /// <param name="value"></param>
    public Behaviour(A value)
    {
      Subject = new BehaviorSubject<A>(value);
    }

    /// <summary>
    /// The current value of the Behaviour. This should only be used where
    /// really necessary to interface with other APIs. Otherwise, use a 
    /// Behaviour function such as Snapshot.
    /// </summary>
    public A Value
    {
      get { return Subject.Value; }
    }

    /// <summary>
    /// Receive notifications for the Behaviour's value. Subscribers must be 
    /// idempotent because the same value may be propagated many times. This
    /// should only be used where necessary to interface with other APIs.
    /// Otherwise, use a Behaviour function such as Select.
    /// </summary>
    /// <param name="observer"></param>
    /// <returns></returns>
    public IDisposable Subscribe(IObserver<A> observer)
    {
      return Subject.Subscribe(observer);
    }

    /// <summary>
    /// Receive notifications for the Behaviour's value. Subscribers must be
    /// idempotent because the same value may be propagated many times. This
    /// should only be used where necessary to interface with other APIs.
    /// Otherwise, use a Behaviour function such as Select.
    /// </summary>
    /// <param name="observer"></param>
    /// <returns></returns>
    public IDisposable Subscribe(Action<A> observer)
    {
      return Subject.Subscribe(observer);
    }

    /// <summary>
    /// Receive notifications for the Behaviour's value. Subscribers must be
    /// idempotent because the same value may be propagated many times. This
    /// should only be used where necessary to interface with other APIs.
    /// Otherwise, use a Behaviour function such as Select.
    /// </summary>
    /// <param name="observer"></param>
    /// <returns></returns>
    public IDisposable Subscribe(Action<A> observer, Action<Exception> onError)
    {
      return Subject.Subscribe(observer, onError);
    }

    /// <summary>
    /// Behaviours should not complete, but this is provided anyways for
    /// debugging purposes.
    /// </summary>
    /// <param name="observer"></param>
    /// <param name="onComplete"></param>
    /// <param name="onError"></param>
    public IDisposable Subscribe
      ( Action<A> observer
      , Action<Exception> onError
      , Action onComplete
      )
    {
      return Subject.Subscribe(observer, onError, onComplete);
    }

    /// <summary>
    /// Take the value of the Behaviour at the same time the Event occurs.
    /// </summary>
    public IObservable<Tuple<A, B>> Snapshot<B>(IObservable<B> when)
    {
      return when.Select(b => Tuple.Create(Value, b));
    }

    /// <summary>
    /// Map many Behaviours to a single Behaviour with a function.
    /// </summary>
    public Behaviour<R> Combine<B, R>(Func<A, B, R> combine, Behaviour<B> b)
    {
      var r = new Behaviour<R>(combine(Value, b.Value));
      Subject.CombineLatest(b.Subject, combine).Subscribe(r.Subject);
      return r;
    }

    /// <summary>
    /// Map many Behaviours to a single Behaviour using a function.
    /// </summary>
    public Behaviour<R> Combine<B, C, R>(
      Func<A, B, C, R> combine,
      Behaviour<B> b,
      Behaviour<C> c
    )
    {
      var r = new Behaviour<R>(combine(Value, b.Value, c.Value));
      Subject.CombineLatest(b.Subject, c.Subject, combine).Subscribe(r.Subject);
      return r;
    }

    /// <summary>
    /// Map many Behaviours to a single Behaviour using a function.
    /// </summary>
    public Behaviour<R> Combine<B, C, D, R>(
      Func<A, B, C, D, R> combine,
      Behaviour<B> b,
      Behaviour<C> c,
      Behaviour<D> d
    )
    {
      var r = new Behaviour<R>(combine(Value, b.Value, c.Value, d.Value));
      Subject.CombineLatest(b.Subject, c.Subject, d.Subject, combine)
        .Subscribe(r.Subject);
      return r;
    }

    /// <summary>
    /// Observers (subscribers) will receive notifications on the specified
    /// SynchronizationContext. For subscribers which need to manipulate GUI
    /// make sure this is the GUI SynchronizationContext.
    /// </summary>
    public Behaviour<A> ObserveOn(SynchronizationContext context)
    {
      var r = new Behaviour<A>(Value);
      Subject.ObserveOn(context).Subscribe(r.Subject);
      return r;
    }

    /// <summary>
    /// Run observers on <paramref name="scheduler"/>.
    /// </summary>
    /// <returns></returns>
    /// <param name="scheduler"></param>
    public Behaviour<A> ObserveOn(IScheduler scheduler)
    {
      var r = new Behaviour<A>(Value);
      Subject.ObserveOn(scheduler).Subscribe(r.Subject);
      return r;
    }

    public Behaviour<A> ObserveOn(Maybe<SynchronizationContext> context)
    {
      return context.Cases
        ( () => this
        , c => ObserveOn(c)
        );
    }

    public Behaviour<A> ObserveOn(Maybe<IScheduler> scheduler)
    {
      return scheduler.Cases
        ( () => this
        , s => ObserveOn(s)
        );
    }

    /// <summary>
    /// Map a Behaviour of one type to a Behaviour of another using a function.
    /// </summary>
    public Behaviour<B> Select<B>(Func<A, B> selector)
    {
      var r = new Behaviour<B>(selector(Value));
      Subject.Select(selector).Subscribe(r.Subject);
      return r;
    }

    /// <summary>
    /// Limits the rate at which the Behaviour may change value. TODO: This 
    /// likely violates the meaning of a Behaviour.
    /// </summary>
    public IObservable<A> Throttle(TimeSpan timeSpan)
    {
      var r = new BehaviorSubject<A>(Value);
      Subject.Throttle(timeSpan).Subscribe(r);
      return r;
    }

    /// <summary>
    /// Values are shared by all subscribers.
    /// </summary>
    public Tuple<UntypedConnectable, Behaviour<A>> Hot()
    {
      var r = new Behaviour<A>(Value);
      var connectable = Subject.Publish();
      connectable.Subscribe(r.Subject);
      return Tuple.Create(UntypedConnectable.Create(connectable), r);
    }

    public Behaviour<A> SubscribeOn(SynchronizationContext context)
    {
      var r = new Behaviour<A>(Value);
      Subject.SubscribeOn(context).Subscribe(r.Subject);
      return r;
    }

    public Behaviour<A> SubscribeOn(IScheduler scheduler)
    {
      var r = new Behaviour<A>(Value);
      Subject.SubscribeOn(scheduler).Subscribe(r.Subject);
      return r;
    }

    public Behaviour<A> SubscribeOn(Maybe<SynchronizationContext> context)
    {
      return context.Cases
        ( () => this
        , c => SubscribeOn(c)
        );
    }

    public Behaviour<A> SubscribeOn(Maybe<IScheduler> scheduler)
    {
      return scheduler.Cases
        ( () => this
        , s => SubscribeOn(s)
        );
    }

    /// <summary>
    /// A Behaviour that begins with an initial value and steps to the value of
    /// the Event at the same time the Event occurs.
    /// </summary>
    public static Behaviour<A> Step(A initial, IObservable<A> with)
    {
      var r = new Behaviour<A>(initial);
      with.Subscribe(r.Subject);
      return r;
    }

    /// <summary>
    /// A Behaviour that begins with an initial value and steps when the Event
    /// occurs to a function-mapped value from the Event value and the previous
    /// Behaviour value.
    /// </summary>
    public static Behaviour<A> Scan<B>(
      A initial,
      IObservable<B> source,
      Func<A, B, A> next)
    {
      var r = new Behaviour<A>(initial);
      source.Select(x => next(r.Value, x)).Subscribe(r.Subject);
      return r;
    }

    public static Behaviour<A> Join(Behaviour<Behaviour<A>> a)
    {
      var r = new Behaviour<A>(a.Value.Value);
      a.Subject.Select(b => b.Value).Subscribe(r.Subject);
      return r;
    }

    /// <summary>
    /// The events of <paramref name="source"/> when <paramref name="gate"/> is 
    /// true.
    /// </summary>
    public static IObservable<A> Gate(
      IObservable<A> source, 
      Behaviour<bool> gate)
    {
      return source.Where(_ => gate.Value);
    }

    public static Tuple<UntypedConnectable, Behaviour<A>> LatelyBound(
      A initial,
      Func<Behaviour<A>> getBinding)
    {
      var connectable = LateObservable.Create(
        () => getBinding().Subject);
      var r = new Behaviour<A>(initial);
      connectable.Subscribe(r.Subject);
      return Tuple.Create(UntypedConnectable.Create(connectable), r);
    }

  }

}
