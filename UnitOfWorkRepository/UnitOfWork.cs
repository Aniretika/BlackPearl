using GameLibrary;
using Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace UnitOfWorkRepository
{

    public class UnitOfWork: IUnitOfWork
    {
        public SqlConnection Context { get; set; }
       // public SqlTransaction Transaction { get; set; } 
        private bool _disposedValue = false;

        public Dictionary<Type, object> Repositories;
       

        public UnitOfWork(string connectionString)
        {
            Context = new SqlConnection(connectionString);
            try
            {
                Context.Open();
                //Transaction = Context.BeginTransaction();
                Repositories = new Dictionary<Type, object>();
                
                Console.WriteLine("Connection was executed");
            }
            catch
            {
                Console.WriteLine("Connection wasn't executed");
            }
               
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;

            if (disposing)
            {
                Context.Close();
            }

            _disposedValue = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : IEntityBase
        {
            if (Repositories.Keys.Contains(typeof(TEntity)))
                return Repositories[typeof(TEntity)] as IRepository<TEntity>;

            var repository = new ReposiroryPattern<TEntity>(this.Context/*, Transaction*/);
            Repositories.Add(typeof(TEntity), repository);
            return repository;
        }
      
    }
}
