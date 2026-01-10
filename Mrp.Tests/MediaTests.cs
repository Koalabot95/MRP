using System;
using Xunit;
using FHTW.Swen1.Forum.System;
using MRP.System;

namespace FHTW.Swen1.Forum.Tests
{
    public sealed class MediaTests
    {
        private readonly IMediaRepository _repo;

        public MediaTests()
        {
            //vor dem ersten Zugriff auf Repositories.Media setzen
            AppConfig.UsePostgres = false;

            _repo = Repositories.Media;

            if (_repo is MediaRepository mr)
                mr.Clear();
        }

         
        [Fact]
        public void Save_NewMedia_AssignsId_AndIsStored()
        {
            // Arrange
            var m = new Media
            {
                Id = 0,
                Title = "Der Klient",
                Description = "Antwaltsdrama",
                Type = MediaType.movie,
                ReleaseYear = 1994,
                Genres = "Drama",
                AgeRestriction = 12,
                OwnerUserName = "max"
            };

            
            m.Save();

            
            Assert.True(m.Id > 0);
            var stored = _repo.Get(m.Id);
            Assert.NotNull(stored);
            Assert.Equal("Inception", stored!.Title);
        }

        [Fact]
        public void Save_ExistingMedia_AsNonOwner_ThrowsUnauthorized()
        {
            //Owner erstellt Media
            var ownerSession = Session.Create("max");
            var m = new Media(ownerSession)
            {
                Title = "Original",
                OwnerUserName = "max",
                Type = MediaType.series
            };
            m.Save();

            //Fremder versucht bestehendes Objekt zu speichern
            var foreignSession = Session.Create("bob");
            m.BeginEdit(foreignSession);

           
            Assert.Throws<UnauthorizedAccessException>(() => m.Save());
        }

   
        [Fact]
        public void Delete_ForeignDenied_OwnerAllowed()
        {
           
            var ownerSession = Session.Create("max");
            var m = new Media(ownerSession)
            {
                Title = "Delete me",
                OwnerUserName = "max",
                Type = MediaType.game
            };
            m.Save();
            int id = m.Id;

            // Fremder darf nicht löschen
            var foreignSession = Session.Create("bob");
            m.BeginEdit(foreignSession);
            Assert.Throws<UnauthorizedAccessException>(() => m.Delete());
            Assert.NotNull(_repo.Get(id)); // noch da

            // Owner darf löschen
            m.BeginEdit(ownerSession);
            m.Delete();
            Assert.Null(_repo.Get(id));
        }

   
        [Fact]
        public void Refresh_LoadsValuesFromRepository()
        {
            var stored = new Media
            {
                Title = "Der Klient",
                Description = "Antwaltsdrama",
                Type = MediaType.movie,
                ReleaseYear = 1994,
                Genres = "Drama",
                AgeRestriction = 12,
                OwnerUserName = "max"
  
            };
            stored.Save();

            // neues Objekt mit nur Id (lokal - falsch)
            var m = new Media
            {
                Id = stored.Id,
                Title = "New Name",
                OwnerUserName = "fred"
            };

            
            m.Refresh();

             
            Assert.Equal("Der Klient", m.Title);
            Assert.Equal("Antwaltsdrama", m.Description);
            Assert.Equal(MediaType.movie, m.Type);
            Assert.Equal(2024, m.ReleaseYear);
            Assert.Equal("Drama", m.Genres);
            Assert.Equal(12, m.AgeRestriction);
            Assert.Equal("max", m.OwnerUserName);
        }
    }
}
