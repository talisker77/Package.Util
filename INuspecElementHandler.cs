using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCS.Package.Util
{
  public interface INuspecElementHandler<out TResult>
  {
    //void Handler<TIn, TOut>(Func<TIn, TOut> method);
    TResult Handler(object value);
  }

  public interface IElementConverter
  {
     
  }
}
