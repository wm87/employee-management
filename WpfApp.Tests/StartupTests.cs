using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace WpfApp.Tests
{
    public class StartupTests
    {
        private static string FindRepoRoot()
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "Project.sln")))
                    return dir.FullName;
                dir = dir.Parent;
            }

            throw new InvalidOperationException("Repository root (Project.sln) not found from current directory.");
        }

        [Fact]
        public void AppSettings_DefaultConnection_IsPresent()
        {
            var repoRoot = FindRepoRoot();
            var basePath = Path.Combine(repoRoot, "WpfApp");

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            var conn = config.GetConnectionString("DefaultConnection");
            Assert.False(string.IsNullOrWhiteSpace(conn));
        }

        [Fact]
        public void AppSettings_SerilogSection_Exists()
        {
            var repoRoot = FindRepoRoot();
            var basePath = Path.Combine(repoRoot, "WpfApp");

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            var serilogSection = config.GetSection("Serilog");
            Assert.True(serilogSection.Exists());
            var writeTo = serilogSection.GetSection("WriteTo");
            Assert.True(writeTo.Exists());
        }
    }
}
