using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PluginDemo.NewDomain
{
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

            sb.AppendLine($"如果一个类型 【不是】 {nameof(MarshalByRefObject)}的子类 并且 【没有标记】 {nameof(SerializableAttribute)}, ");
            sb.AppendLine($"则该类型的对象不能被其他AppDomain中的对象所访问, 当然这种情况下的该类型对象中的成员也不可能被访问到了 ");
            sb.AppendLine("反之，则可以被其他AppDomain中的对象所访问 ");

            sb.AppendLine();
            sb.AppendLine($"如果一个类型 【是】 {nameof(MarshalByRefObject)}的子类, 则跨AppDomain所得到的是 【对象的引用】（为了好理解说成对象引用，实质为代理）");

            sb.AppendLine();
            sb.AppendLine($"如果一个类型 【标记】 {nameof(SerializableAttribute)}, 则跨AppDomain所得到的是  【对象的副本】，该副本是通过序列化进行值封送的 ");
            sb.AppendLine("此时传递到其他AppDomain 中的对象 和 当前对象已经不是同一个对象了（只传递了副本），这一点 通过 index 字段值 可以得到印证 ");
            sb.AppendLine("MSDN 参考文档");
            sb.AppendLine("https://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=ZH-CN&k=k(System.MarshalByRefObject);k(SolutionItemsProject);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.0);k(DevLang-csharp)&rd=true#备注");


            sb.AppendLine();
            sb.AppendLine($"如果一个类型 【是】 {nameof(MarshalByRefObject)}的子类  并且 【标记了】 {nameof(SerializableAttribute)}, ");
            sb.AppendLine($"则 {nameof(MarshalByRefObject)}  的优先级更高 ");

            sb.AppendLine();

            sb.AppendLine("另外：.net 基本类型 、string 类型、 List<T> 等类型，虽然没有标记 SerializableAttribute， 但是他们依然可以序列化。也就是说这些类型都可以在不同的AppDomain之间通信，只是传递的都是对象副本。");

            sb.AppendLine();

            var result = this.GetType().IsSubclassOf(typeof(MarshalByRefObject));
            sb.AppendLine($"该类型是否是 {nameof(MarshalByRefObject)} 的子类：  {result.ToString()}");

            var arr = this.GetType().GetCustomAttributes(typeof(SerializableAttribute), false);
            sb.AppendLine($"该类型是否是 标记了 {nameof(SerializableAttribute)} ：  {(arr != null && arr.Length > 0 ? true : false).ToString()}");


            throw new Exception(sb.ToString());
        }
    }

}
