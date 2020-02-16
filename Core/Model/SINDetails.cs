﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Model
{
    [Table("sin_details")]
    public class SINDetails : BaseEntity
    {
        [Column("wallet")]
        public int WalletId { get; set; }
        [ForeignKey("wallet")]
        public virtual Wallet Wallet { get; set; }
        [Column("sin")]
        public int SINId { get; set; }
        [ForeignKey("sin")]
        public virtual SIN SIN { get; set; }
        [Column("scoring")]
        public int? Scoring { get; set; }
        [Column("work")]
        public int? Work { get; set; }
        [Column("ikar")]
        public int? IKAR { get; set; }
    }
}
