using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Diagnostics;

namespace AOPDemo.Common
{
    /// <summary>
    /// 反射工具类
    /// </summary>
    public static class ReflectionUtil
    {
        /// <summary>
        /// 获取标记的 Attribute 对象
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static T GetCustomAttribute<T>(MemberInfo member) where T : Attribute
        {
            object[] objs = member.GetCustomAttributes(typeof(T), false);

            if (objs != null && objs.Length == 1)
                return objs[0] as T;

            return null;
        }


        /// <summary>
        /// 获取类型的默认值
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static object GetDefaultValue(Type parameter)
        {
            Type defaultGeneratorType = typeof(DefaultGenerator<>).MakeGenericType(parameter);

            return defaultGeneratorType.InvokeMember(
              "GetDefault",
              BindingFlags.Static |
              BindingFlags.Public |
              BindingFlags.InvokeMethod,
              null, null, new object[0]);
        }


        /// <summary> 
        /// 根据 形参的命名参数列表 反射调用构造函数，初始化对象
        /// <para>不支持 out ref 参数</para>
        ///<remarks>
        /// <para>寻找构造函数的逻辑为：</para>
        /// <para>1、先寻找 “构造函数中参数列表的范围” 大于等于 “namedParameter中参数列表的范围”  的构造函数列表 </para>
        /// <para>2、在一步骤的列表寻找参数个数最少的第一个构造函数</para>
        /// </remarks>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="namedParameter">
        /// 以 冒号 和 分号 分割的形参的命名参数列表
        /// <para>支持的数据类型有：string, int, bool 及 可通过静态方法Parse 反序列化的类型</para>
        /// <para>不支持 out ref 参数</para>
        /// </param>
        /// <returns></returns>
        public static object InvokeConstructor(Type type, string namedParameter)
        {
            // 转换命名参数的格式
            Dictionary<string, string> dic = GetNamedDictionary(namedParameter);

            List<MethodBase> methodBaseList = GetConstructors(type);

            // 选取合适的构造函数
            MethodBase methodBase = GetMethodBase(methodBaseList, dic);

            // 创建实参列表
            List<object> list = CreateParameterList(methodBase, dic);

            // 反射，根据参数列表初始化对象
            return Activator.CreateInstance(type, list.ToArray());
        }


        /// <summary>
        /// 使用命名参数 调用对象的 指定方法
        /// <para>不支持 out ref 参数</para>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="methodName">方法名</param>
        /// <param name="namedParameter">
        /// 以 冒号 和 分号 分割的形参的命名参数列表
        /// <para>支持的数据类型有：string, int, bool 及 可通过静态方法Parse 反序列化的类型</para>
        /// <para>不支持 out ref 参数</para>
        /// </param>
        /// <returns></returns>
        public static object InvokeMethod(object target, string methodName, string namedParameter)
        {
            // 转换命名参数的格式
            Dictionary<string, string> dic = GetNamedDictionary(namedParameter);

            List<MethodBase> methodBaseList = GetMethods(target.GetType(), methodName);

            // 选取合适的构造函数
            MethodBase methodBase = GetMethodBase(methodBaseList, dic);

            // 创建实参列表
            List<object> list = CreateParameterList(methodBase, dic);

            // 调用方法
            return methodBase.Invoke(target, list.ToArray());

        }


        /// <summary>
        /// 动态获取调用该方法的方法名
        /// </summary>
        /// <returns></returns>
        public static string DynamicMethodName()
        {
            return (new StackTrace()).GetFrame(1).GetMethod().Name;
        }

        /// <summary>
        /// 动态获取调用该方法的类名
        /// <para>注意:是指的方法所处于类的类名，而不能获取继承该类的子类的类名</para>
        /// </summary>
        /// <returns></returns>
        public static string DynamicClassName()
        {
            return (new StackTrace()).GetFrame(1).GetMethod().ReflectedType.Name;
        }

        /// <summary>
        /// 动态获取类名.方法名
        /// </summary>
        /// <returns></returns>
        public static string DynamicClassMethodName()
        {
            MethodBase method = (new StackTrace()).GetFrame(1).GetMethod();

            return string.Format("{0}.{1}", method.ReflectedType.Name, method.Name);
        }


        /// <summary>
        /// 获取所有方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        private static List<MethodBase> GetMethods(Type type, string methodName)
        {
            List<MethodBase> methodBaseList = new List<MethodBase>();

            foreach (var item in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (string.Compare(item.Name, methodName) == 0)
                {
                    methodBaseList.Add(item);
                }
            }

            return methodBaseList;
        }

