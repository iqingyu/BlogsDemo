using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace AOPDemo.Common
{
    /// <summary>
    /// 抽象的增强类
    /// </summary>
    public abstract class AdviceAbstract
    {
        public abstract IMessage Invoke(MarshalByRefObject target, IMethodCallMessage callMessage);
    }
}