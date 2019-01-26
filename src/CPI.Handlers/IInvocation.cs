using System;
using System.Collections.Generic;
using System.Text;
using Lotus.Core;

namespace CPI.Handlers
{
    public interface IInvocation
    {
        ObjectResult Invoke();
    }
}
