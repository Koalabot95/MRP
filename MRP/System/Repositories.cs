using FHTW.Swen1.Forum.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.System
{
    //erzeugt alle repo instanzen
    public static class Repositories
    {
        public static IUserRepository Users { get; } = new UserRepository();
        public static IMediaRepository Media { get; } = new MediaRepository();
        public static IRatingRepository Rating { get; } = new RatingRepository();
    }

}
