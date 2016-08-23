using OneUpside.Data;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Threading;

namespace OneUpside.Reactive
{

  /// <summary>
  /// Extension methods for the Rx library.
  /// </summary>
  public static class RxExtensions
  {

    /// <summary>
    /// Filters events to only those which are Just-constructed, and also
    /// projects the value from the constructor.
    /// 
    /// Example:
    ///   WhenJust [(t1, Just 1), (t2, Nothing), (t3, Just 3)]
    ///     = [(t1, 1), (t3, 3)]
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IObservable<A> WhenJust<A>
      ( this IObservable<Maybe<A>> source
      )
    {
      return source
        .Where(x => x.IsJust)
        .Select
        ( x =>
            x.Cases
            ( () =>
              {
                throw new Exception("Impossible");
              }
            , y => y
            )
        );
    }

    /// <summary>
    /// Filters events to only thsoe which are Left-constructed, and also
    /// projects the value from the constructor.
    /// 
    /// Example:
    ///   WhenLeft [(t1, Left 1), (t2, Right 2), (t3, Left 3)]
    ///     = [(t1, 1), (t3, 3)]
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IObservable<A> WhereLeft<A, B>(
      this IObservable<Either<A, B>> source)
    {
      return source
        .Where(x => x.IsLeft)
        .Select(
          x => x.Cases(
            y => y,
            _ => { throw new Exception("impossible"); }
          )
        );
    }

    /// <summary>
    /// Filters events to only thsoe which are Right-constructed, and also
    /// projects the value from the constructor.
    /// 
    /// Example:
    ///   WhenRight [(t1, Left 1), (t2, Right 2), (t3, Left 3)]
    ///     = [(t2, 2)]
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IObservable<B> WhereRight<A, B>(
      this IObservable<Either<A, B>> source)
    {
      return source
        .Where(x => x.IsRight)
        .Select(
          x => x.Cases(
            _ => { throw new Exception("impossible"); },
            y => y
          )
        );
    }

    /// <summary>
    /// View Observables as lists of 2-tuples of time-and-value, sorted by
    /// non-decreasing time. Then:
    /// 
    ///   bindEither source binder 
    ///     = concat (map (\(t,v) -> case v of 
    ///                                Left l  -> [(t, Left l)] 
    ///                                Right r -> binder r
    ///                   )
    ///              )
    /// 
    /// Loosely, if the source observable produces an error (i.e. 
    /// Left-constructed value) then this error is passed through (i.e. further 
    /// computation is aborted). If the source observable produces a non-error
    /// (i.e. a Right-constructed value) then this value is passed to the next
    /// computation. This is similar to how Exceptions abort the normal flow of
    /// a program.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    /// <typeparam name="C"></typeparam>
    /// <param name="source"></param>
    /// <param name="binder"></param>
    /// <returns></returns>
    public static IObservable<Either<A, C>> BindEither<A, B, C>(
      this IObservable<Either<A, B>> source,
      Func<B, IObservable<Either<A, C>>> binder)
    {
      return source.Select(
        v => v.Cases(
          l => Observable.Return(Either.Right<C>.Left(l)),
          binder
        )
      ).Concat();
    }

    /// <summary>
    /// View Observables as lists of 2-tuples of time-and-value, sorted by
    /// non-decreasing time. Then:
    /// 
    ///   sequence []     = Right []
    ///   sequence (x:xs) = case x of
    ///                       Left l1  -> Left l1
    ///                       Right r1 -> case sequence xs of
    ///                                     Left l2  -> Left l2
    ///                                     Right r2 -> Right (r1:r2)
    /// 
    /// Loosely, if the Observable has any Left-constructed events then the
    /// result is the first Left-constructed event. Otherwise, the result is
    /// an Observable with all the events.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static Either<A, IObservable<B>> Sequence<A, B>(
      this IObservable<Either<A, B>> source)
    {
      return source.ToEnumerable().Sequence().MapRight(r => r.ToObservable());
    }

    /// <summary>
    /// The same event times, but without the event data.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IObservable<Unit> Occurrences<A>(this IObservable<A> source)
    {
      return source.Select(_ => Unit.Default);
    }

