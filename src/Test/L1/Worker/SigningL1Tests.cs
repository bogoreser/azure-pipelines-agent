// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.TeamFoundation.DistributedTask.Pipelines;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.VisualStudio.Services.Agent.Tests.L1.Worker
{
    [Collection("Worker L1 Tests")]
    public class SigningL1Tests : L1TestBase
    {
        [Fact]
        [Trait("Level", "L1")]
        [Trait("Category", "Worker")]
        // TODO: When NuGet works cross-platform, remove these traits. Also, package NuGet with the Agent.
        [Trait("SkipOn", "darwin")]
        [Trait("SkipOn", "linux")]
        public async Task SignatureEnforcementMode_PassesWhenAllTasksAreSigned()
        {
            try
            {
                // Arrange
                SetupL1();
                FakeConfigurationStore fakeConfigurationStore = GetMockedService<FakeConfigurationStore>();
                AgentSettings settings = fakeConfigurationStore.GetSettings();
                settings.Fingerprint = _fingerprint;
                fakeConfigurationStore.UpdateSettings(settings);

                var message = LoadTemplateMessage();
                message.Steps.Clear();
                message.Steps.Add(GetSignedTask());

                // Act
                var results = await RunWorker(message);

                // Assert
                FakeJobServer fakeJobServer = GetMockedService<FakeJobServer>();
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(fakeJobServer.Timelines));

                AssertJobCompleted();
                Assert.Equal(TaskResult.Succeeded, results.Result);
            }
            finally
            {
                TearDown();
            }
        }

        [Fact]
        [Trait("Level", "L1")]
        [Trait("Category", "Worker")]
        // TODO: When NuGet works cross-platform, remove these traits. Also, package NuGet with the Agent.
        [Trait("SkipOn", "darwin")]
        [Trait("SkipOn", "linux")]
        public async Task SignatureEnforcementMode_FailsWhenTasksArentSigned()
        {
            try
            {
                // Arrange
                SetupL1();
                FakeConfigurationStore fakeConfigurationStore = GetMockedService<FakeConfigurationStore>();
                AgentSettings settings = fakeConfigurationStore.GetSettings();
                settings.Fingerprint = _fingerprint;
                fakeConfigurationStore.UpdateSettings(settings);
                var message = LoadTemplateMessage();

                // Act
                var results = await RunWorker(message);

                // Assert
                AssertJobCompleted();
                Assert.Equal(TaskResult.Failed, results.Result);
            }
            finally
            {
                TearDown();
            }
        }

        private static TaskStep GetSignedTask()
        {
            var step = new TaskStep
            {
                Reference = new TaskStepDefinitionReference
                {
                    Id = Guid.Parse("5515f72c-5faa-4121-8a46-8f42a8f42132"),
                    Name = "servicetree-link-build-task-signed",
                    Version = "1.52.1"
                },
                Name = "servicetree-link-build-task-signed",
                DisplayName = "ServiceTree Integration - SIGNED",
                Id = Guid.NewGuid()
            };

            // These inputs let the task itself succeed.
            step.Inputs.Add("Service", "23ddace0-0682-541f-bfa9-6cbc76d9c051");
            step.Inputs.Add("ServiceTreeLinkNotRequiredIds", "2"); // Set to system.definitionId
            step.Inputs.Add("ServiceTreeGateway", "Foo");

            return step;
        }

        private static string _fingerprint = "3F9001EA83C560D712C24CF213C3D312CB3BFF51EE89435D3430BD06B5D0EECE";
    }
}