﻿using System;
using Exceptionless.Core.Models;
using Exceptionless.Core.Repositories.Configuration;
using FluentValidation;
using Foundatio.Caching;
using Foundatio.Messaging;
using Nest;

namespace Exceptionless.Core.Repositories {
    public class WebHookRepository : ElasticSearchRepositoryOwnedByOrganizationAndProject<WebHook>, IWebHookRepository {
        public WebHookRepository(IElasticClient elasticClient, OrganizationIndex index, IValidator<WebHook> validator = null, ICacheClient cacheClient = null, IMessagePublisher messagePublisher = null) 
            : base(elasticClient, index, validator, cacheClient, messagePublisher) { }

        public void RemoveByUrl(string targetUrl) {
            var filter = Filter<WebHook>.Term(e => e.Url, targetUrl);
            RemoveAll(new ElasticSearchOptions<WebHook>().WithFilter(filter));
        }

        public FindResults<WebHook> GetByOrganizationIdOrProjectId(string organizationId, string projectId) {
            var filter = Filter<WebHook>.Term(e => e.OrganizationId, organizationId) || Filter<WebHook>.Term(e => e.ProjectId, projectId);
            return Find(new ElasticSearchOptions<WebHook>()
                .WithFilter(filter)
                .WithCacheKey(String.Concat("org:", organizationId, "-project:", projectId))
                .WithExpiresIn(TimeSpan.FromMinutes(5)));
        }
        
        public static class EventTypes {
            // TODO: Add support for these new web hook types.
            public const string NewError = "NewError";
            public const string CriticalError = "CriticalError";
            public const string NewEvent = "NewEvent";
            public const string CriticalEvent = "CriticalEvent";
            public const string StackRegression = "StackRegression";
            public const string StackPromoted = "StackPromoted";
        }
        
        public override void InvalidateCache(WebHook hook) {
            if (!EnableCache || Cache == null)
                return;

            Cache.Remove(GetScopedCacheKey(String.Concat("org:", hook.OrganizationId, "-project:", hook.ProjectId)));
            base.InvalidateCache(hook);
        }
    }
}