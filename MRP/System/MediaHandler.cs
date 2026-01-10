using FHTW.Swen1.Forum.Handlers;
using FHTW.Swen1.Forum.Server;
using MRP.System;
using System.Net;
using System.Runtime.Intrinsics.X86;
using System.Text.Json.Nodes;

namespace FHTW.Swen1.Forum.System
{
    /// <summary>This class implements a Handler for media endpoints.</summary>
    public sealed class MediaHandler : Handler, IHandler
    {
        public override void Handle(HttpRestEventArgs e)
        {
            // wenn der Pfad nicht mit /media beginnt, macht dieser Handler nichts
            if (!e.Path.StartsWith("/media"))
                return;

            try
            {
                // -----------------------------
                // GET /media  -> Array mit allen Medien
                // -----------------------------
                if (e.Path == "/media" && e.Method == HttpMethod.Get)
                {
                    var allMedia = Repositories.Media.GetAll(); 
                    JsonArray arr = new();
                    var sessionOpt = TryGetSession(e);

                    foreach (var m in allMedia)
                    {
                        var (avg, cnt) = Repositories.Rating.GetStatsForMedia(m.Id);

                        bool? isFav = null;

                        if (sessionOpt != null)
                            isFav = Repositories.Favorites.IsFavorite(sessionOpt.UserName, m.Id);

                        var obj = new JsonObject
                        {
                            ["id"] = m.Id,
                            ["title"] = m.Title,
                            ["description"] = m.Description,
                            ["type"] = m.Type.ToString(),
                            ["releaseYear"] = m.ReleaseYear,
                            ["genres"] = m.Genres,
                            ["ageRestriction"] = m.AgeRestriction,
                            ["ownerUserName"] = m.OwnerUserName,
                            ["avgRating"] = avg,
                            ["ratingCount"] = cnt
                        };

                        if (isFav != null)
                            obj["isFavorite"] = isFav.Value;

                        arr.Add(obj);
                    }

                    e.Respond(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["media"] = arr
                    });

                    e.Responded = true;
                    return;
                }

                // -----------------------------
                // POST /media  -> Media anlegen (Token nötig)
                // -----------------------------
                if (e.Path == "/media" && e.Method == HttpMethod.Post)
                {
                    //Session prüfen
                    Session session = GetSessionOrThrow(e);

                    Media media = new(session);
                    media.BeginEdit(session);

                    media.Title = e.Content?["title"]?.GetValue<string>() ?? string.Empty;
                    media.Description = e.Content?["description"]?.GetValue<string>() ?? string.Empty;

                    // Enum parse (Movie/Series/Game)
                    string typeStr = e.Content?["type"]?.GetValue<string>() ?? "Movie";
                    if (!Enum.TryParse<MediaType>(typeStr, true, out var parsedType))
                        parsedType = MediaType.movie;
                    media.Type = parsedType;

                    media.ReleaseYear = e.Content?["releaseYear"]?.GetValue<int>() ?? 0;
                    media.Genres = e.Content?["genres"]?.GetValue<string>() ?? string.Empty;
                    media.AgeRestriction = e.Content?["ageRestriction"]?.GetValue<int>() ?? 0;

                    // Owner aus Session setzen
                    media.OwnerUserName = session.UserName;

                    media.Save();

                    e.Respond(HttpStatusCode.Created, new JsonObject
                    {
                        ["success"] = true,
                        ["id"] = media.Id,
                        ["message"] = "Media created."
                    });

                    e.Responded = true;
                    return;
                }

                ///media/{id}
                if (e.Path.StartsWith("/media/"))
                {
                    //Pfad parsen
                    string rest = e.Path.Substring("/media/".Length);   // z.B. "1/ratings"
                    string idPart = rest.Split('/', 2)[0];              // -> "1"

                    if (!int.TryParse(idPart, out int id))
                    {
                        e.Respond(HttpStatusCode.BadRequest, new JsonObject
                        {
                            ["success"] = false,
                            ["reason"] = "Invalid media id."
                        });
                        e.Responded = true;
                        return;
                    }
                    if (rest.Contains("/"))
                        return;
                    // -----------------------------
                    // GET /media/{id} -> Medium aufrufen (kein Token notwendig)
                    // -----------------------------
                    if (e.Method == HttpMethod.Get)
                    {
                        Media? media = Repositories.Media.Get(id);

                        if (media == null)
                        {
                            e.Respond(HttpStatusCode.NotFound, new JsonObject
                            {
                                ["success"] = false,
                                ["reason"] = "Media not found."
                            });
                        }

                        else
                        {
                            var sessionOpt = TryGetSession(e);

                            bool? isFav = null;
                            if (sessionOpt != null)
                                isFav = Repositories.Favorites.IsFavorite(sessionOpt.UserName, media.Id);

                            var (avg, cnt) = Repositories.Rating.GetStatsForMedia(media.Id);

                            var obj = new JsonObject
                            {
                                ["success"] = true,
                                ["id"] = media.Id,
                                ["title"] = media.Title,
                                ["description"] = media.Description,
                                ["type"] = media.Type.ToString(),
                                ["releaseYear"] = media.ReleaseYear,
                                ["genres"] = media.Genres,
                                ["ageRestriction"] = media.AgeRestriction,
                                ["ownerUserName"] = media.OwnerUserName,
                                ["avgRating"] = avg,
                                ["ratingCount"] = cnt
                            };

                            if (isFav != null)
                                obj["isFavorite"] = isFav.Value;

                            e.Respond(HttpStatusCode.OK, obj);
                        }


                        e.Responded = true;
                        return;
                    }

                    // -----------------------------
                    // PUT /media/{id} -> ändern (Token + Owner/Admin)
                    // -----------------------------
                    if (e.Method == HttpMethod.Put)
                    {
                        //Prüft Bearer Token
                        Session session = GetSessionOrThrow(e);

                        Media? media = Repositories.Media.Get(id);
                        if (media == null)
                        {
                            e.Respond(HttpStatusCode.NotFound, new JsonObject
                            {
                                ["success"] = false,
                                ["reason"] = "Media not found."
                            });
                            e.Responded = true;
                            return;
                        }

                        media.BeginEdit(session);

                        // nur überschreiben, wenn Feld im Body vorhanden ist
                        if (e.Content?["title"] != null)
                            media.Title = e.Content["title"]!.GetValue<string>();

                        if (e.Content?["description"] != null)
                            media.Description = e.Content["description"]!.GetValue<string>();

                        if (e.Content?["type"] != null)
                        {
                            string typeStr = e.Content["type"]!.GetValue<string>();
                            if (Enum.TryParse<MediaType>(typeStr, true, out var parsedType))
                                media.Type = parsedType;
                        }

                        if (e.Content?["releaseYear"] != null)
                            media.ReleaseYear = e.Content["releaseYear"]!.GetValue<int>();

                        if (e.Content?["genres"] != null)
                            media.Genres = e.Content["genres"]!.GetValue<string>();

                        if (e.Content?["ageRestriction"] != null)
                            media.AgeRestriction = e.Content["ageRestriction"]!.GetValue<int>();

                        //Admin/Owner Prüfung(Media)
                        media.Save();

                        e.Respond(HttpStatusCode.OK, new JsonObject
                        {
                            ["success"] = true,
                            ["message"] = "Media updated."
                        });

                        e.Responded = true;
                        return;
                    }

                    // -----------------------------
                    // DELETE /media/{id} -> löschen (Token + Owner/Admin)
                    // -----------------------------
                    if (e.Method == HttpMethod.Delete)
                    {
                        Session session = GetSessionOrThrow(e);

                        Media? media = Repositories.Media.Get(id);
                        if (media == null)
                        {
                            e.Respond(HttpStatusCode.NotFound, new JsonObject
                            {
                                ["success"] = false,
                                ["reason"] = "Media not found."
                            });
                            e.Responded = true;
                            return;
                        }

                        media.BeginEdit(session);
                        //Admin/Owner Prüfung (Media)
                        media.Delete();

                        e.Respond(HttpStatusCode.OK, new JsonObject
                        {
                            ["success"] = true,
                            ["message"] = "Media deleted."
                        });

                        e.Responded = true;
                        return;
                    }
                }

                // -----------------------------
                // Fallback
                // -----------------------------
                e.Respond(HttpStatusCode.BadRequest, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "Invalid media endpoint."
                });
                e.Responded = true;
            }
            catch (UnauthorizedAccessException ex)
            {
                e.Respond(HttpStatusCode.Unauthorized, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = ex.Message
                });
                e.Responded = true;
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = ex.Message
                });
                e.Responded = true;
            }
        }

        // -----------------------------
        // Helper
        // -----------------------------


        // Prüft Bearer-Token und Session, gibt gültige Session zurück
        private static Session GetSessionOrThrow(HttpRestEventArgs e)
        {
            string? raw =
                e.Context.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(raw))
                throw new UnauthorizedAccessException("Missing bearer token.");

            // Case-insensitive "Bearer "
            const string prefix = "bearer ";
            string token = raw.Trim();

            if (token.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                token = token.Substring(prefix.Length).Trim();

            if (string.IsNullOrWhiteSpace(token))
                throw new UnauthorizedAccessException("Missing bearer token.");

            Session? session = Session.Get(token);

            if (session == null || !session.Valid)
                throw new UnauthorizedAccessException("Invalid session.");
            //Session enthält UserName, IsAdmin, Timestamp, Token
            return session;
        }

        private static Session? TryGetSession(HttpRestEventArgs e)
        {
            try
            {
                return GetSessionOrThrow(e);
            }
            catch
            {
                return null;
            }
        }


    }
}
