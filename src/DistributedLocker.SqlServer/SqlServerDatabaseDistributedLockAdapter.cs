using DistributedLocker.DataBase;
using DistributedLocker.Internal;
using DistributedLocker.SqlServer.Extensions;
using Microsoft.Data.SqlClient;
using System;
using System.Data.Common;

namespace DistributedLocker.SqlServer
{
    public class SqlServerDatabaseDistributedLockAdapter : IDatabaseDistributedLockAdapter
    {
        private const string CREATE = @"
							IF OBJECT_ID(N'SysLocker',N'U') IS NULL
								BEGIN
									CREATE TABLE [dbo].[SysLocker](
										[ID] 			INT IDENTITY(1,1) 	PRIMARY KEY,
										[BusinessType] 	VARCHAR(32) 		NOT NULL,
										[BusinessCode] 	VARCHAR(32) 		NOT NULL,
										[BeginTime] 	BIGINT 				NOT NULL,
										[EndTime] 		BIGINT 				NOT NULL,
										[IP] 			VARCHAR(32),
										[Token] 		VARCHAR(32) 		NOT NULL,
										[DelayTimes] 	INT DEFAULT 0 		NOT NULL,
										[IsPersistence] BIT DEFAULT 0 		NOT NULL,
										[LockMsg] 		NVARCHAR(100),
										[ConflictMsg] 	NVARCHAR(100),
										[HostName] 		NVARCHAR(50),
										[OperCode] 		VARCHAR (20),
										[OperName] 		NVARCHAR (50),
										[OperType] 		VARCHAR (20),
										[OperTime] 		DATETIME DEFAULT GETDATE() NOT NULL
									);
		
									CREATE UNIQUE INDEX UN_SYSLOCKER_BUSINESS ON SysLocker(BusinessType, BusinessCode);
									CREATE UNIQUE INDEX UN_SYSLOCKER_Token ON SysLocker(Token);
		
									EXEC sp_addextendedproperty N'MS_Description', N'业务类型', N'SCHEMA', N'dbo',N'TABLE', N'SysLocker', N'COLUMN', N'BusinessType';
									EXEC sp_addextendedproperty N'MS_Description', N'业务代码', N'SCHEMA', N'dbo',N'TABLE', N'SysLocker', N'COLUMN', N'BusinessCode';
									EXEC sp_addextendedproperty N'MS_Description', N'锁定时间', N'SCHEMA', N'dbo',N'TABLE', N'SysLocker', N'COLUMN', N'BeginTime';
									EXEC sp_addextendedproperty N'MS_Description', N'过期时间', N'SCHEMA', N'dbo',N'TABLE', N'SysLocker', N'COLUMN', N'EndTime';
									EXEC sp_addextendedproperty N'MS_Description', N'IP', N'SCHEMA', N'dbo',N'TABLE', N'SysLocker', N'COLUMN', N'IP';
									EXEC sp_addextendedproperty N'MS_Description', N'控制重入或校验的Token', N'SCHEMA', N'dbo',N'TABLE', N'SysLocker', N'COLUMN', N'Token';
									EXEC sp_addextendedproperty N'MS_Description', N'延期存活次数', N'SCHEMA', N'dbo',N'TABLE', N'SysLocker', N'COLUMN', N'DelayTimes';
									EXEC sp_addextendedproperty N'MS_Description', N'是否持久化', N'SCHEMA', N'dbo',N'TABLE', N'SysLocker', N'COLUMN', N'IsPersistence';
									EXEC sp_addextendedproperty N'MS_Description', N'锁定消息', N'SCHEMA', N'dbo',N'TABLE', N'SysLocker', N'COLUMN', N'LockMsg';
									EXEC sp_addextendedproperty N'MS_Description', N'并发提示消息', N'SCHEMA', N'dbo',N'TABLE', N'SysLocker', N'COLUMN', N'ConflictMsg';
									EXEC sp_addextendedproperty N'MS_Description', N'主机名', N'SCHEMA', N'dbo',N'TABLE', N'SysLocker', N'COLUMN', N'HostName';
									EXEC sp_addextendedproperty N'MS_Description', N'操作者代码', N'SCHEMA', N'dbo',N'TABLE', N'SysLocker', N'COLUMN', N'OperCode';
									EXEC sp_addextendedproperty N'MS_Description', N'操作者姓名', N'SCHEMA', N'dbo',N'TABLE', N'SysLocker', N'COLUMN', N'OperName';
									EXEC sp_addextendedproperty N'MS_Description', N'操作类型', N'SCHEMA', N'dbo',N'TABLE', N'SysLocker', N'COLUMN', N'OperType';
									EXEC sp_addextendedproperty N'MS_Description', N'操作时间', N'SCHEMA', N'dbo',N'TABLE', N'SysLocker', N'COLUMN', N'OperTime';

