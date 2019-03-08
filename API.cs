using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using CODBO4.Enums;
using CODBO4.Models;
using CODBO4.Models.Match;
using CODBO4.Models.Profile;
using Newtonsoft.Json;
using Blackout = CODBO4.Models.Profile.Blackout;
using Multiplayer = CODBO4.Models.Profile.Multiplayer;

namespace CODBO4
{
    public static class Api
    {
        /// <summary>
        /// Get user data from http://bo4tracker.com, will return successful if stats are available
        /// </summary>
        /// <param name="username">Username of the user</param>
        /// <param name="platform">Platform the user is on</param>
        /// <returns>User data</returns>
        public static Validate ValidateUser(string username, Platform platform)
        {
            var url = Uri.EscapeUriString($"{Utilities.ValidateUrl}/{username}/{platform.GetDescription()}");
            return DownloadData<Validate>(url);
        }

        /// <summary>
        /// Get user's profile stats for the specified mode
        /// </summary>
        /// <param name="username">Username of the user</param>
        /// <param name="platform">Platform the user is on</param>
        /// <param name="mode">Type of stats to fetch</param>
        /// <returns>Profile data</returns>
        public static object GetProfile(string username, Platform platform, Mode mode)
        {
            var url = Uri.EscapeUriString($"{Utilities.StatsUrl}/{username}/{platform.GetDescription()}?type={mode}");

            return (mode == Mode.Multiplayer)
                ? (object)DownloadData<Multiplayer>(url)
                : DownloadData<Blackout>(url);
        }

        /// <summary>
        /// Get user's profile stats for the specified mode
        /// </summary>
        /// <param name="username">Username of the user</param>
        /// <param name="userid">Id the user</param>
        /// <param name="platform">Platform the user is on</param>
        /// <param name="mode">Type of stats to fetch</param>
        /// <returns>Profile data</returns>
        public static object GetProfile(string username, long userid, Platform platform, Mode mode)
        {
            var url = Uri.EscapeUriString($"{Utilities.StatsUrl}/{username}/{platform.GetDescription()}?type={mode}?u={userid}");

            return mode == Mode.Multiplayer
                ? (object)DownloadData<Multiplayer>(url)
                : DownloadData<Blackout>(url);
        }

        /// <summary>
        /// Get user's last 20 matches for the specified mode
        /// </summary>
        /// <param name="username"></param>
        /// <param name="platform"></param>
        /// <param name="mode"></param>
        /// <returns>Recent matches data</returns>
        public static object GetUserMatches(string username, Platform platform, Mode mode)
        {
            var url = $"{Utilities.UserMatchesUrl}/{username}/{platform.GetDescription()}?type={mode}";

            return mode == Mode.Multiplayer
                ? (object)DownloadData<Multiplayer>(url)
                : DownloadData<Blackout>(url);
        }

        /// <summary>
        /// Get up to the last 100 recent matches played
        /// </summary>
        /// <param name="rows">Number of rows to fetch</param>
        /// <returns>Recent matches data</returns>
        public static RecentMatches GetRecentMatches(string rows)
        {
            var url = $"{Utilities.RecentMatchesUrl}?rows={rows}";
            return DownloadData<RecentMatches>(url);
        }

        /// <summary>
        /// Get matches using match id
        /// </summary>
        /// <param name="matchId">Id of the match to fetch</param>
        /// <returns>Match data</returns>
        public static Matches GetMatch(string matchId)
        {
            var url = $"{Utilities.MatchesUrl}?id={matchId}";
            return DownloadData<Matches>(url);
        }

        /// <summary>
        /// Get leader board data based on scope
        /// </summary>
        /// <param name="platform">Platform to get leader boards for</param>
        /// <param name="scope">Type of scope to search for</param>
        /// <param name="rows">Number of rows to fetch (100 Max)</param>
        /// <returns>LeaderBoard data</returns>
        public static Leaderboards GetLeaderBoard(Platform platform, Scope scope, int rows)
        {
            var url = $"{Utilities.LeaderBoardsUrl}/{platform.GetDescription()}/{scope.GetDescription()}?rows={rows}";
            return DownloadData<Leaderboards>(url);
        }

        /// <summary>
        /// Get username from their user id
        /// </summary>
        /// <param name="userid">User id's to fetch (500 Max)</param>
        /// <returns>Username data</returns>
        public static User GetUserById(IEnumerable<long> userid)
        {
            var userIds = userid.Aggregate("", (current, id) => current + $"?id={id}&");
            var url = $"{Utilities.UseridToUsernameUrl}?{userIds}";
            return DownloadData<User>(url);
        }

        private static T DownloadData<T>(string url)
        {
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception($"Bad response {response.StatusCode}");

                var responseData = response.Content.ReadAsStringAsync().Result;

                if (Utilities.IsValidResponse(responseData))
                    return JsonConvert.DeserializeObject<T>(responseData);

                throw new Exception(JsonConvert.DeserializeObject(responseData).ToString());
            }
        }
    }
}
