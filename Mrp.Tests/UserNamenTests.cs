using Xunit;
using FHTW.Swen1.Forum.System;
using System;

public class UserNameRulesTests
{
    [Fact]
    public void UserName_Set_When_New_Should_Work()
    {
        var u = new User();
        u.UserName = "max";

        Assert.Equal("max", u.UserName);
    }

    [Fact]
    public void UserName_Set_To_Whitespace_Should_Throw()
    {
        var u = new User();
        Assert.Throws<ArgumentException>(() => u.UserName = " ");
    }

    [Fact]
    public void UserName_Set_Twice_Should_Throw_Exception()
    {
        var u = new User();
        u.UserName = "max";

        Assert.Throws<InvalidOperationException>(() =>
        {
            u.UserName = "peter";
        });
    }

}
