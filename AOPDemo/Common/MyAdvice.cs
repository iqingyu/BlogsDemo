using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace AOPDemo.Common
{
    public class MyAdvice : ServiceAdviceAbstract<Exception>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keepConnection">是否保持(长)连接，即自动管理连接</param>
        /// <param name="useTransaction">是否使用事务，即自动管理事务</param>
        public MyAdvice(bool keepConnection, bool useTransaction)
            : base(keepConnection, useTransaction)
        {

        }



        /// <summary>
        /// 保持连接
        /// </summary>
        /// <returns></returns>
        public static MyAdvice GetKeepConnection()
        {
            return new MyAdvice(true, false);
        }

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns></returns>
        public static MyAdvice GetUseTransaction()
        {
            return new MyAdvice(true, true);
        }



        #region Override

        protected override void OpenConnection()
        {
            Console.WriteLine("打开连接");
        }

        protected override void CloseConnection()
        {
            Console.WriteLine("关闭连接");
        }

        protected override void BeginTransaction(bool onlyBeginTransaction)
        {
            Console.WriteLine("开启事务");
        }

        protected override void CommitTransaction(bool onlyCommitTransaction)
        {
            Console.WriteLine("提交事务");
        }

        protected override void RollBackTransaction(bool onlyRollBackTransaction)
        {
            Console.WriteLine("回滚事务");
        }

        protected override bool HasOpenConnection()
        {
            Console.WriteLine("打开连接");
            // TODO 
            return false;
        }

        protected override bool HasBeginTransaction()
        {
            Console.WriteLine("开启事务");
            // TODO 
            return false;
        }

        #endregion

    }
}