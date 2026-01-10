using FHTW.Swen1.Forum.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MRP.System
{
    public static class Repositories
    {
        //initialisiert ein Repository über eine Factory(Funktion die Objekete erzeugt), loggt den Initialisierungserfolg
        private static T Init<T>(string name, Func<T> factory)
        {
            try
            {
                var obj = factory();
                Console.WriteLine($"[Repositories] {name} OK -> {obj.GetType().Name}");
                return obj;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Repositories] {name} INIT FAILED:");
                Console.WriteLine(ex);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("[Repositories] INNER:");
                    Console.WriteLine(ex.InnerException);
                }
                throw;
            }
        }
        //Lazy - Repository wird nur erzeugt, wenn wirklich benötigt. Erzeugt Repo in Abhängigkeit von AppConfig
        private static readonly Lazy< IUserRepository> _users =
            new(() => Init< IUserRepository>("Users", () =>
                 AppConfig.UsePostgres
                    ? new  PostgresUserRepo()
                    : new  UserRepository()));

        private static readonly Lazy< IMediaRepository> _media =
            new(() => Init< IMediaRepository>("Media", () =>
                 AppConfig.UsePostgres
                    ? new  PostgresMediaRepo()
                    : new  MediaRepository()));

        private static readonly Lazy< IRatingRepository> _rating =
            new(() => Init< IRatingRepository>("Rating", () =>
                 AppConfig.UsePostgres
                    ? new PostgresRatingRepo()
                    : new RatingRepository()));

        private static readonly Lazy<IFavoriteRepository> _favorites =
            new(() => Init<IFavoriteRepository>("Favorites", () =>
                 AppConfig.UsePostgres
                    ? new PostgresFavoriteRepo()
                    : new FavoriteRepository()));

        private static readonly Lazy<IRatingLikeRepository> _ratingLikes =
            new(() => Init<IRatingLikeRepository>("RatingLikes", () =>
                 AppConfig.UsePostgres
                    ? new PostgresRatingLikeRepo()
                    : new RatingLikeRepository()));

        //properties
        public static IUserRepository Users => _users.Value;
        public static IMediaRepository Media => _media.Value;
        public static IRatingRepository Rating => _rating.Value;
        public static IFavoriteRepository Favorites => _favorites.Value;
        public static IRatingLikeRepository RatingLikes => _ratingLikes.Value;

    }
}
