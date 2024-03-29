﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using DataMigrator.Common;
using DataMigrator.Common.Data;
using DataMigrator.Common.Models;

namespace DataMigrator.Csv
{
    [Export(typeof(IMigrationPlugin))]
    public class CsvMigrationPlugin : IMigrationPlugin
    {
        #region IMigrationPlugin Members

        public string ProviderName => Constants.PROVIDER_NAME;

        public IConnectionControl ConnectionControl => new CsvConnectionControl();

        public BaseProvider GetDataProvider(ConnectionDetails connectionDetails) => new CsvProvider(connectionDetails);

        public ISettingsControl SettingsControl => null;

        public IEnumerable<IMigrationTool> Tools => null;

        #endregion IMigrationPlugin Members
    }
}