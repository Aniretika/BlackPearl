using GameLibrary;
using Repository.Interfaces;
using System;
using System.Collections.Generic;

namespace UnitOfWorkRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> GetRepository<T>() where T : IEntityBase;
    }
}
