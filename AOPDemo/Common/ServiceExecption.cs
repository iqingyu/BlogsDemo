using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AOPDemo.Common
{

    /// <summary>
    /// 自定义的服务异常
    /// </summary>
    [Serializable]
    public class ServiceException : Exception
    {
        /// <summary>
        /// 为异常提供附加数据
        /// <para>用户不可见</para>
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 为异常提供附加数据
        /// <para>用户不可见</para>
        /// </summary>
        public string Tag { get; set; }

        public ServiceException() { }
        public ServiceException(string message) : base(message) { }
        public ServiceException(string message, Exception inner) : base(message, inner) { }
        protected ServiceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }


}