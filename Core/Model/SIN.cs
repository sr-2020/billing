﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Model
{
    [Table("sin")]
    public class SIN : BaseEntity
    {
        [Column("character")]
        public int CharacterId { get; set; }
        public Character Character { get; set; }
        [Column("sin_text")]
        public string Sin { get; set; }
        [Column("person_name")]
        public string PersonName { get; set; }
        [Column("metatype")]
        [ForeignKey("metatype")]
        public int? MetatypeId { get; set; }
        public Metatype Metatype { get; set; }
        [Column("citizenship")]
        public int? Citizenship { get; set; }
        [ForeignKey("wallet")]
        [Column("wallet")]
        public int WalletId { get; set; }
        public virtual Wallet Wallet { get; set; }
        [Column("scoring")]
        public int ScoringId { get; set; }
        public Scoring Scoring { get; set; }
        [Column("work")]
        public int? Work { get; set; }
        [Column("ikar")]
        public int? IKAR { get; set; }
        [Column("eversion")]
        public string EVersion { get; set; }
        [Column("last_income")]
        public decimal LastIncome { get; set; }
        [Column("last_outcome")]
        public decimal LastOutcome { get; set; }

    }
}
