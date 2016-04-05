﻿namespace Bookmarks.Web.Controllers
{
    using System.Web.Mvc;
    using Data;

    public abstract class BaseController : Controller
    {
        protected BaseController(IBookmarksData data)
        {
            this.Data = data;
        }

        public IBookmarksData Data { get; private set; }
    }
}