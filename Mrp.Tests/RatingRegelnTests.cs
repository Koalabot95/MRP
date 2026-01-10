using System;
using Xunit;
using FHTW.Swen1.Forum.System;

public class RatingRulesTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Rating_Stars_Out_Of_Range_Should_Throw(int stars)
    {
        var r = new Rating();

        Assert.Throws<ArgumentOutOfRangeException>(() => r.Stars = stars);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Rating_Stars_Within_1_To_5_Should_Work(int stars)
    {
        var r = new Rating();
        r.Stars = stars;

        Assert.Equal(stars, r.Stars);
    }

    [Fact]
    public void Rating_Comment_Can_Be_Empty_And_Should_Not_Throw()
    {
        var r = new Rating();
        r.Stars = 5;

        r.Comment = ""; 

        Assert.Equal("", r.Comment);
    }

}
