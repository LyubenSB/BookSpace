using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BookSpace.Data.Contracts;
using BookSpace.Models;
using BookSpace.Web.Models.CommentViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookSpace.Web.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly IRepository<ApplicationUser> applicationUserRepository;
        private readonly IRepository<Book> bookRepository;
        private readonly IRepository<Comment> commentRepository;
        private readonly IUpdateService<Comment> commentUpdateService;
        private readonly IMapper objectMapper;

        public CommentController(IRepository<ApplicationUser> applicationUserRepository, 
                                 IRepository<Book> bookRepository, 
                                 IRepository<Comment> commentRepository,
                                 IUpdateService<Comment> commentUpdateService,
                                 IMapper objectMapper)
        {
            this.applicationUserRepository = applicationUserRepository;
            this.bookRepository = bookRepository;
            this.commentRepository = commentRepository;
            this.commentUpdateService = commentUpdateService;
            this.objectMapper = objectMapper;
        }

        [HttpPost]
        public async Task<IActionResult> EditCommentAsync(CommentEditViewModel commentEditViewModel)
        {
            if (!this.ModelState.IsValid)
            {
                 return RedirectToAction("BookDetails", "Book", new { id = commentEditViewModel.BookId });
            }

            var comment = await this.commentRepository.GetByIdAsync(commentEditViewModel.CommentId);

            if (comment == null)
            {
                return RedirectToAction("BookDetails", "Book", new { id = commentEditViewModel.BookId });
            }

            var isAdmin = this.User.IsInRole("Admin");
            var currentLoggedUser = await this.applicationUserRepository.GetByExpressionAsync(u => u.Id == comment.UserId);
            var isOwner = currentLoggedUser.Id == comment.UserId;

            if (!isAdmin && !isOwner)
            {
                return RedirectToAction("Index", "Home");
            }

            comment.Content = commentEditViewModel.Content;
            await this.commentUpdateService.UpdateAsync(comment);

            return RedirectToAction("BookDetails", "Book", new { id = commentEditViewModel.BookId });
        }

        [HttpGet]
        public async Task<IActionResult> EditCommentAsync(CommentEditGetViewModel commentData)
        {
            if (!this.ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home");
            }

            var comment = await this.commentRepository.GetByIdAsync(commentData.CommentId);

            if (comment == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var isAdmin = this.User.IsInRole("Admin");
            var currentLoggedUser = await this.applicationUserRepository.GetByExpressionAsync(u => u.Id == comment.UserId);
            var isOwner = currentLoggedUser.Id == comment.UserId;

            if (!isAdmin && !isOwner)
            {
                return RedirectToAction("Index", "Home");
            }

            var commentEditModel = this.objectMapper.Map<CommentEditViewModel>(comment);

            return View("CommentEditView", commentEditModel);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCommentAsync(CommentDeleteViewModel commentDeleteData)
        {
            if (!this.ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home");
            }

            var commentToDelete = await this.commentRepository.GetByIdAsync(commentDeleteData.CommentId);

            if(commentToDelete == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var isAdmin = this.User.IsInRole("Admin");
            var currentLoggedUser = await this.applicationUserRepository.GetByExpressionAsync(u => u.Id == commentToDelete.UserId);
            var isOwner = currentLoggedUser.Id == commentToDelete.UserId;

            if (!isAdmin && !isOwner)
            {
                return RedirectToAction("Index", "Home");
            }

            await this.commentUpdateService.DeleteAsync(commentToDelete);

            var comments = await this.bookRepository.GetOneToManyAsync(b => b.BookId == commentToDelete.BookId,
                                                       bg => bg.Comments);
            var commentsViewModel = this.objectMapper.Map<IEnumerable<CommentViewModel>>(comments);

            foreach (var comment in commentsViewModel)
            {
                var commentCreatorId = comment.UserId;
                var currentUser = await this.applicationUserRepository.GetByExpressionAsync(u => u.UserName == this.User.Identity.Name);
                var isCreator = commentCreatorId == currentUser.Id;

                comment.CanEdit = isAdmin || isCreator;

                var comemntCreator = await this.applicationUserRepository.GetByIdAsync(comment.UserId);
                comment.Author = comemntCreator.UserName;
                comment.AuthorPicUrl = comemntCreator.ProfilePictureUrl;
            }

            return PartialView("Book/_BookCommentsPartial", new KeyValuePair<string, IEnumerable<CommentViewModel>>(commentToDelete.BookId, commentsViewModel));
        }
    }
}
