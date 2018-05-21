using BookSpace.Data.Contracts;
using Microsoft.EntityFrameworkCore;

namespace BookSpace.Data
{
    public class DatabaseMigrateService : IDatabaseMigrateService
    {
        private readonly BookSpaceContext ctx;

        public DatabaseMigrateService(BookSpaceContext ctx)
        {
            this.ctx = ctx;
        }

        public void Migrate()
        {
            this.ctx.Database.Migrate();
        }
    }
}
