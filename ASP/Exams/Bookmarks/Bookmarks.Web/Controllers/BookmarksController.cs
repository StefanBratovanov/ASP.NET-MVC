﻿
using System.Data.Entity;
using System.Net;
using System.Web.Services.Protocols;
using Microsoft.AspNet.Identity;

namespace Bookmarks.Web.Controllers
{
    using System.Linq;
    using InputModels;
    using AutoMapper.QueryableExtensions;
    using Common;
    using Data;
    using ViewModels;
    using PagedList;
    using System.Web.Mvc;
    using AutoMapper;
    using Bookmarks.Models;
    using System.Web.Mvc.Expressions;


    [Authorize]
    public class BookmarksController : BaseController
    {
        public BookmarksController(IBookmarksData data) : base(data)
        {
        }

        [AllowAnonymous]
        public ActionResult Index(int? page)
        {
            var bookmarks = this.Data.Bookmarks
                .All()
                .OrderBy(x => x.Title)
                .Project()
                .To<BookmarkViewModel>()
                .ToPagedList(page ?? GlobalConstants.DefaultStartPage, GlobalConstants.DefaultPageSize);

            return this.View(bookmarks);
        }


        public ActionResult Details(int id)
        {
            var bookmark = this.Data.Bookmarks
                .All()
                .Include(x => x.Votes)
                .FirstOrDefault(x => x.Id == id);

            var bookmarkViewModel = Mapper.Map<BookmarkDetailsViewModel>(bookmark);

            var userId = this.User.Identity.GetUserId();
            bookmarkViewModel.UserHasVoted = bookmark.Votes.Any(x => x.UserId == userId);

            return this.View(bookmarkViewModel);


            //var bookmark = this.Data.Bookmarks
            //    .All()
            //    .Where(x => x.Id == id)
            //    .Project()
            //    .To<BookmarkDetailsViewModel>()
            //    .FirstOrDefault();
            //return View(bookmark);


        }

        public ActionResult Create()
        {
            this.LoadCategories();

            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BookmarkInputModel model)
        {
            if (model != null && this.ModelState.IsValid)
            {
                var bookmark = Mapper.Map<Bookmark>(model);
                this.Data.Bookmarks.Add(bookmark);
                this.Data.SaveChanges();

                return this.RedirectToAction(x => x.Details(bookmark.Id));
                // return this.RedirectToAction("Details", new { id = bookmark.Id });
            }

            this.LoadCategories();
            return this.View(model);
        }

        private void LoadCategories()
        {
            this.ViewBag.Categories = this.Data.Categories
                .All()
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name,
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComment(CommentInputModel model)
        {
            if (model != null && this.ModelState.IsValid)
            {
                model.UserId = this.User.Identity.GetUserId();
                var comment = Mapper.Map<Comment>(model);
                this.Data.Comments.Add(comment);
                this.Data.SaveChanges();

                var commentDb = this.Data.Comments
                    .All()
                    .Where(x => x.Id == comment.Id)
                    .Project()
                    .To<CommentViewModel>()
                    .FirstOrDefault();

                return this.PartialView("DisplayTemplates/CommentViewModel", commentDb);
            }

            return this.Json("Error");
        }

        public ActionResult DeleteComment(int commentId)
        {
            var comment = this.Data.Comments
                .All()
                .FirstOrDefault(x => x.Id == commentId);

            if (comment != null && comment.UserId == this.User.Identity.GetUserId())
            {
                this.Data.Comments.Remove(comment);
                this.Data.SaveChanges();

                return this.Content(string.Empty);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Cannot delete comment");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Vote(int bookmarkId)
        {
            var bookmark = this.Data.Bookmarks
                .All()
                .FirstOrDefault(x => x.Id == bookmarkId);

            if (bookmark != null)
            {
                var userHasVoted = bookmark.Votes.Any(x => x.UserId == this.User.Identity.GetUserId());
                if (!userHasVoted)
                {
                    this.Data.Votes.Add(new Vote
                    {
                        BookmarkId = bookmarkId,
                        UserId = this.User.Identity.GetUserId(),
                        Value = 1
                    });
                    this.Data.SaveChanges();
                }

                var votesCout = bookmark.Votes.Sum(x => x.Value);
                return this.Content(votesCout.ToString());
            }

            return new EmptyResult();
        }
    }
}