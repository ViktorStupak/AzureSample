using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureSample.Context.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace AzureSample.Context.Infrastructure
{
    /// <summary>
    /// Connect to database according to the <see cref="DataContext"/>.
    /// </summary>
    /// <seealso cref="IRepository" />
    public class Repository : IRepository
    {
        private readonly DataContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public Repository(DataContext context)
        {
            this._context = context;
            Log.Debug("create repository");
        }

        /// <summary>
        /// Get the all Person.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Person> GetPersons()
        {
            return _context.Persons.AsNoTracking();
        }
    }
}