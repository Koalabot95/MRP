namespace FHTW.Swen1.Forum.System;

/// <summary>This class represents a session.</summary>
public sealed class Session
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // private constants                                                                                                //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Alphabet.</summary>
    private const string _ALPHABET = "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// Token lifetime
    private const int TIMEOUT_MINUTES = 30;



    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // private static members                                                                                           //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Sessions.</summary>
    
    //session wird hier gespeichert
    private static readonly Dictionary<string, Session> _Sessions = new();



    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // constructors                                                                                                     //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Creates a new instance of a class.</summary>
    /// <param name="userName">User name.</param>
    /// <param name="password">Password.</param>
    
    //beim login -> neue session wird erzeugt
    private Session(string userName)
    {
        UserName = userName;
        IsAdmin = (userName == "admin");
        Timestamp = DateTime.UtcNow;

        //Token generieren
        Token = string.Empty;
        Random rnd = new();
        for (int i = 0; i < 24; i++) { Token += _ALPHABET[rnd.Next(0, 62)]; }
    }



    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //properties                                                                                                //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Gets the session token.</summary>
    public string Token { get; }


    /// <summary>Gets the user name of the session owner.</summary>
    public string UserName { get; }


    /// <summary>Gets the session timestamp.</summary>
    public DateTime Timestamp
    {
        get; private set;
    }


    /// <summary>Gets if the session is valid.</summary>
    
    //session ist solange gültig, solang der token gespeichert ist
    public bool Valid
    {
        get { return _Sessions.ContainsKey(Token); }
    }


    /// <summary>Gets a value indicating if the session owner has administrative privileges.</summary>
    public bool IsAdmin { get; }



    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // public static methods                                                                                            //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Creates a new session.</summary>
    /// <param name="userName">User name.</param>
    /// <param name="password">Password.</param>
    /// <returns>Returns a session instance, or NULL if user couldn't be logged in.</returns>
    public static Session Create(string userName)
    {
        Session session = new Session(userName);

        lock (_Sessions)
        {
            _Sessions[session.Token] = session; // speichert token dictionary
        }

        return session;
    }


    /// <summary>Gets a session by its token.</summary>
    /// <param name="token">Session token.</param>
    /// <returns>Returns the session represented by the token, or NULL if there is no session for the token.</returns>
    public static Session? Get(string token)
    {
        Session? rval = null;

        _Cleanup(); //alle abgelaufenene sesssions werden gelöscht

        lock (_Sessions)
        {
            if (_Sessions.ContainsKey(token)) //session noch im dictionary?
            {
                rval = _Sessions[token];
                rval.Timestamp = DateTime.UtcNow;
            }
        }

        return rval;
    }

    /// <summary>Closes all outdated sessions.</summary>
    private static void _Cleanup()
    {
        List<string> toRemove = new();

        lock (_Sessions)
        {
            foreach (KeyValuePair<string, Session> pair in _Sessions)
            {
                if ((DateTime.UtcNow - pair.Value.Timestamp).TotalMinutes > TIMEOUT_MINUTES) { toRemove.Add(pair.Key); }
            }
            foreach (string key in toRemove) { _Sessions.Remove(key); }
        }
    }

    /// <summary>Closes the session.</summary>
    
    //session vorzeitig beenden
    public void Close()
    {
        lock (_Sessions)
        {
            if (_Sessions.ContainsKey(Token)) { _Sessions.Remove(Token); }
        }
    }
}