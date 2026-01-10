using Npgsql;

namespace FHTW.Swen1.Forum.System;

//Erstellt mit dem in AppConfig geladenen Connection-String eine neue NpsqlConnection, öffnet sie und gibt sie zurück
public static class DbConnection
{
    public static NpgsqlConnection CreateAndOpen()
    {
        var c = new NpgsqlConnection(AppConfig.PostgresConnectionString);
        c.Open();
        return c;
    }

}
