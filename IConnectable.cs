using System;

namespace OneUpside.Reactive
{
  public interface IConnectable
  {
    IDisposable Connect();
  }
}
