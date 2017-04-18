using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Reflection;
using System.Data.Common;

namespace AOPDemo.Common
{

    /// <summary>
    /// 抽象的服务增强类
    /// <para>增强以下功能：</para>
    /// <para>1、自动管理数据库连接[可选]</para>
    /// <para>2、自动管理数据库事务，当接收到异常后（无论什么异常）事务将自动回滚[可选]</para>
    /// 
    /// <para>3、服务级加锁[必选]</para>
    /// <para>4、以统一方式处理 服务异常 及 错误, 包括数据库异常 和 主动抛出的异常[必选]</para>
    /// </summary>
    public abstract class ServiceAdviceAbstract<T> : AdviceAbstract where T : Exception
    {

        #region 属性

        /// <summary>
        /// 是否保持(长)连接，即自动管理连接
        /// </summary>
        public bool KeepConnection { get; private set; }

        /// <summary>
        /// 是否使用事务，即自动管理事务
        /// </summary>
        public bool UseTransaction { get; private set; }



        /// <summary>
        /// 是否在当前方法的增强中打开了连接
        /// </summary>
        protected bool CurrentKeepConnection { get; set; }

        /// <summary>
        /// 是否在当前方法的增强中开启了事务
        /// </summary>
        protected bool CurrentUseTransaction { get; set; }

        #endregion


        #region 构造函数

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keepConnection">是否保持(长)连接，即自动管理连接</param>
        /// <param name="useTransaction">是否使用事务，即自动管理事务</param>
        public ServiceAdviceAbstract(bool keepConnection, bool useTransaction)
        {
            this.KeepConnection = keepConnection;
            this.UseTransaction = useTransaction;
        }

        #endregion


        public sealed override IMessage Invoke(MarshalByRefObject target, IMethodCallMessage callMessage)
        {
            ServiceAbstract service = target as ServiceAbstract;

            // 服务类型校验 其抛出的异常不会被捕获
            Check(service);

            return LockInvoke(service, callMessage);
        }


        #region 可以扩展的虚函数

        /// <summary>
        /// 执行Lock加锁调用
        /// </summary>
        /// <param name="target"></param>
        /// <param name="callMessage"></param>
        /// <returns></returns>
        protected virtual IMessage LockInvoke(ServiceAbstract target, IMethodCallMessage callMessage)
        {
            lock (target.GetLockObject())
            {
                return CatchAdviceInvoke(target, callMessage);
            }
        }

        /// <summary>
        /// 执行Try...Catch增强调用
        /// </summary>
        /// <param name="target"></param>
        /// <param name="callMessage"></param>
        /// <returns></returns>
        protected virtual IMessage CatchAdviceInvoke(ServiceAbstract target, IMethodCallMessage callMessage)
        {
            try
            {
                BeforeInvokeBeProxy(target);

                IMessage message = DelayProxyUtil.InvokeBeProxy(target, callMessage);

                AfterInvokeBeProxy(target);

                return message;
            }
            // 调用方法时，内部抛出的异常
            catch (TargetInvocationException targetEx)
            {
                string msg = string.Empty;

                if (!(targetEx.InnerException is ServiceException))
                {
                    if (targetEx.InnerException is DbException)
                    {
                        msg = "数据异常:";
                    }
                    else if (targetEx.InnerException is T)
                    {
                        msg = "服务异常:";
                    }
                    else
                    {
                        msg = "系统异常:";
                    }
                }

                return ReturnError(msg + targetEx.InnerException.Message, targetEx.InnerException, target, callMessage);
            }
            catch (ServiceException sEx)
            {
                return ReturnError(sEx.Message, sEx, target, callMessage);
            }
            catch (DbException dbEx)
            {
                return ReturnError("数据异常:" + dbEx.Message, dbEx, target, callMessage);
            }
            catch (T tEx)
            {
                return ReturnError("服务异常:" + tEx.Message, tEx, target, callMessage);
            }
            catch (Exception ex)
            {
                return ReturnError("系统异常:" + ex.Message, ex, target, callMessage);
            }
        }


