using Microsoft.Extensions.Configuration;

namespace AMMS.Infrastructure.Persistence
{

    public static class DesignTimeDbConnection
    {
        public static string Resolve()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            foreach (var basePath in GetCandidateBasePaths())
            {
                if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
                {
                    continue;
                }

                var configuration = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile($"appsettings.{environment}.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    return connectionString;
                }
            }

            var environmentConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            if (!string.IsNullOrWhiteSpace(environmentConnection))
            {
                return environmentConnection;
            }

            throw new InvalidOperationException(
                "Design-time connection string bulunamadı. " +
                "AMMS.Api appsettings içinde ConnectionStrings:DefaultConnection tanımlayın, " +
                "dotnet ef komutlarında --startup-project Hosts/AMMS.Api/AMMS.Api kullanın " +
                "veya ConnectionStrings__DefaultConnection ortam değişkenini ayarlayın.");
        }

        private static IEnumerable<string> GetCandidateBasePaths()
        {
            var current = Directory.GetCurrentDirectory();
            yield return current;

            var directory = new DirectoryInfo(current);
            while (directory is not null)
            {
                var mainApiPath = Path.Combine(directory.FullName, "Hosts", "AMMS.Api", "AMMS.Api");
                if (Directory.Exists(mainApiPath))
                {
                    yield return mainApiPath;
                }

                directory = directory.Parent;
            }
        }
    }



}
