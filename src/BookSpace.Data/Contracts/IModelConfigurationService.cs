using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BookSpace.Data.Contracts
{
    public interface IModelConfigurationService
    {
        void ConfigureModels(ModelBuilder builder);
    }
}
