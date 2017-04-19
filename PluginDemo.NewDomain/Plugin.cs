using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PluginDemo;

namespace PluginDemo.NewDomain
{

    /// <summary>
    /// 支持跨应用程序域访问
    /// </summary>
    public class Plugin : MarshalByRefObject, IPlugin
    {
        // AppDomain被卸载后，静态成员的内存会被释放掉
        private static int length;

        /// <summary>
        /// int 作为基础数据类型, 是持续序列化的.
        /// <para>在与其他AppDomain通讯时，传递的是对象副本（通过序列化进行的值封送）</para>
        /// </summary>
        /// <returns></returns>
        public int GetInt()
        {
            length += new Random().Next(10000);

            return length;
        }


        /// <summary>
        /// string 作为特殊的class, 也是持续序列化的.
        /// <para>在与其他AppDomain通讯时，传递的是对象副本（通过序列化进行的值封送）</para>
        /// </summary>
        /// <returns></returns>
        public string GetString()
        {
            return "iqingyu";
        }



        /// <summary>
        /// 未继承 MarshalByRefObject 并且 不支持序列化 的 class, 是不可以跨AppDomain通信的，也就是说其他AppDomain是获取不到其对象的
        /// </summary>
        /// <returns></returns>
        public object GetNonMarshalByRefObject()
        {
            return new NonMarshalByRefObject();
        }

        private NonMarshalByRefObjectAction obj = new NonMarshalByRefObjectAction();

        /// <summary>
        /// 委托，和 委托所指向的类型相关
        /// <para>也就是说，如果其指向的类型支持跨AppDomain通信，那个其他AppDomain就可以获取都该委托， 反之，则不能获取到</para>
        /// </summary>
        /// <returns></returns>
        public Action GetAction()
        {
            obj.Add();
            obj.Add();
            //obj.Add();

            return obj.TestAction;
        }

        private List<string> list = new List<string>() { "A", "B" };

        /// <summary>
        /// List<T> 也是持续序列化的, 当然前提是T也必须支持跨AppDomain通信
        /// <para>在与其他AppDomain通讯时，传递的是对象副本（通过序列化进行的值封送）</para>
        /// </summary>
        /// <returns></returns>
        public List<string> GetList()
        {
            return this.list;
            // return new List<Action>() { this.GetAction() };
        }

    }


}
