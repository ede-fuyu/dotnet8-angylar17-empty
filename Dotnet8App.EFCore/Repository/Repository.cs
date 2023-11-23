using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dotnet8App.EFCore
{
    public class Repository<T>(ApplicationDbContext context) : IRepository<T> where T : class
    {
        public IUnitOfWork UnitOfWork { get; set; } = new UnitOfWork(context);

        private DbSet<T>? _objectset;

        private DbSet<T> ObjectSet
        {
            get
            {
                return _objectset ??= UnitOfWork.Context.Set<T>();
            }
        }

        public virtual IDbContextTransaction BeginTransaction()
        {
            return UnitOfWork.BeginTransaction();
        }

        public virtual void Commit()
        {
            UnitOfWork.Commit();
        }

        public virtual void RollBack()
        {
            UnitOfWork.RollBack();
        }

        public virtual IQueryable<T> GetAll()
        {
            return ObjectSet.AsQueryable();
        }

        public virtual void Insert(T entity)
        {
            ObjectSet.Add(entity);
        }

        public virtual void Update(T entity)
        {
            UnitOfWork.Context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Delete(T entity)
        {
            ObjectSet.Remove(entity);
        }
    }


    public interface IRepository<T>
    {
        IUnitOfWork UnitOfWork { get; set; }

        IDbContextTransaction BeginTransaction();

        void Commit();

        void RollBack();

        IQueryable<T> GetAll();

        void Insert(T entity);

        void Update(T entity);

        void Delete(T entity);
    }
}