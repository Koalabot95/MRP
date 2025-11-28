using MRP.System;
using System;
using System.Collections.Generic;

namespace FHTW.Swen1.Forum.System
{
    public sealed class Rating : Atom, IAtom
    {
        private bool _New;

        public Rating(Session? session = null)
        {
            _EditingSession = session;
            _New = true;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// PROPERTIES
        /// </summary>

        //rating id
        public Guid Id { get; private set; } = Guid.NewGuid();

        //fremdschlüssel zu mediaid
        public int MediaId { get; set; }

        ///welcher user hat den eintrag erstellt
        public string Creator { get; set; } = string.Empty;

        ///1-5 stars
        private int _Stars;
        public int Stars
        {
            get => _Stars;
            set
            {
                if (value < 1 || value > 5)
                    throw new ArgumentException("Stars must be 1–5.");

                _Stars = value;
            }
        }

        /// Optional Kommentar
        public string Comment { get; set; } = string.Empty;

        //timestamp wann rating erstellt wurde
        public DateTime Timestamp { get; private set; }

        /// menge aller user die den beitrag geliked haben
        public HashSet<string> Likes { get; private set; } = new();


        /// <summary>
        /// BUSINESS METHODS
        /// </summary>

        public void Like(string username)
        {
            Likes.Add(username); // max 1 like per user 
        }

        public void Unlike(string username)
        {
            Likes.Remove(username);
        }


        /// <summary>
        /// CRUD
        /// </summary>

        public override void Save()
        {
            if (_New)
            {
                if (string.IsNullOrWhiteSpace(Creator))
                    throw new InvalidOperationException("Creator must be set.");
                //Erstellt neues Rating
                Repositories.Rating.Add(this);
                _New = false;
            }
            else
            {
                //Prüft wieder Owner/Admin
                _EnsureAdminOrOwner(Creator);
                //Update im Repo
                if (!Repositories.Rating.Update(this))
                    throw new InvalidOperationException("Rating not found.");
            }

            _EndEdit();
        }

        public override void Delete()
        {
            //Prüft wieder Owner / Admin
            _EnsureAdminOrOwner(Creator);

            if (!Repositories.Rating.Delete(Id))
                throw new InvalidOperationException("Rating not found.");

            _EndEdit();
        }

        public override void Refresh()
        {
            //Holt rating aus Repo
            var existing = Repositories.Rating.Get(Id)
                ?? throw new InvalidOperationException("Rating not found.");

            //überschreibt die akutellen Felder
            Stars = existing.Stars;
            Comment = existing.Comment;
            Likes = existing.Likes;
            Timestamp = existing.Timestamp;

            _EndEdit();
        }
    }
}
