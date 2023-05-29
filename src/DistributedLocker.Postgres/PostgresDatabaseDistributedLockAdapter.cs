using DistributedLocker.DataBase;
using DistributedLocker.Internal;
using DistributedLocker.Postgres.Extensions;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace DistributedLocker.Postgres
{
    public class PostgresDatabaseDistributedLockAdapter : IDatabaseDistributedLockAdapter
    {
        private const string CREATE = @"
                                do $$
                                declare
                                    table_exists boolean := false;
                                begin

	                                select true into table_exists from pg_tables where tablename = 'sys_locker';

	                                if table_exists is null or table_exists <> true then

		                                create table if not exists sys_locker (
		                                  id serial primary key,
		                                  business_type varchar(32) not null,
		                                  business_code varchar(32) not null,
		                                  begin_time bigint not null,
		                                  end_time bigint not null,
		                                  ip varchar(32),
		                                  token varchar(32) not null,
		                                  delay_times integer default 0 not null,
		                                  is_persistence smallint default 0 not null,
		                                  lock_msg varchar(100),
		                                  conflict_msg varchar(100),
		                                  host_name varchar(50),
		                                  oper_code varchar (20),
		                                  oper_name varchar (50),
		                                  oper_type varchar (20),
		                                  oper_time timestamp default current_timestamp not null
		                                );

		                                create unique index un_syslocker_business on sys_locker(business_type, business_code);
		                                create unique index un_syslocker_token on sys_locker(token);
				
		                                comment on column sys_locker.business_type is '业务类型';
		                                comment on column sys_locker.business_code is '业务代码';
		                                comment on column sys_locker.begin_time is '锁定时间';
		                                comment on column sys_locker.end_time is '过期时间';
		                                comment on column sys_locker.ip is 'ip';
		                                comment on column sys_locker.token is '控制重入或校验的token';
		                                comment on column sys_locker.delay_times is '延期存活次数';
		                                comment on column sys_locker.is_persistence is '是否持久化';
		                                comment on column sys_locker.lock_msg is '锁定消息';
		                                comment on column sys_locker.conflict_msg is '并发提示消息';
		                                comment on column sys_locker.host_name is '主机名';
		                                comment on column sys_locker.oper_code is '操作者代码';
		                                comment on column sys_locker.oper_name is '操作者姓名';
		                                comment on column sys_locker.oper_type is '操作类型';
		                                comment on column sys_locker.oper_time is '操作时间';
	                                end if;
                                commit;
                                end $$;
                            ";

        private const string INSERT = @"
	                                delete from sys_locker where end_time < (select floor(extract(epoch from((current_timestamp - timestamp '1970-01-01 00:00:00') * 1000))));

	                                insert into sys_locker(
		                                business_type,
		                                business_code,
		                                begin_time,
		                                end_time,
		                                ip,
		                                token,
		                                delay_times,
		                                is_persistence,
		                                lock_msg,
		                                conflict_msg,
		                                host_name,
		                                oper_code,
		                                oper_name,
		                                oper_type,
		                                oper_time)
	                                values(
		                                @BusinessType,
		                                @BusinessCode,
		                                (select floor(extract(epoch from((current_timestamp - timestamp '1970-01-01 00:00:00') * 1000)))),
		                                (select floor(extract(epoch from((current_timestamp - timestamp '1970-01-01 00:00:00') * 1000)))) + @Duation,
		                                @IP,
		                                @Token,
		                                @DelayTimes,
		                                @IsPersistence,
		                                @LockMsg,
		                                @ConflictMsg,
		                                @HostName,
		                                @OperCode,
		                                @OperName,
		                                @OperType,
		                                current_timestamp
	                                );
                            ";

        private const string SELECT = @"
                                select
                                    business_type   as   ""BusinessType"",
                                    business_code   as   ""BusinessCode"",
                                    begin_time      as   ""BeginTime"",
                                    end_time        as   ""EndTime"",
                                    ip              as   ""IP"",
                                    token           as   ""Token"",
                                    delay_times     as   ""DelayTimes"",
                                    is_persistence  as   ""IsPersistence"",
                                    lock_msg        as   ""LockMsg"",
                                    conflict_msg    as   ""ConflictMsg"",
                                    host_name       as   ""HostName"",
                                    oper_code       as   ""OperCode"",
                                    oper_name       as   ""OperName"",
                                    oper_type       as   ""OperType"",
                                    oper_time       as   ""OperTime""
                                from
                                    sys_locker
                                where
                                    business_type   =   @BusinessType
                                and business_code   =   @BusinessCode
                            ";

        private const string DELETE = @"
                                delete from sys_locker 
                                where 
	                                token = @Token
                                and is_persistence <> 1
                            ";

        private const string UPDATE = @"
                                update sys_locker 
                                set 
	                                delay_times = delay_times + 1, 
	                                end_time = end_time + :delay 
                                where
	                                token = @Token
                            ";

        private readonly ILockOptions _options;

        public PostgresDatabaseDistributedLockAdapter(ILockOptions options)
        {
            UtilMethods.ThrowIfNull(options, nameof(options));

            _options = options;
        }

        public bool CheckIfConflictException(Exception exception)
        {
            if (exception is PostgresException pgex
                && pgex.SqlState == "23505")
            {
                return true;
            }

            if (exception.Message
                .ToUpper()
                .Contains("23505"))
            {
                return true;
            }

            return false;
        }

        public DbConnection CreateDbConnection()
        {
            var exoptions = this._options.FindExtension<PostgresDataBaseLockOptionsExtension>();
            return new NpgsqlConnection(exoptions.ConnectionString);
        }

        public string CreateCreate()
        {
            return CREATE;
        }

        public string CreateInsert(Locker locker)
        {
            return INSERT;
        }

        public string CreateDelete(Lockey lockey)
        {
            return DELETE;
        }

        public string CreateSelect(Lockey lockey)
        {
            return SELECT;
        }

        public string CreateUpdate(Lockey lockey)
        {
            return UPDATE;
        }
    }
}
