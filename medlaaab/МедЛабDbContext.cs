using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace medlaaab
{
    public class МедЛабDbContext : DbContext
    {
        public МедЛабDbContext() : base("name=МедЛабConnectionString")
        {

        }

        public DbSet<Пользователи> Пользователи { get; set; }
        public DbSet<Роли> Роли { get; set; }
        public DbSet<Пациенты> Пациенты { get; set; }
        public DbSet<Заказы> Заказы { get; set; }
        public DbSet<Услуги> Услуги { get; set; }
        public DbSet<УслугиВЗаказе> УслугиВЗаказе { get; set; }
        public DbSet<Анализаторы> Анализаторы { get; set; }
        public DbSet<СтраховыеКомпании> СтраховыеКомпании { get; set; }
        public DbSet<ИсторияВходов> ИсторияВходов { get; set; }
        public DbSet<Блокировки> Блокировки { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Конфигурации моделей
            modelBuilder.Entity<Пользователи>().ToTable("Пользователи");
            modelBuilder.Entity<Роли>().ToTable("Роли");
            modelBuilder.Entity<Пациенты>().ToTable("Пациенты");
            modelBuilder.Entity<Заказы>().ToTable("Заказы");
            modelBuilder.Entity<Услуги>().ToTable("Услуги");
            modelBuilder.Entity<УслугиВЗаказе>().ToTable("УслугиВЗаказе");
            modelBuilder.Entity<Анализаторы>().ToTable("Анализаторы");
            modelBuilder.Entity<СтраховыеКомпании>().ToTable("СтраховыеКомпании");
            modelBuilder.Entity<ИсторияВходов>().ToTable("ИсторияВходов");
            modelBuilder.Entity<Блокировки>().ToTable("Блокировки");
        }
    }
}
