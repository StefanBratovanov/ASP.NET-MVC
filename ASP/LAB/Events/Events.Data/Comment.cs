﻿
using System;
using System.ComponentModel.DataAnnotations;

namespace Events.Data
{
    public class Comment
    {
        public Comment()
        {
            this.Date = DateTime.Now;
        }
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string AuthorId { get; set; }

        public virtual ApplicationUser Author { get; set; }

        public string EventId { get; set; }

        [Required]
        public virtual Event Event { get; set; }
    }
}
