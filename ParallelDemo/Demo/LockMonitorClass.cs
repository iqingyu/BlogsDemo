using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelDemo.Demo
{
    public class LockMonitorClass : AbstractClass
    {
        public LockMonitorClass(IView view) : base(view)
        {
        }

        public object thisLock = new object();
        private long index;

        public void AddIndex()
        {
            lock (this.thisLock)
            {
                this.index++;

                if (this.index > long.MaxValue / 2)
                {
                    this.index = 0;
                }
                // 和 index 无关的大量操作
            }
        }

        public long GetIndex()
        {
            return this.index;
        }





        public class BankAccount
        {
            private long id;
            private decimal m_balance = 0.0M;

            private object m_balanceLock = new object();

            public void Deposit(decimal delta)
            {
                lock (m_balanceLock)
                {
                    m_balance += delta;
                }
            }

            public void Withdraw(decimal delta)
            {
                lock (m_balanceLock)
                {
                    if (m_balance < delta)
                        throw new Exception("Insufficient funds");
                    m_balance -= delta;
                }
            }

            public static void ErrorTransfer(BankAccount a, BankAccount b, decimal delta)
            {
                a.Withdraw(delta);
                b.Deposit(delta);
            }


            public static void Transfer(BankAccount a, BankAccount b, decimal delta)
            {
                lock (a.m_balanceLock)
                {
                    lock (b.m_balanceLock)
                    {
                        a.Withdraw(delta);
                        b.Deposit(delta);
                    }
                }
            }

            public static void RightTransfer(BankAccount a, BankAccount b, decimal delta)
            {
                if (a.id < b.id)
                {
                    Monitor.Enter(a.m_balanceLock); // A first
                    Monitor.Enter(b.m_balanceLock); // ...and then B
                }
                else
                {
                    Monitor.Enter(b.m_balanceLock); // B first
                    Monitor.Enter(a.m_balanceLock); // ...and then A 
                }

                try
                {
                    a.Withdraw(delta);
                    b.Deposit(delta);
                }
                finally
                {
                    Monitor.Exit(a.m_balanceLock);
                    Monitor.Exit(b.m_balanceLock);
                }
            }

        }


    }
}
