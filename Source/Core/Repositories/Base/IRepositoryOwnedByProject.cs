﻿#region Copyright 2014 Exceptionless

// This program is free software: you can redistribute it and/or modify it 
// under the terms of the GNU Affero General Public License as published 
// by the Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
//     http://www.gnu.org/licenses/agpl-3.0.html

#endregion

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Exceptionless.Models;

namespace Exceptionless.Core.Repositories {
    public interface IRepositoryOwnedByProject<T> : IRepository<T> where T : class, IOwnedByProject, IIdentity, new() {
        IList<T> GetByProjectId(string projectId, bool useCache = false, TimeSpan? expiresIn = null);
        IList<T> GetByProjectId(IList<string> projectIds, bool useCache = false, TimeSpan? expiresIn = null);
        void RemoveAllByProjectId(string projectId);
        Task RemoveAllByProjectIdAsync(string projectId);
    }

    public interface IRepositoryOwnedByOrganizationAndProject<T> : IRepositoryOwnedByOrganization<T>, IRepositoryOwnedByProject<T> where T : class, IOwnedByOrganization, IOwnedByProject, IIdentity, new() {}
}