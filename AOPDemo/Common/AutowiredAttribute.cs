using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AOPDemo.Common
{
    /// <summary>
    /// 标记需要自动装配的属性
    /// <para>为属性对象启用代理，并延迟初始化被代理的对象</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class AutowiredAttribute : Attribute
    {
        /// <summary>
        /// 是否启用代理
        /// <para>True: 启用代理（延迟初始化）,  False: 不启用代理（不延迟初始化）</para>
        /// <para>默认值True</para>
        /// </summary>
        public bool UseProxy { get; private set; }


        /// <summary>
        /// 为属性对象启用代理，并延迟初始化被代理的对象
        /// </summary>
        public AutowiredAttribute()
            : this(true)
        {

        }

        /// <summary>
        /// 使用指定参数初始化 Attribute
        /// </summary>
        /// <param name="useProxy">True: 启用代理（延迟初始化）, False: 不启用代理（不延迟初始化）</param>
        public AutowiredAttribute(bool useProxy)
        {
            this.UseProxy = useProxy;
        }

    }

}