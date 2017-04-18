using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using System.Reflection;

namespace AOPDemo.Common
{
    /// <summary>
    /// 延迟初始化代理工具类
    /// </summary>
    public static class DelayProxyUtil
    {
        /// <summary>
        /// 自动装配属性
        /// <para>为属性对象启用代理，并延迟初始化被代理的对象</para>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static void AutowiredProperties(object obj)
        {
            if (obj == null)
                return;

            // 获取公共实例属性
            PropertyInfo[] infos = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            if (infos != null && infos.Length > 0)
            {
                foreach (PropertyInfo pInfo in infos)
                {
                    AutowiredAttribute autoProxy = ReflectionUtil.GetCustomAttribute<AutowiredAttribute>(pInfo);

                    if (autoProxy == null)
                    {
                        continue;
                    }

                    object pValue = DelayProxyUtil.CreateProxy(pInfo.PropertyType, autoProxy);

                    pInfo.SetValue(obj, pValue, null);
                }
            }
        }


        /// <summary>
        /// 为指定类型 创建代理
        /// </summary>
        /// <returns></returns>
        public static object CreateProxy(Type type, AutowiredAttribute autoProxy)
        {
            // 为属性对象启用代理，并延迟初始化被代理的对象
            if (autoProxy.UseProxy)
            {
                return DelayProxyUtil.GetTransparentProxy(type, null, true);
            }

            // 不启用代理，并不延迟初始化
            object instance = Activator.CreateInstance(type);

            // 自动装配属性
            // 为属性对象启用代理，并延迟初始化被代理的对象
            DelayProxyUtil.AutowiredProperties(instance);

            return instance;
        }


        /// <summary>
        /// 调用被代理对象中方法，返回 被代理对象的 方法返回值
        /// <para>支持 out ref 参数</para>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="callMessage"></param>
        /// <returns></returns>
        public static IMessage InvokeBeProxy(MarshalByRefObject target, IMethodCallMessage callMessage)
        {
            var args = callMessage.Args;

            object returnValue = callMessage.MethodBase.Invoke(target, args);

            return new ReturnMessage(returnValue, args, args.Length, callMessage.LogicalCallContext, callMessage);
        }

        /// <summary>
        /// 返回方法默认值
        /// </summary>
        /// <param name="target"></param>
        /// <param name="callMessage"></param>
        /// <returns></returns>
        public static IMessage ReturnDefaultValue(MarshalByRefObject target, IMethodCallMessage callMessage)
        {
            MethodInfo info = callMessage.MethodBase as MethodInfo;

            object returnValue = info.ReturnType == typeof(void) ? null : ReflectionUtil.GetDefaultValue(info.ReturnType);

            return new ReturnMessage(returnValue, null, 0, callMessage.LogicalCallContext, callMessage);
        }

        /// <summary>
        /// 向上层抛出异常
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="callMessage"></param>
        /// <returns></returns>
        public static IMessage ReturnExecption(Exception ex, IMethodCallMessage callMessage)
        {
            return new ReturnMessage(ex, callMessage);
        }





        /// <summary>
        /// 获取对象的代理
        /// </summary>
        /// <param name="type">该类型必须为 ServiceAbstract 类型的子类</param>
        /// <param name="instance"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        private static object GetTransparentProxy(Type type, object instance, bool delay)
        {
            if (!type.IsSubclassOf(typeof(MarshalByRefObject)))
            {
                throw new Exception("所代理的对象的类型必须为 ServiceAbstract 类型的子类");
            }

            Type tmpType = typeof(DelayProxy<>);

            tmpType = tmpType.MakeGenericType(type);

            RealProxy proxy = Activator.CreateInstance(tmpType, new object[] { instance, delay }) as RealProxy;

            return proxy.GetTransparentProxy();
        }


    }

}