        /// <summary>
        /// 获取所有构造函数
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static List<MethodBase> GetConstructors(Type type)
        {
            List<MethodBase> methodBaseList = new List<MethodBase>();

            foreach (var item in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
            {
                methodBaseList.Add(item);
            }
            return methodBaseList;
        }

        /// <summary>
        /// 创建实参列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        private static List<object> CreateParameterList(MethodBase methodBase, Dictionary<string, string> dic)
        {
            List<object> list = new List<object>();

            object value = null;

            // 支持的数据类型有 string, int, bool 及 可通过静态方法Parse 反序列化的类型
            foreach (ParameterInfo parameter in methodBase.GetParameters())
            {
                value = null;
                if (dic.ContainsKey(parameter.Name))
                {
                    if (parameter.ParameterType == typeof(string))
                    {
                        value = dic[parameter.Name];
                    }
                    else
                    {
                        value = parameter.ParameterType.InvokeMember("Parse",
                            BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod,
                            null, null, new object[1] { dic[parameter.Name] });
                    }
                }
                else
                {
                    // 不具备默认值
                    if (parameter.DefaultValue == DBNull.Value)
                    {
                        value = GetDefaultValue(parameter.ParameterType);
                    }
                    else
                    {
                        value = parameter.DefaultValue;
                    }
                }

                list.Add(value);
            }

            return list;
        }

        /// <summary>
        /// 查询合适的函数
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        private static MethodBase GetMethodBase(List<MethodBase> methodBaseList, Dictionary<string, string> dic)
        {
            MethodBase methodBase = null;

            List<MethodBase> infoList = new List<MethodBase>();

            foreach (var info in methodBaseList)
            {
                // 构造函数中参数的个数 小于 命名参数中参数的个数
                if (info.GetParameters().Length < dic.Keys.Count)
                {
                    continue;
                }

                bool exist = true;

                // 遍历命名参数列表，保证每一个命名参数都在构造函数的参数列表中存在
                // 即 “构造函数中参数列表的范围” 大于等于 “namedParameter中参数列表的范围”
                foreach (string argsName in dic.Keys)
                {
                    if (!info.GetParameters().Any<ParameterInfo>(p => string.Compare(p.Name, argsName, true) == 0))
                    {
                        exist = false;
                        break;
                    }
                }

                if (exist)
                {
                    infoList.Add(info);
                }
            }

            if (infoList.Count > 0)
            {
                if (infoList.Count == 1)
                {
                    methodBase = infoList[0];
                }
                else
                {
                    foreach (var item in infoList)
                    {
                        if (methodBase == null)
                        {
                            methodBase = item;
                            continue;
                        }

                        if (item.GetParameters().Length < methodBase.GetParameters().Length)
                        {
                            methodBase = item;
                        }
                    }
                }
            }

            if (methodBase == null)
            {
                throw new Exception("没有找到合适的函数，请校验命名参数列表是否正确");
            }

            return methodBase;
        }

        /// <summary>
        /// 换换数据格式
        /// </summary>
        /// <param name="namedParameter"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetNamedDictionary(string namedParameter)
        {
            if (string.IsNullOrWhiteSpace(namedParameter))
            {
                throw new Exception("命名参数列表不能为空");
            }

            string[] named = namedParameter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            if (named == null || named.Length == 0)
            {
                throw new Exception("命名参数列表不能为空");
            }

            Dictionary<string, string> dic = new Dictionary<string, string>();

            string[] arr = null;
            foreach (var item in named)
            {
                arr = item.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                // 命名参数 必须以‘键：值’ 的形式出现
                // 形参名不为空 且 不重复 
                // 实参值不为空 
                if (arr == null || arr.Length != 2
                    || string.IsNullOrWhiteSpace(arr[0]) || dic.ContainsKey(arr[0].Trim())
                    || string.IsNullOrWhiteSpace(arr[1]))
                {
                    throw new Exception("命名参数列表格式不合法");
                }

                dic.Add(arr[0].Trim(), arr[1].Trim());
            }

            if (dic.Count == 0)
            {
                throw new Exception("命名参数列表格式不合法");
            }
            return dic;
        }


        /// <summary>
        /// 为获取泛型默认值服务的类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class DefaultGenerator<T>
        {
            public static T GetDefault()
            {
                return default(T);
            }
        }


    }
}