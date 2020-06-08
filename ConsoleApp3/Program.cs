using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using Z.EntityFramework.Plus;

namespace includefilterfail
{
    class Program
    {
        static void Main(string[] args)
        {
            var random = new Random();
            using (var db = new SomeDbContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var classA = new ClassA { SomeProp = random.Next() };
                classA.ClassBs = new List<ClassB>();
                for (int i = 1; i < 2000; i++)
                {
                    classA.ClassBs.Add(new ClassB { SomeProp = random.Next() });
                }
                db.ClassAs.Add(classA);
                db.SaveChanges();
            }

            using (var db = new SomeDbContext())
            {
                var classA = db.ClassAs.
                     IncludeFilter(x => x.ClassBs.Where(y => y.SomeProp < (int.MaxValue - 1000)).OrderBy(y => y.Id).Take(5))
                     .Single();
            }

            /* This query get executed: 
             * 
             *
-- EF+ Query Future: 1 of 2
SELECT TOP(2) [c].[Id], [c].[SomeProp]
FROM [ClassAs] AS [c]
;

-- EF+ Query Future: 2 of 2
SELECT [x].[Id], [x].[SomeProp], [x.ClassBs].[Id], [x.ClassBs].[ClassAId], [x.ClassBs].[SomeProp]
FROM [ClassAs] AS [x]
LEFT JOIN [ClassBs] AS [x.ClassBs] ON [x].[Id] = [x.ClassBs].[ClassAId]
ORDER BY [x].[Id]
;
             * */

            Console.WriteLine("Hello World!");
            Console.Read();
        }
    }

    public class SomeDbContext : DbContext
    {
        public DbSet<ClassA> ClassAs { get; set; }
        public DbSet<ClassB> ClassBs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.\\SQLExpress;Initial Catalog=includefilterfail;Persist Security Info=True;Integrated Security=true;MultipleActiveResultSets=True;Application Name=EntityFramework");
        }
    }

    public class ClassA
    {
        public int Id { get; set; }
        public int SomeProp { get; set; }
        public virtual List<ClassB> ClassBs { get; set; }
    }

    public class ClassB
    {
        public int Id { get; set; }
        public int SomeProp { get; set; }
    }

}
