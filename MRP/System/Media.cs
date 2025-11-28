using FHTW.Swen1.Forum.System;
using MRP.System;

namespace FHTW.Swen1.Forum.System;

public enum MediaType
{
    Movie,
    Series,
    Game
}

public sealed class Media : Atom, IAtom
{
    private bool _New;

    // Konstruktor setzt optional die aktuelle Editing-Session.
    // Wird eine Session übergeben, kann der Benutzer anschließend
    // diesen Media-Eintrag bearbeiten oder erstellen, sofern er berechtigt ist
    public Media(Session? session = null)
    {
        _EditingSession = session;
        _New = true;
    }

    // Eigenschaften eines Media-Eintrags
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MediaType Type { get; set; }
    public int ReleaseYear { get; set; }
    public string Genres { get; set; } = string.Empty;
    public int AgeRestriction { get; set; }
    public string OwnerUserName { get; set; } = string.Empty;

    // Speichert den Media-Eintrag.
    // Bei neuen Einträgen wird ohne Berechtigungsprüfung gespeichert.
    // Bei bestehenden Einträgen wird geprüft, ob der Benutzer Admin oder Owner ist.
    // Nach dem Speichern wird der Bearbeitungsvorgang beendet.

    public override void Save()
    {
        if (!_New)
        {
            _EnsureAdminOrOwner(OwnerUserName);
        }

        Repositories.Media.Add(this);
        _New = false;
        _EndEdit(); //aus Atom
    }

    // Löscht den Media-Eintrag. Nur Admin oder Owner dürfen löschen.
    // Delegiert das eigentliche Löschen an das Repository.
    public override void Delete()
    {
        _EnsureAdminOrOwner(OwnerUserName);
        Repositories.Media.Delete(Id);
        _EndEdit();
    }

    // Aktualisiert die lokalen Property-Werte. keine Berechtigungsprüfung da
    // Kein Endpoint darauf zugreifen kann 
    // _EndEdit() damit Bearbeitungsvorgang beendet wird
    // und keine aktive Editing-Session fälschlicherweise bestehen bleibt.
    public override void Refresh()
    {
        var existing = Repositories.Media.Get(Id)
                      ?? throw new InvalidOperationException("Media not found.");

        Title = existing.Title;
        Description = existing.Description;
        Type = existing.Type;
        ReleaseYear = existing.ReleaseYear;
        Genres = existing.Genres;
        AgeRestriction = existing.AgeRestriction;
        OwnerUserName = existing.OwnerUserName;

        _EndEdit();
    }
}