        /// <summary>
        /// 调用被代理对象方法前执行
        /// </summary>
        /// <param name="target"></param>
        protected virtual void BeforeInvokeBeProxy(ServiceAbstract target)
        {
            target.ResetError();

            this.CurrentKeepConnection = false;
            this.CurrentUseTransaction = false;

            if (!this.KeepConnection && !this.UseTransaction)
            {
                return;
            }

            // 已经开启了事务            
            if (this.HasBeginTransaction())
            {
                // 不需要在当前方法的增强中进行任何处理
                return;
            }

            // 已经打开了连接
            if (this.HasOpenConnection())
            {
                if (this.UseTransaction)
                {
                    this.BeginTransaction(true);
                    this.CurrentUseTransaction = true;
                    return;
                }

                return;
            }


            // 即没有开启事务，又没有打开连接
            if (this.UseTransaction)
            {
                this.BeginTransaction(false);
                this.CurrentKeepConnection = true;
                this.CurrentUseTransaction = true;
            }
            else if (this.KeepConnection)
            {
                this.OpenConnection();
                this.CurrentKeepConnection = true;
            }
        }

        /// <summary>
        /// 调用被代理对象方法后执行
        /// </summary>
        /// <param name="target"></param>
        protected virtual void AfterInvokeBeProxy(ServiceAbstract target)
        {
            // 当前增强 只打开了连接
            if (this.CurrentKeepConnection && !this.CurrentUseTransaction)
            {
                this.CloseConnection();
            }
            // 当前增强 只开启了事务
            else if (!this.CurrentKeepConnection && this.CurrentUseTransaction)
            {
                this.CommitTransaction(true);
            }
            // 当前增强 既打开了连接，又开启了事务
            else if (this.CurrentKeepConnection && this.CurrentUseTransaction)
            {
                this.CommitTransaction(false);
            }
        }

        /// <summary>
        /// 返回错误信息
        /// <para>拦截所有异常，将错误信息存储到 ExtensionServiceAbstract 对象中，并返回被调用方法的默认值</para>
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ex"></param>
        /// <param name="target"></param>
        /// <param name="callMessage"></param>
        /// <returns></returns>
        protected virtual IMessage ReturnError(string msg, Exception ex,
            ServiceAbstract target, IMethodCallMessage callMessage)
        {
            try
            {
                // 当前增强 只打开了连接
                if (this.CurrentKeepConnection && !this.CurrentUseTransaction)
                {
                    this.CloseConnection();
                }
                // 当前增强 只开启了事务
                else if (!this.CurrentKeepConnection && this.CurrentUseTransaction)
                {
                    this.RollBackTransaction(true);
                }
                // 当前增强 既打开了连接，又开启了事务
                else if (this.CurrentKeepConnection && this.CurrentUseTransaction)
                {
                    this.RollBackTransaction(false);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            // 如果 逻辑上下文中已经进行了Try...Catch调用，
            // 则   将捕获的异常向上层抛出
            //if (this.HasTryCatch)
            //{
            //    return DelayProxyUtil.ReturnExecption(ex, callMessage);
            //}

            target.SetError(msg, ex);

            // 记录日志
            WriteLog(ex);

            return DelayProxyUtil.ReturnDefaultValue(target, callMessage);
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void WriteLog(Exception ex)
        {

        }




        /// <summary>
        /// 校验被代理的对象的类型
        /// </summary>
        /// <param name="service"></param>
        protected virtual void Check(ServiceAbstract service)
        {
            if (service == null)
            {
                throw new ServiceException("服务增强类 AdviceAbstractGeneric 只能用于 MyBatisServiceAbstract类型的子类型 ");
            }
        }

        #endregion


        #region 管理数据库连接和事务


        /// <summary>
        /// 打开连接
        /// </summary>
        protected abstract void OpenConnection();

        /// <summary>
        /// 关闭连接
        /// </summary>
        protected abstract void CloseConnection();

        /// <summary>
        /// 开启事务
        /// </summary>
        protected abstract void BeginTransaction(bool onlyBeginTransaction);

        /// <summary>
        /// 提交事务
        /// </summary>
        protected abstract void CommitTransaction(bool onlyCommitTransaction);

        /// <summary>
        /// 回滚事务
        /// </summary>
        protected abstract void RollBackTransaction(bool onlyRollBackTransaction);


        /// <summary>
        /// 是否打开了连接
        /// </summary>
        /// <returns></returns>
        protected abstract bool HasOpenConnection();

        /// <summary>
        /// 是否开启了事务
        /// </summary>
        /// <returns></returns>
        protected abstract bool HasBeginTransaction();

        #endregion

    }

}