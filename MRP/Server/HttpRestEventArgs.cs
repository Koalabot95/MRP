using System.Net; //für HttpListenerContext
using System.Text; //für Textcodierung (Encoding.UTF8)
using System.Text.Json.Nodes; //für JSON-Objekte (JsonObject, JsonNode)

namespace FHTW.Swen1.Forum.Server;

/// <summary>This class defines event arguments for the <see "cref="HttpRestServer.RequestReceived"/> event.</summary>
public class HttpRestEventArgs : EventArgs //EventArgs bedeutet: Die Klasse kann in einem Event verwendet werden
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // constructors                                                                                                     //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Creates a new instance of this class.</summary>
    public HttpRestEventArgs(HttpListenerContext context)
    {
        Context = context;
        Method = HttpMethod.Parse(context.Request.HttpMethod); //Liest aus, ob es ein GET, POST, PUT, … ist.
        Path = context.Request.Url?.AbsolutePath ?? string.Empty; //Wenn null, dann nimm leeren String
        //Body = Body;
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"Received: {Method} {Path}");

        if (context.Request.HasEntityBody) //wenn Request Daten enthählt, wird der gesamte Datenstrom gelesen
        {
            using Stream input = context.Request.InputStream;
            using StreamReader re = new(input, context.Request.ContentEncoding);
            Body = re.ReadToEnd();
            Content = JsonNode.Parse(Body)?.AsObject() ?? new JsonObject();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Body);

        } else
        {
            Body = string.Empty;
            Content = new JsonObject();
        }

    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // public properites                                                                                                //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>Gets the underlying HTTP listener context object.</summary>

    public HttpListenerContext Context { get; }

    public HttpMethod Method { get; }

    public string Path { get; }

    public string Body { get; }

    public JsonObject Content { get; }

    public bool Responded
    {
        get; set;
    } = false;


    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // public methods                                                                                                   //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>Gets the underlying HTTP listener context object.</summary>
    public void Respond(HttpStatusCode statusCode, JsonObject? content)
    {
        HttpListenerResponse response = Context.Response; //Zugriff auf das Antwort-Objekt
        response.StatusCode = (int)statusCode; //setzt den HTTP-Status (z. B. 200, 404, 500 …).
        string rstr = content?.ToString() ?? string.Empty; //konvertiert JSON in string

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Responding: {statusCode}: {rstr}\n\n");

        byte[] buf = Encoding.UTF8.GetBytes(rstr); //wandelt Text in Bytes um
        response.ContentLength64 = buf.Length;
        response.ContentType = "application/json; charset=UTF8";

        using Stream output = response.OutputStream; //Datenkanal zurück zum Client
        output.Write(buf, 0, buf.Length);
        output.Close();
    }

}