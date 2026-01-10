using FHTW.Swen1.Forum.Handlers;
using FHTW.Swen1.Forum.Server;
using MRP.System;
using System.Net;
using System.Text.Json.Nodes;



namespace FHTW.Swen1.Forum.System;

/// <summary>This class implements a Handler for user endpoints.</summary>
public sealed class UserHandler : Handler, IHandler
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // [override] Handler                                                                                               //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Handles a request if possible.</summary>
    /// <param name="e">Event arguments.</param>
    /// POST
    public override void Handle(HttpRestEventArgs e)
    {
        // wenn der Pfad nicht mit /users beginnt, nicht zuständig
        if (!e.Path.StartsWith("/users"))
            return;
        //subroutes und parameter extrahieren
        string path = e.Path.Trim('/');          // "users/registration"
        string[] parts = path.Split('/');        // ["users","registration"...]
        string sub = parts.Length > 1 ? parts[1].ToLower() : "";

        // POST /users/registration  -> User anlegen
        if(e.Method == HttpMethod.Post && sub == "registration")
        {
            try
            {
                User user = new()
                {
                    UserName = e.Content?["username"]?.GetValue<string>() ?? string.Empty,
                    FullName = e.Content?["fullname"]?.GetValue<string>() ?? string.Empty,
                    EMail = e.Content?["email"]?.GetValue<string>() ?? string.Empty
                };
                user.SetPassword(e.Content?["password"]?.GetValue<string>() ?? string.Empty);

                Repositories.Users.Add(user);


                e.Respond(HttpStatusCode.OK, new JsonObject
                {
                    ["success"] = true,
                    ["message"] = "User registered."
                });
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = ex.Message
                });
            }

            e.Responded = true;
            return;
        }

        // POST /users/login
        if (e.Method == HttpMethod.Post && sub == "login")
        {   //liest aus postman ein
            string username = e.Content?["username"]?.GetValue<string>() ?? string.Empty;
            string password = e.Content?["password"]?.GetValue<string>() ?? string.Empty;
            //sucht user aus repo
            User? user = Repositories.Users.Get(username);

            if (user == null || !user.VerifyPassword(password))
            {
                e.Respond(HttpStatusCode.Unauthorized, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "Invalid credentials."
                });

                e.Responded = true;
                return;
            }

            // bei erfolg Token generieren
            Session session = Session.Create(user.UserName);

            //token an client zurückgeben
            e.Respond(HttpStatusCode.OK, new JsonObject
            {
                ["success"] = true,
                ["token"] = session.Token
            });

            e.Responded = true;
            return;
        }


        // GET /users/{username}
        if (e.Method == HttpMethod.Get && parts.Length == 2)
        {
            string username = parts[1];

            
            User? user = Repositories.Users.Get(username);


            if (user == null)
            {
                e.Respond(HttpStatusCode.NotFound, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "User not found."
                });
            }
            else
            {
                e.Respond(HttpStatusCode.OK, new JsonObject
                {
                    ["success"] = true,
                    ["username"] = user.UserName,
                    ["fullname"] = user.FullName,
                    ["email"] = user.EMail
                });
            }

            e.Responded = true;
            return;
        }

        // PUT /users/{username} -> User updaten
        if (e.Method == HttpMethod.Put && parts.Length == 2)
        {
            string username = parts[1];
            User? user = Repositories.Users.Get(username);

            if (user == null)
            {
                e.Respond(HttpStatusCode.NotFound, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "User not found."
                });
                e.Responded = true;
                return;
            }

            // Felder aus JSON lesen 
            string? newFullname = e.Content?["fullname"]?.GetValue<string>();
            string? newEmail = e.Content?["email"]?.GetValue<string>();

            if (!string.IsNullOrEmpty(newFullname)) user.FullName = newFullname;
            if (!string.IsNullOrEmpty(newEmail)) user.EMail = newEmail;

            Repositories.Users.Update(user);

            e.Respond(HttpStatusCode.OK, new JsonObject
            {
                ["success"] = true,
                ["message"] = "User updated."
            });
            e.Responded = true;
            return;
        }

        // DELETE /users/{username}  -> User löschen
        if (e.Method == HttpMethod.Delete && parts.Length == 2)
        {
            string username = parts[1];
            User? user = Repositories.Users.Get(username);

            if (user == null)
            {
                e.Respond(HttpStatusCode.NotFound, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "User not found."
                });
            }
            else
            {
                Repositories.Users.Delete(username);
                e.Respond(HttpStatusCode.NoContent, new JsonObject
                {
                    ["success"] = true
                });
            }

            e.Responded = true;
            return;
        }


        // Fallback
        e.Respond(HttpStatusCode.BadRequest, new JsonObject
        {
            ["success"] = false,
            ["reason"] = "Invalid user endpoint."
        });
        e.Responded = true;
    }

}