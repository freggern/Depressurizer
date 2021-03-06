#region LICENSE

//     This file (GlobalSuppressions.cs) is part of DepressurizerCore.
//     Copyright (C) 2018  Martijn Vegter
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Depressurizer", Scope = "namespace", Target = "DepressurizerCore")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Depressurizer")]
[assembly: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames")]
[assembly: SuppressMessage("Microsoft.Design", "CA1014:MarkAssembliesWithClsCompliant")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Depressurizer", Scope = "namespace", Target = "DepressurizerCore.Models")]
[assembly: SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "member", Target = "DepressurizerCore.Models.VRSupport.#Headsets")]
[assembly: SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "member", Target = "DepressurizerCore.Models.VRSupport.#Input")]
[assembly: SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "member", Target = "DepressurizerCore.Models.VRSupport.#PlayArea")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "DepressurizerCore.Models.VRSupport.#Headsets")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "DepressurizerCore.Models.VRSupport.#Input")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "DepressurizerCore.Models.VRSupport.#PlayArea")]
[assembly: SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "member", Target = "DepressurizerCore.Models.LanguageSupport.#Subtitles")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "DepressurizerCore.Models.LanguageSupport.#FullAudio")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "DepressurizerCore.Models.LanguageSupport.#Interface")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "DepressurizerCore.Models.LanguageSupport.#Subtitles")]
[assembly: SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "member", Target = "DepressurizerCore.Models.LanguageSupport.#FullAudio")]
[assembly: SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "member", Target = "DepressurizerCore.Models.LanguageSupport.#Interface")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Depressurizer", Scope = "namespace", Target = "DepressurizerCore.Helpers")]
[assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope = "type", Target = "DepressurizerCore.Models.Category")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DepressurizerCore")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DepressurizerCore.Helpers")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DepressurizerCore.Models")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y", Scope = "member", Target = "DepressurizerCore.Settings.#Y")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X", Scope = "member", Target = "DepressurizerCore.Settings.#X")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member", Target = "DepressurizerCore.Settings.#Save()")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member", Target = "DepressurizerCore.Settings.#Load()")]
[assembly: SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "member", Target = "DepressurizerCore.Helpers.Steam.#GrabBanners(System.Collections.Generic.List`1<System.Int32>)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope = "type", Target = "DepressurizerCore.Helpers.Location+Folder")]
[assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope = "type", Target = "DepressurizerCore.Helpers.Location+File")]
[assembly: SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "member", Target = "DepressurizerCore.Models.DatabaseEntry.#Tags")]
[assembly: SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "member", Target = "DepressurizerCore.Models.DatabaseEntry.#Publishers")]
[assembly: SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "member", Target = "DepressurizerCore.Models.DatabaseEntry.#Genres")]
[assembly: SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "member", Target = "DepressurizerCore.Models.DatabaseEntry.#Flags")]
[assembly: SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "member", Target = "DepressurizerCore.Models.DatabaseEntry.#Developers")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags", Scope = "member", Target = "DepressurizerCore.Models.DatabaseEntry.#Flags")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "DepressurizerCore.Models.DatabaseEntry.#Tags")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "DepressurizerCore.Models.DatabaseEntry.#Publishers")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "DepressurizerCore.Models.DatabaseEntry.#Genres")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "DepressurizerCore.Models.DatabaseEntry.#Flags")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "DepressurizerCore.Models.DatabaseEntry.#Developers")]
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.