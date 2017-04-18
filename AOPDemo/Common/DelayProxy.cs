using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Channels;

namespace AOPDemo.Common
{
    /// <summary>
    /// 支持泛型、支持延迟初始化的代理类， 可为 MarshalByRefObject 的子类型提供代理
    /// <para>在执行代理的过程中，获取 AdviceAttribute 所指定的增强，并织入该增强</para>
    /// </summary>
    public class DelayProxy<T> : RealProxy where T : MarshalByRefObject
    {
        private static object objLock = new object();

        /// <summary>
        /// 被代理的对象
        /// </summary>
        private T target;

        /// <summary>
        /// 是否延迟初始化
        /// <para>True：延迟, False: 不延迟</para>
        /// </summary>
        private readonly bool delay;

        public DelayProxy(T target, bool delay)
            : base(typeof(T))
        {
            this.target = target;
            this.delay = delay;
        }

        /// <summary>
        /// 调用被代理对象
        /// <para>支持 out ref 参数</para>
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public override IMessage Invoke(IMessage msg)
        {
            if (this.delay && this.target == null)
            {
                lock (objLock)
                {
                    if (this.delay && this.target == null)
                    {
                        T instance = Activator.CreateInstance(typeof(T)) as T;

                        // 自动装配属性
                        // 为属性对象启用代理，并延迟初始化被代理的对象
                        // DelayProxyUtil.AutowiredProperties(instance);

                        this.target = instance;
                    }
                }
            }

            IMethodCallMessage callMessage = (IMethodCallMessage)msg;

            AdviceAttribute attri = ReflectionUtil.GetCustomAttribute<AdviceAttribute>(callMessage.MethodBase);

            if (attri != null && attri.Advice != null)
            {
                return attri.Advice.Invoke(this.target, callMessage);
            }

            return DelayProxyUtil.InvokeBeProxy(this.target, callMessage);
        }

    }

}