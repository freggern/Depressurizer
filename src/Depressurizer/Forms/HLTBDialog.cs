﻿#region LICENSE

//     This file (HLTBDialog.cs) is part of Depressurizer.
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

using DepressurizerCore;

namespace Depressurizer.Forms
{
    internal class HLTBDialog : CancelableDialog
    {
        #region Constructors and Destructors

        public HLTBDialog() : base("Updating HowLongToBeat times", false)
        {
            SetText("Updating HowLongToBeat times...");
        }

        #endregion

        #region Public Properties

        public int Updated { get; private set; } = 0;

        #endregion

        #region Methods

        protected override void OnFinish()
        {
            if (!Canceled)
            {
                OnJobCompletion();
            }
        }

        protected override void OnStart()
        {
            Updated = Database.Instance.UpdateFromHLTB(Settings.Instance.IncludeImputedTimes);
            CloseDialog();
        }

        #endregion
    }
}