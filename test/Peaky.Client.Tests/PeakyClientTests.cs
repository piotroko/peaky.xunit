using System;
using System.Linq;
using System.Net.Http;
using FakeHttpService;
using FluentAssertions;
using Xunit;

namespace Peaky.Client.Tests
{
    public class PeakyClientTests
    {
        [Fact]
        public async void It_can_load_tests_from_a_Peaky_service_by_BaseAddress()
        {
            using (var server = new FakeHttpService.FakeHttpService()
                .WithContentAt("/tests", PeakyResponses.Tests))
            {
                var client = new PeakyClient(server.BaseAddress);

                var tests = await client.GetTests();

                tests.Should().HaveCount(15);
            }
        }
        
        [Fact]
        public async void It_can_load_tests_from_a_Peaky_service_by_HttpClient()
        {
            using (var server = new FakeHttpService.FakeHttpService()
                .WithContentAt("/tests", PeakyResponses.Tests))
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = server.BaseAddress
                };
                
                var client = new PeakyClient(httpClient);

                var tests = await client.GetTests();

                tests.Should().HaveCount(15);
            }
        }

        [Fact]
        public async void It_retrieves_attributes_from_a_Peaky_service()
        {
            using (var server = new FakeHttpService.FakeHttpService()
                .WithContentAt("/tests", PeakyResponses.Tests))
            {
                var client = new PeakyClient(server.BaseAddress);

                var tests = await client.GetTests();

                var test = tests.First();

                test.Application.Should().Be("bing");

                test.Environment.Should().Be("prod");

                test.Url.Should()
                    .Be("/tests/prod/bing/bing_homepage_returned_in_under_5ms");

                test.Tags.Should().BeEquivalentTo("LiveSite", "NonSideEffecting");
            }
        }

        [Fact]
        public async void It_retrieves_results_for_a_passing_test()
        {
            using (var server = new FakeHttpService.FakeHttpService()
                .WithTestResultAt("/a_passing_test", "abcdefg", true))
            {
                var client = new PeakyClient(server.BaseAddress);

                var test = new Test("", "", new Uri(server.BaseAddress, "/a_passing_test"), null);

                var testResult = await client.GetResultFor(test);

                testResult.Passed.Should().BeTrue();

                testResult.Content.Should().Be("abcdefg");
            }
        }

        [Fact]
        public async void It_retrieves_results_for_a_failing_test()
        {
            using (var server = new FakeHttpService.FakeHttpService()
                .WithTestResultAt("/a_failing_test", "hijklmnop", false))
            {
                var client = new PeakyClient(server.BaseAddress);

                var test = new Test("", "", new Uri(server.BaseAddress, "/a_failing_test"), null);

                var testResult = await client.GetResultFor(test);

                testResult.Passed.Should().BeFalse();

                testResult.Content.Should().Be("hijklmnop");
            }
        }

        [Fact]
        public async void It_retrieves_results_for_a_passing_test_Url()
        {
            using (var server = new FakeHttpService.FakeHttpService()
                .WithTestResultAt("/a_passing_test", "abcdefg", true))
            {
                var client = new PeakyClient(server.BaseAddress);

                var test = new Test("", "", new Uri(server.BaseAddress, "/a_passing_test"), null);

                var testResult = await client.GetResultFor(test.Url);

                testResult.Passed.Should().BeTrue();

                testResult.Content.Should().Be("abcdefg");
            }
        }

        [Fact]
        public async void It_retrieves_results_for_a_failing_test_Url()
        {
            using (var server = new FakeHttpService.FakeHttpService()
                .WithTestResultAt("/a_failing_test", "hijklmnop", false))
            {
                var client = new PeakyClient(server.BaseAddress);

                var test = new Test("", "", new Uri(server.BaseAddress, "/a_failing_test"), null);

                var testResult = await client.GetResultFor(test.Url);

                testResult.Passed.Should().BeFalse();

                testResult.Content.Should().Be("hijklmnop");
            }
        }
    }
}
