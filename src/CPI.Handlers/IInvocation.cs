using System;
using System.Collections.Generic;
using System.Text;
using ATBase.Core;

namespace CPI.Handlers
{
    public interface IInvocation
    {
        ObjectResult Invoke();
    }
}
