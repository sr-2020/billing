﻿using Core.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Model
{
    [Table("hangfire_job")]
    public class HangfireJob : BaseEntity
    {
        [Column("start_time")]
        public DateTime StartTime { get; set; }
        [Column("end_time")]
        public DateTime EndTime { get; set; }
        [Column("cron")]
        public string Cron { get; set; }
        [Column("job_name")]
        public string JobName { get; set; }
        [ForeignKey("job_type")]
        public int JobTypeId { get; set; }
        [ForeignKey("job_type")]
        public virtual HangFireJobType JobType { get; set; }
        [Column("hangfire_startid")]
        public string HangfireStartId { get; set; }
        [Column("hangfire_reccurentid")]
        public string HangfireReccurentId { get; set; }
        [Column("hangfire_endid")]
        public string HangfireEndId { get; set; }
    }
}