using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AOPDemo.Common
{

    /// <summary>
    /// 扩展的抽象服务类
    /// <para>配合增强类，完成以下功能：</para>
    /// <para>1、自动管理数据库连接[可选]</para>
    /// <para>2、自动管理数据库事务，当接收到异常后（无论什么异常）事务将自动回滚[可选]</para>
    /// 
    /// <para>3、服务级加锁[必选]</para>
    /// <para>4、以统一方式处理服务异常及错误处理,包括数据库异常 和 主动抛出的异常[必选]</para>
    /// </summary>
    public abstract class ServiceAbstract : MarshalByRefObject
    {
        /// <summary>
        /// 是否发生错误
        /// </summary>
        public bool Error { get; protected set; }

        /// <summary>
        /// 错误提示信息（友好的，用户可见）
        /// </summary>
        public string ErrorMsg { get; protected set; }

        /// <summary>
        /// 错误详情
        /// <para>所有错误，均通过异常抛出</para>
        /// </summary>
        public Exception ErrorEx { get; protected set; }



        /// <summary>
        /// 重置错误信息
        /// </summary>
        public void ResetError()
        {
            this.Error = false;
            this.ErrorMsg = string.Empty;
            this.ErrorEx = null;
        }

        /// <summary>
        /// 设置错误信息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ex"></param>
        public void SetError(string msg, Exception ex)
        {
            this.Error = true;
            this.ErrorEx = ex;
            this.ErrorMsg = msg;
        }


        /// <summary>
        /// 获取服务级别的锁定对象，以完成系统应用层加锁（具体而言是Service层加锁）
        /// </summary>
        /// <returns></returns>
        public abstract object GetLockObject();

    }

}