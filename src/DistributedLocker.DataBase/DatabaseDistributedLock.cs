using Dapper;
using DistributedLocker.Extensions;
using DistributedLocker.Internal;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocker.DataBase
{
    public class DatabaseDistributedLock : CachedDistributedLock
    {
        private static readonly object _syncobj = new object();

        private static bool _tableCreatedFlag = false;

        private readonly IDatabaseDistributedLockAdapter _adapter = null;

        private readonly ILockOptions _options = null;

        private readonly bool? _useCache = false;

        public DatabaseDistributedLock(IDatabaseDistributedLockAdapter adapter,
            ILockOptions options)
            : base(options)
        {
            UtilMethods.ThrowIfNull(adapter, nameof(adapter));

            _adapter = adapter;

            _options = options;

            _useCache = _options.FindExtension<CoreLockOptionsExtension>()
                            .UseMemoryCache;

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

        public override ValueTask<Locker> EnterAsync(Lockey lockey, LockParameter param)
        {
            if (_useCache == true)
            {
                return this.EnterAsync(
                        lockey,
                        this.EnterCoreAsync,
                        param);
            }
            else
            {
                return this.EnterCoreAsync(lockey, param);
            }
        }
        protected virtual async ValueTask<Locker> EnterCoreAsync(Lockey lockey, LockParameter param)
        {
            if (param == null)
            {
                param = this.CreatLockParameter(lockey);
            }

            var locker = this.CreateLocker(lockey, param);

            int retrys = 0;

            using (var conn = this._adapter.CreateDbConnection())
            {
                do
                {
                    string insert = this._adapter.CreateInsert(locker);
                    string select = this._adapter.CreateSelect(lockey);

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

                    Locker exists = null;

                    if (param.ConflictPloy != ConflictPloy.Wait
                        || param.RetryTimes == retrys)
                    {
                        //  ExecuteAsync 和 QueryFirstOrDefaultAsync 两个操作不是一个原子性的操作
                        //  所以 exists 还是可能出现为 null 的情况
                        //  也就是在 ExecuteAsync 出现并发之后 QueryFirstOrDefaultAsync 之前
                        //  冲突的锁被解掉了
                        exists = await conn.QueryFirstOrDefaultAsync<Locker>(
                                    select,
                                    new
                                    {
#pragma warning disable IDE0037
                                        BusinessType = locker.BusinessType,
                                        BusinessCode = locker.BusinessCode
#pragma warning restore IDE0037
                                    });
                    }

                    this.ThrowIfConflicted(
                        exists,
                        param,
                        ref retrys);

                    await Task.Delay(param.RetryInterval);
                }
                while (true);
            }
        }


        public override ValueTask<bool> TryEnterAsync(Lockey lockey,
            LockParameter parameter,
            out Locker locker)
        {
            try
            {
                locker = this.EnterAsync(lockey, parameter).Result;
                return UtilMethods.ValueTaskFromResult(true);
            }
            catch (LockConflictException)
            {
                locker = null;
                return UtilMethods.ValueTaskFromResult(false);
            }
        }

        public override ValueTask KeepAsync(Lockey lockey, TimeSpan span)
        {
            if (_useCache == true)
            {
                return this.KeepAsync(
                        lockey,
                        this.KeepCoreAsync,
                        span);
            }
            else
            {
                return this.KeepCoreAsync(lockey, span);
            }
        }
        protected virtual async ValueTask KeepCoreAsync(Lockey lockey, TimeSpan span)
        {
            UtilMethods.ThrowIfNull(lockey, nameof(lockey));

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
                    throw new LockExpiredException();
                }
            }
        }

        public override ValueTask ExitAsync(Lockey lockey)
        {
            if (_useCache == true)
            {
                return this.ExitAsync(
                        lockey,
                        this.ExitCoreAsync);
            }
            else
            {
                return this.ExitCoreAsync(lockey);
            }
        }
        protected virtual async ValueTask ExitCoreAsync(Lockey lockey)
        {
            UtilMethods.ThrowIfNull(lockey, nameof(lockey));

            string delete = this._adapter.CreateDelete(lockey);

            using (var conn = this._adapter.CreateDbConnection())
            {
                await conn.ExecuteAsync(
                    delete,
                    lockey);
            }
        }


        public override Locker Enter(Lockey lockey, LockParameter param)
        {
            if (_useCache == true)
            {
                return this.Enter(
                        lockey,
                        this.EnterCore,
                        param);
            }
            else
            {
                return this.EnterCore(lockey, param);
            }
        }








        /*

        protected override Locker EnterCore(Lockey lockey, Locker locker, LockParameter param)
        {
            string insert = this._adapter.CreateInsert(locker);
            string select = this._adapter.CreateSelect(lockey);

            try
            {
                this._dbConnection.Execute(
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

            Locker exists = null;

            //  ExecuteAsync 和 QueryFirstOrDefaultAsync 两个操作不是一个原子性的操作
            //  所以 exists 还是可能出现为 null 的情况
            //  也就是在 ExecuteAsync 出现并发之后 QueryFirstOrDefaultAsync 之前
            //  冲突的锁被解掉了
            exists = this._dbConnection.QueryFirstOrDefault<Locker>(
                        select,
                        new
                        {
#pragma warning disable IDE0037
                            BusinessType = locker.BusinessType,
                            BusinessCode = locker.BusinessCode
#pragma warning restore IDE0037
                        });

            return exists;
        }
    }

        */



        protected virtual Locker EnterCore(Lockey lockey, LockParameter param)
        {
            if (param == null)
            {
                param = this.CreatLockParameter(lockey);
            }

            var locker = this.CreateLocker(lockey, param);

            int retrys = 0;

            using (var conn = this._adapter.CreateDbConnection())
            {
                do
                {
                    string insert = this._adapter.CreateInsert(locker);
                    string select = this._adapter.CreateSelect(lockey);

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

                    Locker exists = null;

                    if (param.ConflictPloy != ConflictPloy.Wait
                        || param.RetryTimes == retrys)
                    {
                        //  ExecuteAsync 和 QueryFirstOrDefaultAsync 两个操作不是一个原子性的操作
                        //  所以 exists 还是可能出现为 null 的情况
                        //  也就是在 ExecuteAsync 出现并发之后 QueryFirstOrDefaultAsync 之前
                        //  冲突的锁被解掉了
                        exists = conn.QueryFirstOrDefault<Locker>(
                                    select,
                                    new
                                    {
#pragma warning disable IDE0037
                                        BusinessType = locker.BusinessType,
                                        BusinessCode = locker.BusinessCode
#pragma warning restore IDE0037
                                    });
                    }

                    this.ThrowIfConflicted(
                        exists,
                        param,
                        ref retrys);

                    Thread.Sleep(param.RetryInterval);
                }
                while (true);
            }
        }




        public override bool TryEnter(Lockey lockey,
            LockParameter parameter,
            out Locker locker)
        {
            try
            {
                locker = this.Enter(lockey, parameter);
                return true;
            }
            catch (LockConflictException)
            {
                locker = null;
                return false;
            }
        }

        public override void Keep(Lockey lockey, TimeSpan span)
        {
            if (_useCache == true)
            {
                this.Keep(lockey,
                    this.KeepCore,
                    span);
            }
            else
            {
                this.KeepCore(lockey, span);
            }
        }
        protected virtual void KeepCore(Lockey lockey, TimeSpan span)
        {
            UtilMethods.ThrowIfNull(lockey, nameof(lockey));

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
                    throw new LockExpiredException();
                }
            }
        }
        public override void Exit(Lockey lockey)
        {
            if (_useCache == true)
            {
                this.Exit(lockey,
                    this.ExitCore);
            }
            else
            {
                this.ExitCore(lockey);
            }
        }
        protected virtual void ExitCore(Lockey lockey)
        {
            UtilMethods.ThrowIfNull(lockey, nameof(lockey));

            string delete = this._adapter.CreateDelete(lockey);

            using (var conn = this._adapter.CreateDbConnection())
            {
                conn.Execute(
                    delete,
                    lockey);
            }
        }
    }
}
