using FHTW.Swen1.Forum.System;
using MRP.System;
using System;
using Xunit;

public class RatingLikeRulesTests
{
    [Fact]
    public void RatingLike_First_Time_By_User_Should_Work()
    {
        var ratingId = Guid.NewGuid();

        //Like
        Repositories.RatingLikes.Add("max", ratingId);

        Assert.Equal(1, Repositories.RatingLikes.Count(ratingId));
    }

    [Fact]
    public void RatingLike_Twice_By_Same_User_Should_Throw()
    {
        var ratingId = Guid.NewGuid();

        // erster Like
        Repositories.RatingLikes.Add("max", ratingId);

        // zweiter Like 
        Assert.Throws<InvalidOperationException>(() =>
            Repositories.RatingLikes.Add("max", ratingId)
        );
    }

    [Fact]
    public void RatingLike_Unlike_Should_Remove_Like()
    {
        var ratingId = Guid.NewGuid();

        Repositories.RatingLikes.Add("max", ratingId);
        Assert.Equal(1, Repositories.RatingLikes.Count(ratingId));

        Repositories.RatingLikes.Remove("max", ratingId);

        Assert.Equal(0, Repositories.RatingLikes.Count(ratingId));
    }


}


