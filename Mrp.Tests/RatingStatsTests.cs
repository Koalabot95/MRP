using System;
using Xunit;
using FHTW.Swen1.Forum.System;
using MRP.System;

namespace FHTW.Swen1.Forum.Tests
{
    public sealed class RatingStatsTests
    {
        private readonly IRatingRepository _repo;

        public RatingStatsTests()
        {
            AppConfig.UsePostgres = false;

            _repo = Repositories.Rating;

            // InMemory Cleanup 
            if (_repo is RatingRepository rr) rr.Clear();
        }

        [Fact]
        public void GetStatsForMedia_NoRatings_ReturnsZeroZero()
        {
            var (avg, cnt) = _repo.GetStatsForMedia(123);
            Assert.Equal(0.0, avg);
            Assert.Equal(0, cnt);
        }

        [Fact]
        public void GetStatsForMedia_TwoRatings_ReturnsCorrectAvgAndCount()
        {
            _repo.Add(new Rating { MediaId = 1, Creator = "max", Stars = 5, Comment = "" });
            _repo.Add(new Rating { MediaId = 1, Creator = "bob", Stars = 3, Comment = "" });

            var (avg, cnt) = _repo.GetStatsForMedia(1);

            Assert.Equal(2, cnt);
            Assert.Equal(4.0, avg); 
        }

        [Fact]
        public void GetStatsForMedia_IgnoresOtherMedia()
        {
            _repo.Add(new Rating { MediaId = 1, Creator = "max", Stars = 5 });
            _repo.Add(new Rating { MediaId = 2, Creator = "bob", Stars = 1 });

            var (avg, cnt) = _repo.GetStatsForMedia(1);

            Assert.Equal(1, cnt);
            Assert.Equal(5.0, avg);
        }

        [Fact]
        public void GetStatsForMedia_ThreeRatings_ReturnsFractionalAverage()
        {
            _repo.Add(new Rating { MediaId = 7, Creator = "a", Stars = 5 });
            _repo.Add(new Rating { MediaId = 7, Creator = "b", Stars = 4 });
            _repo.Add(new Rating { MediaId = 7, Creator = "c", Stars = 4 });

            var (avg, cnt) = _repo.GetStatsForMedia(7);

            Assert.Equal(3, cnt);
            Assert.Equal((5 + 4 + 4) / 3.0, avg);
        }
    }
}
