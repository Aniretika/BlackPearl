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
        private SqlConnection Context { get; set; }
        private SqlTransaction Transaction { get; set; } 
        private bool _disposedValue = false; // To detect redundant calls  

        public Dictionary<Type, object> Repositories;
       

        public UnitOfWork(string connectionString)
        {
            using (Context = new SqlConnection(connectionString))
            {
                try
                {
                    Context.Open();
                    Repositories = new Dictionary<Type, object>();
                    Console.WriteLine("Connection was executed");
                    Transaction = Context.BeginTransaction();
                }
               catch
                {
                    Console.WriteLine("Connection wasn't executed");
                }
               
            }
               
        }
        

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;

            if (disposing)
            {
                //dispose managed state (managed objects).  
            }

            // free unmanaged resources (unmanaged objects) and override a finalizer below.  
            // set large fields to null.  

            _disposedValue = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            if (Repositories.Keys.Contains(typeof(TEntity)))
                return Repositories[typeof(TEntity)] as IRepository<TEntity>;

            var repository = new ReposiroryPattern<TEntity>(Context, Transaction);
            Repositories.Add(typeof(TEntity), repository);
            return repository;
        }
      
    }
}
