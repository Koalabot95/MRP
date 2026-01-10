using FHTW.Swen1.Forum.Handlers;
using FHTW.Swen1.Forum.Server;
using MRP.System;
using System.Net;
using System.Text.Json.Nodes;

namespace FHTW.Swen1.Forum.System
{
    public sealed class RatingHandler : Handler, IHandler
    {
        public override void Handle(HttpRestEventArgs e)
        {

            // nur /ratings/... oder /media/{id}/ratings
            if (!e.Path.StartsWith("/ratings") && !e.Path.StartsWith("/media"))
                return;

            // /media muss wirklich /media/{id}/ratings sein, sonst return
            if (e.Path.StartsWith("/media") && !TryGetMediaIdFromRatingsPath(e, out _))
                return;


            Console.WriteLine($"[RatingHandler] Path='{e.Path}' Method={e.Method}");
           
            int mediaId;
            Console.WriteLine("[RatingHandler] BEFORE POST/GET checks");

            if (e.Method == HttpMethod.Post && TryGetMediaIdFromRatingsPath(e, out mediaId))
            {
                Console.WriteLine($"[RatingHandler] POST MATCH mediaId={mediaId}");
                Session session = GetSessionOrThrow(e);

                Rating rating = new(session);
                rating.BeginEdit(session);

                rating.MediaId = mediaId;
                rating.Creator = session.UserName;
                rating.Stars = e.Content?["stars"]?.GetValue<int>() ?? 0;
                rating.Comment = e.Content?["comment"]?.GetValue<string>() ?? string.Empty;

                rating.Save();

                e.Respond(HttpStatusCode.Created, new JsonObject
                {
                    ["success"] = true,
                    ["id"] = rating.Id.ToString()
                });

                e.Responded = true;
                return;
            }


            if (e.Method == HttpMethod.Get && TryGetMediaIdFromRatingsPath(e, out mediaId))
            {
                var ratings = Repositories.Rating.GetAllForMedia(mediaId);
                JsonArray arr = new();

                foreach (var r in ratings)
                {
                    arr.Add(new JsonObject
                    {
                        ["id"] = r.Id.ToString(),
                        ["creator"] = r.Creator,
                        ["stars"] = r.Stars,
                        ["comment"] = r.Comment,
                        ["likesCount"] = Repositories.RatingLikes.Count(r.Id)
                    });
                }

                e.Respond(HttpStatusCode.OK, new JsonObject
                {
                    ["success"] = true,
                    ["ratings"] = arr
                });

                e.Responded = true;
                return;
            }

            // -----------------------------
            // /ratings/{id}/likes
            // -----------------------------
            if (e.Path.StartsWith("/ratings/", StringComparison.OrdinalIgnoreCase))
            {
                // erwartet /ratings/{guid}/likes
                var rest = e.Path.Substring("/ratings/".Length);
                var parts = rest.Split('/', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2 && parts[1].Equals("likes", StringComparison.OrdinalIgnoreCase))
                {
                    if (!Guid.TryParse(parts[0], out Guid ratingId))
                    {
                        e.Respond(HttpStatusCode.BadRequest, new JsonObject
                        {
                            ["success"] = false,
                            ["reason"] = "Invalid rating id."
                        });
                        e.Responded = true;
                        return;
                    }

                    Session session = GetSessionOrThrow(e);

                    // POST = like
                    if (e.Method == HttpMethod.Post)
                    {
                        bool created = Repositories.RatingLikes.Add(session.UserName, ratingId);

                        e.Respond(created ? HttpStatusCode.Created : HttpStatusCode.OK, new JsonObject
                        {
                            ["success"] = true,
                            ["message"] = created ? "Rating liked." : "Rating already liked.",
                            ["likeCount"] = Repositories.RatingLikes.Count(ratingId)
                        });

                        e.Responded = true;
                        return;
                    }

                    // DELETE = unlike
                    if (e.Method == HttpMethod.Delete)
                    {
                        bool removed = Repositories.RatingLikes.Remove(session.UserName, ratingId);

                        e.Respond(HttpStatusCode.OK, new JsonObject
                        {
                            ["success"] = true,
                            ["message"] = removed ? "Rating like removed." : "Rating was not liked.",
                            ["likeCount"] = Repositories.RatingLikes.Count(ratingId)
                        });

                        e.Responded = true;
                        return;
                    }
                }
            }



            // -----------------------------
            // /ratings/{id}
            // -----------------------------
            if (e.Path.StartsWith("/ratings/"))
            {
                string idStr = e.Path.Substring("/ratings/".Length);

                if (!Guid.TryParse(idStr, out Guid id))
                {
                    e.Respond(HttpStatusCode.BadRequest, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Invalid rating id."
                    });
                    e.Responded = true;
                    return;
                }

                try
                {
                    if (e.Method == HttpMethod.Get)
                    {
                        Rating? rating = Repositories.Rating.Get(id);
                        if (rating == null)
                        {
                            e.Respond(HttpStatusCode.NotFound, new JsonObject
                            {
                                ["success"] = false,
                                ["reason"] = "Rating not found."
                            });
                            e.Responded = true;
                            return;
                        }

                        e.Respond(HttpStatusCode.OK, new JsonObject
                        {
                            ["success"] = true,
                            ["id"] = rating.Id.ToString(),
                            ["mediaId"] = rating.MediaId,
                            ["creator"] = rating.Creator,
                            ["stars"] = rating.Stars,
                            ["comment"] = rating.Comment,
                            ["timestamp"] = rating.Timestamp
                        });

                        e.Responded = true;
                        return;
                    }

                    // PUT /ratings/{id}
                    if (e.Method == HttpMethod.Put)
                    {
                        Session session = GetSessionOrThrow(e);

                        Rating? rating = Repositories.Rating.Get(id);
                        if (rating == null)
                        {
                            e.Respond(HttpStatusCode.NotFound, new JsonObject
                            {
                                ["success"] = false,
                                ["reason"] = "Rating not found."
                            });
                            e.Responded = true;
                            return;
                        }

                        rating.BeginEdit(session);

                        if (e.Content?["stars"] != null)
                            rating.Stars = e.Content["stars"]!.GetValue<int>();

                        if (e.Content?["comment"] != null)
                            rating.Comment = e.Content["comment"]!.GetValue<string>();

                        rating.Save();

                        e.Respond(HttpStatusCode.OK, new JsonObject
                        {
                            ["success"] = true,
                            ["message"] = "Rating updated."
                        });

                        e.Responded = true;
                        return;
                    }

                    // DELETE /ratings/{id}
                    if (e.Method == HttpMethod.Delete)
                    {
                        Session session = GetSessionOrThrow(e);

                        Rating? rating = Repositories.Rating.Get(id);
                        if (rating == null)
                        {
                            e.Respond(HttpStatusCode.NotFound, new JsonObject
                            {
                                ["success"] = false,
                                ["reason"] = "Rating not found."
                            });
                            e.Responded = true;
                            return;
                        }

                        rating.BeginEdit(session);
                        rating.Delete();

                        e.Respond(HttpStatusCode.OK, new JsonObject
                        {
                            ["success"] = true,
                            ["message"] = "Rating deleted."
                        });

                        e.Responded = true;
                        return;
                    }

                    // wenn /ratings/{id} aber falsche Methode
                    e.Respond(HttpStatusCode.BadRequest, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Invalid ratings endpoint."
                    });
                    e.Responded = true;
                }
                catch (UnauthorizedAccessException ex)
                {
                    RespondUnauthorized(e, ex.Message);
                }
                catch (ArgumentException ex)
                {
                    e.Respond(HttpStatusCode.BadRequest, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = ex.Message
                    });
                    e.Responded = true;
                }
                catch (Exception ex)
                {
                    RespondError(e, ex.Message);
                }
            }
        }

        // --- Helpers  ---

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

        // Sendet 401 Unauthorized Response
        private static void RespondUnauthorized(HttpRestEventArgs e, string msg)
        {
            e.Respond(HttpStatusCode.Unauthorized, new JsonObject
            {
                ["success"] = false,
                ["reason"] = msg
            });
            e.Responded = true;
        }

        // Sendet 500 Error Response
        private static void RespondError(HttpRestEventArgs e, string msg)
        {
            e.Respond(HttpStatusCode.InternalServerError, new JsonObject
            {
                ["success"] = false,
                ["reason"] = msg
            });
            e.Responded = true;
        }

        // Extrahiert Media-ID aus Pfad
        private static bool TryGetMediaIdFromRatingsPath(HttpRestEventArgs e, out int mediaId)
        {
            mediaId = 0;

            string path = e.Path.TrimEnd('/');
            string[] parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // erwartet: /media/{id}/ratings
            if (parts.Length != 3)
                return false;

            if (!parts[0].Equals("media", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!parts[2].Equals("ratings", StringComparison.OrdinalIgnoreCase))
                return false;

            return int.TryParse(parts[1], out mediaId);
        }


    }

}
