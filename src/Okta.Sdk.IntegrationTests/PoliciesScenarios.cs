﻿// <copyright file="PoliciesScenarios.cs" company="Okta, Inc">
// Copyright (c) 2014 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Okta.Sdk.IntegrationTests
{
    [Collection(nameof(PoliciesScenarios))]
    public class PoliciesScenarios
    {
        [Fact]
        public async Task CreateSignOnPolicy()
        {
            var client = TestClient.Create();

            IPolicy policy = new Policy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: CreateSignOnPolicy {Guid.NewGuid()}".Substring(0, 50),
                Type = PolicyType.OktaSignOn,
                Status = "ACTIVE",
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);

            try
            {
                createdPolicy.Should().NotBeNull();
                createdPolicy.Name.Should().Be(policy.Name);
                createdPolicy.Type.Should().Be(PolicyType.OktaSignOn);
                createdPolicy.Status.Should().Be("ACTIVE");
                createdPolicy.Description.Should().Be(policy.Description);
            }
            finally
            {
                await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy.Id);
            }
        }

        [Fact]
        public async Task GetPolicy()
        {
            var client = TestClient.Create();

            IPolicy policy = new Policy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: GetPolicy {Guid.NewGuid()}".Substring(0, 50),
                Type = PolicyType.OktaSignOn,
                Status = "ACTIVE",
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);

            try
            {
                var retrievedPolicy = await client.Policies.GetPolicyAsync(createdPolicy.Id);

                retrievedPolicy.Should().NotBeNull();
                retrievedPolicy.Name.Should().Be(policy.Name);
                retrievedPolicy.Type.Should().Be(PolicyType.OktaSignOn);
                retrievedPolicy.Status.Should().Be("ACTIVE");
                retrievedPolicy.Description.Should().Be(policy.Description);
            }
            finally
            {
                await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy.Id);
            }
        }

        [Fact]
        public async Task GetPolicyOfType()
        {
            var client = TestClient.Create();
            var guid = Guid.NewGuid();

            var policy = new OktaSignOnPolicy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: GetPolicyOfType {guid}".Substring(0, 50),
                Type = PolicyType.OktaSignOn,
                Status = "ACTIVE",
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);

            try
            {
                var retrievedPolicy = await client.Policies.GetPolicyAsync<IOktaSignOnPolicy>(createdPolicy.Id);

                retrievedPolicy.Should().NotBeNull();
                retrievedPolicy.Should().BeAssignableTo<IOktaSignOnPolicy>();
                retrievedPolicy.Name.Should().Be(policy.Name);
                retrievedPolicy.Type.Should().Be(PolicyType.OktaSignOn);
                retrievedPolicy.Status.Should().Be("ACTIVE");
                retrievedPolicy.Description.Should().Be(policy.Description);
            }
            finally
            {
                await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy.Id);
            }
        }

        [Fact]
        public async Task CreateSignOnPolicyWithGroupConditions()
        {
            var client = TestClient.Create();
            var guid = Guid.NewGuid();

            var createdGroup = await client.Groups.CreateGroupAsync(new CreateGroupOptions
            {
                Name = $"dotnet-sdk: CreateSignOnPolicyWithGroupConditions {guid}",
            });

            var policy = new OktaSignOnPolicy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: CreateSignOnPolicyWithGroupConditions {guid}".Substring(0, 50),
                Type = PolicyType.OktaSignOn,
                Status = "ACTIVE",
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            IOktaSignOnPolicyConditions policyConditions = new OktaSignOnPolicyConditions()
            {
                People = new PolicyPeopleCondition()
                {
                    Groups = new GroupCondition()
                    {
                        Include = new List<string>() { createdGroup.Id },
                    },
                },
            };

            policy.Conditions = policyConditions;

            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);

            try
            {
                createdPolicy.Should().NotBeNull();
                createdPolicy.Name.Should().Be(policy.Name);
                createdPolicy.Type.Should().Be(PolicyType.OktaSignOn);
                createdPolicy.Status.Should().Be("ACTIVE");
                createdPolicy.Description.Should().Be(policy.Description);
                ((IOktaSignOnPolicy)createdPolicy).Conditions.Should().NotBeNull();
                ((IOktaSignOnPolicy)createdPolicy).Conditions.People.Groups.Should().NotBeNull();
                ((IOktaSignOnPolicy)createdPolicy).Conditions.People.Groups.Include.Should().HaveCount(1);
                ((IOktaSignOnPolicy)createdPolicy).Conditions.People.Groups.Include.First().Should().Be(createdGroup.Id);
            }
            finally
            {
                await createdGroup.DeleteAsync();
                await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy.Id);
            }
        }

        [Fact]
        public async Task GetAllPoliciesByType()
        {
            var client = TestClient.Create();

            var policy1 = new OktaSignOnPolicy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: Sign On GetAllPoliciesByType {Guid.NewGuid()}".Substring(0, 50),
                Type = PolicyType.OktaSignOn,
                Status = "ACTIVE",
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var policy2 = new PasswordPolicy()
            {
                Name = $"dotnet-sdk: Password GetAllPoliciesByType {Guid.NewGuid()}".Substring(0, 50),
                Type = PolicyType.Password,
                Status = "ACTIVE",
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var createdPolicy1 = await client.Policies.CreatePolicyAsync(policy1);
            var createdPolicy2 = await client.Policies.CreatePolicyAsync(policy2);

            try
            {
                var signOnPolicies = await client.Policies.ListPolicies(PolicyType.OktaSignOn).ToList();
                signOnPolicies.Should().NotBeNullOrEmpty();
                signOnPolicies.First(x => x.Id == createdPolicy1.Id).Should().NotBeNull();
                signOnPolicies.FirstOrDefault(x => x.Id == createdPolicy2.Id).Should().BeNull();

                var passwordPolicies = await client.Policies.ListPolicies(PolicyType.Password).ToList();
                passwordPolicies.Should().NotBeNullOrEmpty();
                passwordPolicies.First(x => x.Id == createdPolicy2.Id).Should().NotBeNull();
                passwordPolicies.FirstOrDefault(x => x.Id == createdPolicy1.Id).Should().BeNull();
            }
            finally
            {
                await client.Policies.DeactivatePolicyAsync(createdPolicy1.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy1.Id);
                await client.Policies.DeactivatePolicyAsync(createdPolicy2.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy2.Id);
            }
        }

        [Fact]
        public async Task DeletePolicy()
        {
            var client = TestClient.Create();

            var policy = new OktaSignOnPolicy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: DeletePolicy {Guid.NewGuid()}".Substring(0, 50),
                Type = PolicyType.OktaSignOn,
                Status = "ACTIVE",
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);
            var retrievedPolicy = await client.Policies.GetPolicyAsync(createdPolicy.Id);
            retrievedPolicy.Should().NotBeNull();

            await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
            await client.Policies.DeletePolicyAsync(createdPolicy.Id);

            Func<Task> act = async () => await client.Policies.GetPolicyAsync(createdPolicy.Id);
            act.Should().Throw<OktaApiException>();
        }

        [Fact]
        public async Task UpdatePolicy()
        {
            var client = TestClient.Create();

            var policy = new OktaSignOnPolicy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: UpdatePolicy {Guid.NewGuid()}".Substring(0, 50),
                Type = PolicyType.OktaSignOn,
                Status = "ACTIVE",
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);

            createdPolicy.Name = $"dotnet-sdk: Updated UpdatePolicy {Guid.NewGuid()}".Substring(0, 50);
            createdPolicy.Description = "This description was updated";
            // NIT: it would be nicer to do this intead: client.Policies.UpdatePolicyAsync(createdPolicy);
            await client.Policies.UpdatePolicyAsync(createdPolicy, createdPolicy.Id);

            var updatedPolicy = await client.Policies.GetPolicyAsync(createdPolicy.Id);

            try
            {
                updatedPolicy.Id.Should().Be(createdPolicy.Id);
                updatedPolicy.Type.Should().Be(createdPolicy.Type);
                updatedPolicy.Name.Should().StartWith("dotnet-sdk: Updated");
                updatedPolicy.Description.Should().Be("This description was updated");
            }
            finally
            {
                await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy.Id);
            }
        }

        [Fact]
        public async Task ActivateAnInactivePolicy()
        {
            var client = TestClient.Create();

            var policy = new OktaSignOnPolicy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: ActivatePolicy {Guid.NewGuid()}".Substring(0, 50),
                Type = PolicyType.OktaSignOn,
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);

            try
            {
                await createdPolicy.DeactivateAsync();
                var retrievedPolicy = await client.Policies.GetPolicyAsync(createdPolicy.Id);

                retrievedPolicy.Status.Should().Be("INACTIVE");
                await createdPolicy.ActivateAsync();

                retrievedPolicy = await client.Policies.GetPolicyAsync(createdPolicy.Id);
                retrievedPolicy.Status.Should().Be("ACTIVE");
            }
            finally
            {
                await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy.Id);
            }
        }

        [Fact]
        public async Task CreatePasswordPolicy()
        {
            var client = TestClient.Create();
            var guid = Guid.NewGuid();

            var policy = new PasswordPolicy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: CreatePasswordPolicy {guid}".Substring(0, 50),
                Type = PolicyType.Password,
                Status = "ACTIVE",
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);

            try
            {
                createdPolicy.Should().NotBeNull();
                createdPolicy.Type.Should().Be(PolicyType.Password);
                createdPolicy.Status.Should().Be("ACTIVE");
                createdPolicy.Description.Should().Be(policy.Description);
            }
            finally
            {
                await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy.Id);
            }
        }

        [Fact]
        public async Task CreatePasswordPolicyRule()
        {
            var client = TestClient.Create();
            var guid = Guid.NewGuid();

            var policy = new PasswordPolicy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: CreatePasswordPolicyRule {guid}".Substring(0, 50),
                Type = PolicyType.Password,
                Status = "ACTIVE",
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            // NIT: Create an AddPasswordPolicyRuleOptions helper class with plain properties, encapsulate all the creation logic below and use this instead: client.Policies.AddPolicyRuleAsync(addPasswordPolicyRuleOptions);
            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);

            IPasswordPolicyRule policyRule = new PasswordPolicyRule()
            {
                Name = $"dotnet-sdk: CreatePasswordPolicyRule {guid}".Substring(0, 50),
                Type = PolicyType.Password,
                Conditions = new PasswordPolicyRuleConditions()
                {
                    People = new PolicyPeopleCondition()
                    {
                        Users = new UserCondition()
                        {
                            Exclude = new List<string>(),
                        },
                    },
                    Network = new PolicyNetworkCondition()
                    {
                        Connection = "ANYWHERE",
                    },
                },
                Actions = new PasswordPolicyRuleActions()
                {
                    PasswordChange = new PasswordPolicyRuleAction()
                    {
                        Access = "ALLOW",
                    },
                    SelfServicePasswordReset = new PasswordPolicyRuleAction()
                    {
                        Access = "ALLOW",
                    },
                    SelfServiceUnlock = new PasswordPolicyRuleAction()
                    {
                        Access = "DENY",
                    },
                },
            };

            var createdPolicyRule = await client.Policies.AddPolicyRuleAsync(policyRule, createdPolicy.Id);

            try
            {
                createdPolicyRule.Should().NotBeNull();
                ((IPasswordPolicyRule)createdPolicyRule).Name.Should().Be(policyRule.Name);
                ((IPasswordPolicyRule)createdPolicyRule).Actions.Should().NotBeNull();
                ((IPasswordPolicyRule)createdPolicyRule).Actions.PasswordChange.Access.Should().Be("ALLOW");
                ((IPasswordPolicyRule)createdPolicyRule).Actions.SelfServicePasswordReset.Access.Should().Be("ALLOW");
                ((IPasswordPolicyRule)createdPolicyRule).Actions.SelfServiceUnlock.Access.Should().Be("DENY");
                ((IPasswordPolicyRule)createdPolicyRule).Conditions.People.Users.Exclude.Should().BeNullOrEmpty();
                ((IPasswordPolicyRule)createdPolicyRule).Conditions.Network.Connection.Should().Be("ANYWHERE");
                ((IPasswordPolicyRule)createdPolicyRule).Type.Should().Be(PolicyType.Password);
            }
            finally
            {
                await client.Policies.DeactivatePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeletePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy.Id);
            }
        }

        [Fact]
        public async Task CreateOktaSignOnOnPremPolicyRule()
        {
            var client = TestClient.Create();
            var guid = Guid.NewGuid();

            var policy = new OktaSignOnPolicy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: CreateOktaSignOnOnPremPolicyRule {guid}".Substring(0, 50),
                Type = PolicyType.OktaSignOn,
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);

            // NIT: Create an AddSignOnPolicyRuleOptions helper class with plain properties, encapsulate all the creation logic below and use this instead: client.Policies.AddPolicyRuleAsync(addSignOnPolicyRuleOptions);
            IOktaSignOnPolicyRule policyRule = new OktaSignOnPolicyRule()
            {
                Name = $"dotnet-sdk: CreateOktaSignOnOnPremPolicyRule {guid}".Substring(0, 50),
                Type = "SIGN_ON",
                Actions = new OktaSignOnPolicyRuleActions()
                {
                    Signon = new OktaSignOnPolicyRuleSignonActions()
                    {
                        Access = "ALLOW",
                        RequireFactor = false,
                        RememberDeviceByDefault = false,
                        Session = new OktaSignOnPolicyRuleSignonSessionActions()
                        {
                            UsePersistentCookie = false,
                            MaxSessionIdleMinutes = 720,
                            MaxSessionLifetimeMinutes = 0,
                        },
                    },
                },
                Conditions = new OktaSignOnPolicyRuleConditions()
                {
                    AuthContext = new PolicyRuleAuthContextCondition()
                    {
                        AuthType = "ANY",
                    },
                },
            };

            var createdPolicyRule = await client.Policies.AddPolicyRuleAsync(policyRule, createdPolicy.Id);

            try
            {
                createdPolicyRule.Should().NotBeNull();
                ((IOktaSignOnPolicyRule)createdPolicyRule).Name.Should().Be(policyRule.Name);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Type.Should().Be("SIGN_ON");
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.Access.Should().Be("ALLOW");
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.RequireFactor.Should().Be(false);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.RememberDeviceByDefault.Should().Be(false);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.Session.UsePersistentCookie.Should().Be(false);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.Session.MaxSessionIdleMinutes.Should().Be(720);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.Session.MaxSessionLifetimeMinutes.Should().Be(0);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Conditions.AuthContext.AuthType.Should().Be("ANY");
            }
            finally
            {
                await client.Policies.DeactivatePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeletePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy.Id);
            }
        }

        [Fact]
        public async Task CreateOktaSignOnRadiusPolicyRule()
        {
            var client = TestClient.Create();
            var guid = Guid.NewGuid();

            var policy = new OktaSignOnPolicy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: CreateOktaSignOnRadiusPolicyRule {guid}".Substring(0, 50),
                Type = PolicyType.OktaSignOn,
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);
            // NIT: Create an AddSignOnPolicyRuleOptions helper class with plain properties, encapsulate all the creation logic below and use this instead: client.Policies.AddPolicyRuleAsync(addSignOnPolicyRuleOptions);
            IOktaSignOnPolicyRule policyRule = new OktaSignOnPolicyRule()
            {
                Name = $"dotnet-sdk: CreateOktaSignOnRadiusPolicyRule {guid}".Substring(0, 50),
                Type = "SIGN_ON",
                Actions = new OktaSignOnPolicyRuleActions()
                {
                    Signon = new OktaSignOnPolicyRuleSignonActions()
                    {
                        Access = "ALLOW",
                        RequireFactor = true,
                        FactorPromptMode = "ALWAYS",
                        RememberDeviceByDefault = false,
                        Session = new OktaSignOnPolicyRuleSignonSessionActions()
                        {
                            UsePersistentCookie = false,
                            MaxSessionIdleMinutes = 720,
                            MaxSessionLifetimeMinutes = 0,
                        },
                    },
                },
                Conditions = new OktaSignOnPolicyRuleConditions()
                {
                    Network = new PolicyNetworkCondition()
                    {
                        Connection = "ANYWHERE",
                    },
                    AuthContext = new PolicyRuleAuthContextCondition()
                    {
                        AuthType = "RADIUS",
                    },
                },
            };

            var createdPolicyRule = await client.Policies.AddPolicyRuleAsync(policyRule, createdPolicy.Id);

            try
            {
                createdPolicyRule.Should().NotBeNull();
                ((IOktaSignOnPolicyRule)createdPolicyRule).Name.Should().Be(policyRule.Name);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Type.Should().Be("SIGN_ON");
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.Access.Should().Be("ALLOW");
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.RequireFactor.Should().Be(true);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.FactorPromptMode.Should().Be("ALWAYS");
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.RememberDeviceByDefault.Should().Be(false);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.Session.UsePersistentCookie.Should().Be(false);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.Session.MaxSessionIdleMinutes.Should().Be(720);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.Session.MaxSessionLifetimeMinutes.Should().Be(0);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Conditions.AuthContext.AuthType.Should().Be("RADIUS");
                ((IOktaSignOnPolicyRule)createdPolicyRule).Conditions.Network.Connection.Should().Be("ANYWHERE");
            }
            finally
            {
                await client.Policies.DeactivatePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeletePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy.Id);
            }
        }

        [Fact]
        public async Task CreateOktaSignOnCloudPolicyRule()
        {
            var client = TestClient.Create();
            var guid = Guid.NewGuid();

            var policy = new OktaSignOnPolicy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: CreateOktaSignOnCloudPolicyRule {guid}".Substring(0, 50),
                Type = PolicyType.OktaSignOn,
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);
            // NIT: Create an AddSignOnPolicyRuleOptions helper class with plain properties, encapsulate all the creation logic below and use this instead: client.Policies.AddPolicyRuleAsync(addSignOnPolicyRuleOptions);
            IOktaSignOnPolicyRule policyRule = new OktaSignOnPolicyRule()
            {
                Name = $"dotnet-sdk: CreateOktaSignOnCloudPolicyRule {guid}".Substring(0, 50),
                Type = "SIGN_ON",
                Actions = new OktaSignOnPolicyRuleActions()
                {
                    Signon = new OktaSignOnPolicyRuleSignonActions()
                    {
                        Access = "ALLOW",
                        RequireFactor = true,
                        FactorPromptMode = "ALWAYS",
                        RememberDeviceByDefault = false,
                        Session = new OktaSignOnPolicyRuleSignonSessionActions()
                        {
                            UsePersistentCookie = false,
                            MaxSessionIdleMinutes = 720,
                            MaxSessionLifetimeMinutes = 0,
                        },
                    },
                },
                Conditions = new OktaSignOnPolicyRuleConditions()
                {
                    Network = new PolicyNetworkCondition()
                    {
                        Connection = "ANYWHERE",
                    },
                    AuthContext = new PolicyRuleAuthContextCondition()
                    {
                        AuthType = "ANY",
                    },
                    People = new PolicyPeopleCondition()
                    {
                        Users = new UserCondition()
                        {
                            Include = new List<string>(),
                            Exclude = new List<string>(),
                        },
                        Groups = new GroupCondition()
                        {
                            Include = new List<string>(),
                            Exclude = new List<string>(),
                        },
                    },
                },
            };

            var createdPolicyRule = await client.Policies.AddPolicyRuleAsync(policyRule, createdPolicy.Id);

            try
            {
                createdPolicyRule.Should().NotBeNull();
                ((IOktaSignOnPolicyRule)createdPolicyRule).Name.Should().Be(policyRule.Name);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Type.Should().Be("SIGN_ON");
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.Access.Should().Be("ALLOW");
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.RequireFactor.Should().Be(true);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.FactorPromptMode.Should().Be("ALWAYS");
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.RememberDeviceByDefault.Should().Be(false);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.Session.UsePersistentCookie.Should().Be(false);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.Session.MaxSessionIdleMinutes.Should().Be(720);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.Session.MaxSessionLifetimeMinutes.Should().Be(0);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Conditions.AuthContext.AuthType.Should().Be("ANY");
                ((IOktaSignOnPolicyRule)createdPolicyRule).Conditions.Network.Connection.Should().Be("ANYWHERE");
                ((IOktaSignOnPolicyRule)createdPolicyRule).Conditions.People.Users.Include.Should().BeNullOrEmpty();
                ((IOktaSignOnPolicyRule)createdPolicyRule).Conditions.People.Users.Exclude.Should().BeNullOrEmpty();
                ((IOktaSignOnPolicyRule)createdPolicyRule).Conditions.People.Groups.Include.Should().BeNullOrEmpty();
                ((IOktaSignOnPolicyRule)createdPolicyRule).Conditions.People.Groups.Exclude.Should().BeNullOrEmpty();
            }
            finally
            {
                await client.Policies.DeactivatePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeletePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy.Id);
            }
        }

        [Fact]
        public async Task CreateOktaSignOnDenyPolicyRule()
        {
            var client = TestClient.Create();
            var guid = Guid.NewGuid();

            var policy = new OktaSignOnPolicy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: CreateOktaSignOnDenyPolicyRule {guid}".Substring(0, 50),
                Type = PolicyType.OktaSignOn,
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);
            // NIT: Create an AddSignOnPolicyRuleOptions helper class with plain properties, encapsulate all the creation logic below and use this instead: client.Policies.AddPolicyRuleAsync(addSignOnPolicyRuleOptions);
            IOktaSignOnPolicyRule policyRule = new OktaSignOnPolicyRule()
            {
                Name = $"dotnet-sdk: CreateOktaSignOnDenyPolicyRule {guid}".Substring(0, 50),
                Type = "SIGN_ON",
                Actions = new OktaSignOnPolicyRuleActions()
                {
                    Signon = new OktaSignOnPolicyRuleSignonActions()
                    {
                        Access = "DENY",
                        RequireFactor = false,
                    },
                },
                Conditions = new OktaSignOnPolicyRuleConditions()
                {
                    AuthContext = new PolicyRuleAuthContextCondition()
                    {
                        AuthType = "ANY",
                    },
                    Network = new PolicyNetworkCondition()
                    {
                        Connection = "ANYWHERE",
                    },
                },
            };

            var createdPolicyRule = await client.Policies.AddPolicyRuleAsync(policyRule, createdPolicy.Id);

            try
            {
                createdPolicyRule.Should().NotBeNull();
                ((IOktaSignOnPolicyRule)createdPolicyRule).Name.Should().Be(policyRule.Name);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Type.Should().Be("SIGN_ON");
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.Access.Should().Be("DENY");
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.RequireFactor.Should().Be(false);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Conditions.AuthContext.AuthType.Should().Be("ANY");
                ((IOktaSignOnPolicyRule)createdPolicyRule).Conditions.Network.Connection.Should().Be("ANYWHERE");
            }
            finally
            {
                await client.Policies.DeactivatePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeletePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy.Id);
            }
        }

        [Fact]
        public async Task UpdateOktaSignOnPolicyRule()
        {
            var client = TestClient.Create();
            var guid = Guid.NewGuid();

            var policy = new OktaSignOnPolicy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: UpdateOktaSignOnPolicyRule {guid}".Substring(0, 50),
                Type = PolicyType.OktaSignOn,
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);

            IOktaSignOnPolicyRule policyRule = new OktaSignOnPolicyRule()
            {
                Name = $"dotnet-sdk: UpdateOktaSignOnPolicyRule {guid}".Substring(0, 50),
                Type = "SIGN_ON",
                Actions = new OktaSignOnPolicyRuleActions()
                {
                    Signon = new OktaSignOnPolicyRuleSignonActions()
                    {
                        Access = "ALLOW",
                        RequireFactor = true,
                        FactorPromptMode = "ALWAYS",
                        RememberDeviceByDefault = false,
                        Session = new OktaSignOnPolicyRuleSignonSessionActions()
                        {
                            UsePersistentCookie = false,
                            MaxSessionIdleMinutes = 720,
                            MaxSessionLifetimeMinutes = 0,
                        },
                    },
                },
            };

            // NIT: use this instead: client.Policies.AddPolicyRuleAsync(policyRule);
            var createdPolicyRule = await client.Policies.AddPolicyRuleAsync(policyRule, createdPolicy.Id);

            try
            {
                createdPolicyRule.Should().NotBeNull();
                ((IOktaSignOnPolicyRule)createdPolicyRule).Name.Should().Be(policyRule.Name);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Type.Should().Be("SIGN_ON");
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.Access.Should().Be("ALLOW");
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.RequireFactor.Should().Be(true);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.FactorPromptMode.Should().Be("ALWAYS");
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.RememberDeviceByDefault.Should().Be(false);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.Session.UsePersistentCookie.Should().Be(false);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.Session.MaxSessionIdleMinutes.Should().Be(720);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Actions.Signon.Session.MaxSessionLifetimeMinutes.Should().Be(0);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Conditions.AuthContext.AuthType.Should().Be("ANY");

                ((IOktaSignOnPolicyRule)createdPolicyRule).Name = $"dotnet-sdk: Updated {((IOktaSignOnPolicyRule)createdPolicyRule).Name}".Substring(0, 50);
                ((IOktaSignOnPolicyRule)createdPolicyRule).Conditions.Network = new PolicyNetworkCondition() { Connection = "ANYWHERE" };

                var updatedPolicyRule = await client.Policies.UpdatePolicyRuleAsync(createdPolicyRule, createdPolicy.Id, createdPolicyRule.Id);
                ((IOktaSignOnPolicyRule)updatedPolicyRule).Name.Should().StartWith("dotnet-sdk: Updated");
                ((IOktaSignOnPolicyRule)updatedPolicyRule).Type.Should().Be("SIGN_ON");
                ((IOktaSignOnPolicyRule)updatedPolicyRule).Actions.Signon.Access.Should().Be("ALLOW");
                ((IOktaSignOnPolicyRule)updatedPolicyRule).Actions.Signon.RequireFactor.Should().Be(true);
                ((IOktaSignOnPolicyRule)updatedPolicyRule).Actions.Signon.FactorPromptMode.Should().Be("ALWAYS");
                ((IOktaSignOnPolicyRule)updatedPolicyRule).Actions.Signon.RememberDeviceByDefault.Should().Be(false);
                ((IOktaSignOnPolicyRule)updatedPolicyRule).Actions.Signon.Session.UsePersistentCookie.Should().Be(false);
                ((IOktaSignOnPolicyRule)updatedPolicyRule).Actions.Signon.Session.MaxSessionIdleMinutes.Should().Be(720);
                ((IOktaSignOnPolicyRule)updatedPolicyRule).Actions.Signon.Session.MaxSessionLifetimeMinutes.Should().Be(0);
                ((IOktaSignOnPolicyRule)updatedPolicyRule).Conditions.AuthContext.AuthType.Should().Be("ANY");
                ((IOktaSignOnPolicyRule)updatedPolicyRule).Conditions.Network.Connection.Should().Be("ANYWHERE");
            }
            finally
            {
                await client.Policies.DeactivatePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeletePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy.Id);
            }
        }

        [Fact]
        public async Task GetPolicyRules()
        {
            var client = TestClient.Create();
            var guid = Guid.NewGuid();

            var policy = new PasswordPolicy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: GetPolicyRules {guid}".Substring(0, 50),
                Type = PolicyType.Password,
                Status = "ACTIVE",
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);
            // NIT: Create an AddSignOnPolicyRuleOptions helper class with plain properties, encapsulate all the creation logic below and use this instead: client.Policies.AddPolicyRuleAsync(addSignOnPolicyRuleOptions);
            IPasswordPolicyRule policyRule = new PasswordPolicyRule()
            {
                Name = $"dotnet-sdk: GetPolicyRules {guid}".Substring(0, 50),
                Type = PolicyType.Password,
                Conditions = new PasswordPolicyRuleConditions()
                {
                    People = new PolicyPeopleCondition()
                    {
                        Users = new UserCondition()
                        {
                            Exclude = new List<string>(),
                        },
                    },
                    Network = new PolicyNetworkCondition()
                    {
                        Connection = "ANYWHERE",
                    },
                },
                Actions = new PasswordPolicyRuleActions()
                {
                    PasswordChange = new PasswordPolicyRuleAction()
                    {
                        Access = "ALLOW",
                    },
                    SelfServicePasswordReset = new PasswordPolicyRuleAction()
                    {
                        Access = "ALLOW",
                    },
                    SelfServiceUnlock = new PasswordPolicyRuleAction()
                    {
                        Access = "DENY",
                    },
                },
            };

            var createdPolicyRule = await client.Policies.AddPolicyRuleAsync(policyRule, createdPolicy.Id);

            try
            {
                var policyRules = await createdPolicy.ListPolicyRules().ToList();

                policyRules.Should().NotBeNullOrEmpty();
                policyRules.Should().HaveCount(1);
                ((IPasswordPolicyRule)policyRules.First()).Name.Should().Be(policyRule.Name);
                ((IPasswordPolicyRule)policyRules.First()).Type.Should().Be(PolicyType.Password);
                ((IPasswordPolicyRule)policyRules.First()).Conditions.People.Users.Exclude.Should().BeNullOrEmpty();
                ((IPasswordPolicyRule)policyRules.First()).Conditions.Network.Connection.Should().Be("ANYWHERE");
                ((IPasswordPolicyRule)policyRules.First()).Actions.PasswordChange.Access.Should().Be("ALLOW");
                ((IPasswordPolicyRule)policyRules.First()).Actions.SelfServicePasswordReset.Access.Should().Be("ALLOW");
                ((IPasswordPolicyRule)policyRules.First()).Actions.SelfServiceUnlock.Access.Should().Be("DENY");
            }
            finally
            {
                await client.Policies.DeactivatePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeletePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy.Id);
            }
        }

        [Fact]
        public async Task DeletePolicyRule()
        {
            var client = TestClient.Create();
            var guid = Guid.NewGuid();

            var policy = new PasswordPolicy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: DeletePolicyRule {guid}".Substring(0, 50),
                Type = PolicyType.Password,
                Status = "ACTIVE",
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);

            IPasswordPolicyRule policyRule = new PasswordPolicyRule()
            {
                Name = $"dotnet-sdk: DeletePolicyRule {guid}".Substring(0, 50),
                Type = PolicyType.Password,
                Conditions = new PasswordPolicyRuleConditions()
                {
                    People = new PolicyPeopleCondition()
                    {
                        Users = new UserCondition()
                        {
                            Exclude = new List<string>(),
                        },
                    },
                },
                Actions = new PasswordPolicyRuleActions()
                {
                    PasswordChange = new PasswordPolicyRuleAction()
                    {
                        Access = "ALLOW",
                    },
                },
            };

            var createdPolicyRule = await client.Policies.AddPolicyRuleAsync(policyRule, createdPolicy.Id);

            try
            {
                var retrievedPolicyRule = await client.Policies.GetPolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                retrievedPolicyRule.Should().NotBeNull();

                await client.Policies.DeactivatePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeletePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);

                Func<Task> act = async () => await client.Policies.GetPolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                act.Should().Throw<OktaApiException>();
            }
            finally
            {
                await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy.Id);
            }
        }

        [Fact]
        public async Task GetPolicyRule()
        {
            var client = TestClient.Create();
            var guid = Guid.NewGuid();

            var policy = new PasswordPolicy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: GetPolicyRule {guid}".Substring(0, 50),
                Type = PolicyType.Password,
                Status = "ACTIVE",
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);

            IPasswordPolicyRule policyRule = new PasswordPolicyRule()
            {
                Name = $"dotnet-sdk: GetPolicyRule {guid}".Substring(0, 50),
                Type = PolicyType.Password,
                Conditions = new PasswordPolicyRuleConditions()
                {
                    People = new PolicyPeopleCondition()
                    {
                        Users = new UserCondition()
                        {
                            Exclude = new List<string>(),
                        },
                    },
                },
                Actions = new PasswordPolicyRuleActions()
                {
                    PasswordChange = new PasswordPolicyRuleAction()
                    {
                        Access = "ALLOW",
                    },
                },
            };

            var createdPolicyRule = await client.Policies.AddPolicyRuleAsync(policyRule, createdPolicy.Id);

            try
            {
                var retrievedPolicyRule = await client.Policies.GetPolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                retrievedPolicyRule.Should().NotBeNull();
                retrievedPolicyRule.Id.Should().Be(createdPolicyRule.Id);
                ((IPasswordPolicyRule)retrievedPolicyRule).Name.Should().Be(policyRule.Name);
                ((IPasswordPolicyRule)retrievedPolicyRule).Type.Should().Be(PolicyType.Password);
                ((IPasswordPolicyRule)retrievedPolicyRule).Conditions.People.Users.Exclude.Should().BeNullOrEmpty();
                ((IPasswordPolicyRule)retrievedPolicyRule).Actions.PasswordChange.Access.Should().Be("ALLOW");
            }
            finally
            {
                await client.Policies.DeactivatePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeletePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy.Id);
            }
        }

        [Fact]
        public async Task UpdatePasswordPolicyRule()
        {
            var client = TestClient.Create();
            var guid = Guid.NewGuid();

            var policy = new PasswordPolicy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: UpdatePasswordPolicyRule {guid}".Substring(0, 50),
                Type = PolicyType.Password,
                Status = "ACTIVE",
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);

            IPasswordPolicyRule policyRule = new PasswordPolicyRule()
            {
                Name = $"dotnet-sdk: UpdatePasswordPolicyRule {guid}".Substring(0, 50),
                Type = PolicyType.Password,
                Conditions = new PasswordPolicyRuleConditions()
                {
                    People = new PolicyPeopleCondition()
                    {
                        Users = new UserCondition()
                        {
                            Exclude = new List<string>(),
                        },
                    },
                },
                Actions = new PasswordPolicyRuleActions()
                {
                    PasswordChange = new PasswordPolicyRuleAction()
                    {
                        Access = "ALLOW",
                    },
                },
            };

            var createdPolicyRule = await client.Policies.AddPolicyRuleAsync(policyRule, createdPolicy.Id);

            try
            {
                var retrievedPolicyRule = await client.Policies.GetPolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                retrievedPolicyRule.Should().NotBeNull();
                retrievedPolicyRule.Id.Should().Be(createdPolicyRule.Id);
                ((IPasswordPolicyRule)retrievedPolicyRule).Name.Should().Be(policyRule.Name);
                ((IPasswordPolicyRule)retrievedPolicyRule).Type.Should().Be(PolicyType.Password);
                ((IPasswordPolicyRule)retrievedPolicyRule).Conditions.People.Users.Exclude.Should().BeNullOrEmpty();
                ((IPasswordPolicyRule)retrievedPolicyRule).Actions.PasswordChange.Access.Should().Be("ALLOW");

                // Update values
                ((IPasswordPolicyRule)retrievedPolicyRule).Actions.PasswordChange.Access = "DENY";
                ((IPasswordPolicyRule)retrievedPolicyRule).Actions.SelfServicePasswordReset.Access = "DENY";
                ((IPasswordPolicyRule)retrievedPolicyRule).Name = $"dotnet-sdk: Updated {policyRule.Name}".Substring(0, 50);

                var updatedPolicyRule = await client.Policies.UpdatePolicyRuleAsync(retrievedPolicyRule, createdPolicy.Id, retrievedPolicyRule.Id);
                ((IPasswordPolicyRule)updatedPolicyRule).Actions.PasswordChange.Access.Should().Be("DENY");
                ((IPasswordPolicyRule)updatedPolicyRule).Actions.SelfServicePasswordReset.Access.Should().Be("DENY");
                ((IPasswordPolicyRule)updatedPolicyRule).Name.Should().StartWith("dotnet-sdk: Updated");
                ((IPasswordPolicyRule)updatedPolicyRule).Type.Should().Be(PolicyType.Password);
                ((IPasswordPolicyRule)updatedPolicyRule).Conditions.People.Users.Exclude.Should().BeNullOrEmpty();
            }
            finally
            {
                await client.Policies.DeactivatePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeletePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy.Id);
            }
        }

        [Fact]
        public async Task DeactivatePolicyRule()
        {
            var client = TestClient.Create();
            var guid = Guid.NewGuid();

            var policy = new PasswordPolicy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: DeactivatePolicyRule {guid}".Substring(0, 50),
                Type = PolicyType.Password,
                Status = "ACTIVE",
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);

            IPasswordPolicyRule policyRule = new PasswordPolicyRule()
            {
                Name = $"dotnet-sdk: DeactivatePolicyRule {guid}".Substring(0, 50),
                Type = PolicyType.Password,
                Conditions = new PasswordPolicyRuleConditions()
                {
                    People = new PolicyPeopleCondition()
                    {
                        Users = new UserCondition()
                        {
                            Exclude = new List<string>(),
                        },
                    },
                },
                Actions = new PasswordPolicyRuleActions()
                {
                    PasswordChange = new PasswordPolicyRuleAction()
                    {
                        Access = "ALLOW",
                    },
                },
            };

            var createdPolicyRule = await client.Policies.AddPolicyRuleAsync(policyRule, createdPolicy.Id);

            try
            {
                var retrievedPolicyRule = await client.Policies.GetPolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                retrievedPolicyRule.Should().NotBeNull();
                // Default status
                retrievedPolicyRule.Status.Should().Be("ACTIVE");

                await retrievedPolicyRule.DeactivateAsync(createdPolicy.Id);
                retrievedPolicyRule = await client.Policies.GetPolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                retrievedPolicyRule.Status.Should().Be("INACTIVE");
            }
            finally
            {
                await client.Policies.DeactivatePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeletePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy.Id);
            }
        }

        [Fact]
        public async Task ActivateAnInactivePolicyRule()
        {
            var client = TestClient.Create();
            var guid = Guid.NewGuid();

            var policy = new PasswordPolicy()
            {
                // Name has a maximum of 50 chars
                Name = $"dotnet-sdk: ActivatePolicyRule {guid}".Substring(0, 50),
                Type = PolicyType.Password,
                Status = "ACTIVE",
                Description = "The default policy applies in all situations if no other policy applies.",
            };

            var createdPolicy = await client.Policies.CreatePolicyAsync(policy);

            IPasswordPolicyRule policyRule = new PasswordPolicyRule()
            {
                Name = $"dotnet-sdk: ActivatePolicyRule {guid}".Substring(0, 50),
                Type = PolicyType.Password,
                Conditions = new PasswordPolicyRuleConditions()
                {
                    People = new PolicyPeopleCondition()
                    {
                        Users = new UserCondition()
                        {
                            Exclude = new List<string>(),
                        },
                    },
                },
                Actions = new PasswordPolicyRuleActions()
                {
                    PasswordChange = new PasswordPolicyRuleAction()
                    {
                        Access = "ALLOW",
                    },
                },
            };

            var createdPolicyRule = await client.Policies.AddPolicyRuleAsync(policyRule, createdPolicy.Id);

            try
            {
                var retrievedPolicyRule = await client.Policies.GetPolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                retrievedPolicyRule.Should().NotBeNull();
                // Default status
                retrievedPolicyRule.Status.Should().Be("ACTIVE");

                await retrievedPolicyRule.DeactivateAsync(createdPolicy.Id);
                retrievedPolicyRule = await client.Policies.GetPolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                retrievedPolicyRule.Status.Should().Be("INACTIVE");

                await retrievedPolicyRule.ActivateAsync(createdPolicy.Id);
                retrievedPolicyRule = await client.Policies.GetPolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                retrievedPolicyRule.Status.Should().Be("ACTIVE");
            }
            finally
            {
                await client.Policies.DeactivatePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeletePolicyRuleAsync(createdPolicy.Id, createdPolicyRule.Id);
                await client.Policies.DeactivatePolicyAsync(createdPolicy.Id);
                await client.Policies.DeletePolicyAsync(createdPolicy.Id);
            }
        }
    }
}
