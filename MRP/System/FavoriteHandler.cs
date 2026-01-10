using FHTW.Swen1.Forum.Handlers;
using FHTW.Swen1.Forum.Server;
using FHTW.Swen1.Forum.System;
using MRP.System;
using System;
using System.Net;
using System.Text.Json.Nodes;

namespace FHTW.Swen1.Forum.System
{
    public sealed class FavoriteHandler : Handler, IHandler
    {
        public override void Handle(HttpRestEventArgs e)
        {
            // nur /favorites und /media/{id}/favorite
            if (!(e.Path == "/favorites" || e.Path.StartsWith("/media/")))
                return;

            try
            {
                // -----------------------------
                // GET /favorites  -> Liste der favorisierten Medien (Token nötig)
                // -----------------------------
                if (e.Path == "/favorites" && e.Method == HttpMethod.Get)
                {
                    var session = GetSessionOrThrow(e);

                    var ids = Repositories.Favorites.GetFavoriteMediaIds(session.UserName);
                    JsonArray arr = new();

                    foreach (var id in ids)
                    {
                        var m = Repositories.Media.Get(id);
                        if (m == null) continue; // falls Media gelöscht wurde

                        var (avg, cnt) = Repositories.Rating.GetStatsForMedia(m.Id);

                        arr.Add(new JsonObject
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
                            ["ratingCount"] = cnt,
                            ["isFavorite"] = true
                        });
                    }

                    e.Respond(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["favorites"] = arr
                    });

                    e.Responded = true;
                    return;
                }

                // -----------------------------
                // /media/{id}/favorite
                // -----------------------------
                if (e.Path.StartsWith("/media/") && e.Path.EndsWith("/favorite", StringComparison.OrdinalIgnoreCase))
                {
                    // parse id
                    var rest = e.Path.Substring("/media/".Length); // "{id}/favorite"
                    var parts = rest.Split('/', StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length != 2 || !parts[1].Equals("favorite", StringComparison.OrdinalIgnoreCase))
                        return;

                    if (!int.TryParse(parts[0], out int mediaId))
                    {
                        e.Respond(HttpStatusCode.BadRequest, new JsonObject
                        {
                            ["success"] = false,
                            ["reason"] = "Invalid media id."
                        });
                        e.Responded = true;
                        return;
                    }

                    // Token nötig
                    var session = GetSessionOrThrow(e);

                    // Media existiert?
                    var media = Repositories.Media.Get(mediaId);
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

                    // POST -> set favorite
                    if (e.Method == HttpMethod.Post)
                    {
                        var created = Repositories.Favorites.Add(session.UserName, mediaId);

                        e.Respond(created ? HttpStatusCode.Created : HttpStatusCode.OK, new JsonObject
                        {
                            ["success"] = true,
                            ["message"] = created ? "Favorite added." : "Already favorite.",
                            ["mediaId"] = mediaId
                        });

                        e.Responded = true;
                        return;
                    }

                    // DELETE -> remove favorite
                    if (e.Method == HttpMethod.Delete)
                    {
                        var removed = Repositories.Favorites.Remove(session.UserName, mediaId);

                        e.Respond(HttpStatusCode.OK, new JsonObject
                        {
                            ["success"] = true,
                            ["message"] = removed ? "Favorite removed." : "Was not a favorite.",
                            ["mediaId"] = mediaId
                        });

                        e.Responded = true;
                        return;
                    }

                    // method nicht erlaubt
                    e.Respond(HttpStatusCode.BadRequest, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Invalid favorite endpoint."
                    });
                    e.Responded = true;
                    return;
                }

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


        // Prüft Bearer-Token und Session, gibt gültige Session zurück
        private static Session GetSessionOrThrow(HttpRestEventArgs e)
        {
            string? raw = e.Context.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(raw))
                throw new UnauthorizedAccessException("Missing bearer token.");

            const string prefix = "bearer ";
            string token = raw.Trim();

            if (token.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                token = token.Substring(prefix.Length).Trim();

            if (string.IsNullOrWhiteSpace(token))
                throw new UnauthorizedAccessException("Missing bearer token.");

            Session? session = Session.Get(token);

            if (session == null || !session.Valid)
                throw new UnauthorizedAccessException("Invalid session.");

            return session;
        }
    }
}


