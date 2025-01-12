using Borealis.Core.Requests;
using Microsoft.EntityFrameworkCore;

namespace Borealis.Core.Services;

public abstract class QueryServiceBase<T> where T : class {
    protected abstract string DefaultSortProperty { get; }
    protected abstract bool DefaultSortAscending { get; }

    protected virtual IQueryable<T> AddSorting(IQueryable<T> dbQuery, QueryBase query) {
        if(!string.IsNullOrWhiteSpace(query.SortField)) {
            if(query.SortAscending) {
                dbQuery = dbQuery
                    .OrderBy(x => EF.Property<T>(x, query.SortField))
                    .ThenBy(x => EF.Property<T>(x, DefaultSortProperty));
            } else {
                dbQuery = dbQuery
                    .OrderByDescending(x => EF.Property<T>(x, query.SortField))
                    .ThenByDescending(x => EF.Property<T>(x, DefaultSortProperty));
            }
        } else {
            if(DefaultSortAscending) {
                dbQuery = dbQuery.OrderBy(x => EF.Property<T>(x, DefaultSortProperty));
            } else {
                dbQuery = dbQuery.OrderByDescending(x => EF.Property<T>(x, DefaultSortProperty));
            }
        }

        return dbQuery;
    }
}
