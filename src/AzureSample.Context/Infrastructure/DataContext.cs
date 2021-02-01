using AzureSample.Context.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace AzureSample.Context.Infrastructure
{
    /// <summary>
    /// Context of the DB.
    /// </summary>
    /// <seealso cref="DbContext" />
    public sealed class DataContext : DbContext
    {
        private readonly string _connectionString;

        public DataContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           if (!string.IsNullOrWhiteSpace(_connectionString)) optionsBuilder.UseSqlServer(_connectionString);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DataContext"/> class.
        /// </summary>
        /// <param name="opts">The opts.</param>
        public DataContext(DbContextOptions<DataContext> opts) : base(opts)
        {

            //this.Database.EnsureDeleted();
            this.Database.EnsureCreated();
            Log.Debug("create DataContext ");
        }


        /// <summary>
        /// Get or set the products.
        /// </summary>
        /// <value>
        /// The products.
        /// </value>
        public DbSet<Person> Persons { get; set; }
    }
}