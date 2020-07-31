using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Roslyn.Interfaces
{
    public interface ICrudServices<T>
    {
        Task<IEnumerable<T>> GetAsync();
        Task<IEnumerable<T>> GetAsync(T t);
        Task<T> GetAsync(int id);
        Task<T> PostAsync(T t);
        Task<T> PutAsync(T t);
        Task<bool> DeleteAsync(int id);
    }
}
