using Microsoft.EntityFrameworkCore;

namespace CSharp_Web.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<Data> Datas { get; set; }
    }
}
