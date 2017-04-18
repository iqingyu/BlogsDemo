using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AOPDemo.Common
{
    /// <summary>
    /// 为方法标记指定的增强对象
    /// <para>指定的增强，可通过代理 DelayProxy 织入</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class AdviceAttribute : Attribute
    {
        /// <summary>
        /// 增强对象
        /// </summary>
        public AdviceAbstract Advice { get; private set; }

        /// <summary>
        /// 使用指定类型的默认增强对象
        /// <para>如果类型为空 则不使用任何增强</para>
        /// </summary>
        /// <param name="type"></param>
        public AdviceAttribute(Type type)
            : this(type, string.Empty)
        {

        }

        /// <summary>
        /// 使用公有静态方法名初始化指定类型的增强对象
        /// <para>如果类型为空 则不使用任何增强</para>
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="methodName">
        /// 公有静态方法名
        /// <para>如果方法名为空，调用默认构造函数</para>
        /// </param>
        public AdviceAttribute(Type type, string methodName)
        {
            // 如果类型为空 则不使用任何增强
            if (type == null)
            {
                this.Advice = null;
                return;
            }

            if (string.IsNullOrWhiteSpace(methodName))
            {
                this.Advice = Activator.CreateInstance(type) as AdviceAbstract;
                return;
            }

            this.Advice = type.InvokeMember(
                methodName,
                System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static,
                null, null, null) as AdviceAbstract;
        }





        #region 以下两种方式效果不是很好，不推荐使用，故 构造函数私有化

        /// <summary>
        /// 使用参数列表初始化指定类型的增强对象
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="objs">参数列表</param>
        private AdviceAttribute(Type type, params object[] objs)
        {
            this.Advice = Activator.CreateInstance(type, objs) as AdviceAbstract;
        }

        /// <summary>
        /// 使用命名参数初始化指定类型的增强对象
        /// </summary>
        /// <param name="namedParameter">
        /// 以 冒号 和 分号 分割的形参的命名参数列表
        /// <para>支持的数据类型有：string, int, bool 及 可通过静态方法Parse 反序列化的类型</para>
        /// </param>
        /// <param name="type"></param>
        private AdviceAttribute(string namedParameter, Type type)
        {
            this.Advice = ReflectionUtil.InvokeConstructor(type, namedParameter) as AdviceAbstract;
        }

        #endregion

    }

}