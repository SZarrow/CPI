using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CPI.Common.Models;
using CPI.Config;
using Lotus.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CPI.Data.PostgreSQL
{
    public class CPIDbContext : DbContext
    {
        private String _connectionString;

        public CPIDbContext() : this(null) { }

        public CPIDbContext(String connectionString)
        {
            if (String.IsNullOrWhiteSpace(connectionString))
            {
                _connectionString = DbConfig.PgSQLDbConnectionString;

                if (String.IsNullOrWhiteSpace(_connectionString))
                {
                    throw new ArgumentNullException(nameof(connectionString));
                }
            }
            else
            {
                _connectionString = connectionString;
            }

            this.Id = Guid.NewGuid();
        }

        public Guid Id { get; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connectionString);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SysApp>();
            modelBuilder.Entity<PayOrder>();
            modelBuilder.Entity<PayChannel>();
            modelBuilder.Entity<AgreePayBankCardInfo>();
            modelBuilder.Entity<AgreePayBankCardBindInfo>();
            modelBuilder.Entity<BankBaseInfo>();
            modelBuilder.Entity<BankCardBin>();
            modelBuilder.Entity<AppChannelRoute>();
            modelBuilder.Entity<FundOutOrder>();
            modelBuilder.Entity<WithdrawBankCardBindInfo>();
            modelBuilder.Entity<AllotAmountOrder>();
            modelBuilder.Entity<AllotAmountWithdrawOrder>();
            modelBuilder.Entity<PersonalSubAccount>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