    /// <summary>
    /// For all adjacent event time pairs (t1,t2) in 
    /// <paramref name="barrier"/>, the first event of 
    /// <paramref name="source"/> which occurrs in the interval (t1,t2]. Also 
    /// the first event of <paramref name="source"/> which occurs after all 
    /// events of <paramref name="barrier"/>. Iff
    /// <paramref name="includeFirstSource"/> is true, then also the first
    /// event of <paramref name="source"/> which occurs before all events of
    /// <paramref name="barrier"/>.
    /// </summary>
    /// <example>
    /// includeFirstSource = true
    /// 
    /// ----------+---------------------
    ///  Source   | x x - - x - x x - x
    /// ----------+---------------------
    ///  Barrier  | - - x - - - x - x x
    /// ----------+---------------------
    ///  Output   | x - - - x - - x - x
    /// ----------+---------------------
    /// 
    /// </example>
    /// <typeparam name="A">Source event type</typeparam>
    /// <typeparam name="B">Throttle event type</typeparam>
    /// <param name="source">See <see cref="Barrier{A, B}(IObservable{A}, IObservable{B}, bool)"/></param>
    /// <param name="barrier">See <see cref="Barrier{A, B}(IObservable{A}, IObservable{B}, bool)"/></param>
    /// <param name="includeFirstSource">See <see cref="Barrier{A, B}(IObservable{A}, IObservable{B}, bool)"/></param>
    /// <returns></returns>
    public static IObservable<A> Barrier<A,B>(
      this IObservable<A> source,
      IObservable<B> barrier,
      bool includeFirstSource)
    {
      return source
        .Select(Maybe.Just)
        .Merge(barrier.Select(_ => Maybe.Nothing<A>()))
        .Synchronize()
        .Scan(
          Tuple.Create(default(A), includeFirstSource ? 0 : 2),
          (a, b) => 
            b.Cases(
              () => Tuple.Create(default(A), 0),
              x => a.Item2 == 0 ? Tuple.Create(x, 1) 
                                : Tuple.Create(default(A), 2)
            )
        )
        .Where(x => x.Item2 == 1)
        .Select(x => x.Item1);
    }

    /// <summary>
    /// The events of <paramref name="source"/> when <paramref name="gate"/> is 
    /// true.
    /// </summary>
    public static IObservable<A> Gate<A>(
      this IObservable<A> source,
      Behaviour<bool> gate)
    {
      return Behaviour.Gate(source, gate);
    }

    /// <summary>
    /// Let through at most one occurrence in the given interval. Additional occurrences are dropped.
    /// </summary>
    /// <returns></returns>
    /// <param name="source"></param>
    /// <param name="interval"></param>
    /// <typeparam name="A"></typeparam>
    public static IObservable<A> RateLimit<A>
      ( this IObservable<A> source
      , TimeSpan interval
      )
    {
      return source.Synchronize().Scan
        ( Tuple.Create(DateTime.Now, Maybe.Nothing<A>())
        , (a, x) => 
          { 
            var now = DateTime.Now;
            if (now - a.Item1 >= interval)
            {
              return Tuple.Create(now, Maybe.Just(x));
            }
            else
            {
              return Tuple.Create(a.Item1, Maybe.Nothing<A>());
            }
          }
        )
        .Select(t => t.Item2)
        .WhenJust();
    }

    public static IObservable<A> ObserveOn<A>
      ( this IObservable<A> source
      , Maybe<IScheduler> scheduler
      )
    {
      return scheduler.Cases
        ( () => source
        , s => source.ObserveOn(s)
        );
    }

    public static IObservable<A> ObserveOn<A>
      ( this IObservable<A> source
      , Maybe<SynchronizationContext> context
      )
    {
      return context.Cases
        ( () => source
        , c => source.ObserveOn(c)
        );
    }

    public static IObservable<A> SubscribeOn<A>
      ( this IObservable<A> source
      , Maybe<IScheduler> scheduler
      )
    {
      return scheduler.Cases
        ( () => source
        , s => source.SubscribeOn(s)
        );
    }

    public static IObservable<A> SubscribeOn<A>
      ( this IObservable<A> source
      , Maybe<SynchronizationContext> context
      )
    {
      return context.Cases
        ( () => source
        , c => source.SubscribeOn(c)
        );
    }

  }
}
