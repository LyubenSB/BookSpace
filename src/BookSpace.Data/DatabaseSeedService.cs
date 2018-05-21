using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BookSpace.BlobStorage;
using BookSpace.BlobStorage.Contracts;
using BookSpace.Data.Contracts;
using BookSpace.Data.DTO;
using BookSpace.Factories;
using BookSpace.Factories.DTO;
using BookSpace.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace BookSpace.Data
{
    public class DatabaseSeedService : IDatabaseSeedService
    {
        private readonly BookSpaceContext ctx;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IBlobStorageService blobStorageService;
        private readonly IHttpService httpService;
        private readonly IFactory<ApplicationUser, UserCreateDto> userFactory;
        private readonly Random rnd;

        public DatabaseSeedService(BookSpaceContext ctx, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IBlobStorageService blobStorageService, IHttpService httpService, IFactory<ApplicationUser, UserCreateDto> userFactory)
        {
            this.ctx = ctx;
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.blobStorageService = blobStorageService;
            this.httpService = httpService;
            this.userFactory = userFactory;
            this.rnd = new Random();
        }

        public async Task SeedDataAsync()
        {
            await this.SeedBooksAsync();
            await this.SeedGenresAsync();
            await this.SeedRolesAsync();
            await this.SeedUsersAsync();
            await this.SeedCommentsAsync();
            await this.SeedGenresBooksAsync();
            await this.SeedBookUsersAsync();
            await this.SeedTagsAsync();
            await this.SeedBookTags();
        }

        private async Task SeedBookTags()
        {
            if (!this.ctx.BooksTags.Any())
            {
                string[] tagsIds = await this.ctx.Tags.Select(i => i.TagId).ToAsyncEnumerable().ToArray();
                string[] bookIds = await this.ctx.Books.Select(i => i.BookId).ToAsyncEnumerable().ToArray();

                List<BookTag> bookTags = new List<BookTag>();

                // For each book add 3 tags
                int chunkSize = tagsIds.Length / 3;
                for (int i = 0; i < tagsIds.Length; i++)
                {
                    bookTags.Add(new BookTag()
                    {
                        BookId = bookIds[i],
                        TagId = tagsIds[this.rnd.Next(0, chunkSize)]
                    });

                    bookTags.Add(new BookTag()
                    {
                        BookId = bookIds[i],
                        TagId = tagsIds[this.rnd.Next(0 + chunkSize, chunkSize * 2)]
                    });

                    bookTags.Add(new BookTag()
                    {
                        BookId = bookIds[i],
                        TagId = tagsIds[this.rnd.Next(0 + 2 * chunkSize, chunkSize * 3)]
                    });
                }

                await this.ctx.BooksTags.AddRangeAsync(bookTags);
                await this.ctx.SaveChangesAsync();
            }
        }

        private async Task SeedTagsAsync()
        {
            if (!this.ctx.Tags.Any())
            {
                BlobObjectInfo blobInfo = await this.blobStorageService.GetAsync("Tags.json", "unknown");
                HttpResponseBodyDto httpResponse = await this.httpService.GetAsync(blobInfo);
                Tag[] allTags = JsonConvert.DeserializeObject<Tag[]>(httpResponse.Body);

                await this.ctx.Tags.AddRangeAsync(allTags);
                await this.ctx.SaveChangesAsync();
            }
        }

        private async Task SeedBookUsersAsync()
        {
            if (!this.ctx.BooksUsers.Any())
            {
                string[] bookIds = this.ctx.Books.Select(b => b.BookId).ToArray();
                string[] userIds = this.ctx.Users.Select(u => u.Id).ToArray();

                // For each user add 2 books with different state
                List<BookUser> bookUsers = new List<BookUser>();
                foreach (var uId in userIds)
                {
                    bookUsers.Add(new BookUser
                    {
                        UserId = uId,
                        BookId = bookIds[this.rnd.Next(0, bookIds.Length / 2)],
                        State = Models.Enums.BookState.Read
                    });

                    bookUsers.Add(new BookUser
                    {
                        UserId = uId,
                        BookId = bookIds[this.rnd.Next(bookIds.Length / 2, bookIds.Length)],
                        State = Models.Enums.BookState.ToRead
                    });
                }

                await this.ctx.BooksUsers.AddRangeAsync(bookUsers);
                await this.ctx.SaveChangesAsync();
            }
        }

        private async Task SeedGenresBooksAsync()
        {
            if (!this.ctx.BooksGenres.Any())
            {
                string[] bookIds = this.ctx.Books.Select(b => b.BookId).ToArray();
                string[] genreIds = this.ctx.Genres.Select(b => b.GenreId).ToArray();

                List<BookGenre> booksGenres = new List<BookGenre>();
                foreach (var bId in bookIds)
                {
                    booksGenres.Add(new BookGenre
                    {
                        BookId = bId,
                        GenreId = genreIds[this.rnd.Next(0, genreIds.Length)]
                    });
                }

                await this.ctx.BooksGenres.AddRangeAsync(booksGenres);
                await this.ctx.SaveChangesAsync();
            }
        }

        private async Task SeedCommentsAsync()
        {
            if (!this.ctx.Comments.Any())
            {
                BlobObjectInfo blobInfo = await this.blobStorageService.GetAsync("Comments.json", "unknown");
                HttpResponseBodyDto httpResponse = await this.httpService.GetAsync(blobInfo);
                Comment[] allComments = JsonConvert.DeserializeObject<Comment[]>(httpResponse.Body);

                string[] usersIds = this.ctx.Users.Select(i => i.Id).ToArray();
                string[] bookIds = this.ctx.Books.Select(b => b.BookId).ToArray();

                foreach (var comment in allComments)
                {
                    comment.UserId = usersIds[this.rnd.Next(0, usersIds.Length)];
                    comment.BookId = bookIds[this.rnd.Next(0, bookIds.Length)];
                }

                await this.ctx.Comments.AddRangeAsync(allComments);
                await this.ctx.SaveChangesAsync();
            }
        }

        private async Task SeedUsersAsync()
        {
            if (!this.ctx.Users.Any())
            {
                // Seed admin
                var adminEmail = "admin@admin.com";
                var adminUsername = "admin";
                var adminPassword = "Asd12@";
                await this.CreateUserAsync(new UserCreateDto { Email = adminEmail, Username = adminUsername, Password = adminPassword, Role = "Admin" });

                // Add some users
                BlobObjectInfo blobInfo = await this.blobStorageService.GetAsync("Users.json", "unknown");
                HttpResponseBodyDto httpResponse = await this.httpService.GetAsync(blobInfo);
                UserSeedDto[] allUsers = JsonConvert.DeserializeObject<UserSeedDto[]>(httpResponse.Body);

                foreach (var user in allUsers)
                {
                    await this.CreateUserAsync(new UserCreateDto { Email = user.Email, Username = user.Username, Password = "Asd12@", Role = "User" });
                }

                await this.ctx.SaveChangesAsync();
            }
        }

        private async Task SeedRolesAsync()
        {
            if (!await this.roleManager.RoleExistsAsync("User"))
            {
                IdentityRole role = new IdentityRole { Name = "User" };

                IdentityResult roleResult = await this.roleManager.CreateAsync(role);
            }

            if (!this.roleManager.RoleExistsAsync("Admin").Result)
            {
                IdentityRole role = new IdentityRole { Name = "Admin" };

                IdentityResult roleResult = await this.roleManager.CreateAsync(role);
            }
        }

        private async Task SeedGenresAsync()
        {
            if (!this.ctx.Genres.Any())
            {
                BlobObjectInfo blobInfo = await this.blobStorageService.GetAsync("Genres.json", "unknown");
                HttpResponseBodyDto httpResponse = await this.httpService.GetAsync(blobInfo);
                Genre[] allGenres = JsonConvert.DeserializeObject<Genre[]>(httpResponse.Body);

                await this.ctx.Genres.AddRangeAsync(allGenres);
                await this.ctx.SaveChangesAsync();
            }
        }

        private async Task SeedBooksAsync()
        {
            if (!this.ctx.Books.Any())
            {
                BlobObjectInfo blobInfo = await this.blobStorageService.GetAsync("Books.json", "unknown");
                HttpResponseBodyDto httpResponse = await this.httpService.GetAsync(blobInfo);
                Book[] allBooks = JsonConvert.DeserializeObject<Book[]>(httpResponse.Body);

                await this.ctx.Books.AddRangeAsync(allBooks);
                await this.ctx.SaveChangesAsync();
            }
        }

        private async Task CreateUserAsync(UserCreateDto userCreateDto)
        {
            ApplicationUser user = this.userFactory.Create(userCreateDto);

            IdentityResult createResult = await this.userManager.CreateAsync(user, userCreateDto.Password);
            IdentityResult addRoleResult = await this.userManager.AddToRoleAsync(user, userCreateDto.Role);

            if (!createResult.Succeeded)
            {
                throw new Exception(string.Join(";", createResult.Errors));
            }

            if (!addRoleResult.Succeeded)
            {
                throw new Exception(string.Join(";", addRoleResult.Errors));
            }

            if (userCreateDto.Role == "User")
            {
                user.LockoutEnabled = true;
            }
        }
    }
}
