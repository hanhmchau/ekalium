using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kalium.Server.Context;
using Kalium.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Kalium.Server.Repositories
{
    internal class SearchHelper<T>
    {
        protected IQueryable<T> Collection;
        protected ApplicationDbContext Context;
        public SearchHelper(ApplicationDbContext context)
        {
            Context = context;
        }
        public async Task<ICollection<T>> Get() => await Collection.ToListAsync();
        public async Task<int> Count() => await Collection.CountAsync();

        public SearchHelper<T> Page(int page, int pageSize)
        {
            Collection = Collection.Skip((page - 1) * pageSize).Take(pageSize);
            return this;
        }
    }
}