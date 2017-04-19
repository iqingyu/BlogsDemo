using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PluginDemo.NewDomain
{
    /// <summary>
    /// 未继承 MarshalByRefObject，  不可以跨AppDomain交换消息
    /// </summary>
    public class NonMarshalByRefObject
    {

    }
}
