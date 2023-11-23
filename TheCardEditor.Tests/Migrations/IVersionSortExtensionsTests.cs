using FluentAssertions;
using NSubstitute;
using System;
using System.Collections;
using System.Resources;
using TheCardEditor.DataModel.Migrations;
using Xunit;

namespace TheCardEditor.Shared.Tests.Migrations;

public class IVersionSortExtensionsTests
{
    private class ResourceReaderMock(IEnumerable<string> values) : IResourceReader
    {
        private readonly Dictionary<string, string> _values = values.ToDictionary(v => v);

        public void Close()
        {
        }

        public void Dispose()
        {
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }
    }

    [Theory]
    [InlineData("1.0.0", 3, "0.0.1", "0.1.0", "1.0.0", "1.0.1", "1.1.0", "1.1.1")]
    [InlineData("0.2.4", 5, "0.1.0", "0.2.4", "0.3.0", "0.3.5", "1.1.2", "1.2.4", "1.1.100")]
    public void GetPatchesToApply_ShouldWork(string currentVersion, int expected, params string[] availableVersions)
    {
        var expectedVersions = availableVersions.Reverse().Take(expected).Reverse().ToList();
        Random.Shared.Shuffle(availableVersions);
        var set = new ResourceSet(new ResourceReaderMock(availableVersions));
        var result = IVersionSort.CreateDefault(currentVersion).GetPatchesToApply(set);
        result.Count().Should().Be(expected);
        result.ToList().Select(r => r.Version.Concat(".")).Should().BeEquivalentTo(expectedVersions);
    }
}
