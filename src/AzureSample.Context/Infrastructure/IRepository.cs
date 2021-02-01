using System.Collections.Generic;
using System.Threading.Tasks;
using AzureSample.Context.Models;

namespace AzureSample.Context.Infrastructure
{
    /// <summary>
    /// Base functionality of data get.
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Get the all Person.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Person> GetPersons();


    }
}