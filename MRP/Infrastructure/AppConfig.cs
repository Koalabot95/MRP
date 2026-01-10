using Microsoft.Extensions.Configuration;

namespace FHTW.Swen1.Forum.System;

public static class AppConfig
{
    public static string PostgresConnectionString { get; set; } = "";

    //Für lokale Datenspeicherung false stellen
    public static bool UsePostgres { get; set; } = true;

    //lädt beim Programmstart die appsettings.json und stellt sie der App zur Verfügung
    static AppConfig()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        PostgresConnectionString = config.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing connection string 'Postgres'.");
    }

   
}

