﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using Depressurizer.Properties;
using DepressurizerCore;
using DepressurizerCore.Helpers;
using DepressurizerCore.Models;
using ValueType = DepressurizerCore.ValueType;

namespace Depressurizer
{
    /// <summary>
    ///     Represents a complete collection of games and categories.
    /// </summary>
    public class GameList
    {
        #region Constants

        public const string FAVORITE_CONFIG_VALUE = "favorite";

        public const string FAVORITE_NEW_CONFIG_VALUE = "<Favorite>";

        #endregion

        #region Static Fields

        private static readonly Regex rxUnicode = new Regex(@"\\u(?<Value>[a-zA-Z0-9]{4})", RegexOptions.Compiled);

        #endregion

        #region Fields

        public List<Category> Categories;

        public List<Filter> Filters;

        public Dictionary<int, GameInfo> Games;

        #endregion

        #region Constructors and Destructors

        public GameList()
        {
            Games = new Dictionary<int, GameInfo>();
            Categories = new List<Category>();
            Filters = new List<Filter>();
            FavoriteCategory = new Category(FAVORITE_NEW_CONFIG_VALUE);
            Categories.Add(FavoriteCategory);
        }

        #endregion

        #region Public Properties

        public Category FavoriteCategory { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Fetches an HTML game list and returns the full page text.
        ///     Mostly just grabs the given HTTP response, except that it throws an errors if the profile is not public, and writes
        ///     approrpriate log entries.
        /// </summary>
        /// <param name="url">The URL to fetch</param>
        /// <returns>The full text of the HTML page</returns>
        public static string FetchHtmlFromUrl(string url)
        {
            try
            {
                string result = "";

                WebRequest req = WebRequest.Create(url);
                using (WebResponse response = req.GetResponse())
                {
                    if (response.ResponseUri.Segments.Length < 4)
                    {
                        throw new ProfileAccessException(GlobalStrings.GameData_SpecifiedProfileNotPublic);
                    }

                    StreamReader sr = new StreamReader(response.GetResponseStream());
                    result = sr.ReadToEnd();
                }

                return result;
            }
            catch (ProfileAccessException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message, e);
            }
        }

        /// <summary>
        ///     Grabs the HTML game list for the given account and returns its full text.
        /// </summary>
        /// <param name="customUrl">The custom name for the account</param>
        /// <returns>Full text of the HTTP response</returns>
        public static string FetchHtmlGameList(string customUrl)
        {
            return FetchHtmlFromUrl(string.Format(Constants.UrlCustomGameListHtml, customUrl));
        }

        /// <summary>
        ///     Grabs the HTML game list for the given account and returns its full text.
        /// </summary>
        /// <param name="accountId">The 64-bit account ID</param>
        /// <returns>Full text of the HTTP response</returns>
        public static string FetchHtmlGameList(long accountId)
        {
            return FetchHtmlFromUrl(string.Format(Constants.UrlGameListHtml, accountId));
        }

        /// <summary>
        ///     Fetches an XML game list and loads it into an XML document.
        /// </summary>
        /// <param name="url">The URL to fetch</param>
        /// <returns>Fetched XML page as an XmlDocument</returns>
        public static XmlDocument FetchXmlFromUrl(string url)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                WebRequest req = WebRequest.Create(url);
                WebResponse response = req.GetResponse();
                if (response.ResponseUri.Segments.Length < 4)
                {
                    throw new ProfileAccessException(GlobalStrings.GameData_SpecifiedProfileNotPublic);
                }

                doc.Load(response.GetResponseStream());
                response.Close();
                if (doc.InnerText.Contains("This profile is private."))
                {
                    throw new ProfileAccessException(GlobalStrings.GameData_SpecifiedProfileNotPublic);
                }

