using MRP.System;
using System.Security.Cryptography;
using System.Text;



namespace FHTW.Swen1.Forum.System;

//bekommt alle wichtigen methoden aus Atom
public sealed class User : Atom, IAtom
{
    private string? _UserName = null;

    private bool _New;

    private string? _PasswordHash = null;



    public User(Session? session = null)
    {
        _EditingSession = session;
        _New = true; //user = neu
    }

    //holt user aus repo
    public static User Get(string userName, Session? session = null)
    {
        var user = Repositories.Users.Get(userName);
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        user.BeginEdit(session ?? throw new UnauthorizedAccessException()); //prüft session und ownership
        user._New = false;   //user = bestehend
        return user;
    }


    //returned username wenn vorhanden, wenn username nicht new kann er nicht gewechselt werden
    public string UserName
    {
        get { return _UserName ?? string.Empty; }
        set
        {
            if (!_New) { throw new InvalidOperationException("User name cannot be changed."); }
            if (string.IsNullOrWhiteSpace(value)) { throw new ArgumentException("User name must not be empty."); }

            _UserName = value;
        }
    }

    //username und passwort werden gehasht
    internal static string _HashPassword(string userName, string password)
    {
        StringBuilder rval = new();
        foreach (byte i in SHA256.HashData(Encoding.UTF8.GetBytes(userName + password)))
        {
            rval.Append(i.ToString("x2"));
        }
        return rval.ToString();
    }

    //kann gelesen und gesetzt werden, anfangswert = leerer string. bei fullname ==null -> NullReferenceException
    public string FullName
    {
        get; set;
    } = string.Empty;


    public string EMail
    {
        get; set;
    } = string.Empty;


    public void SetPassword(string password)
    {
        _PasswordHash = _HashPassword(UserName, password);
    }

    //checkt das passwort bzw den hash vom login
    public bool VerifyPassword(string password)
    {
        if (string.IsNullOrEmpty(_PasswordHash))
            return false;

        string check = _HashPassword(UserName, password);
        return _PasswordHash == check;
    }


    public override void Save()
    {
        if (_New)
        {
            // CREATE
            if (string.IsNullOrWhiteSpace(UserName))
                throw new InvalidOperationException("Username empty.");

            Repositories.Users.Add(this); 
            _New = false;
        }
        else
        {
            // UPDATE → nur Owner oder Admin
            _EnsureAdminOrOwner(UserName);

            Repositories.Users.Update(this); 
        }

       // _PasswordHash = null;
        _EndEdit();
    }


    public override void Delete()
    {
        _EnsureAdminOrOwner(UserName);

        if (!Repositories.Users.Delete(UserName))
            throw new InvalidOperationException("User does not exist.");

        _EndEdit();
    }

    public override void Refresh()
    {
        // user aus repo holen
        var existing = Repositories.Users.Get(UserName)
                       ?? throw new InvalidOperationException("User not found.");

        FullName = existing.FullName;
        EMail = existing.EMail;
        _EndEdit();
    }
}                                                                           