using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Zxw.Framework.NetCore.EfDbContext
{
    public class DefaultModelBuilder:ModelBuilder
    {
        public DefaultModelBuilder(ConventionSet conventions) : base(conventions)
        {
        }

        public override ModelBuilder ApplyConfiguration<TEntity>(IEntityTypeConfiguration<TEntity> configuration)
        {
            return base.ApplyConfiguration(configuration);
        }
    }
}