								END;
                            ";

        private const string INSERT = @"
								--格林尼治时间
								--SELECT DATEDIFF(S,'1970-01-01 00:00:00',GETUTCDATE())

                                DELETE FROM SysLocker WHERE EndTime < (SELECT CAST((DATEDIFF(MI,'1970-01-01 00:00:00',GETDATE()) * 60 + DATEPART(SS,GETDATE())) AS BIGINT) * 1000 + DATEPART(MS,GETDATE()));

								INSERT INTO SysLocker(
									[BusinessType],
									[BusinessCode],
									[BeginTime],
									[EndTime],
									[IP],
									[Token],
									[DelayTimes],
									[IsPersistence],
									[LockMsg],
									[ConflictMsg],
									[HostName],
									[OperCode],
									[OperName],
									[OperType],
									[OperTime])
								VALUES(
									@BusinessType,
									@BusinessCode,
									(SELECT CAST((DATEDIFF(MI,'1970-01-01 00:00:00',GETDATE()) * 60 + DATEPART(SS,GETDATE())) AS BIGINT) * 1000 + DATEPART(MS,GETDATE())),
									(SELECT CAST((DATEDIFF(MI,'1970-01-01 00:00:00',GETDATE()) * 60 + DATEPART(SS,GETDATE())) AS BIGINT) * 1000 + DATEPART(MS,GETDATE())) + @Duation,
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
									GETDATE()
								);
                            ";

        private const string SELECT = @"
                                SELECT
	                                [BusinessType]   AS   ""BusinessType"",
                                    [BusinessCode]   AS   ""BusinessCode"",
	                                [BeginTime]      AS   ""BeginTime"",
	                                [EndTime]        AS   ""EndTime"",
	                                [IP]             AS   ""IP"",
	                                [Token]          AS   ""Token"",
	                                [DelayTimes]     AS   ""DelayTimes"",
	                                [IsPersistence]  AS   ""IsPersistence"",
	                                [LockMsg]        AS   ""LockMsg"",
	                                [ConflictMsg]    AS   ""ConflictMsg"",
	                                [HostName]       AS   ""HostName"",
	                                [OperCode]       AS   ""OperCode"",
	                                [OperName]       AS   ""OperName"",
	                                [OperType]       AS   ""OperType"",
	                                [OperTime]       AS   ""OperTime""
                                FROM
                                    SysLocker
                                WHERE
                                    [BusinessType]   =   @BusinessType
                                AND [BusinessCode]   =   @BusinessCode
                            ";

        private const string DELETE = @"
                                DELETE FROM SysLocker 
                                WHERE 
                                    Token = @Token
                                AND IsPersistence <> 1
                            ";

        private const string UPDATE = @"
                                UPDATE SysLocker 
                                SET 
                                    DelayTimes = DelayTimes + 1, 
                                    EndTime = EndTime + @Delay 
                                WHERE
                                    Token = @Token
                            ";


        private readonly ILockOptions _options;

        public SqlServerDatabaseDistributedLockAdapter(ILockOptions options)
        {
            UtilMethods.ThrowIfNull(options, nameof(options));

            _options = options;
        }

        public bool CheckIfConflictException(Exception exception)
        {
            if (exception is SqlException sqlex 
				&& sqlex.Class == 14)
            {
				return true;
            }

			return false;
        }

        public DbConnection CreateDbConnection()
        {
            var exoptions = this._options.FindExtension<SqlServerDataBaseLockOptionsExtension>();
            return new SqlConnection(exoptions.ConnectionString);
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
