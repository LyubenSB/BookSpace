using System;
using System.Collections.Generic;
using System.Text;

namespace BookSpace.Data.Contracts
{
    public interface IDatabaseMigrateService
    {
        void Migrate();
    }
}
