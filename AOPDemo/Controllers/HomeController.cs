using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AOPDemo.Common;

namespace AOPDemo.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 使用 Autowired Attribute 自动初始化代理对象
        /// </summary>
        [Autowired]
        public Service myService { get; set; }


        public ActionResult Index()
        {
            myService.Test();

            var msg = myService.ErrorMsg;
            Console.WriteLine(msg);

            // 当然 ServiceException 中的 Code属性也可以存储在 ServiceAbstract 对象中

            return View();
        }
    }


    public class Service : ServiceAbstract
    {
        #region 自动加锁

        private static object objLock = new object();

        public override object GetLockObject()
        {
            return objLock;
        }

        #endregion


        /// <summary>
        /// 自动开启事务
        /// </summary>
        [Advice(typeof(MyAdvice), "GetUseTransaction")]
        public void Test()
        {
            Console.WriteLine("调用Test方法,在调用方法前开启事务，调用方法后提交或回滚事务");

            throw new ServiceException("发生异常，回滚事务") { Code = 999 };
        }
    }


}
