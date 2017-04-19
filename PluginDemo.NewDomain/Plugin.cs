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
        /// <summary>
        /// int 作为基础数据类型, 是持续序列化的.
        /// <para>在与其他AppDomain通讯时，传递的是对象副本（通过序列化进行的值封送）</para>
        /// </summary>
        /// <returns></returns>
        public int GetInt()
        {
            return int.MaxValue - new Random().Next();
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


    [Serializable]
    public class NonMarshalByRefObjectAction // : MarshalByRefObject
    {
        private int index = 0;

        public void Add()
        {
            this.index++;
        }

        public void TestAction()
        {
            this.index++;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"current index is {index} ");

            sb.AppendLine($"如果该类型 【不是】 {nameof(MarshalByRefObject)}的子类 并且 【没有标记】 {nameof(SerializableAttribute)}, ");
            sb.AppendLine($"则该类型的对象不能被其他AppDomain中的代码所访问, 当然这种情况下的该类型对象中的成员也不可能被访问到了 ");
            sb.AppendLine("反之，则可以被其他AppDomain中的代码所访问 ");

            sb.AppendLine();
            sb.AppendLine($"如果该类型 【是】 {nameof(MarshalByRefObject)}的子类, 则跨AppDomain所得到的是 【对象的引用】（为了好理解说成对象引用，实质为代理）");

            sb.AppendLine();
            sb.AppendLine($"如果该类型 【标记】 {nameof(SerializableAttribute)}, 则跨AppDomain所得到的是  【对象的副本】，该副本是通过序列化进行值封送的 ");
            sb.AppendLine("此时传递到其他AppDomain 中的对象 和 当前对象已经不是同一个对象了（只传递了副本），这一点 通过 index 字段值 可以得到印证 ");
            sb.AppendLine("MSDN 参考文档");
            sb.AppendLine("https://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=ZH-CN&k=k(System.MarshalByRefObject);k(SolutionItemsProject);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.0);k(DevLang-csharp)&rd=true#备注");


            sb.AppendLine();
            sb.AppendLine($"如果该类型 【是】 {nameof(MarshalByRefObject)}的子类  并且 【标记了】 {nameof(SerializableAttribute)}, ");
            sb.AppendLine($"则 {nameof(MarshalByRefObject)}  的优先级更高 ");

            sb.AppendLine();

            var result = this.GetType().IsSubclassOf(typeof(MarshalByRefObject));
            sb.AppendLine($"该类型是否是 {nameof(MarshalByRefObject)} 的子类：  {result.ToString()}");

            var arr = this.GetType().GetCustomAttributes(typeof(SerializableAttribute), false);
            sb.AppendLine($"该类型是否是 标记了 {nameof(SerializableAttribute)} ：  {(arr != null && arr.Length > 0 ? true : false).ToString()}");


            throw new Exception(sb.ToString());
        }
    }
}
