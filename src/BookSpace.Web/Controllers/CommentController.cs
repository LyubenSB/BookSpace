using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookSpace.Data.Contracts;
using BookSpace.Models;
using BookSpace.Web.Models.CommentViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BookSpace.Web.Controllers
{
    public class CommentController : Controller
    {
        private readonly IRepository<ApplicationUser> applicationUserRepository;
        private readonly IRepository<Book> bookRepository;
        private readonly IRepository<Comment> commentRepository;
        private readonly IMapper objectMapper;

        public CommentController(IRepository<ApplicationUser> applicationUserRepository, 
                                 IRepository<Book> bookRepository, 
                                 IRepository<Comment> commentRepository, 
                                 IMapper objectMapper)
        {
            this.applicationUserRepository = applicationUserRepository;
            this.bookRepository = bookRepository;
            this.commentRepository = commentRepository;
            this.objectMapper = objectMapper;
      
        }

        [HttpPost]
        public async Task<IActionResult> EditCommentAsync(CommentEditViewModel commentEditViewModel)
        {
            if (commentEditViewModel == null)
            {
                return RedirectToAction("Index", "Home");
            }
           
            if(commentEditViewModel.Content != null)
            {
                var comment = await this.commentRepository.GetByIdAsync(commentEditViewModel.CommentId);
                comment.Content = commentEditViewModel.Content;

                await this.commentRepository.UpdateAsync(comment);
            }

            return RedirectToAction("BookDetails", "Book", new { id = commentEditViewModel.BookId });
        }

        [HttpGet]
        public async Task<IActionResult> EditCommentAsync(string commentId)
        {
            if (commentId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var comment = await this.commentRepository.GetByIdAsync(commentId);

            var commentEditModel = new CommentEditViewModel
            {
                BookId = comment.BookId,
                CommentId = comment.CommentId,
                Content = comment.Content
            };

            return View("CommentEditView", commentEditModel);
        }

        //TODO:THIS IS TOO BIG
        [HttpPost]
        public async Task<IActionResult> DeleteCommentAsync(string commentId, string bookId, string userId)
        {
            if (commentId == null || bookId == null || userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!this.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            var isAdmin = this.User.IsInRole("Admin");

            var currentLoggedUser = await this.applicationUserRepository.GetAsync(u => u.Id == userId);

            var isOwner = currentLoggedUser.Id == userId;

            if (!isAdmin && !isOwner)
            {
                return RedirectToAction("Index", "Home");
            }

            var commentToDelete = await this.commentRepository.GetAsync(c => c.CommentId == commentId);
            await this.commentRepository.DeleteAsync(commentToDelete);

            var comments = await this.bookRepository.GetOneToManyAsync(b => b.BookId == bookId,
                                                       bg => bg.Comments);
            var commentsViewModel = this.objectMapper.Map<IEnumerable<CommentViewModel>>(comments);

            foreach (var comment in commentsViewModel)
            {
                var commentCreatorId = comment.UserId;
                var currentUser = await this.applicationUserRepository.GetAsync(u => u.UserName == this.User.Identity.Name);
                var isCreator = commentCreatorId == currentUser.Id;

                comment.CanEdit = isAdmin || isCreator;

                var user = await this.applicationUserRepository.GetByIdAsync(comment.UserId);
                comment.Author = user.UserName;
            }

            return PartialView("Book/_BookCommentsPartial", new KeyValuePair<string, IEnumerable<CommentViewModel>>(bookId, commentsViewModel));
        }
    }
}
