using FHTW.Swen1.Forum.Server;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.ConstrainedExecution;

namespace FHTW.Swen1.Forum.Handlers;

public abstract class Handler : IHandler
{
    private static List<IHandler>? _Handlers = null;

    /// stellt liste von allen handlern zusammen und gibt sie zurück
    private static List<IHandler> _GetHandlers()
    {
        List<IHandler> rval = new();

        ///durchsuche alle Klassen und filtere jene heraus, die das Interface IHandler implementieren 
        ///und nicht abstrakt sind -> Reflektion
        ///.Where ist ein Filter - behaltet nur jene Handler für die die Bedingung wahr ist, m = der aktuelle Typ (zB UserHandler, MediaHandler,..)
        foreach (Type i in Assembly.GetExecutingAssembly().GetTypes()
            .Where(m => m.IsAssignableTo(typeof(IHandler)) && !m.IsAbstract))
        {
            ///erstellt automatisch ein Objekt der jeweiligen Klasse. also wenn h nicht null ist, ist zB h = new Userhandler().
            IHandler? h = (IHandler?)Activator.CreateInstance(i);
            if (h is not null) { rval.Add(h); }
        }

        return rval;
    }

    //Initialisiert bei Bedarf die Handler-Liste und ruft sie der Reihe nach auf, bis ein Handler die Anfrage verarbeitet hat
    public static void HandleEvent(object? sender, HttpRestEventArgs e)
    {

        ///Wenn _Handlers noch leer ist, wird _GetHandlers() aufgerufen, ?? = wenn null, dann initialisieren
        ///sobald ein Handler zuständig ist und responded, wird abgebrochen
        foreach (IHandler i in (_Handlers ??= _GetHandlers()))
        {
            i.Handle(e);
            if (e.Responded) break;
        }
    }


    public abstract void Handle(HttpRestEventArgs e);
}