                return doc;
            }
            catch (ProfileAccessException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message, e);
            }
        }

        /// <summary>
        ///     Grabs the XML game list for the given account and reads it into an XmlDocument.
        /// </summary>
        /// <param name="customUrl">The custom name for the account</param>
        /// <returns>Fetched XML page as an XmlDocument</returns>
        public static XmlDocument FetchXmlGameList(string customUrl)
        {
            return FetchXmlFromUrl(string.Format(Constants.UrlCustomGameListXml, customUrl));
        }

        /// <summary>
        ///     Grabs the XML game list for the given account and reads it into an XmlDocument.
        /// </summary>
        /// <param name="accountId">The 64-bit account ID</param>
        /// <returns>Fetched XML page as an XmlDocument</returns>
        public static XmlDocument FetchXmlGameList(long steamId)
        {
            return FetchXmlFromUrl(string.Format(Constants.UrlGameListXml, steamId));
        }

        /// <summary>
        ///     Adds a new category to the list.
        /// </summary>
        /// <param name="name">Name of the category to add</param>
        /// <returns>The added category. Returns null if the category already exists.</returns>
        public Category AddCategory(string name)
        {
            if (string.IsNullOrEmpty(name) || CategoryExists(name))
            {
                return null;
            }

            Category newCat = new Category(name);
            Categories.Add(newCat);

            return newCat;
        }

        /// <summary>
        ///     Adds a new Filter to the list.
        /// </summary>
        /// <param name="name">Name of the Filter to add</param>
        /// <returns>The added Filter. Returns null if the Filter already exists.</returns>
        public Filter AddFilter(string name)
        {
            if (string.IsNullOrEmpty(name) || FilterExists(name))
            {
                return null;
            }

            Filter newFilter = new Filter(name);
            Filters.Add(newFilter);

            return newFilter;
        }

        /// <summary>
        ///     Adds a single category to a single game
        /// </summary>
        /// <param name="gameID">Game ID to add category to</param>
        /// <param name="c">Category to add</param>
        public void AddGameCategory(int gameID, Category c)
        {
            GameInfo g = Games[gameID];
            g.AddCategory(c);
        }

        /// <summary>
        ///     Adds a single category to each member of a list of games
        /// </summary>
        /// <param name="gameIDs">List of game IDs to add to</param>
        /// <param name="c">Category to add</param>
        public void AddGameCategory(int[] gameIDs, Category c)
        {
            for (int i = 0; i < gameIDs.Length; i++)
            {
                AddGameCategory(gameIDs[i], c);
            }
        }

        /// <summary>
        ///     Adds a set of categories to a single game
        /// </summary>
        /// <param name="gameID">Game ID to add to</param>
        /// <param name="cats">Categories to add</param>
        public void AddGameCategory(int gameID, ICollection<Category> cats)
        {
            GameInfo g = Games[gameID];
            g.AddCategory(cats);
        }

        /// <summary>
        ///     Adds a set of game categories to each member of a list of games
        /// </summary>
        /// <param name="gameIDs">List of game IDs to add to</param>
        /// <param name="cats">Categories to add</param>
        public void AddGameCategory(int[] gameIDs, ICollection<Category> cats)
        {
            for (int i = 0; i < gameIDs.Length; i++)
            {
                AddGameCategory(gameIDs[i], cats);
            }
        }

        /// <summary>
        ///     Checks to see if a category with the given name exists
        /// </summary>
        /// <param name="name">Name of the category to look for</param>
        /// <returns>True if the name is found, false otherwise</returns>
        public bool CategoryExists(string name)
        {
            // Favorite category always exists
            if (name == FAVORITE_NEW_CONFIG_VALUE || name == FAVORITE_CONFIG_VALUE)
            {
                return true;
            }

            foreach (Category c in Categories)
            {
                if (string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            Games.Clear();
            Categories.Clear();
        }

        /// <summary>
        ///     Clears all categories from a single game
        /// </summary>
        /// <param name="gameID">Game ID to clear categories from</param>
        /// <param name="cats">If true, preserves the favorite category.</param>
        public void ClearGameCategories(int gameID, bool preserveFavorite)
        {
            Games[gameID].ClearCategories(!preserveFavorite);
        }

        /// <summary>
        ///     Clears all categories from a set of games
        /// </summary>
        /// <param name="gameID">List of game IDs to clear categories from</param>
        /// <param name="cats">If true, preserves the favorite category.</param>
        public void ClearGameCategories(int[] gameIDs, bool preserveFavorite)
        {
            foreach (int id in gameIDs)
            {
                ClearGameCategories(id, preserveFavorite);
            }
        }

        /// <summary>
        ///     Writes Steam game category information to Steam user config file.
        /// </summary>
        /// <param name="steamId">Steam ID of user to save the config file for</param>
        /// <param name="discardMissing">
        ///     If true, any pre-existing game entries in the file that do not have corresponding entries
        ///     in the GameList are removed
        /// </param>
        /// <param name="includeShortcuts">If true, also saves the Steam shortcut category data</param>
        public void ExportSteamConfig(long steamId, bool discardMissing, bool includeShortcuts)
        {
            string filePath = string.Format(Constants.ConfigFilePath, Settings.Instance.SteamPath, Profile.ID64toDirName(steamId));
            ExportSteamConfigFile(filePath, discardMissing);
            if (includeShortcuts)
            {
                ExportSteamShortcuts(steamId);
            }
        }

        /// <summary>
        ///     Writes Steam game category information to Steam user config file.
        /// </summary>
        /// <param name="filePath">Full path of the steam config file to save</param>
        /// <param name="discardMissing">
        ///     If true, any pre-existing game entries in the file that do not have corresponding entries
        ///     in the GameList are removed
        /// </param>
        public void ExportSteamConfigFile(string filePath, bool discardMissing)
        {
            VDFNode fileData = new VDFNode();
            try
            {
                using (StreamReader reader = new StreamReader(filePath, false))
                {
                    fileData = VDFNode.LoadFromText(reader, true);
                }
            }
            catch (Exception e) { }

            VDFNode appListNode = fileData.GetNodeAt(new[]
            {
                "Software",
                "Valve",
                "Steam",
                "apps"
            }, true);

            // Run through all Delete category data for any games not found in the GameList
            if (discardMissing)
            {
                Dictionary<string, VDFNode> gameNodeArray = appListNode.NodeArray;
                if (gameNodeArray != null)
                {
                    foreach (KeyValuePair<string, VDFNode> pair in gameNodeArray)
                    {
                        int gameId;
                        if (!(int.TryParse(pair.Key, out gameId) && Games.ContainsKey(gameId)))
                        {
                            pair.Value.RemoveSubnode("tags");
                        }
                    }
                }
            }

            // Force appListNode to be an array, we can't do anything if it's a value
            appListNode.MakeArray();

            foreach (GameInfo game in Games.Values)
            {
                if (game.Id > 0)
                {
                    // External games have negative identifier

                    VDFNode gameNode = appListNode[game.Id.ToString()];
                    gameNode.MakeArray();

                    VDFNode tagsNode = gameNode["tags"];
                    tagsNode.MakeArray();

                    Dictionary<string, VDFNode> tags = tagsNode.NodeArray;
                    if (tags != null)
                    {
                        tags.Clear();
                    }

                    int key = 0;
                    foreach (Category c in game.Categories)
                    {
                        string name = c.Name;
                        if (name == FAVORITE_NEW_CONFIG_VALUE)
                        {
                            name = FAVORITE_CONFIG_VALUE;
                        }

                        tagsNode[key.ToString()] = new VDFNode(name);
                        key++;
                    }

                    if (game.Hidden)
                    {
                        gameNode["hidden"] = new VDFNode("1");
                    }
                    else
                    {
                        gameNode.RemoveSubnode("hidden");
                    }
                }
            }

            appListNode.CleanTree();

            VDFNode fullFile = new VDFNode();
            fullFile["UserLocalConfigStore"] = fileData;
            try
            {
                Utility.BackupFile(filePath, Settings.Instance.ConfigBackupCount);
            }
            catch (Exception e) { }

            try
            {
                string filePathTmp = filePath + ".tmp";
                FileInfo f = new FileInfo(filePathTmp);
                f.Directory.Create();
                FileStream fStream = f.Open(FileMode.Create, FileAccess.Write, FileShare.None);
                using (StreamWriter writer = new StreamWriter(fStream))
                {
                    fullFile.SaveAsText(writer);
                }

                fStream.Close();
                File.Delete(filePath);
                File.Move(filePathTmp, filePath);
            }
            catch (ArgumentException e)
            {
                throw new ApplicationException(GlobalStrings.GameData_FailedToSaveSteamConfigBadPath, e);
            }
            catch (IOException e)
            {
                throw new ApplicationException(GlobalStrings.GameData_FailedToSaveSteamConfigFile + e.Message, e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new ApplicationException(GlobalStrings.GameData_AccessDeniedSteamConfigFile + e.Message, e);
            }
        }

        /// <summary>
        ///     Writes category info for shortcut games to shortcuts.vdf config file for specified Steam user.
        ///     Loads the shortcut config file, then tries to match each game in the file against one of the games in the gamelist.
        ///     If it finds a match, it updates the config file with the new category info.
        /// </summary>
        /// <param name="SteamId">Identifier of Steam user to save information</param>
        /// <param name="discardMissing">If true, category information in shortcuts.vdf file is removed if game is not in Game list</param>
        public void ExportSteamShortcuts(long SteamId)
        {
            string filePath = string.Format(Constants.ShortCutsFilePath, Settings.Instance.SteamPath, Profile.ID64toDirName(SteamId));

            FileStream fStream = null;
            BinaryReader binReader = null;
            VDFNode dataRoot = null;
            try
            {
                fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                binReader = new BinaryReader(fStream);

                dataRoot = VDFNode.LoadFromBinary(binReader, binReader.BaseStream.Length);
            }
            catch (FileNotFoundException e) { }
            catch (IOException e) { }

            if (binReader != null)
            {
                binReader.Close();
            }

            if (fStream != null)
            {
                fStream.Close();
            }

            if (dataRoot != null)
            {
                List<GameInfo> gamesToSave = new List<GameInfo>();
                foreach (int id in Games.Keys)
                {
                    if (id < 0)
                    {
                        gamesToSave.Add(Games[id]);
                    }
                }

                StringDictionary launchIds = new StringDictionary();
                LoadShortcutLaunchIds(SteamId, out launchIds);

                VDFNode appsNode = dataRoot.GetNodeAt(new[]
                {
                    "shortcuts"
                }, false);
                foreach (KeyValuePair<string, VDFNode> shortcutPair in appsNode.NodeArray)
                {
                    VDFNode nodeGame = shortcutPair.Value;
                    int nodeId = -1;
                    int.TryParse(shortcutPair.Key, out nodeId);

                    int matchingIndex = FindMatchingShortcut(nodeId, nodeGame, gamesToSave, launchIds);

                    if (matchingIndex >= 0)
                    {
                        GameInfo game = gamesToSave[matchingIndex];
                        gamesToSave.RemoveAt(matchingIndex);

                        VDFNode tagsNode = nodeGame.GetNodeAt(new[]
                        {
                            "tags"
                        }, true);
                        Dictionary<string, VDFNode> tags = tagsNode.NodeArray;
                        if (tags != null)
                        {
                            tags.Clear();
                        }

                        int index = 0;
                        foreach (Category c in game.Categories)
                        {
                            string name = c.Name;
                            if (name == FAVORITE_NEW_CONFIG_VALUE)
                            {
                                name = FAVORITE_CONFIG_VALUE;
                            }

                            tagsNode[index.ToString()] = new VDFNode(name);
                            index++;
                        }

                        nodeGame["hidden"] = new VDFNode(game.Hidden ? 1 : 0);
                    }
                }

                if (dataRoot.NodeType == ValueType.Array)
                {
                    try
                    {
                        Utility.BackupFile(filePath, Settings.Instance.ConfigBackupCount);
                    }
                    catch (Exception e) { }

                    try
                    {
                        string filePathTmp = filePath + ".tmp";
                        BinaryWriter binWriter;
                        fStream = new FileStream(filePathTmp, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                        binWriter = new BinaryWriter(fStream);
                        dataRoot.SaveAsBinary(binWriter);
                        binWriter.Close();
                        fStream.Close();
                        File.Delete(filePath);
                        File.Move(filePathTmp, filePath);
                    }
                    catch (ArgumentException e)
                    {
                        throw new ApplicationException(GlobalStrings.GameData_FailedToSaveSteamConfigBadPath, e);
                    }
                    catch (IOException e)
                    {
                        throw new ApplicationException(GlobalStrings.GameData_FailedToSaveSteamConfigFile + e.Message, e);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        throw new ApplicationException(GlobalStrings.GameData_AccessDeniedSteamConfigFile + e.Message, e);
                    }
                }
            }
        }

        /// <summary>
        ///     Checks to see if a Filter with the given name exists
        /// </summary>
        /// <param name="name">Name of the Filter to look for</param>
        /// <returns>True if the name is found, false otherwise</returns>
        public bool FilterExists(string name)
        {
            foreach (Filter f in Filters)
            {
                if (string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Gets the category with the given name. If the category does not exist, creates it.
        /// </summary>
        /// <param name="name">Name to get the category for</param>
        /// <returns>A category with the given name. Null if any error is encountered.</returns>
        public Category GetCategory(string name)
        {
            // Categories must have a name
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            // Check for Favorite category
            if (name == FAVORITE_NEW_CONFIG_VALUE || name == FAVORITE_CONFIG_VALUE)
            {
                return FavoriteCategory;
            }

            // Look for a matching category in the list and return if found
            foreach (Category c in Categories)
            {
                if (string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return c;
                }
            }

            // Create a new category and return it
            return AddCategory(name);

            //Category newCat = new Category( name );
            //Categories.Add( newCat );
            //return newCat;
        }

        /// <summary>
        ///     Gets the Filter with the given name. If the Filter does not exist, creates it.
        /// </summary>
        /// <param name="name">Name to get the Filter for</param>
        /// <returns>A Filter with the given name. Null if any error is encountered.</returns>
        public Filter GetFilter(string name)
        {
            // Filters must have a name
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            // Look for a matching Filter in the list and return if found
            foreach (Filter f in Filters)
            {
                if (string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return f;
                }
            }

            // Create a new Filter and return it
            Filter newFilter = new Filter(name);
            Filters.Add(newFilter);

            return newFilter;
        }

        /// <summary>
        ///     Add or Remove the hidden attribute of a single game
        /// </summary>
        /// <param name="gameID">Game ID to hide/unhide</param>
        /// <param name="hide">Whether the game should be hidden.</param>
        public void HideGames(int gameID, bool hide)
        {
            Games[gameID].SetHidden(hide);
        }

        /// <summary>
        ///     Add or Remove the hidden attribute from a set of games
        /// </summary>
        /// <param name="gameIDs">List of game IDs to hide/unhide</param>
        /// <param name="hide">Whether the games should be hidden.</param>
        public void HideGames(int[] gameIDs, bool hide)
        {
            foreach (int id in gameIDs)
            {
                HideGames(id, hide);
            }
        }

        /// <summary>
        ///     Loads category info from the steam config file for the given Steam user.
        /// </summary>
        /// <param name="SteamId">Identifier of Steam user</param>
        /// <param name="ignore">Set of games to ignore</param>
        /// <param name="forceInclude">If true, include games that do not match the included types</param>
        /// <param name="includeShortcuts">If true, also import shortcut data</param>
        /// <returns>The number of game entries found</returns>
        public int ImportSteamConfig(long SteamId, SortedSet<int> ignore, bool includeShortcuts)
        {
            string filePath = string.Format(Constants.ConfigFilePath, Settings.Instance.SteamPath, Profile.ID64toDirName(SteamId));
            int result = ImportSteamConfigFile(filePath, ignore);
            if (includeShortcuts)
            {
                result += ImportSteamShortcuts(SteamId);
            }

            return result;
        }

        /// <summary>
        ///     Loads category info from the given steam config file.
        /// </summary>
        /// <param name="filePath">The path of the file to open</param>
        /// <param name="ignore">Set of game IDs to ignore</param>
        /// <param name="forceInclude">If true, include games even if they are not of an included type</param>
        /// <returns>The number of game entries found</returns>
        public int ImportSteamConfigFile(string filePath, SortedSet<int> ignore)
        {
            VDFNode dataRoot;

            try
            {
                using (StreamReader reader = new StreamReader(filePath, false))
                {
                    dataRoot = VDFNode.LoadFromText(reader, true);
                }
            }
            catch (DataException e)
            {
                throw new DataException("Error parsing Steam config file: {0} {1}" + e.Message + e);
            }
            catch (IOException e)
            {
                throw new ApplicationException(GlobalStrings.GameData_ErrorOpeningSteamConfigFile + e.Message, e);
            }

            VDFNode appsNode = dataRoot.GetNodeAt(new[]
            {
                "Software",
                "Valve",
                "Steam",
                "apps"
            }, true);
            int count = IntegrateGamesFromVdf(appsNode, ignore);

            return count;
        }

        /// <summary>
        ///     Updates set of non-Steam games. Will remove any games that are currently in the list but not found in the Steam
        ///     config.
        /// </summary>
        /// <param name="SteamId">The ID64 of the account to load shortcuts for</param>
        /// <param name="overwriteCategories">
        ///     If true, overwrite categories for found games. If false, only load categories for
        ///     games without a category already set.
        /// </param>
        /// <returns>Total number of entries processed</returns>
        public int ImportSteamShortcuts(long SteamId)
        {
            if (SteamId <= 0)
            {
                return 0;
            }

            int loadedGames = 0;

            string filePath = string.Format(Constants.ShortCutsFilePath, Settings.Instance.SteamPath, Profile.ID64toDirName(SteamId));
            FileStream fStream = null;
            BinaryReader binReader = null;

            try
            {
                fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                binReader = new BinaryReader(fStream);

                VDFNode dataRoot = VDFNode.LoadFromBinary(binReader, binReader.BaseStream.Length);

                VDFNode shortcutsNode = dataRoot.GetNodeAt(new[]
                {
                    "shortcuts"
                }, false);

                if (shortcutsNode != null)
                {
                    // Remove existing shortcuts
                    List<int> oldShortcutIds = new List<int>();
                    foreach (int id in Games.Keys)
                    {
                        if (id < 0)
                        {
                            oldShortcutIds.Add(id);
                        }
                    }

                    foreach (int g in oldShortcutIds)
                    {
                        Games.Remove(g);
                    }

                    // Load launch IDs
                    StringDictionary launchIds = null;
                    bool launchIdsLoaded = LoadShortcutLaunchIds(SteamId, out launchIds);

                    // Load shortcuts
                    foreach (KeyValuePair<string, VDFNode> shortcutPair in shortcutsNode.NodeArray)
                    {
                        VDFNode nodeGame = shortcutPair.Value;

                        int gameId = -1;
                        if (int.TryParse(shortcutPair.Key, out gameId))
                        {
                            if (IntegrateShortcut(gameId, nodeGame, launchIds))
                            {
                                loadedGames++;
                            }
                        }
                    }
                }
            }
            catch (FileNotFoundException e) { }
            catch (IOException e) { }
            catch (DataException e) { }
            finally
            {
                if (binReader != null)
                {
                    binReader.Close();
                }

                if (fStream != null)
                {
                    fStream.Close();
                }
            }

            return loadedGames;
        }

        /// <summary>
        ///     Integrates list of games from an HTML page into the loaded game list.
        /// </summary>
        /// <param name="page">The full text of the page to load</param>
        /// <param name="overWrite">If true, overwrite the names of games already in the list.</param>
        /// <param name="ignore">A set of item IDs to ignore. Can be null.</param>
        /// <param name="ignoreDlc">Ignore any items classified as DLC in the database.</param>
        /// <param name="newItems">The number of new items actually added</param>
        /// <returns>Returns the number of games successfully processed and not ignored.</returns>
        public int IntegrateHtmlGameList(string page, bool overWrite, SortedSet<int> ignore, out int newItems)
        {
            newItems = 0;
            int totalItems = 0;

            Regex srch = new Regex("\"appid\":([0-9]+),\"name\":\"([^\"]+)\"");
            MatchCollection matches = srch.Matches(page);
            foreach (Match m in matches)
            {
                if (m.Groups.Count < 3)
                {
                    continue;
                }

                string appIdString = m.Groups[1].Value;
                string appName = m.Groups[2].Value;

                int appId;
                if (appName != null && appIdString != null && int.TryParse(appIdString, out appId))
                {
                    appName = ProcessUnicode(appName);
                    bool isNew;
                    GameInfo integratedGame = IntegrateGame(appId, appName, overWrite, ignore, GameListingSource.WebProfile, out isNew);
                    if (integratedGame != null)
                    {
                        totalItems++;
                        if (isNew)
                        {
                            newItems++;
                        }
                    }
                }
            }

            return totalItems;
        }

        /// <summary>
        ///     Integrates list of games from an XmlDocument into the loaded game list.
        /// </summary>
        /// <param name="doc">The XmlDocument containing the new game list</param>
        /// <param name="overWrite">If true, overwrite the names of games already in the list.</param>
        /// <param name="ignore">A set of item IDs to ignore.</param>
        /// <param name="ignoreDlc">Ignore any items classified as DLC in the database.</param>
        /// <param name="newItems">The number of new items actually added</param>
        /// <returns>Returns the number of games successfully processed and not ignored.</returns>
        public int IntegrateXmlGameList(XmlDocument doc, bool overWrite, SortedSet<int> ignore, out int newItems)
        {
            newItems = 0;
            if (doc == null)
            {
                return 0;
            }

            int loadedGames = 0;
            XmlNodeList gameNodes = doc.SelectNodes("/gamesList/games/game");
            foreach (XmlNode gameNode in gameNodes)
            {
                int appId;
                XmlNode appIdNode = gameNode["appID"];
                if (appIdNode != null && int.TryParse(appIdNode.InnerText, out appId))
                {
                    XmlNode nameNode = gameNode["name"];
                    if (nameNode != null)
                    {
                        bool isNew;
                        GameInfo integratedGame = IntegrateGame(appId, nameNode.InnerText, overWrite, ignore, GameListingSource.WebProfile, out isNew);
                        if (integratedGame != null)
                        {
                            loadedGames++;
                            if (isNew)
                            {
                                newItems++;
                            }
                        }
                    }
                }
            }

            return loadedGames;
        }

        /// <summary>
        ///     Searches a string for HTML unicode entities ('\u####') and replaces them with actual unicode characters.
        /// </summary>
        /// <param name="val">The string to process</param>
        /// <returns>The processed string</returns>
        public string ProcessUnicode(string val)
        {
            return rxUnicode.Replace(val, m => ((char) int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString());
        }

        /// <summary>
        ///     Removes the given category.
        /// </summary>
        /// <param name="c">Category to remove.</param>
        /// <returns>True if removal was successful, false if it was not in the list anyway</returns>
        public bool RemoveCategory(Category c)
        {
            // Can't remove favorite category
            if (c == FavoriteCategory)
            {
                return false;
            }

            if (Categories.Remove(c))
            {
                foreach (GameInfo g in Games.Values)
                {
                    g.RemoveCategory(c);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Remove all empty categories from the category list.
        /// </summary>
        /// <returns>Number of categories removed</returns>
        public int RemoveEmptyCategories()
        {
            Dictionary<Category, int> counts = new Dictionary<Category, int>();
            foreach (Category c in Categories)
            {
                if (c != FavoriteCategory)
                {
                    counts.Add(c, 0);
                }
            }

            foreach (GameInfo g in Games.Values)
            {
                foreach (Category c in g.Categories)
                {
                    if (counts.ContainsKey(c))
                    {
                        counts[c]++;
                    }
                }
            }

            int removed = 0;
            foreach (KeyValuePair<Category, int> pair in counts)
            {
                if (pair.Value == 0)
                {
                    if (Categories.Remove(pair.Key))
                    {
                        removed++;
                    }
                }
            }

            return removed;
        }

        /// <summary>
        ///     Removes the given Filter.
        /// </summary>
        /// <param name="f">Filter to remove.</param>
        /// <returns>True if removal was successful, false if it was not in the list anyway</returns>
        public bool RemoveFilter(Filter f)
        {
            if (Filters.Remove(f))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Removes a single category from a single game.
        /// </summary>
        /// <param name="gameID">Game ID to remove from</param>
        /// <param name="c">Category to remove</param>
        public void RemoveGameCategory(int gameID, Category c)
        {
            GameInfo g = Games[gameID];
            g.RemoveCategory(c);
        }

        /// <summary>
        ///     Removes a single category from each member of a list of games
        /// </summary>
        /// <param name="gameIDs">List of game IDs to remove from</param>
        /// <param name="c">Category to remove</param>
        public void RemoveGameCategory(int[] gameIDs, Category c)
        {
            for (int i = 0; i < gameIDs.Length; i++)
            {
                RemoveGameCategory(gameIDs[i], c);
            }
        }

        /// <summary>
        ///     Removes a set of categories from a single game
        /// </summary>
        /// <param name="gameID">Game ID to remove from</param>
        /// <param name="cats">Set of categories to remove</param>
        public void RemoveGameCategory(int gameID, ICollection<Category> cats)
        {
            GameInfo g = Games[gameID];
            g.RemoveCategory(cats);
        }

        /// <summary>
        ///     Removes a set of categories from a set of games
        /// </summary>
        /// <param name="gameIDs">List of game IDs to remove from</param>
        /// <param name="cats">Set of categories to remove</param>
        public void RemoveGameCategory(int[] gameIDs, ICollection<Category> cats)
        {
            for (int i = 0; i < gameIDs.Length; i++)
            {
                RemoveGameCategory(i, cats);
            }
        }

        /// <summary>
        ///     Renames the given category.
        /// </summary>
        /// <param name="c">Category to rename.</param>
        /// <param name="newName">Name to assign to the new category.</param>
        /// <returns>The new category, if the operation succeeds. Null otherwise.</returns>
        public Category RenameCategory(Category c, string newName)
        {
            if (c == FavoriteCategory)
            {
                return null;
            }

            Category newCat = AddCategory(newName);
            if (newCat != null)
            {
                Categories.Sort();
                foreach (GameInfo game in Games.Values)
                {
                    if (game.ContainsCategory(c))
                    {
                        game.RemoveCategory(c);
                        game.AddCategory(newCat);
                    }
                }

                RemoveCategory(c);

                return newCat;
            }

            return null;
        }

        public void SetGameCategories(int gameID, Category cat, bool preserveFavorites)
        {
            SetGameCategories(gameID, new List<Category>
            {
                cat
            }, preserveFavorites);
        }

        public void SetGameCategories(int[] gameIDs, Category cat, bool preserveFavorites)
        {
            SetGameCategories(gameIDs, new List<Category>
            {
                cat
            }, preserveFavorites);
        }

        /// <summary>
        ///     Sets a game's categories to a particular set
        /// </summary>
        /// <param name="gameID">Game ID to modify</param>
        /// <param name="catSet">Set of categories to apply</param>
        /// <param name="preserveFavorites">If true, will not remove "favorite" category</param>
        public void SetGameCategories(int gameID, ICollection<Category> catSet, bool preserveFavorites)
        {
            Games[gameID].SetCategories(catSet, preserveFavorites);
        }

        /// <summary>
        ///     Sets multiple games' categories to a particular set
        /// </summary>
        /// <param name="gameID">Game IDs to modify</param>
        /// <param name="catSet">Set of categories to apply</param>
        /// <param name="preserveFavorites">If true, will not remove "favorite" category</param>
        public void SetGameCategories(int[] gameIDs, ICollection<Category> catSet, bool preserveFavorites)
        {
            for (int i = 0; i < gameIDs.Length; i++)
            {
                SetGameCategories(gameIDs[i], catSet, preserveFavorites);
            }
        }

        public int UpdateGameListFromOwnedPackageInfo(long accountId, SortedSet<int> ignoreList, out int newApps)
        {
            newApps = 0;
            int totalApps = 0;

            try
            {
                Dictionary<int, PackageInfo> allPackages = PackageInfo.LoadPackages(string.Format(Constants.PackageInfoPath, Settings.Instance.SteamPath));
                Dictionary<int, GameListingSource> ownedApps = new Dictionary<int, GameListingSource>();

                string localConfigPath = string.Format(Constants.LocalConfigPath, Settings.Instance.SteamPath, Profile.ID64toDirName(accountId));

                VDFNode vdfFile = VDFNode.LoadFromText(new StreamReader(localConfigPath));

                VDFNode licensesNode = vdfFile.GetNodeAt(new[]
                {
                    "UserLocalConfigStore",
                    "Licenses"
                }, false);

                if (licensesNode.NodeType == ValueType.Array)
                {
                    foreach (string key in licensesNode.NodeArray.Keys)
                    {
                        if (!int.TryParse(key, out int ownedPackageId))
                        {
                            continue;
                        }

                        if (!allPackages.ContainsKey(ownedPackageId))
                        {
                            continue;
                        }

                        PackageInfo ownedPackage = allPackages[ownedPackageId];
                        if (ownedPackageId == 0)
                        {
                            continue;
                        }

                        GameListingSource src = ownedPackage.BillingType == PackageBillingType.FreeOnDemand || ownedPackage.BillingType == PackageBillingType.AutoGrant ? GameListingSource.PackageFree : GameListingSource.PackageNormal;
                        foreach (int ownedAppId in ownedPackage.AppIds)
                        {
                            if (!ownedApps.ContainsKey(ownedAppId) || src == GameListingSource.PackageNormal && ownedApps[ownedAppId] == GameListingSource.PackageFree)
                            {
                                ownedApps[ownedAppId] = src;
                            }
                        }
                    }
                }

                // update LastPlayed
                VDFNode appsNode = vdfFile.GetNodeAt(new[]
                {
                    "UserLocalConfigStore",
                    "Software",
                    "Valve",
                    "Steam",
                    "apps"
                }, false);
                GetLastPlayedFromVdf(appsNode, ignoreList);

                foreach (KeyValuePair<int, GameListingSource> kv in ownedApps)
                {
                    string name = Database.Instance.GetName(kv.Key);
                    GameInfo newGame = IntegrateGame(kv.Key, name, false, ignoreList, kv.Value, out bool isNew);
                    if (newGame != null)
                    {
                        totalApps++;
                    }

                    if (isNew)
                    {
                        newApps++;
                    }
                }
            }
            catch (Exception e)
            {
                SentryLogger.LogException(e);
            }

            return totalApps;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Searches a list of games, looking for the one that matches the information in the shortcut node.
        ///     Checks launch ID first, then checks a combination of name and ID, then just checks name.
        /// </summary>
        /// <param name="shortcutId">ID of the shortcut node</param>
        /// <param name="shortcutNode">Shotcut node itself</param>
        /// <param name="gamesToMatchAgainst">List of game objects to match against</param>
        /// <param name="shortcutLaunchIds">List of launch IDs referenced by name</param>
        /// <returns>The index of the matching game if found, -1 otherwise.</returns>
        private int FindMatchingShortcut(int shortcutId, VDFNode shortcutNode, List<GameInfo> gamesToMatchAgainst, StringDictionary shortcutLaunchIds)
        {
            VDFNode nodeName = shortcutNode.GetNodeAt(new[]
            {
                "appname"
            }, false);
            string gameName = nodeName != null ? nodeName.NodeString : null;
            string launchId = shortcutLaunchIds[gameName];
            // First, look for games with matching launch IDs.
            for (int i = 0; i < gamesToMatchAgainst.Count; i++)
            {
                if (gamesToMatchAgainst[i].LaunchString == launchId)
                {
                    return i;
                }
            }

            // Second, look for games with matching names AND matching shortcut IDs.
            for (int i = 0; i < gamesToMatchAgainst.Count; i++)
            {
                if (gamesToMatchAgainst[i].Id == -(shortcutId + 1) && gamesToMatchAgainst[i].Name == gameName)
                {
                    return i;
                }
            }

            // Third, just look for name matches
            for (int i = 0; i < gamesToMatchAgainst.Count; i++)
            {
                if (gamesToMatchAgainst[i].Name == gameName)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        ///     Get LastPlayed date from a VDF node containing a list of games.
        ///     Any games in the node not found in the game list will be added to the gamelist.
        /// </summary>
        /// <param name="appsNode">Node containing the game nodes</param>
        /// <param name="ignore">Set of games to ignore</param>
        /// <param name="forceInclude">Include games even if their type is not an included type</param>
        private void GetLastPlayedFromVdf(VDFNode appsNode, SortedSet<int> ignore)
        {
            Dictionary<string, VDFNode> gameNodeArray = appsNode.NodeArray;
            if (gameNodeArray != null)
            {
                foreach (KeyValuePair<string, VDFNode> gameNodePair in gameNodeArray)
                {
                    int gameId;
                    if (int.TryParse(gameNodePair.Key, out gameId))
                    {
                        if (ignore != null && ignore.Contains(gameId)) { }
                        else if (gameNodePair.Value != null && gameNodePair.Value.NodeType == ValueType.Array)
                        {
                            GameInfo game = null;

                            // Add the game to the list if it doesn't exist already
                            if (!Games.ContainsKey(gameId))
                            {
                                game = new GameInfo(gameId, Database.Instance.GetName(gameId), this);
                                Games.Add(gameId, game);
                            }
                            else
                            {
                                game = Games[gameId];
                            }

                            if (gameNodePair.Value.ContainsKey("LastPlayed") && gameNodePair.Value["LastPlayed"].NodeInt != 0)
                            {
                                game.LastPlayed = gameNodePair.Value["LastPlayed"].NodeInt;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Adds a new game to the database, or updates an existing game with new information.
        /// </summary>
        /// <param name="appId">App ID to add or update</param>
        /// <param name="appName">Name of app to add, or update to</param>
        /// <param name="overwriteName">If true, will overwrite any existing games. If false, will fail if the game already exists.</param>
        /// <param name="ignore">Set of games to ignore. Can be null. If the game is in this list, no action will be taken.</param>
        /// <param name="forceInclude">If true, include the game even if it is of an ignored type.</param>
        /// <param name="src">The listing source that this request came from.</param>
        /// <param name="isNew">If true, a new game was added. If false, an existing game was updated, or the operation failed.</param>
        /// <returns>True if the game was integrated, false otherwise.</returns>
        private GameInfo IntegrateGame(int appId, string appName, bool overwriteName, SortedSet<int> ignore, GameListingSource src, out bool isNew)
        {
            isNew = false;
            if (ignore != null && ignore.Contains(appId) || !Database.Instance.Contains(appId))
            {
                return null;
            }

            GameInfo result = null;
            if (!Games.ContainsKey(appId))
            {
                result = new GameInfo(appId, appName, this);
                Games.Add(appId, result);
                isNew = true;
            }
            else
            {
                result = Games[appId];
                if (overwriteName)
                {
                    result.Name = appName;
                }
            }

            result.ApplySource(src);

            return result;
        }

        /// <summary>
        ///     Loads in games from a VDF node containing a list of games.
        ///     Any games in the node not found in the game list will be added to the gamelist.
        ///     If a game in the node has a tags subnode, the "favorite" field will be overwritten.
        ///     If a game in the node has a category set, it will overwrite any categories in the gamelist.
        ///     If a game in the node does NOT have a category set, the category in the gamelist will NOT be cleared.
        /// </summary>
        /// <param name="appsNode">Node containing the game nodes</param>
        /// <param name="ignore">Set of games to ignore</param>
        /// <param name="forceInclude">Include games even if their type is not an included type</param>
        /// <returns>Number of games loaded</returns>
        private int IntegrateGamesFromVdf(VDFNode appsNode, SortedSet<int> ignore)
        {
            int loadedGames = 0;

            Dictionary<string, VDFNode> gameNodeArray = appsNode.NodeArray;
            if (gameNodeArray != null)
            {
                foreach (KeyValuePair<string, VDFNode> gameNodePair in gameNodeArray)
                {
                    int gameId;
                    if (int.TryParse(gameNodePair.Key, out gameId))
                    {
                        if (ignore != null && ignore.Contains(gameId) || !Database.Instance.Contains(gameId)) { }
                        else if (gameNodePair.Value != null && gameNodePair.Value.NodeType == ValueType.Array)
                        {
                            GameInfo game = null;

                            // Add the game to the list if it doesn't exist already
                            if (!Games.ContainsKey(gameId))
                            {
                                game = new GameInfo(gameId, Database.Instance.GetName(gameId), this);
                                Games.Add(gameId, game);
                            }
                            else
                            {
                                game = Games[gameId];
                            }

                            loadedGames++;

                            game.ApplySource(GameListingSource.SteamConfig);

                            game.Hidden = gameNodePair.Value.ContainsKey("hidden") && gameNodePair.Value["hidden"].NodeInt != 0;

                            VDFNode tagsNode = gameNodePair.Value["tags"];
                            if (tagsNode != null)
                            {
                                Dictionary<string, VDFNode> tagArray = tagsNode.NodeArray;
                                if (tagArray != null)
                                {
                                    List<Category> cats = new List<Category>(tagArray.Count);
                                    foreach (VDFNode tag in tagArray.Values)
                                    {
                                        string tagName = tag.NodeString;
                                        if (tagName != null)
                                        {
                                            Category c = GetCategory(tagName);
                                            if (c != null)
                                            {
                                                cats.Add(c);
                                            }
                                        }
                                    }

                                    if (cats.Count > 0)
                                    {
                                        SetGameCategories(gameId, cats, false);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return loadedGames;
        }

        /// <summary>
        ///     Adds a non-steam game to the gamelist.
        /// </summary>
        /// <param name="gameId">ID of the game in the steam config file</param>
        /// <param name="gameNode">Node for the game in the steam config file</param>
        /// <param name="launchIds">Dictionary of launch ids (name:launchId)</param>
        /// <param name="newGames">Number of NEW games that have been added to the list</param>
        /// <param name="preferSteamCategories">
        ///     If true, prefers to use the categories from the steam config if there is a
        ///     conflict. If false, prefers to use the categories from the existing gamelist.
        /// </param>
        /// <returns>True if the game was successfully added</returns>
        private bool IntegrateShortcut(int gameId, VDFNode gameNode, StringDictionary launchIds)
        {
            VDFNode nodeName = gameNode.GetNodeAt(new[]
            {
                "appname"
            }, false);
            string gameName = nodeName != null ? nodeName.NodeString : null;
            // The ID of the created game must be negative
            int newId = -(gameId + 1);

            // This should never happen, but just in case
            if (Games.ContainsKey(newId))
            {
                return false;
            }

            //Create the new GameInfo
            GameInfo game = new GameInfo(newId, gameName, this);
            Games.Add(newId, game);

            // Fill in the LaunchString
            game.LaunchString = launchIds[gameName];
            VDFNode nodeExecutable = gameNode.GetNodeAt(new[]
            {
                "exe"
            }, false);
            game.Executable = nodeExecutable != null ? nodeExecutable.NodeString : game.Executable;

            VDFNode nodeLastPlayTime = gameNode.GetNodeAt(new[]
            {
                "LastPlayTime"
            }, false);
            game.LastPlayed = nodeLastPlayTime != null ? nodeExecutable.NodeInt : game.LastPlayed;

            // Fill in categories
            VDFNode tagsNode = gameNode.GetNodeAt(new[]
            {
                "tags"
            }, false);
            foreach (KeyValuePair<string, VDFNode> tag in tagsNode.NodeArray)
            {
                string tagName = tag.Value.NodeString;
                game.AddCategory(GetCategory(tagName));
            }

            // Fill in Hidden
            game.Hidden = false;
            if (gameNode.ContainsKey("IsHidden"))
            {
                VDFNode hiddenNode = gameNode["IsHidden"];
                game.Hidden = hiddenNode.NodeString == "1" || hiddenNode.NodeInt == 1;
            }

            return true;
        }

        /// <summary>
        ///     Load launch IDs for external games from screenshots.vdf
        /// </summary>
        /// <param name="SteamId">Steam user identifier</param>
        /// <param name="shortcutLaunchIds">Found games listed as pairs of {gameName, gameId} </param>
        /// <returns>True if file was successfully loaded, false otherwise</returns>
        private bool LoadShortcutLaunchIds(long SteamId, out StringDictionary shortcutLaunchIds)
        {
            bool result = false;
            string filePath = string.Format(Constants.ScreenshotsFilePath, Settings.Instance.SteamPath, Profile.ID64toDirName(SteamId));

            shortcutLaunchIds = new StringDictionary();

            StreamReader reader = null;
            try
            {
                reader = new StreamReader(filePath, false);
                VDFNode dataRoot = VDFNode.LoadFromText(reader, true);

                VDFNode appsNode = dataRoot.GetNodeAt(new[]
                {
                    "shortcutnames"
                }, false);

                foreach (KeyValuePair<string, VDFNode> shortcutPair in appsNode.NodeArray)
                {
                    string launchId = shortcutPair.Key;
                    string gameName = (string) shortcutPair.Value.NodeData;
                    if (!shortcutLaunchIds.ContainsKey(gameName))
                    {
                        shortcutLaunchIds.Add(gameName, launchId);
                    }
                }

                result = true;
            }
            catch (FileNotFoundException e) { }
            catch (IOException e) { }

            if (reader != null)
            {
                reader.Close();
            }

            return result;
        }

        /// <summary>
        ///     Removes a game from the game list.
        /// </summary>
        /// <param name="appId">Id of game to remove.</param>
        /// <returns>True if game was removed, false otherwise</returns>
        private bool RemoveGame(int appId)
        {
            bool removed = false;
            if (appId < 0)
            {
                if (Games.ContainsKey(appId))
                {
                    GameInfo removedGame = Games[appId];
                    removedGame.ClearCategories(true);
                    removed = Games.Remove(appId);
                }
            }

            return removed;
        }

        #endregion
    }
}