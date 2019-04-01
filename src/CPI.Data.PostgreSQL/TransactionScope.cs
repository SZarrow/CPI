using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using CPI.Common;
using ATBase.Core;
using ATBase.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace CPI.Data.PostgreSQL
{
    /// <summary>
    /// 
    /// </summary>
    ///<exception cref="NullReferenceException">当无法实例化DbContext时会抛异常</exception>
    public sealed class TransactionScope : IDisposable
    {
        private const String SERVICE = "CPI.Data.PostgreSQL.TransactionScope";

        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly IDbContextTransaction _transaction = null;
        private Boolean _isCompleted = false;

        /// <summary>
        /// 初始化TransactionScope类的实例
        /// </summary>
        /// <param name="isolationLevel">事务隔离级别</param>
        public TransactionScope(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            var db = XDI.Resolve<CPIDbContext>();

            if (db == null)
            {
                _logger.Error(TraceType.DAL.ToString(), CallResultStatus.ERROR.ToString(), $"{nameof(XDI)}.{nameof(XDI.Resolve)}<{nameof(CPIDbContext)}>", "事务TransactionScope初始化失败", "解析CPIDbContext类型的实例失败");
                throw new ArgumentNullException(nameof(db));
            }

            _transaction = db.Database.BeginTransaction(isolationLevel);
            _logger.Trace(TraceType.DAL.ToString(), CallResultStatus.OK.ToString(), SERVICE, $"tx_{_transaction.TransactionId.ToString()}", LogPhase.BEGIN, "开始数据库事务");
        }
        /// <summary>
        /// 
        /// </summary>
        public void Complete()
        {
            if (!_isCompleted)
            {
                _transaction.Commit();
                _isCompleted = true;
                _logger.Trace(TraceType.DAL.ToString(), CallResultStatus.OK.ToString(), SERVICE, $"tx_{_transaction.TransactionId.ToString()}", LogPhase.END, "完成数据库事务");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (!_isCompleted)
            {
                _transaction.Rollback();
                _logger.Trace(TraceType.DAL.ToString(), CallResultStatus.ERROR.ToString(), SERVICE, $"tx_{_transaction.TransactionId.ToString()}", LogPhase.END, "数据库事务回滚");
            }

            _transaction.Dispose();
            _isCompleted = true;

            GC.SuppressFinalize(this);
        }
    }
}
