using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GameLibrary;

namespace Repository.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<int> AddAsync(T item);
        Task<int> UpdateAsync(T item);
        Task<int> DeleteAsync(int id);
        T GetItem(int id);
        T Include(T item, Type joinedType);
    }

}
