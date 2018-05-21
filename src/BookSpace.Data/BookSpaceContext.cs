using System.IO;
using System.Threading.Tasks;
using BookSpace.Data.Contracts;
using BookSpace.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BookSpace.Data
{
    public class BookSpaceContext : IdentityDbContext<ApplicationUser>, IDbContext
    {
        private readonly IModelConfigurationService modelConfigurationService;

        public BookSpaceContext(DbContextOptions<BookSpaceContext> options, IModelConfigurationService modelConfigurationService)
            : base(options)
        {
            this.modelConfigurationService = modelConfigurationService;
        }

        public DbSet<Book> Books { get; set; }

        public DbSet<BookGenre> BooksGenres { get; set; }

        public DbSet<BookUser> BooksUsers { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Genre> Genres { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<BookTag> BooksTags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json")
                   .Build();
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            this.modelConfigurationService.ConfigureModels(builder);
        }

        public DbSet<TEntity> DbSet<TEntity>() where TEntity : class
        {
            //TODO suspicios
            return this.Set<TEntity>();
        }

        public async Task<int> SaveAsync()
        {
            return await this.SaveChangesAsync();
        }
    }
}
