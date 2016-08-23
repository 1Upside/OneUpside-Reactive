using System;
using System.Reactive.Subjects;
using System.Reflection;

namespace OneUpside.Reactive
{
  /// <summary>
  ///   An <see cref="IConnectableObservable{A}"/> for some type 'A'.
  /// </summary>
  public sealed class UntypedConnectable : IConnectable
  {

    /// <summary>
    ///   The <see cref="IConnectableObservable{A}"/> object.
    /// </summary>
    private readonly object Instance;

    /// <summary>
    ///   The .Connect() method on the <see cref="IConnectableObservable{A}"/> 
    ///   object.
    /// </summary>
    private readonly MethodInfo Connect_;

    /// <summary>
    ///   Construct an <see cref="UntypedConnectable"/>.
    /// </summary>
    /// <param name="instance">
    ///   The <see cref="IConnectableObservable{A}"/> object.
    /// </param>
    /// <param name="connect">
    ///   The .Connect() method on the <see cref="IConnectableObservable{A}"/>
    ///   object.
    /// </param>
    private UntypedConnectable(object instance, MethodInfo connect)
    {
      Instance = instance;
      Connect_ = connect;
    }

    /// <summary>
    ///   Call .Connect() on the <see cref="IConnectableObservable{A}"/> 
    ///   object.
    /// </summary>
    /// <returns>
    ///   The IDisposable returned from .Connect() on the 
    ///   <see cref="IConnectableObservable{A}"/> object.
    /// </returns>
    public IDisposable Connect()
    {
      return (IDisposable)Connect_.Invoke(Instance, null);
    }

    /// <summary>
    ///   Construct an <see cref="UntypedConnectable"/>.
    /// </summary>
    /// <typeparam name="A">A type.</typeparam>
    /// <param name="connectable">
    ///   The <see cref="IConnectableObservable{A}"/> 
    ///   to construct the UntypedConnectable from.
    /// </param>
    /// <returns>The constructed <see cref="UntypedConnectable"/></returns>
    public static UntypedConnectable Create<A>(
      IConnectableObservable<A> connectable)
    {
      return new UntypedConnectable(
        connectable,
        connectable.GetType().GetTypeInfo().GetDeclaredMethod("Connect")
      );
    }

  }
}
