using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ATBase.Core;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace CPI.IData
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IUnitOfWork<T> where T : class
    {
        /// <summary>
        /// 添加一个实体到数据库
        /// </summary>
        /// <param name="entity">要添加的实体</param>
        void Add(T entity);

        /// <summary>
        /// 批量添加实体到数据库
        /// </summary>
        /// <param name="entities">要添加的实体</param>
        void Add(IEnumerable<T> entities);

        /// <summary>
        /// 从数据库中删除实体
        /// </summary>
        /// <param name="entity">要删除的实体</param>
        void Remove(T entity);

        /// <summary>
        /// 从数据库中删除实体
        /// </summary>
        /// <param name="entity">要删除的实体</param>
        void Remove(IEnumerable<T> entity);

        /// <summary>
        /// 更新实体到数据库
        /// </summary>
        /// <param name="entity">要更新的实体</param>
        void Update(T entity);

        /// <summary>
        /// 批量更新实体到数据库，注意：实体的主键字段必须不为null，否则会更新失败
        /// </summary>
        /// <param name="entities">要更新的实体</param>
        void Update(IEnumerable<T> entities);

        /// <summary>
        /// 判断满足指定条件的记录是否存在
        /// </summary>
        /// <param name="predicates">条件表达式</param>
        Boolean Exists(Expression<Func<T, Boolean>> predicates);

        /// <summary>
        /// 获取数据库查询接口
        /// </summary>
        IQueryable<T> QueryProvider { get; }

        /// <summary>
        /// 获取数据库访问接口
        /// </summary>
        DatabaseFacade Database { get; }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="sql"></param>
        XResult<Int32> ExecuteSql(FormattableString sql);

        /// <summary>
        /// 提交所有更改到数据库
        /// </summary>
        /// <returns>返回受影响的行数</returns>
        XResult<Int32> SaveChanges();

        /// <summary>
        /// 提交所有更改到数据库
        /// </summary>
        /// <param name="cancellationToken">任务取消令牌</param>
        /// <returns>返回受影响的行数</returns>
        Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
