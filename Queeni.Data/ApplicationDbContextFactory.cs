using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Queeni.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            var path = GetFullDatabasePath();

            optionsBuilder.UseSqlite($"Data Source={path}");

            return new ApplicationDbContext(optionsBuilder.Options);
        }

        public static string GetFullDatabasePath()
        {
            return Path.Combine(GetDatabaseDirectoryPath(), GetDatabaseFileName());
        }

        public static string GetWorkingDatabasePath()
        {
            return Path.Combine(ApplicationDbContextFactory.GetDatabaseDirectoryPath(), $"{ShortGuid.NewGuid()}-{QueeniConfigManager.DefaultDbFileName}");
        }

        public static string GetDatabaseDirectoryPath()
        {
            string databasePath;

#if ANDROID
            // Android specific
            databasePath = FileSystem.AppDataDirectory, databaseName);
#else
            // Default fallback (e.g. Windows design-time and WinUI)
            databasePath = QueeniConfigManager.AppPath;
#endif
            if (!Directory.Exists(databasePath))
                Directory.CreateDirectory(databasePath);

            return databasePath;
        }

        public static string GetDatabaseFileName()
        {
            var dir = GetDatabaseDirectoryPath();

            // Вземаме всички .db файлове с префикс и оригиналното име
            var files = Directory.GetFiles(dir, $"*-{QueeniConfigManager.DefaultDbFileName}");

            // Ако има такива, взимаме последния (или първия, зависи от логиката ти)
            if (files.Length > 0)
            {
                // Може да направим и сортиране по дата ако искаш
                var result = Path.GetFileName(files.OrderByDescending(f => File.GetLastWriteTime(f)).First());
                return result;
            }

            // Ако няма нито един такъв файл – връщаме оригиналното име
            return QueeniConfigManager.DefaultDbFileName;
        }

    }
}
