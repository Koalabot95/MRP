namespace FHTW.Swen1.Forum.System;

/// <summary>This class provides a base implementation for data objects.</summary>
public abstract class Atom : IAtom
{
    protected Session? _EditingSession = null;


    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // protected methods                                                                                                //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // Validiert die übergebene oder bereits gesetzte Session.
    // Wenn eine Session als Parameter übergeben wird, wird sie als aktuelle Editing-Session gespeichert.
    // Falls keine Session vorhanden ist oder sie ungültig ist, wird eine UnauthorizedAccessException geworfen.
    protected void _VerifySession(Session? session = null)
    {
        if (session is not null) { _EditingSession = session; }
        if (_EditingSession is null || !_EditingSession.Valid) { throw new UnauthorizedAccessException("Invalid session."); }
    }


    // Beendet den aktuellen Bearbeitungsvorgang, indem die Editing-Session auf null gesetzt wird
    protected void _EndEdit()
    {
        _EditingSession = null;
    }


    // Stellt sicher, dass die aktuelle Session gültig ist und der Benutzer Administratorrechte besitzt
    // Wird keine gültige Admin-Session gefunden, wird eine UnauthorizedAccessException ausgelöst
    protected void _EnsureAdmin()
    {
        _VerifySession();
        if (!_EditingSession!.IsAdmin) { throw new UnauthorizedAccessException("Admin privileges required."); }
    }


    // Stellt sicher, dass die Session gültig ist und der Benutzer entweder Administrator ist
    // oder Eigentümer des betreffenden Objekts (z.B. MediaEntry)
    // Wenn weder Admin noch Owner, wird eine UnauthorizedAccessException ausgelöst
    protected void _EnsureAdminOrOwner(string owner)
    {
        _VerifySession();
        if (!(_EditingSession!.IsAdmin || (_EditingSession.UserName == owner)))
        {
            throw new UnauthorizedAccessException("Admin or owner privileges required.");
        }
    }



    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // [interface] IAtom                                                                                                //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Begins editing the object.</summary>
    /// <param name="session">Session.</param>
    public virtual void BeginEdit(Session session)
    {
        _VerifySession(session);
    }


    /// <summary>Saves the object.</summary>
    public abstract void Save();


    /// <summary>Deletes the object.</summary>
    public abstract void Delete();


    /// <summary>Refreshes the object.</summary>
    public abstract void Refresh();
}