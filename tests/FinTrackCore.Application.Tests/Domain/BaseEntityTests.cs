using FinTrackCore.Domain.Common;
using FluentAssertions;

namespace FinTrackCore.Application.Tests.Domain;

public class BaseEntityTests
{
    [Fact]
    public void NewEntity_HasIdAndCreatedAt()
    {
        var entity = new TestEntity();

        entity.Id.Should().NotBe(Guid.Empty);
        entity.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    private sealed class TestEntity : BaseEntity;
}
