using BookSpace.Data.Contracts;
using BookSpace.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookSpace.Services
{
    public class UserDataServices
    {
        private readonly IDbContext dbContext;
        private readonly IRepository<Genre> genreRepository;
        private readonly IRepository<Tag> tagRepository;

        public UserDataServices(IDbContext dbCtx, IRepository<Genre> genreRepository, IRepository<Tag> tagRepository)
        {
            this.dbContext = dbCtx ?? throw new ArgumentNullException(nameof(dbContext));
            this.genreRepository = genreRepository;
            this.tagRepository = tagRepository;
        }

        //splitting tags genres response to seperate entities
       

        public async Task MatchGenresToBookAsync(IEnumerable<string> genres, string bookId)
        {
            foreach (var genreName in genres)
            {
                var genreId = this.genreRepository.GetByExpressionAsync(g => g.Name == genreName).Result.GenreId;

                var bookGenreRecord = new BookGenre()
                {
                    BookId = bookId,
                    GenreId = genreId
                };

                await this.dbContext.DbSet<BookGenre>().AddAsync(bookGenreRecord);
            }

            await this.dbContext.SaveAsync();
        }

        public async Task MatchTagToBookAsync(IEnumerable<string> tags, string bookId)
        {
            foreach (var tagName in tags)
            {
                var tagId = this.tagRepository.GetByExpressionAsync(t => t.Value == tagName).Result.TagId;

                var bookTagRecord = new BookTag()
                {
                    BookId = bookId,
                    TagId = tagId
                };

                await this.dbContext.DbSet<BookTag>().AddAsync(bookTagRecord);
            }

            await this.dbContext.SaveAsync();
        }
    }

}
