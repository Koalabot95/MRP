using Xunit;
using FHTW.Swen1.Forum.System;

public class UserPasswordTests
{
    [Fact]
    public void VerifyPassword_Should_Return_False_If_Password_Not_Set()
    {
        var u = new User();
        u.UserName = "max";

        Assert.False(u.VerifyPassword("pass"));
    }

    [Fact]
    public void VerifyPassword_Should_Return_True_For_Correct_Password()
    {
        var u = new User();
        u.UserName = "max";
        u.SetPassword("secret");

        Assert.True(u.VerifyPassword("secret"));
    }

    [Fact]
    public void VerifyPassword_Should_Return_False_For_Wrong_Password()
    {
        var u = new User();
        u.UserName = "max";
        u.SetPassword("secret");

        Assert.False(u.VerifyPassword("wrong"));
    }

    [Fact]
    public void Same_User_And_Password_Should_Produce_Same_Hash()
    {
        var h1 = User._HashPassword("max", "secret");
        var h2 = User._HashPassword("max", "secret");

        Assert.Equal(h1, h2);
    }

    [Fact]
    public void Different_Usernames_Should_Produce_Different_Hashes()
    {
        var h1 = User._HashPassword("max", "secret");
        var h2 = User._HashPassword("anna", "secret");

        Assert.NotEqual(h1, h2);
    }
}
