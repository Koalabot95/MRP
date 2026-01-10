using FHTW.Swen1.Forum.System;
using System.Xml.Linq;
using Xunit;

namespace Mrp.Tests;

// Helper-Klasse weil protected
public class TestableAtom : Atom
{
    public void EnsureAdmin_Public() => _EnsureAdmin();
    public void EnsureAdminOrOwner_Public(string owner) => _EnsureAdminOrOwner(owner);

    public void SetSession_Public(Session session)
    {
        _EditingSession = session; 
    }

    // Stubs
    public override void Save() { }
    public override void Refresh() { }
    public override void Delete() { }
}

public class AtomAuthorizationTests
{
    [Fact]
    public void EnsureAdmin_Should_Work_For_Admin()
    {
        var a = new TestableAtom();
        a.SetSession_Public(Session.Create("admin"));

        a.EnsureAdmin_Public(); // keine Exception = OK
    }

    [Fact]
    public void EnsureAdmin_Should_Throw_For_NonAdmin()
    {
        var a = new TestableAtom();
        a.SetSession_Public(Session.Create("max"));

        Assert.Throws<UnauthorizedAccessException>(() => a.EnsureAdmin_Public());
    }

    [Fact]
    public void EnsureAdminOrOwner_Should_Work_For_Owner()
    {
        var a = new TestableAtom();
        a.SetSession_Public(Session.Create("max"));

        a.EnsureAdminOrOwner_Public("max");
    }

    [Fact]
    public void EnsureAdminOrOwner_Should_Work_For_Admin()
    {
        var a = new TestableAtom();
        a.SetSession_Public(Session.Create("admin"));

        a.EnsureAdminOrOwner_Public("someoneElse");
    }

    [Fact]
    public void EnsureAdminOrOwner_Should_Throw_For_Unauthorized_User()
    {
        var a = new TestableAtom();
        a.SetSession_Public(Session.Create("anna"));

        Assert.Throws<UnauthorizedAccessException>(() =>
            a.EnsureAdminOrOwner_Public("max"));
    }
}