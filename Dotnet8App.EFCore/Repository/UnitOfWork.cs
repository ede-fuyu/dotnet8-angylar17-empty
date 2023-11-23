using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dotnet8App.EFCore
{
    public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
    {
        public DbContext Context { get; set; } = context;

        public virtual IDbContextTransaction BeginTransaction()
        {
            return Context.Database.BeginTransaction();
        }

        public void Commit()
        {
            Context.SaveChanges();
        }

        public void RollBack()
        {
            foreach (var entity in Context.ChangeTracker.Entries())
            {
                switch (entity.State)
                {
                    case EntityState.Added:
                        entity.State = EntityState.Detached;
                        break;
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        entity.State = EntityState.Unchanged;
                        break;
                }
            }
        }
    }

    public interface IUnitOfWork
    {
        DbContext Context { get; set; }

        IDbContextTransaction BeginTransaction();

        void Commit();

        void RollBack();
    }
}