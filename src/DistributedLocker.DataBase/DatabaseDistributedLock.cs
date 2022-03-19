using Dapper;
using DistributedLocker.Extensions;
using DistributedLocker.Internal;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocker.DataBase
{
    public class DatabaseDistributedLock : AsyncDistributedLock
    {
        private static readonly object _syncobj = new object();

        private static bool _tableCreatedFlag = false;

        private readonly IDatabaseDistributedLockAdapter _adapter = null;

        public DatabaseDistributedLock(IDatabaseDistributedLockAdapter adapter,
            ILockOptions options,
            IDistributedLockCacher cacher)
            : base(options, cacher)
        {
            UtilMethods.ThrowIfNull(adapter, nameof(adapter));

            _adapter = adapter;

            this.EnsureCreated();
        }

        private void EnsureCreated()
        {
            if (!_tableCreatedFlag)
            {
                lock (_syncobj)
                {
                    if (!_tableCreatedFlag)
                    {
                        var create = this._adapter.CreateCreate();

                        using (var conn = this._adapter.CreateDbConnection())
                        {
                            conn.Execute(create);
                        }

                        _tableCreatedFlag = true;
                    }
                }
            }
        }


        protected override Locker Enter(Lockey lockey,
            Locker locker,
            LockParameter param)
        {
            using (var conn = this._adapter.CreateDbConnection())
            {
                string insert = this._adapter.CreateInsert(locker);

                try
                {
                    conn.Execute(
                        insert,
                        locker);

                    return locker;
                }
                catch (Exception e)
                {
                    if (!this._adapter.CheckIfConflictException(e))
                    {
                        throw;
                    }
                }

                //  ExecuteAsync 和 QueryFirstOrDefaultAsync 两个操作不是一个原子性的操作
                //  所以 exists 还是可能出现为 null 的情况
                //  也就是在 ExecuteAsync 出现并发之后 QueryFirstOrDefaultAsync 之前
                //  冲突的锁被解掉了
                var exists = conn.QueryFirstOrDefault<Locker>(
                                this._adapter.CreateSelect(lockey),
                                new
                                {
#pragma warning disable IDE0037
                                    BusinessType = locker.BusinessType,
                                    BusinessCode = locker.BusinessCode
#pragma warning restore IDE0037
                                });

                throw new LockConflictException(exists);
            }
        }
        protected override async ValueTask<Locker> EnterAsync(Lockey lockey,
            Locker locker,
            LockParameter parameter)
        {
            using (var conn = this._adapter.CreateDbConnection())
            {
                string insert = this._adapter.CreateInsert(locker);

                try
                {
                    await conn.ExecuteAsync(
                        insert,
                        locker);

                    return locker;
                }
                catch (Exception e)
                {
                    if (!this._adapter.CheckIfConflictException(e))
                    {
                        throw;
                    }
                }

                //  ExecuteAsync 和 QueryFirstOrDefaultAsync 两个操作不是一个原子性的操作
                //  所以 exists 还是可能出现为 null 的情况
                //  也就是在 ExecuteAsync 出现并发之后 QueryFirstOrDefaultAsync 之前
                //  冲突的锁被解掉了
                var exists = await conn.QueryFirstOrDefaultAsync<Locker>(
                                this._adapter.CreateSelect(lockey),
                                new
                                {
#pragma warning disable IDE0037
                                    BusinessType = locker.BusinessType,
                                    BusinessCode = locker.BusinessCode
#pragma warning restore IDE0037
                                });

                throw new LockConflictException(exists);
            }
        }


        protected override bool TryEnter(Lockey lockey,
            Locker locker,
            LockParameter parameter)
        {
            try
            {
                this.Enter(
                    lockey,
                    locker,
                    parameter);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        protected override async ValueTask<bool> TryEnterAsync(Lockey lockey,
            Locker locker,
            LockParameter parameter)
        {
            try
            {
                await this.EnterAsync(
                    lockey,
                    locker,
                    parameter);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        protected override void Keep(Lockey lockey,
            Locker locker,
            TimeSpan span)
        {
            string update = this._adapter.CreateUpdate(lockey);

            using (var conn = this._adapter.CreateDbConnection())
            {
                var effect = conn.Execute(
                                update,
                                new
                                {
#pragma warning disable IDE0037
                                    Delay = span.TotalMilliseconds,
                                    Token = lockey.Token
#pragma warning restore IDE0037
                                });

                if (effect <= 0)
                {
                    throw new LockExpiredException(lockey);
                }
            }
        }
        public override async ValueTask KeepAsync(Lockey lockey,
            Locker locker,
            TimeSpan span)
        {
            string update = this._adapter.CreateUpdate(lockey);

            using (var conn = this._adapter.CreateDbConnection())
            {
                var effect = await conn.ExecuteAsync(
                                update,
                                new
                                {
#pragma warning disable IDE0037
                                    Delay = span.TotalMilliseconds,
                                    Token = lockey.Token
#pragma warning restore IDE0037
                                });

                if (effect <= 0)
                {
                    throw new LockExpiredException(lockey);
                }
            }
        }


        protected override void Exit(Lockey lockey, Locker locker)
        {
            string delete = this._adapter.CreateDelete(lockey);

            using (var conn = this._adapter.CreateDbConnection())
            {
                conn.Execute(
                    delete,
                    lockey);
            }
        }
        public override async ValueTask ExitAsync(Lockey lockey, Locker locker)
        {
            string delete = this._adapter.CreateDelete(lockey);

            using (var conn = this._adapter.CreateDbConnection())
            {
                await conn.ExecuteAsync(
                    delete,
                    lockey);
            }
        }
    }
}
