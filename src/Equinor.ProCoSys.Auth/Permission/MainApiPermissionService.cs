﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Auth.Permission
{
    /// <summary>
    /// Service to get permissions for an user. Permissions are read from Main, using  Main Api
    /// </summary>
    public class MainApiPermissionService(
        IMainApiClientForApplication mainApiClientForApplication,
        IMainApiClientForUser mainApiClientForUser,
        IOptionsMonitor<MainApiOptions> options)
        : IPermissionApiService
    {
        private readonly string _apiVersion = options.CurrentValue.ApiVersion;
        private readonly Uri _baseAddress = new(options.CurrentValue.BaseAddress);
        private readonly string _clientFriendlyName = options.CurrentValue.ClientFriendlyName;

        public async Task<List<AccessablePlant>> GetAllPlantsForUserAsync(Guid azureOid)
        {
            var url = $"{_baseAddress}Plants/ForUser" +
                      $"?azureOid={azureOid:D}" +
                      "&includePlantsWithoutAccess=true" +
                      $"&api-version={_apiVersion}";

            return await mainApiClientForApplication.QueryAndDeserializeAsync<List<AccessablePlant>>(url);
        }

        public async Task<List<AccessableProject>> GetAllOpenProjectsForCurrentUserAsync(string plantId)
        {
            // trace users use of plant each time getting projects
            // this will serve the purpose since we want to log once a day pr user pr plant, and ProCoSys clients as Preservation and IPO ALWAYS get projects at startup
            await TracePlantAsync(plantId);

            var url = $"{_baseAddress}Projects" +
                      $"?plantId={plantId}" +
                      "&withCommPkgsOnly=false" +
                      "&includeProjectsWithoutAccess=true" +
                      $"&api-version={_apiVersion}";

            return await mainApiClientForUser.QueryAndDeserializeAsync<List<AccessableProject>>(url) ?? [];
        }

        public async Task<List<string>> GetPermissionsForCurrentUserAsync(string plantId)
        {
            var url = $"{_baseAddress}Permissions" +
                      $"?plantId={plantId}" +
                      $"&api-version={_apiVersion}";

            return await mainApiClientForUser.QueryAndDeserializeAsync<List<string>>(url) ?? [];
        }

        public async Task<List<string>> GetRestrictionRolesForCurrentUserAsync(string plantId)
        {
            var url = $"{_baseAddress}ContentRestrictions" +
                      $"?plantId={plantId}" +
                      $"&api-version={_apiVersion}";

            return await mainApiClientForUser.QueryAndDeserializeAsync<List<string>>(url) ?? [];
        }

        public async Task<UserPlantPermissionData> GetUserPlantPermissionDataAsync(Guid userOid, string plantId, CancellationToken cancellationToken)
        {
            var plantsTask = GetAllPlantsForUserAsync(userOid);
            var permissionsTask = GetPermissionsForCurrentUserAsync(plantId);
            var projectsTask = GetAllOpenProjectsForCurrentUserAsync(plantId);
            var restrictionRolesTask = GetRestrictionRolesForCurrentUserAsync(plantId);

            await Task.WhenAll(plantsTask, permissionsTask, projectsTask, restrictionRolesTask);

            var allPlantsForUser = await plantsTask;
            var permissions = await permissionsTask;
            var projects = await projectsTask;
            var restrictionRoles = await restrictionRolesTask;

            var userPlantPermissionData = new UserPlantPermissionData(
                Oid: userOid,
                Plant: plantId,
                AllPlantsForUser: allPlantsForUser.ToArray(),
                Permissions: permissions.ToArray(),
                Projects: projects.ToArray(),
                RestrictionRoles: restrictionRoles.ToArray()
            );
            return userPlantPermissionData;
        }

        private async Task TracePlantAsync(string plant)
        {
            var url = $"{_baseAddress}Me/TracePlant" +
                      $"?plantId={plant}" +
                      $"&api-version={_apiVersion}";

            var json = JsonSerializer.Serialize(_clientFriendlyName);
            await mainApiClientForUser.PostAsync(url, new StringContent(json, Encoding.Default, "application/json"));
        }
    }
}
