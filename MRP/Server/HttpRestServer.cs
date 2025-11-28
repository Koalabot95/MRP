using System.Net; //Enthält Klassen wie den HttpListener
namespace FHTW.Swen1.Forum.Server;


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// private members                                                                                                  //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/// <summary>HTTP listener object.</summary>
public sealed class HttpRestServer : IDisposable //Server: Interface
{
    public HttpListener _Listener;

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // constructors                                                                                                     //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Creates a new instance of this class.</summary>
    /// <param "name="port">Port number for the server.</param>
    public HttpRestServer(int port = 12000)
    {
        _Listener = new();
        _Listener.Prefixes.Add($"http://+:{port}/");
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // public events                                                                                                    //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>The event is raised when a request has been received.</summary>
    public event EventHandler<HttpRestEventArgs>? RequestReceived;

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // public properties                                                                                                //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Gets a value indicating if the server is running.</summary>
    public bool Running
    {
        get;
        private set;
    }


    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // public methods                                                                                                   //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Starts an runs the server.</summary>
    public void Stop()
    {
        _Listener.Close();
        Running = false;
    }

    public void Run()
    {
        if (Running) return;

        _Listener.Start();
        Running = true;

        while (Running)
        {
            HttpListenerContext context = _Listener.GetContext(); //Wartet auf Anfrage
            _ = Task.Run(() => //Anfrage wird in neuem Task behandelt, dadurch blockiert eine langsame Anfrage nicht gleich alles. _ = discard variable. (ignoriert den Wert)
            {
                HttpRestEventArgs args = new(context); // Anfrage-Infos in eigenes Objekt
                RequestReceived?.Invoke(this, args); //Event auslösen

                if(!args.Responded) // wenn kein Handler geantwortet hat, 404
                {
                    args.Respond(HttpStatusCode.NotFound, new() {
                        ["success"] = false,
                        ["reason"] = "Not found."
                    });
                }
            });
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // [interface] IDisposable                                                                                          //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Disposes the object and releases used resources.</summary>
    /// <exception " cref="NotImplementedException"></exception>
    public void Dispose()
    {
        ((IDisposable)_Listener).Dispose(); // Nimmt das Objekt _Listener, behandelt es als Typ IDisposable,und ruft darauf Dispose() auf
    }                                       //macht man um Ressourcen aufzuräumen (z. B. Netzwerkverbindungen, Dateien, Speicher).
}
