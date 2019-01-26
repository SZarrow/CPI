using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CPI.Common;
using CPI.IData;
using Lotus.Core;
using Lotus.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace CPI.Data.PostgreSQL
{
    public abstract class EFRepository<T> : IUnitOfWork<T> where T : class
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly CPIDbContext _db;
        private readonly Queue<Action> _callbacks;

        protected EFRepository() : this(XDI.Resolve<CPIDbContext>()) { }

        protected EFRepository(CPIDbContext db)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            _callbacks = new Queue<Action>(10);
            _db = db;
        }

        protected DbSet<T> Repository
        {
            get
            {
                try
                {
                    return _db.Set<T>();
                }
                catch (Exception ex)
                {
                    _logger.Error(TraceType.DAL.ToString(), CallResultStatus.ERROR.ToString(), $"{GetTypeFullName()}.Repository", "EFRepository.Repository", $"访问{GetTypeFullName()}.Repository属性出现异常", ex);
                    return null;
                }
            }
        }

        public IQueryable<T> QueryProvider
        {
            get
            {
                return this.Repository != null ? this.Repository as IQueryable<T> : new List<T>(0) as IQueryable<T>;
            }
        }

        public DatabaseFacade Database
        {
            get
            {
                return _db.Database;
            }
        }

        public void Add(T entity)
        {
            if (entity != null)
            {
                var addedEntity = Repository.Add(entity);
                _callbacks.Enqueue(() =>
                {
                    entity = addedEntity.Entity;
                });
            }
        }

        public void Add(IEnumerable<T> entities)
        {
            if (entities != null && entities.Count() > 0)
            {
                Repository.AddRange(entities);
            }
        }

        public void Remove(T entity)
        {
            if (entity != null)
            {
                Repository.Remove(entity);
            }
        }

        public void Remove(IEnumerable<T> entities)
        {
            if (entities != null && entities.Count() > 0)
            {
                Repository.RemoveRange(entities);
            }
        }

        public void Update(T entity)
        {
            if (entity != null)
            {
                Repository.Update(entity);
            }
        }

        public void Update(IEnumerable<T> entities)
        {
            if (entities != null && entities.Count() > 0)
            {
                Repository.UpdateRange(entities);
            }
        }

        public XResult<Int32> ExecuteSql(FormattableString sql)
        {
            try
            {
                String service = $"{GetTypeFullName()}.ExecuteSql(...)";
                String traceMethod = $"{nameof(_db)}.Database.ExecuteSqlCommand(...)";
                _logger.Trace(TraceType.DAL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "开始执行SQL语句", $"SQL：{sql.ToString()}");
                Int32 result = _db.Database.ExecuteSqlCommand(sql);
                _logger.Trace(TraceType.DAL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.END, "完成执行SQL语句", $"受影响{result}行");
                return new XResult<Int32>(result);
            }
            catch (Exception ex)
            {
                _logger.Error(TraceType.DAL.ToString(), CallResultStatus.ERROR.ToString(), $"{GetTypeFullName()}.ExecuteSql()", "数据库更新异常", "执行SQL语句时出现异常", ex, sql);
                return new XResult<Int32>(0, ex);
            }
        }

        public Boolean Exists(Expression<Func<T, Boolean>> predicate)
        {
            if (predicate != null)
            {
                try
                {
                    return this.QueryProvider.Where(predicate).Count() > 0;
                }
                catch (Exception ex)
                {
                    _logger.Error(TraceType.DAL.ToString(), CallResultStatus.ERROR.ToString(), $"{this.GetType().FullName}.Exists()", "数据库查询异常", "查询是否存在时出现异常", ex);
                }
            }

            return false;
        }

        public XResult<Int32> SaveChanges()
        {
            try
            {
                var result = _db.SaveChanges();

                while (_callbacks.Count > 0)
                {
                    _callbacks.Dequeue()();
                }

                return new XResult<Int32>(result);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                //TODO
                _logger.Error(TraceType.DAL.ToString(), CallResultStatus.ERROR.ToString(), $"{GetTypeFullName()}.SaveChanges()", "数据库更新异常", "更新数据库出现并发异常", null, ex);
                return new XResult<Int32>(0, ex);
            }
            catch (Exception ex)
            {
                _logger.Error(TraceType.DAL.ToString(), CallResultStatus.ERROR.ToString(), $"{this.GetType().FullName}.SaveChanges()", "数据库更新异常", "更新数据库出现并发异常", null, ex);
                return new XResult<Int32>(0, ex);
            }
        }

        public Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _db.SaveChangesAsync(cancellationToken);
        }

        private String GetTypeFullName()
        {
            return $"{this.GetType().FullName}<{typeof(T).FullName}>";
        }
    }
}
