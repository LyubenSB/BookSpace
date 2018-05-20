using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BookSpace.Web.Services;
using BookSpace.Data;
using BookSpace.Data.Contracts;
using BookSpace.Models;
using BookSpace.Repositories;
using BookSpace.Repositories.Contracts;
using Microsoft.Extensions.Logging;
using AutoMapper;
using BookSpace.BlobStorage.Contracts;
using BookSpace.BlobStorage;
using BookSpace.CognitiveServices;
using BookSpace.CognitiveServices.Contract;
using BookSpace.Factories;
using BookSpace.Factories.ResponseModels;
using BookSpace.Services;
using BookSpace.Web.Services.SmtpService;
using BookSpace.Web.Services.SmtpService.Contract;

namespace BookSpace.Web
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<BookSpaceContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<BookSpaceContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IDbContext>(provider => provider.GetService<BookSpaceContext>());

            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IDatabaseSeedService, DatabaseSeedService>();
            services.AddTransient<ApplicationUser>();

            //Repositories

            services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IGenreRepository, GenreRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IBookUserRepository, BookUserRepository>();
            services.AddScoped<IBookTagRepository, BookTagRepository>();
            services.AddScoped<IBookGenreRepository, BookGenreRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();

            //bookservices
            services.AddScoped<BookServices>();

            //Factories
            services.AddScoped<IFactory<Book, BookResponseModel>, BookFactory>();
            services.AddScoped<IFactory<Genre, GenreResponseModel>, GenreFactory>();
            services.AddScoped<IFactory<Tag, TagResponseModel>, TagFactory>();
            services.AddScoped<IFactory<Comment, CommentResponseModel>, CommentFactory>();


            //Blob Storage
            services.AddSingleton<BlobStorageInfo>(
                Configuration.GetSection(nameof(BlobStorageInfo))
                    .Get<BlobStorageInfo>());
            services.AddSingleton<IBlobStorageService, BlobStorageService>();

            //FaceApi Storage
            services.AddSingleton<IFaceService, FaceService>();
            services.AddSingleton<FaceServiceStorageInfo>(
                Configuration.GetSection(nameof(FaceServiceStorageInfo))
                    .Get<FaceServiceStorageInfo>());

            //Smtp service
            services.AddSingleton<ISmtpSender,SmtpSender>();

            

            services.AddAutoMapper();
            services.AddMvc();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                 name: "areaRoute",
                 template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
